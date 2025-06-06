using UnityEngine;

public class playerStatsManager : MonoBehaviour
{
    public static playerStatsManager Instance { get; private set; }

    public int currentHealth = 3;
    public int currentLives = 3;
    public int maxHealth = 3;
    public int maxLives = 3;

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

    public void SaveFromPlayer(PlayerHealth player)
    {
        currentHealth = player.currentHealth;
        currentLives = player.currentLives;
    }

    public void LoadIntoPlayer(PlayerHealth player)
    {
        player.currentHealth = currentHealth;
        player.maxHealth = maxHealth;
        player.currentLives = currentLives;
        player.maxLives = maxLives;
    }

    public void ResetStats()
    {
        currentHealth = maxHealth;
        currentLives = maxLives;
    }
}
