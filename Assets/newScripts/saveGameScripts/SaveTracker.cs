using System.Collections.Generic;
using UnityEngine;
public static class GameStateFlags
    {
        public static bool IsLoadingFromSave = false;
    }
public class SaveTracker : MonoBehaviour
{
    public static SaveTracker Instance { get; private set; }

    public HashSet<string> collectedIDs = new();
    public HashSet<string> defeatedEnemyIDs = new();

    public List<ChunkRecord> spawnedChunks = new(); // ✅ Track spawned chunks

    public HashSet<string> disabledPatrolPairs = new();




    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    // ========== COLLECTIBLES ==========
    public void MarkCollected(string id)
    {
        collectedIDs.Add(id);
    }

    public bool IsCollected(string id) => collectedIDs.Contains(id);

    // ========== ENEMIES ==========
    public void MarkEnemyDefeated(string id)
    {
        defeatedEnemyIDs.Add(id);
    }

    public bool IsEnemyDefeated(string id) => defeatedEnemyIDs.Contains(id);

    // ========== CHUNKS ==========

    public void RecordChunk(string chunkID, int index, Vector2 position, float width)
    {
        // Prevent duplicates if needed (optional)
        if (spawnedChunks.Exists(c => c.chunkIndex == index)) return;

        ChunkRecord record = new ChunkRecord
        {
            chunkID = chunkID,
            chunkIndex = index,
            posX = position.x,
            posY = position.y,
            width = width
        };

        spawnedChunks.Add(record);
    }

    public void ClearChunks()
    {
        spawnedChunks.Clear();
    }
    public void MarkPatrolPairDisabled(string id)
    {
        disabledPatrolPairs.Add(id);
    }

    public bool IsPatrolPairDisabled(string id)
    {
        return disabledPatrolPairs.Contains(id);
    }
        public void ClearAll()
    {
        collectedIDs.Clear();
        defeatedEnemyIDs.Clear();
        disabledPatrolPairs.Clear(); // If you added patrol pair disabling
        spawnedChunks.Clear(); // If you track chunks
        Debug.Log("[SaveTracker] Cleared all saved state.");
    }


}
