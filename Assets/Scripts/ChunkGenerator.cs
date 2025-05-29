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
        float minPlayerX = float.MaxValue;

        foreach (Transform player in players)
        {
            if (player != null)
            {
                float x = player.position.x;
                if (x > maxPlayerX) maxPlayerX = x;
                if (x < minPlayerX) minPlayerX = x;
            }
        }

        GameObject lastChunk = activeChunks[activeChunks.Count - 1];
        float lastChunkEndX = lastChunk.transform.position.x + GetChunkWidth(lastChunk);

        // Generate next chunk if needed
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

        // Background
        if (backgroundPrefab != null)
        {
            GameObject background = Instantiate(backgroundPrefab);
            background.transform.position = new Vector3(positionX, positionY, -1);
            activeBackgrounds.Add(background);
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
}
