using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class ChunkData
{
    public GameObject chunkPrefab;
    public float yPosition;   // Custom Y for this chunk
    public float width;       // Custom width for X spacing
}

public class ChunkGenerator : MonoBehaviour
{
    [SerializeField] private float generateDistance = 25f;
    [SerializeField] private int initialChunks = 3;
    [SerializeField] private List<ChunkData> chunkDataList = new List<ChunkData>();

    [SerializeField] private GameObject finalChunkPrefab;
    [SerializeField] private float finalChunkYPosition = 0f;
    [SerializeField] private float finalChunkWidth = 20f;

    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private float backgroundYPosition = 0f;

    [SerializeField] private Transform[] players;
    [SerializeField] private GameObject[] collectiblePrefabs;

    private List<ChunkData> unusedChunks = new List<ChunkData>();
    private List<GameObject> activeChunks = new List<GameObject>();
    private List<GameObject> activeBackgrounds = new List<GameObject>();

    private int currentChunkIndex = 0;
    private bool finalChunkSpawned = false;

    void Start()
    {
        unusedChunks = new List<ChunkData>(chunkDataList);
        ShuffleChunks(unusedChunks);

        for (int i = 0; i < initialChunks && unusedChunks.Count > 0; i++)
        {
            GameObject newChunk = GenerateChunk();
            if (newChunk != null)
                activeChunks.Add(newChunk);
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

        GameObject lastChunk = activeChunks[activeChunks.Count - 1];
        float lastChunkX = lastChunk.transform.position.x;

        float lastChunkWidth = 0f;
        if (currentChunkIndex - 1 < chunkDataList.Count)
            lastChunkWidth = chunkDataList[currentChunkIndex - 1].width;
        else if (finalChunkSpawned)
            lastChunkWidth = finalChunkWidth;

        float lastChunkEndX = lastChunkX + lastChunkWidth;

        if (maxPlayerX + generateDistance > lastChunkEndX)
        {
            GameObject newChunk = GenerateChunk();
            if (newChunk != null)
                activeChunks.Add(newChunk);
        }
    }

    GameObject GenerateChunk()
    {
        float positionX = 0f;

        if (activeChunks.Count > 0)
        {
            GameObject lastChunk = activeChunks[activeChunks.Count - 1];
            float lastChunkX = lastChunk.transform.position.x;

            float lastChunkWidth = 0f;
            if (currentChunkIndex - 1 < chunkDataList.Count)
                lastChunkWidth = chunkDataList[currentChunkIndex - 1].width;
            else if (finalChunkSpawned)
                lastChunkWidth = finalChunkWidth;

            positionX = lastChunkX + lastChunkWidth;
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

        if (chunkToSpawn == null)
            return null;

        GameObject chunk = Instantiate(chunkToSpawn, new Vector3(positionX, chunkY, 0), Quaternion.identity, transform);
        chunk.name = "Chunk_" + currentChunkIndex;
        currentChunkIndex++;

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

        // Collectibles
        Transform spawnPointsParent = chunk.transform.Find("SpawnPoints");
        if (spawnPointsParent != null && collectiblePrefabs.Length > 0)
        {
            int spawnCount = spawnPointsParent.childCount;
            if (spawnCount > 0)
            {
                Transform randomSpawnPoint = spawnPointsParent.GetChild(Random.Range(0, spawnCount));
                GameObject collectibleToSpawn = collectiblePrefabs[Random.Range(0, collectiblePrefabs.Length)];
                Instantiate(collectibleToSpawn, randomSpawnPoint.position, Quaternion.identity, chunk.transform);
            }
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

    private void ShuffleChunks(List<ChunkData> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            ChunkData temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }
}
