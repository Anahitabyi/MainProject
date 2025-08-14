using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerSpawnerTest : NetworkBehaviour
{
    [Header("Player Prefabs")]
    public NetworkObject shooterPrefab;
    public NetworkObject meleePrefab;

    [Header("Chunk Manager")]
    public ChunkGenerator chunkGenerator;

    [Header("Mode")]
    public bool isOfflineMode = false; // Toggle in inspector for local play

    private void Start()
    {
        if (isOfflineMode)
        {
            SpawnOfflinePlayers();
        }
        else
        {
            // Online: listen for connection events
            NetworkManager.Singleton.OnClientConnectedCallback += SpawnOnlinePlayer;
        }
    }

    // ------------------- Offline -------------------
    private void SpawnOfflinePlayers()
    {
        // Spawn Shooter at top position
        var shooterGO = Instantiate(shooterPrefab.gameObject, new Vector3(-17, 7, 0), Quaternion.identity);
        // Spawn Melee at lower position
        var meleeGO = Instantiate(meleePrefab.gameObject, new Vector3(-17, 5, 0), Quaternion.identity);

        // Assign offline keyboard schemes
        var shooterInput = shooterGO.GetComponent<PlayerInput>();
        if (shooterInput != null)
            shooterInput.SwitchCurrentControlScheme("KeyboardLeft", Keyboard.current);

        var meleeInput = meleeGO.GetComponent<PlayerInput>();
        if (meleeInput != null)
            meleeInput.SwitchCurrentControlScheme("KeyboardRight", Keyboard.current);

        // Assign players to ChunkGenerator
        if (chunkGenerator != null)
        {
            chunkGenerator.players = new Transform[] { shooterGO.transform, meleeGO.transform };
        }
    }

    // ------------------- Online -------------------
    private void SpawnOnlinePlayer(ulong clientId)
    {
        // Only the server spawns players
        if (!IsServer) return;

        NetworkObject prefabToSpawn;

        // Hard-coded: host gets shooter, client gets melee
        if (clientId == NetworkManager.Singleton.LocalClientId)
        {
            prefabToSpawn = shooterPrefab;
        }
        else
        {
            prefabToSpawn = meleePrefab;
        }

        // Spawn player
        var spawnPos = new Vector3(-17, 5, 0);
        var playerInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
        playerInstance.SpawnAsPlayerObject(clientId);

        // Assign keyboard schemes for local players only
        var playerInput = playerInstance.GetComponent<PlayerInput>();
        if (playerInput != null && clientId == NetworkManager.Singleton.LocalClientId)
        {
            if (NetworkManager.Singleton.IsHost)
                playerInput.SwitchCurrentControlScheme("KeyboardLeft", Keyboard.current);
            else
                playerInput.SwitchCurrentControlScheme("KeyboardRight", Keyboard.current);
        }

        // Update ChunkGenerator with currently spawned players
        if (chunkGenerator != null)
        {
            chunkGenerator.players = GetAllSpawnedPlayers();
        }
    }

    // ------------------- Helper -------------------
    private Transform[] GetAllSpawnedPlayers()
    {
        var playersList = new List<Transform>();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObj != null)
                playersList.Add(playerObj.transform);
        }
        return playersList.ToArray();
    }
}
