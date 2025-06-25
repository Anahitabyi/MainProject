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

        // ✅ Save player stats
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

        // ✅ Save chunk and other game state data
        if (SaveTracker.Instance != null)
        {
            data.spawnedChunks = new List<ChunkRecord>(SaveTracker.Instance.spawnedChunks);
            data.collectedIDs = new List<string>(SaveTracker.Instance.collectedIDs);
            data.disabledPatrolPairs = new List<string>(SaveTracker.Instance.disabledPatrolPairs);
        }

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
        GameStateFlags.IsLoadingFromSave = true;

        // Wait one frame to allow scene to load cleanly
        yield return null;

        // Begin scene loading
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(data.currentSceneName);
        yield return new WaitUntil(() => asyncLoad.isDone);

        // Wait for playerStatsManager to exist
        yield return new WaitUntil(() => playerStatsManager.Instance != null);

        // ✅ Restore player stats
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

        // ✅ Restore SaveTracker data
        yield return new WaitUntil(() => SaveTracker.Instance != null);

        SaveTracker.Instance.collectedIDs = new HashSet<string>(data.collectedIDs);
        SaveTracker.Instance.disabledPatrolPairs = new HashSet<string>(data.disabledPatrolPairs);

        // ✅ Restore chunk data
        ChunkGenerator chunkGen = FindAnyObjectByType<ChunkGenerator>();
        if (chunkGen != null && data.spawnedChunks != null)
        {
            chunkGen.SpawnFromSavedData(data.spawnedChunks);
        }

        GameStateFlags.IsLoadingFromSave = false;
    }
}
