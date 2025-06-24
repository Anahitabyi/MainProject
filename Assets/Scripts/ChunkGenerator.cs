using UnityEngine;
using System.Collections.Generic;

public class ChunkGenerator : MonoBehaviour
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

    [SerializeField] private Transform[] players;

    public Transform FirstSpawnPoint { get; private set; }

    private List<ChunkData> unusedChunks = new List<ChunkData>();

    private class SpawnedChunk
    {
        public GameObject chunkObject;
        public float width;

        public SpawnedChunk(GameObject obj, float width)
        {
            this.chunkObject = obj;
            this.width = width;
        }
    }

    private List<SpawnedChunk> activeChunks = new List<SpawnedChunk>();
    private List<GameObject> activeBackgrounds = new List<GameObject>();

    private int currentChunkIndex = 0;
    private bool finalChunkSpawned = false;

    private bool useSavedChunks = false; // Prevent auto-gen after load

    void Start()
    {
        if (useSavedChunks)
        {
            ClearAllChunks();
            return;
        }

        unusedChunks = new List<ChunkData>(chunkDataList);
        ShuffleList(unusedChunks);

        for (int i = 0; i < initialChunks && unusedChunks.Count > 0; i++)
        {
            GenerateChunk();
        }
    }

    void Update()
    {
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

        if (chunk.CompareTag("Chunk"))
            {
                SaveTracker.Instance.RecordChunk(
                    chunkToSpawn.name,
                    currentChunkIndex - 1,
                    new Vector2(positionX, chunkY),
                    chunkWidth
                );
            }

        EnemyShooter[] shooters = chunk.GetComponentsInChildren<EnemyShooter>();
        foreach (EnemyShooter shooter in shooters)
        {
            shooter.SetPlayers(GetPlayerGameObjects());
        }

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

    public void SpawnFromSavedData(List<ChunkRecord> savedChunks)
    {
        useSavedChunks = true;

        ClearAllChunks(); // Remove any auto-generated chunks before loading saved ones

        savedChunks.Sort((a, b) => a.chunkIndex.CompareTo(b.chunkIndex)); // maintain order

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

        finalChunkSpawned = true;
    }

    private void ClearAllChunks()
    {
        // Destroy all active chunks
        foreach (var chunk in activeChunks)
        {
            if (chunk.chunkObject != null)
                Destroy(chunk.chunkObject);
        }
        activeChunks.Clear();

        // Destroy all active backgrounds
        foreach (var bg in activeBackgrounds)
        {
            if (bg != null)
                Destroy(bg);
        }
        activeBackgrounds.Clear();

        currentChunkIndex = 0;
        finalChunkSpawned = false;
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
}
