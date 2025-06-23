using System.Collections.Generic;

[System.Serializable]
public class GameData
{
    public PlayerStatsData player1Stats;
    public PlayerStatsData player2Stats;

    public string currentSceneName;

    // Level 1: Chunks, Collectibles, Enemies
    public List<ChunkRecord> spawnedChunks = new List<ChunkRecord>();
    public List<string> collectedIDs = new List<string>();
    public List<string> defeatedEnemyIDs = new List<string>();

    // Level 2: Puzzle states
    public List<string> solvedPuzzleIDs = new List<string>();

    // Level 3: Boss state
    public bool bossFightStarted;
    public bool bossDefeated;
}

[System.Serializable]
public class PlayerStatsData
{
    public int currentHealth;
    public int currentLives;
    public int maxHealth;
    public int maxLives;
}

[System.Serializable]
public class ChunkRecord
{
    public string chunkID;      // a unique name or hash
    public int chunkIndex;      // position in sequence
    public float posX;          // if dynamically placed
    public float posY;
}