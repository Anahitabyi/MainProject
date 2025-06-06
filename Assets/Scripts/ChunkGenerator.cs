using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChunkData
{
    public GameObject chunkPrefab;
    public float yPosition;
}

public class ChunkGenerator : MonoBehaviour
{
    [SerializeField] private float generateDistance = 25f;
    [SerializeField] private int initialChunks = 3;
    [SerializeField] private List<ChunkData> chunkDataList = new List<ChunkData>();

    [SerializeField] private GameObject finalChunkPrefab;
    [SerializeField] private float finalChunkYPosition = 0f;

    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private float backgroundYPosition = 0f;

    [SerializeField] private Transform[] players;

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

    void Start()
    {
        unusedChunks = new List<ChunkData>(chunkDataList);
        ShuffleList(unusedChunks);

        for (int i = 0; i < initialChunks && unusedChunks.Count > 0; i++)
        {
            GenerateChunk();
        }
    }

    void Update()
    {
        if (players == null || players.Length == 0 || finalChunkSpawned) return;

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
            chunkWidth = GetPrefabWidth(data.chunkPrefab);
            unusedChunks.RemoveAt(0);
        }
        else if (!finalChunkSpawned && finalChunkPrefab != null)
        {
            chunkToSpawn = finalChunkPrefab;
            chunkY = finalChunkYPosition;
            chunkWidth = GetPrefabWidth(finalChunkPrefab);
            finalChunkSpawned = true;
        }

        if (chunkToSpawn == null)
            return null;

        GameObject chunk = Instantiate(chunkToSpawn, new Vector3(positionX, chunkY, 0), Quaternion.identity, transform);
        chunk.name = "Chunk_" + currentChunkIndex;
        currentChunkIndex++;

        activeChunks.Add(new SpawnedChunk(chunk, chunkWidth));

        // Set players to enemy shooters
        EnemyShooter[] shooters = chunk.GetComponentsInChildren<EnemyShooter>();
        foreach (EnemyShooter shooter in shooters)
        {
            shooter.SetPlayers(GetPlayerGameObjects());
        }

        // Background
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

    private float GetPrefabWidth(GameObject prefab)
    {
        Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 1f;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds.size.x;
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
}
