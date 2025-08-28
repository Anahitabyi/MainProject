using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class ChunkGenerator : NetworkBehaviour
{
    [System.Serializable]
    public class ChunkData
    {
        public GameObject chunkPrefab;
        public float yPosition;
        public float width;
    }

    [SerializeField] private float generateDistance = 25f;
    [SerializeField] private int initialChunks = 3;
    [SerializeField] private List<ChunkData> chunkDataList = new List<ChunkData>();

    [SerializeField] private GameObject finalChunkPrefab;
    [SerializeField] private float finalChunkYPosition = 0f;
    [SerializeField] private float finalChunkWidth = 20f;

    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private float backgroundYPosition = 0f;

    [HideInInspector] public Transform[] players;

    public Transform FirstSpawnPoint { get; private set; }

    private List<ChunkData> unusedChunks = new List<ChunkData>();

    private class SpawnedChunk
    {
        public GameObject chunkObject;
        public float width;
        public SpawnedChunk(GameObject obj, float width)
        {
            chunkObject = obj;
            this.width = width;
        }
    }

    private List<SpawnedChunk> activeChunks = new List<SpawnedChunk>();
    private List<GameObject> activeBackgrounds = new List<GameObject>();

    private int currentChunkIndex = 0;
    private bool finalChunkSpawned = false;
    private bool useSavedChunks = false;

    private static int chunkSeed; // Shared seed
    private static bool seedSet = false;

    public void Start()
    {
        if (useSavedChunks)
        {
            ClearAllChunks();
            return;
        }

        if (!seedSet)
        {
            // Server chooses seed, clients get it synced
            if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
            {
                chunkSeed = System.Environment.TickCount; // or from SaveTracker
                SetSeedClientRpc(chunkSeed);
            }
        }

        // Use the same seed for deterministic shuffling
        Random.InitState(chunkSeed);

        if (SaveTracker.Instance != null)
            SaveTracker.Instance.ClearChunks();

        unusedChunks = new List<ChunkData>(chunkDataList);
        ShuffleList(unusedChunks);

        for (int i = 0; i < initialChunks && unusedChunks.Count > 0; i++)
        {
            GenerateChunk();
        }
    }

    [ClientRpc]
    private void SetSeedClientRpc(int seed)
    {
        if (!seedSet)
        {
            chunkSeed = seed;
            seedSet = true;
        }
    }

    void Update()
    {
        // Both server and clients can generate chunks now, since they’re deterministic
        if (useSavedChunks || players == null || players.Length == 0 || finalChunkSpawned) return;

        float maxPlayerX = float.MinValue;
        foreach (Transform player in players)
        {
            if (player != null)
                maxPlayerX = Mathf.Max(maxPlayerX, player.position.x);
        }

        if (activeChunks.Count == 0) return;

        SpawnedChunk lastChunk = activeChunks[activeChunks.Count - 1];
        float lastChunkEndX = lastChunk.chunkObject.transform.position.x + lastChunk.width;

        if (maxPlayerX + generateDistance > lastChunkEndX)
        {
            GenerateChunk();
        }
    }

    GameObject GenerateChunk()
    {
        float positionX = 0f;
        if (activeChunks.Count > 0)
        {
            SpawnedChunk lastChunk = activeChunks[activeChunks.Count - 1];
            positionX = lastChunk.chunkObject.transform.position.x + lastChunk.width;
        }

        GameObject chunkToSpawn = null;
        float chunkY = 0f;
        float chunkWidth = 0f;

        if (unusedChunks.Count > 0)
        {
            ChunkData data = unusedChunks[0];
            chunkToSpawn = data.chunkPrefab;
            chunkY = data.yPosition;
            chunkWidth = data.width;
            unusedChunks.RemoveAt(0);
        }
        else if (!finalChunkSpawned && finalChunkPrefab != null)
        {
            chunkToSpawn = finalChunkPrefab;
            chunkY = finalChunkYPosition;
            chunkWidth = finalChunkWidth;
            finalChunkSpawned = true;
        }

        if (chunkToSpawn == null) return null;

        // Chunks are just local objects (not NetworkObjects!)
        GameObject chunk = Instantiate(chunkToSpawn, new Vector3(positionX, chunkY, 0), Quaternion.identity, transform);
        chunk.name = "Chunk_" + currentChunkIndex;
        currentChunkIndex++;

        if (activeChunks.Count == 0)
        {
            Transform spawn = chunk.transform.Find("SpawnPoint");
            if (spawn != null)
                FirstSpawnPoint = spawn;
        }

        activeChunks.Add(new SpawnedChunk(chunk, chunkWidth));

        if (chunk.CompareTag("Chunk") && SaveTracker.Instance != null)
        {
            SaveTracker.Instance.RecordChunk(
                chunkToSpawn.name,
                currentChunkIndex - 1,
                new Vector2(positionX, chunkY),
                chunkWidth
            );
        }

        // Only server spawns enemies as NetworkObjects
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            EnemyShooter[] shooters = chunk.GetComponentsInChildren<EnemyShooter>();
            foreach (EnemyShooter shooter in shooters)
            {
                var netObj = shooter.GetComponent<NetworkObject>();
                if (netObj != null && !netObj.IsSpawned)
                    netObj.Spawn();
                shooter.SetPlayers(GetPlayerGameObjects());
            }
        }
        if (finalChunkSpawned)
        {
            SpawnChunkClientRpc(chunkToSpawn.name, new Vector3(positionX, chunkY, 0), currentChunkIndex - 1, chunkWidth, true);
        }


        // Background (local only, no need to network)
        if (backgroundPrefab != null)
        {
            GameObject background = Instantiate(backgroundPrefab);
            background.transform.position = new Vector3(positionX, backgroundYPosition, -1);
            activeBackgrounds.Add(background);
        }

        return chunk;
    }

    private GameObject[] GetPlayerGameObjects()
    {
        List<GameObject> playerList = new List<GameObject>();
        if (players == null) return playerList.ToArray();
        foreach (Transform t in players)
        {
            if (t != null)
                playerList.Add(t.gameObject);
        }
        return playerList.ToArray();
    }

    private void ShuffleList<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // (Saved data code unchanged…)

    // ------------------- Saved Data -------------------
    public void SpawnFromSavedData(List<ChunkRecord> savedChunks)
    {
        useSavedChunks = true;
        ClearAllChunks();

        savedChunks.Sort((a, b) => a.chunkIndex.CompareTo(b.chunkIndex));

        foreach (var record in savedChunks)
        {
            GameObject prefab = FindChunkPrefabByID(record.chunkID);
            if (prefab == null)
            {
                Debug.LogWarning($"Chunk prefab with ID {record.chunkID} not found!");
                continue;
            }

            GameObject chunk = Instantiate(prefab, new Vector3(record.posX, record.posY, 0), Quaternion.identity, transform);
            chunk.name = $"Chunk_{record.chunkIndex}";
            activeChunks.Add(new SpawnedChunk(chunk, record.width));

            if (record.chunkIndex == 0)
            {
                Transform spawn = chunk.transform.Find("SpawnPoint");
                if (spawn != null)
                    FirstSpawnPoint = spawn;
            }

            // Assign players to enemies
            EnemyShooter[] shooters = chunk.GetComponentsInChildren<EnemyShooter>();
            foreach (EnemyShooter shooter in shooters)
            {
                shooter.SetPlayers(GetPlayerGameObjects());
            }

            if (backgroundPrefab != null)
            {
                GameObject background = Instantiate(backgroundPrefab);
                background.transform.position = new Vector3(record.posX, backgroundYPosition, -1);
                activeBackgrounds.Add(background);
            }
        }
    }

    private GameObject FindChunkPrefabByID(string id)
    {
        foreach (var chunk in chunkDataList)
        {
            if (chunk.chunkPrefab.name == id)
                return chunk.chunkPrefab;
        }

        if (finalChunkPrefab != null && finalChunkPrefab.name == id)
            return finalChunkPrefab;

        return null;
    }

    private void ClearAllChunks()
    {
        foreach (var chunk in activeChunks)
        {
            if (chunk.chunkObject != null)
                Destroy(chunk.chunkObject);
        }
        activeChunks.Clear();

        foreach (var bg in activeBackgrounds)
        {
            if (bg != null)
                Destroy(bg);
        }
        activeBackgrounds.Clear();

        currentChunkIndex = 0;
        finalChunkSpawned = false;
    }
    [ClientRpc]
    private void SpawnChunkClientRpc(string prefabName, Vector3 position, int chunkIndex, float width, bool isFinalChunk)
    {
        GameObject prefab = FindChunkPrefabByID(prefabName);
        if (prefab == null) return;

        GameObject chunk = Instantiate(prefab, position, Quaternion.identity, transform);
        chunk.name = $"Chunk_{chunkIndex}";
        activeChunks.Add(new SpawnedChunk(chunk, width));

        // If first chunk, set spawn point
        if (chunkIndex == 0)
        {
            Transform spawn = chunk.transform.Find("SpawnPoint");
            if (spawn != null)
                FirstSpawnPoint = spawn;
        }

        // Assign players to enemies (local clients just need references)
        EnemyShooter[] shooters = chunk.GetComponentsInChildren<EnemyShooter>();
        foreach (EnemyShooter shooter in shooters)
            shooter.SetPlayers(GetPlayerGameObjects());

        // Optional: spawn background locally
        if (backgroundPrefab != null)
        {
            GameObject background = Instantiate(backgroundPrefab);
            background.transform.position = new Vector3(position.x, backgroundYPosition, -1);
            activeBackgrounds.Add(background);
        }

        // If this is the final chunk, mark it
        if (isFinalChunk) finalChunkSpawned = true;
    }

}