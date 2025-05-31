using UnityEngine;
using System.Collections.Generic;

public class ChunkGenerator : MonoBehaviour
{
    [SerializeField] private float generateDistance = 25f;
    [SerializeField] private int initialChunks = 3;
    [SerializeField] private GameObject[] chunkPrefabs;
    [SerializeField] private GameObject finalChunkPrefab;
    [SerializeField] private GameObject backgroundPrefab;
    [SerializeField] private Transform[] players;
    [SerializeField] private GameObject[] collectiblePrefabs; // Assign your Food, Coin, Powerup prefabs

    public float positionY = 0;

    private List<GameObject> unusedChunkPrefabs = new List<GameObject>();
    private List<GameObject> activeChunks = new List<GameObject>();
    private List<GameObject> activeBackgrounds = new List<GameObject>();
    private int currentChunkIndex = 0;
    private bool finalChunkSpawned = false;

    void Start()
    {
        unusedChunkPrefabs = new List<GameObject>(chunkPrefabs);

        for (int i = 0; i < initialChunks && unusedChunkPrefabs.Count > 0; i++)
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
        float lastChunkEndX = lastChunk.transform.position.x + GetChunkWidth(lastChunk);

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
            float lastChunkEndX = lastChunk.transform.position.x + GetChunkWidth(lastChunk);
            positionX = lastChunkEndX;
        }

        GameObject chunkToSpawn = null;

        if (unusedChunkPrefabs.Count > 0)
        {
            int randomIndex = Random.Range(0, unusedChunkPrefabs.Count);
            chunkToSpawn = unusedChunkPrefabs[randomIndex];
            unusedChunkPrefabs.RemoveAt(randomIndex);
        }
        else if (!finalChunkSpawned && finalChunkPrefab != null)
        {
            chunkToSpawn = finalChunkPrefab;
            finalChunkSpawned = true;
        }

        if (chunkToSpawn == null)
            return null;

        GameObject chunk = Instantiate(chunkToSpawn, transform);
        chunk.name = "Chunk_" + currentChunkIndex;
        chunk.transform.position = new Vector3(positionX, 0, 0);
        currentChunkIndex++;

        // Set players to any enemy shooters
        EnemyShooter[] shooters = chunk.GetComponentsInChildren<EnemyShooter>();
        foreach (EnemyShooter shooter in shooters)
        {
            shooter.SetPlayers(GetPlayerGameObjects());
        }

        // Background
        if (backgroundPrefab != null)
        {
            GameObject background = Instantiate(backgroundPrefab);
            background.transform.position = new Vector3(positionX, positionY, -1);
            activeBackgrounds.Add(background);
        }
        // === Spawn a collectible at a random spawn point in the chunk ===
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

    float GetChunkWidth(GameObject chunk)
    {
        Renderer[] renderers = chunk.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) return 0;

        Bounds bounds = renderers[0].bounds;
        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        return bounds.size.x;
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
}
