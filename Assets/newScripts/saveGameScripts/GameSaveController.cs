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

        GameObject player1GO = null, player2GO = null;
        var identifiers = Object.FindObjectsByType<PlayerIdentifier>(FindObjectsSortMode.None);
        foreach (var id in identifiers)
        {
            if (id.playerType == PlayerIdentifier.PlayerType.Hobbit)
                player1GO = id.gameObject;
            else if (id.playerType == PlayerIdentifier.PlayerType.Hooded)
                player2GO = id.gameObject;
        }


        Vector2 p1Pos = player1GO != null ? (Vector2)player1GO.transform.position : Vector2.zero;
        Vector2 p2Pos = player2GO != null ? (Vector2)player2GO.transform.position : Vector2.zero;

        data.player1Stats = new PlayerStatsData
        {
            currentHealth = p1.currentHealth,
            currentLives = p1.currentLives,
            maxHealth = p1.maxHealth,
            maxLives = p1.maxLives,
            posX = p1Pos.x,
            posY = p1Pos.y
        };

        data.player2Stats = new PlayerStatsData
        {
            currentHealth = p2.currentHealth,
            currentLives = p2.currentLives,
            maxHealth = p2.maxHealth,
            maxLives = p2.maxLives,
            posX = p2Pos.x,
            posY = p2Pos.y
        };

        data.currentSceneName = SceneManager.GetActiveScene().name;
        // ✅ Save chunk and other game state data
        if (SaveTracker.Instance != null)
        {
            Debug.Log("📦 Saving chunks in order:");
        for (int i = 0; i < SaveTracker.Instance.spawnedChunks.Count; i++)
        {
            var c = SaveTracker.Instance.spawnedChunks[i];
            Debug.Log($"Chunk #{i}: ID = {c.chunkID}, Index = {c.chunkIndex}, Pos = ({c.posX}, {c.posY})");
        }
            data.spawnedChunks = new List<ChunkRecord>(SaveTracker.Instance.spawnedChunks);
            data.collectedIDs = new List<string>(SaveTracker.Instance.collectedIDs);
            data.disabledPatrolPairs = new List<string>(SaveTracker.Instance.disabledPatrolPairs);
            data.savedEnemies = new List<EnemyRecord>(SaveTracker.Instance.enemyStates.Values);
            data.collectedLevel2Keys = new List<string>(SaveTracker.Instance.collectedLevel2Keys);
            data.level3CutscenePlayed = SaveTracker.Instance.level3CutscenePlayed;
            data.cameraSizeChanged = SaveTracker.Instance.cameraSizeChanged;


        }
        BossEnemy boss = FindFirstObjectByType<BossEnemy>();
        if (boss != null)
        {
            data.bossHealth = boss.GetCurrentHealth();
        }
        else
        {
            data.bossHealth = -1f;
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

        yield return null;
        // Wait for playerStatsManager to exist
        yield return new WaitUntil(() => playerStatsManager.Instance != null);
        yield return new WaitUntil(() => {
        var ids = Object.FindObjectsByType<PlayerIdentifier>(FindObjectsSortMode.None);
        return ids != null && ids.Length >= 2;  // assuming 2 players
    });

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
        SaveTracker.Instance.level3CutscenePlayed = data.level3CutscenePlayed;
        SaveTracker.Instance.cameraSizeChanged = data.cameraSizeChanged;


        GameObject player1GO = null, player2GO = null;
        var identifiers = Object.FindObjectsByType<PlayerIdentifier>(FindObjectsSortMode.None);
        foreach (var id in identifiers)
        {
            if (id.playerType == PlayerIdentifier.PlayerType.Hobbit)
                player1GO = id.gameObject;
            else if (id.playerType == PlayerIdentifier.PlayerType.Hooded)
                player2GO = id.gameObject;
        }


        if (player1GO != null)
            player1GO.transform.position = new Vector2(data.player1Stats.posX, data.player1Stats.posY);
        else{ Debug.Log("no player found!"); }
        if (player2GO != null)
            player2GO.transform.position = new Vector2(data.player2Stats.posX, data.player2Stats.posY);

        // ✅ Restore chunk data
        ChunkGenerator chunkGen = FindAnyObjectByType<ChunkGenerator>();
        if (chunkGen != null && data.spawnedChunks != null)
        {
             chunkGen.SpawnFromSavedData(data.spawnedChunks);

            // ✅ Restore the original order into SaveTracker
            SaveTracker.Instance.spawnedChunks = new List<ChunkRecord>(data.spawnedChunks);
        }
        foreach (var enemy in Object.FindObjectsByType<EnemyHealth>(FindObjectsSortMode.None))

        {
            GenerateID uid = enemy.GetComponent<GenerateID>();
            if (uid != null && SaveTracker.Instance.TryGetEnemyState(uid.Id, out var saved))
            {
                enemy.SetHealth(saved.currentHealth);
                if (saved.isDead){
                    Debug.Log("killed the extra enemy!");
                    Debug.Log($"Checking enemy {uid.Id} — saved: {saved.isDead}, HP: {saved.currentHealth}");

                    enemy.KillImmediately();} // You’ll implement this
            }
        }
        // Restore Boss Health (if boss exists and data is valid)
        BossEnemy boss = FindFirstObjectByType<BossEnemy>();
        if (boss != null && data.bossHealth > 0)
        {
            boss.SetCurrentHealth(data.bossHealth);
        }


        GameStateFlags.IsLoadingFromSave = false;
    }
}