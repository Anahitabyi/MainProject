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

        SaveSystem.SaveGame(data);
    }

    public void LoadFromFile()
    {
        GameData data = SaveSystem.LoadGame();
        if (data == null) return;

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

        StartCoroutine(LoadSceneAfterFrame(data.currentSceneName));
    }

    private System.Collections.IEnumerator LoadSceneAfterFrame(string sceneName)
    {
        yield return null; // wait one frame so everything loads clean
        SceneManager.LoadScene(sceneName);
    }
}