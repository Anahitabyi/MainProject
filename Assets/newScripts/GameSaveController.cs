using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSaveController : MonoBehaviour
{
    public static GameSaveController Instance { get; private set; }

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

    public void SaveToFile()
    {
        GameData data = new GameData();

        // Save player stats
        var p1 = playerStatsManager.Instance.player1Stats;
        var p2 = playerStatsManager.Instance.player2Stats;

        data.player1Stats = new PlayerStatsData
        {
            currentHealth = p1.currentHealth,
            currentLives = p1.currentLives,
            maxHealth = p1.maxHealth,
            maxLives = p1.maxLives
        };

        data.player2Stats = new PlayerStatsData
        {
            currentHealth = p2.currentHealth,
            currentLives = p2.currentLives,
            maxHealth = p2.maxHealth,
            maxLives = p2.maxLives
        };

        data.currentSceneName = SceneManager.GetActiveScene().name;

        // Save spawned chunks
        ChunkGenerator chunkGen = FindAnyObjectByType<ChunkGenerator>();
        if (chunkGen != null)
        {
            data.spawnedChunks = new List<ChunkRecord>();
            int index = 0;

            foreach (Transform chunk in chunkGen.transform)
            {
                string chunkID = chunk.name.Split('(')[0]; // Or use UniqueID if preferred
                data.spawnedChunks.Add(new ChunkRecord
                {
                    chunkID = chunkID,
                    chunkIndex = index++,
                    posX = chunk.position.x,
                    posY = chunk.position.y
                });
            }
        }

        // Save collected collectibles and defeated enemies
        if (SaveTracker.Instance != null)
        {
            data.collectedIDs = new List<string>(SaveTracker.Instance.collectedIDs);
            //data.defeatedEnemyIDs = new List<string>(SaveTracker.Instance.killedEnemyIDs);
        }

        // Save game to file
        SaveSystem.SaveGame(data);
    }

    public void LoadFromFile()
    {
        GameData data = SaveSystem.LoadGame();
        if (data == null) return;

        StartCoroutine(LoadSceneAfterFrame(data));
    }

    private IEnumerator LoadSceneAfterFrame(GameData data)
    {
        yield return null;
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.currentSceneName);
        yield return new WaitUntil(() => asyncLoad.isDone);

        // Restore player stats
        var p1 = playerStatsManager.Instance.player1Stats;
        var p2 = playerStatsManager.Instance.player2Stats;

        p1.currentHealth = data.player1Stats.currentHealth;
        p1.currentLives = data.player1Stats.currentLives;
        p1.maxHealth = data.player1Stats.maxHealth;
        p1.maxLives = data.player1Stats.maxLives;

        p2.currentHealth = data.player2Stats.currentHealth;
        p2.currentLives = data.player2Stats.currentLives;
        p2.maxHealth = data.player2Stats.maxHealth;
        p2.maxLives = data.player2Stats.maxLives;

        // Restore chunks
        ChunkGenerator chunkGen = FindAnyObjectByType<ChunkGenerator>();
        if (chunkGen != null && data.spawnedChunks != null)
        {
            chunkGen.SpawnFromSavedData(data.spawnedChunks);
        }

        // Restore collectibles and enemies
        if (SaveTracker.Instance != null)
        {
            SaveTracker.Instance.collectedIDs = new HashSet<string>(data.collectedIDs);
            //SaveTracker.Instance.killedEnemyIDs = new HashSet<string>(data.defeatedEnemyIDs);
        }
    }
}
