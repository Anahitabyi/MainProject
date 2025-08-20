using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class playerStatsManager : NetworkBehaviour
{
    public static playerStatsManager Instance { get; private set; }

    [System.Serializable]
    public class PlayerStats
    {
        public NetworkVariable<int> currentHealth = new NetworkVariable<int>(3);
        public NetworkVariable<int> currentLives = new NetworkVariable<int>(3);
        public NetworkVariable<int> maxHealth = new NetworkVariable<int>(3);
        public NetworkVariable<int> maxLives = new NetworkVariable<int>(3);

        // Add a reset method
        public void Reset()
        {
            currentHealth.Value = 3;
            currentLives.Value = 3;
            maxHealth.Value = 3;
            maxLives.Value = 3;
        }
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
        if (IsServer)
        {
            stats.currentHealth.Value = player.currentHealth.Value;
            stats.currentLives.Value = player.currentLives.Value;
            stats.maxHealth.Value = player.maxHealth;
            stats.maxLives.Value = player.maxLives;
        }
        else
        {
            SaveFromPlayerServerRpc(player.playerId, player.currentHealth.Value, player.currentLives.Value, player.maxHealth, player.maxLives);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SaveFromPlayerServerRpc(int playerId, int health, int lives, int maxHealth, int maxLives)
    {
        PlayerStats stats = GetStats(playerId);
        stats.currentHealth.Value = health;
        stats.currentLives.Value = lives;
        stats.maxHealth.Value = maxHealth;
        stats.maxLives.Value = maxLives;
    }

    public void LoadIntoPlayer(PlayerHealth player)
    {
        PlayerStats stats = GetStats(player.playerId);
        player.currentHealth.Value = stats.currentHealth.Value;
        player.maxHealth = stats.maxHealth.Value;
        player.currentLives.Value = stats.currentLives.Value;
        player.maxLives = stats.maxLives.Value;
    }

    private PlayerStats GetStats(int id)
    {
        return id == 1 ? player1Stats : player2Stats;
    }

    public void ResetAllStats()
    {
        if (IsServer)
        {
            player1Stats.Reset();
            player2Stats.Reset();
        }
        else
        {
            ResetAllStatsServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void ResetAllStatsServerRpc()
    {
        player1Stats.Reset();
        player2Stats.Reset();
    }

    [System.Serializable]
    public class PlayerPositionsData
    {
        public float player1PosX, player1PosY;
        public float player2PosX, player2PosY;
    }
}