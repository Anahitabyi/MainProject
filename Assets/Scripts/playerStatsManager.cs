using UnityEngine;
using System.Collections.Generic;
using Unity.Netcode;

public class playerStatsManager : NetworkBehaviour
{
    public static playerStatsManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerStats
    {
        public int currentHealth = 3;
        public int currentLives = 3;
        public int maxHealth = 3;
        public int maxLives = 3;
    }

    public PlayerStats player1Stats = new PlayerStats();
    public PlayerStats player2Stats = new PlayerStats();

    public Dictionary<string, PlayerPositionsData> scenePlayerPositions = new();

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
        PlayerStats stats = GetStats(player.playerId);
        stats.currentHealth = player.currentHealth.Value;
        stats.currentLives = player.currentLives.Value;
        stats.maxHealth = player.maxHealth;
        stats.maxLives = player.maxLives;
    }

    public void LoadIntoPlayer(PlayerHealth player)
    {
        PlayerStats stats = GetStats(player.playerId);
        player.currentHealth.Value = stats.currentHealth;
        player.maxHealth = stats.maxHealth;
        player.currentLives.Value = stats.currentLives;
        player.maxLives = stats.maxLives;
    }

    private PlayerStats GetStats(int id)
    {
        return id == 1 ? player1Stats : player2Stats;
    }

    public void ResetAllStats()
    {
        player1Stats = new PlayerStats();
        player2Stats = new PlayerStats();
    }

    [System.Serializable]
    public class PlayerPositionsData
    {
        public float player1PosX, player1PosY;
        public float player2PosX, player2PosY;
    }
}