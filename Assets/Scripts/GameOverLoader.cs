using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOverLoader : MonoBehaviour
{
    public string gameOverSceneName = "GameOver";

    private static GameOverLoader instance;
    private bool isGameOver = false;

    void Awake()
    {
        // Singleton pattern to persist across scenes
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void TriggerGameOver()
    {
        if (isGameOver) return;

        isGameOver = true;
        Debug.Log("Game Over triggered.");
        SceneManager.LoadScene(gameOverSceneName);
    }

    public static void GameOver()
    {
        if (instance != null)
        {
            instance.TriggerGameOver();
        }
        else
        {
            Debug.LogError("GameOverManager instance is missing!");
        }
    }
}
