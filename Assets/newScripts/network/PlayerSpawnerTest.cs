using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Cinemachine;

public class PlayerSpawnerTest : NetworkBehaviour
{
    [Header("Prefabs (index matches CharacterSelectManager characterId)")]
    [SerializeField] private GameObject[] characters;

    [Header("Offline Prefabs (No Netcode Components)")]
    public GameObject shooterPrefabOffline;
    public GameObject meleePrefabOffline;

    [Header("Chunk Manager")]
    public ChunkGenerator chunkGenerator;

    [Header("Cinemachine")]
    public CinemachineCamera cinemachiCamera1;
    public CinemachineCamera cinemachiCamera2;
    public Camera assignedCamera;

    //[Header("Mode")]
    //public bool isOfflineMode = false; // Toggle in inspector for local play
    public static event Action<GameObject[]> OnPlayerUpdated;

    [Header("Spawn Settings")]
    public Vector3 basePosition = new Vector3(-17, 5, 0); // starting point
    public Vector3 spacing = new Vector3(2f, 0, 0);       // offset per player
    public Vector3[] customPositions;                      // optional full list


    private void Start()
    {
        bool isOfflineMode = (GameModeSelector.Instance != null && !GameModeSelector.Instance.IsOnline());

        Debug.Log($"[Spawner] Starting PlayerSpawnerTest. OfflineMode={isOfflineMode}");

        if (isOfflineMode)
        {
            SpawnOfflinePlayers();
        }
        else
        {
            if (!IsServer)
            {
                Debug.Log("[Spawner] Not the server. Online spawning disabled for this client.");
                return;
            }

            // Spawn immediately on level load
            SpawnAllSelectedPlayers();

            // Listen for any future client connections (optional)
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            Debug.Log("[Spawner] Server ready. Spawned players and listening for future connections.");
        }
    }

    // ------------------- Offline -------------------
    private void SpawnOfflinePlayers()
    {
        Debug.Log("[Spawner] Spawning offline players...");

        var spawnedPlayers = new List<GameObject>();

        // --- Spawn Shooter ---
        var shooterGO = Instantiate(shooterPrefabOffline, new Vector3(-17, 7, 0), Quaternion.identity);
        shooterGO.GetComponent<PlayerInput>()?.SwitchCurrentControlScheme("KeyboardLeft", Keyboard.current);
        spawnedPlayers.Add(shooterGO);
        Debug.Log($"[Spawner] Spawned offline Shooter: {shooterGO.name} at {shooterGO.transform.position}");

        // --- Spawn Melee ---
        var meleeGO = Instantiate(meleePrefabOffline, new Vector3(-17, 5, 0), Quaternion.identity);
        meleeGO.GetComponent<PlayerInput>()?.SwitchCurrentControlScheme("KeyboardRight", Keyboard.current);
        spawnedPlayers.Add(meleeGO);
        Debug.Log($"[Spawner] Spawned offline Melee: {meleeGO.name} at {meleeGO.transform.position}");

        // --- Update ChunkGenerator with player references ---
        if (chunkGenerator != null)
        {
            chunkGenerator.players = new Transform[] { shooterGO.transform, meleeGO.transform };

            // Re-initialize chunk generator for offline mode
            //chunkGenerator.ClearAllChunks(); // clean old chunks just in case
            chunkGenerator.Start();           // generates initial chunks and backgrounds
            Debug.Log("[Spawner] ChunkGenerator initialized for offline mode.");
        }

        // --- Notify listeners about spawned players ---
        OnPlayerUpdated?.Invoke(spawnedPlayers.ToArray());
    }

    // ------------------- Online -------------------
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"[Spawner] Client connected: {clientId}. Spawning all selected players...");
        SpawnAllSelectedPlayers();
    }

    public void SpawnAllSelectedPlayers()
    {
        if (!IsServer) return;
        if (CharacterSelectManager.Instance == null)
        {
            Debug.LogWarning("[Spawner] CharacterSelectManager.Instance is null! Cannot spawn players.");
            return;
        }
        DespawnAllPlayers();
        Debug.Log($"[Spawner] Spawning {CharacterSelectManager.Instance.players.Count} selected players...");

        List<GameObject> spawnedPlayers = new List<GameObject>();

        for (int i = 0; i < CharacterSelectManager.Instance.players.Count; i++)
        {
            var player = CharacterSelectManager.Instance.players[i];

            if (player.characterId < 0 || player.characterId >= characters.Length)
            {
                Debug.LogWarning($"[Spawner] Invalid characterId {player.characterId} for client {player.clientId}");
                continue;
            }

            Vector3 spawnPos = GetSpawnPositionForClient(i);
            var prefabToSpawn = characters[player.characterId];
            Debug.Log($"[Spawner] Instantiating prefab '{prefabToSpawn.name}' for client {player.clientId} at {spawnPos}");

            var playerInstance = Instantiate(prefabToSpawn, spawnPos, Quaternion.identity);
            var netObj = playerInstance.GetComponent<NetworkObject>();

            if (netObj != null)
            {
                netObj.SpawnAsPlayerObject(player.clientId);
                Debug.Log($"[Spawner] Spawned NetworkObject for client {player.clientId}: {playerInstance.name}");
            }
            else
            {
                Debug.LogWarning($"[Spawner] No NetworkObject found on prefab {prefabToSpawn.name}! It won't be networked.");
            }
            var movementScript = playerInstance.GetComponent<playerMovement>();
            if (movementScript != null)
            {
                movementScript.hobbitCamera = assignedCamera;
            }
            var movementScript2 = playerInstance.GetComponent<PlayerControllerNew>();
            if (movementScript2 != null)
            {
                movementScript2.hobbitCamera = assignedCamera;
            }
            var movementScript3 = playerInstance.GetComponent<newShooterPlayerMovement>();
            if (movementScript3 != null)
            {
                movementScript3.hobbitCamera = assignedCamera;
            }


            spawnedPlayers.Add(playerInstance);
        }

        // Update ChunkGenerator
        if (chunkGenerator != null)
            chunkGenerator.players = GetAllSpawnedPlayerTransforms();

        // Assign Boss + Shooter Device targets
        AssignBossAndShooterTargets(GetAllSpawnedPlayerTransforms());

        // Notify listeners
        OnPlayerUpdated?.Invoke(spawnedPlayers.ToArray());
        Debug.Log($"[Spawner] Finished spawning. Total spawned players: {spawnedPlayers.Count}");

        
    }
    private void DespawnAllPlayers()
    {
        if (!IsServer) return;

        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObj != null && playerObj.IsSpawned)
            {
                Debug.Log($"[Spawner] Despawning old player object for client {clientId}: {playerObj.name}");
                playerObj.Despawn(true); // true = destroy on all clients
            }
        }
    }
    public Vector3 GetSpawnPositionForClient(int index)
    {
        // If custom positions are set and index is valid, use them
        if (customPositions != null && index < customPositions.Length)
            return customPositions[index];

        // Otherwise, calculate using basePosition + spacing * index
        return basePosition + Vector3.Scale(spacing, new Vector3(index, index, index));
    }

    private Transform[] GetAllSpawnedPlayerTransforms()
    {
        var playersList = new List<Transform>();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            var playerObj = NetworkManager.Singleton.SpawnManager.GetPlayerNetworkObject(clientId);
            if (playerObj != null)
            {
                playersList.Add(playerObj.transform);
                Debug.Log($"[Spawner] Added player transform for client {clientId}: {playerObj.name}");
            }
        }
        return playersList.ToArray();
    }
    private void AssignBossAndShooterTargets(Transform[] playerTransforms)
    {
        // Look for BossEnemy
        var boss = GameObject.FindFirstObjectByType<BossEnemy>();


        if (boss != null)
        {
            if (playerTransforms.Length > 0) boss.player1 = playerTransforms[0];
            if (playerTransforms.Length > 1) boss.player2 = playerTransforms[1];
            Debug.Log("[Spawner] Assigned players to BossEnemy.");
        }
        else
        {
            Debug.LogWarning("[Spawner] No BossEnemy found in scene.");
        }

        // Look for BossShooterDevice
        var shooter = GameObject.FindFirstObjectByType<BossShooterDevice>();
        if (shooter != null)
        {
            if (playerTransforms.Length > 0) shooter.player1 = playerTransforms[0];
            if (playerTransforms.Length > 1) shooter.player2 = playerTransforms[1];
            Debug.Log("[Spawner] Assigned players to BossShooterDevice.");
        }
        else
        {
            Debug.LogWarning("[Spawner] No BossShooterDevice found in scene.");
        }
}

}
