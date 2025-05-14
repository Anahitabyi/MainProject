using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Tilemaps;

public class ChunkGenerator : MonoBehaviour
{
    [SerializeField] private int chunkWidth = 10;

    [SerializeField] private float generateDistance = 25f;
    [SerializeField] private int maxChunks = 10;

    [SerializeField] private int initialChunks = 3;
    [SerializeField] private GameObject[] chunkPrefabs;

    [SerializeField] private Transform player;

    private List<GameObject> activeChunks = new List<GameObject>();
    private int currentChunk = 0;
    private float chunkWorldWidth;
    
    void Start()
    {
        if (chunkPrefabs.Length == 0)
        {
            Debug.LogError("Chunk Prefabs are empty");
        }
        chunkWorldWidth = chunkWidth;
        
        // generating initial chunks
        for (int i = 0; i < initialChunks; i++)
        {
            GameObject newChunk = GenerateChunk(i);
            activeChunks.Add(newChunk);
        }
    }
    
    void Update()
    {
        // check whether we need to generate more chunks
        float playerX = player.position.x;
        float furthestChunkEndX = (currentChunk - 1 + activeChunks.Count) * chunkWorldWidth;
        if (playerX + generateDistance > furthestChunkEndX)
        {
            GameObject newChunk = GenerateChunk(currentChunk - 1 + activeChunks.Count);
            activeChunks.Add(newChunk);
        }
        
        // remove chunks if far enough
        if (activeChunks.Count > maxChunks)
        {
            GameObject firstChunk = activeChunks[0];
            activeChunks.RemoveAt(0);
            Destroy(firstChunk);
            currentChunk++;
        }
    }

    GameObject GenerateChunk(int chunkIndex)
    {
        // randomly select one chunk from the prefabs.
        int randomChunk = Random.Range(0, chunkPrefabs.Length);
        GameObject selectedChunk = chunkPrefabs[randomChunk];
        
        // now instantiate the selected chunk
        GameObject chunk = Instantiate(selectedChunk, transform);
        chunk.name = "Chunk_" + chunkIndex + "_Type_" + randomChunk;
        
        // position the chunk in the index
        float positionX = chunkWorldWidth * chunkIndex;
        chunk.transform.position = new Vector3(positionX, 0, 0);
        return chunk;
    }
}
