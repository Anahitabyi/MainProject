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
    public HashSet<string> collectedLevel2Keys = new();
    public bool level3CutscenePlayed = false;
    public bool cameraSizeChanged = false;




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
    public Dictionary<string, EnemyRecord> enemyStates = new();

public void RecordEnemyState(string id, int hp, bool dead)
{
    if (enemyStates.TryGetValue(id, out var existing))
    {
        // ✅ If enemy is already marked dead, don’t overwrite with "alive"
        if (existing.isDead) return;
    }

    enemyStates[id] = new EnemyRecord
    {
        uniqueID = id,
        currentHealth = hp,
        isDead = dead
    };

    Debug.Log($"[SaveTracker] Saved state for {id}: HP={hp}, Dead={dead}");
}


public bool TryGetEnemyState(string id, out EnemyRecord record)
{
    return enemyStates.TryGetValue(id, out record);
}

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
        enemyStates.Clear();
        Debug.Log("[SaveTracker] Cleared all saved state.");
    }

    public void MarkLevel2KeyCollected(string keyID)
    {
        collectedLevel2Keys.Add(keyID);
    }

    public bool IsLevel2KeyCollected(string keyID)
    {
        return collectedLevel2Keys.Contains(keyID);
    }

    public void SetLevel2Keys(IEnumerable<string> keys)
    {
        collectedLevel2Keys = new HashSet<string>(keys);
    }

    public List<string> GetLevel2Keys()
    {
        return new List<string>(collectedLevel2Keys);
    }
}

