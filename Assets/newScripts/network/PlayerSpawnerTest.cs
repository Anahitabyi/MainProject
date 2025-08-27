using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using Unity.Cinemachine;

public class PlayerSpawnerTest : NetworkBehaviour
{
    [Header("Online Prefabs (With Netcode)")]
    public NetworkObject shooterPrefabOnline;
    public NetworkObject meleePrefabOnline;

    [Header("Offline Prefabs (No Netcode Components)")]
    public GameObject shooterPrefabOffline;
    public GameObject meleePrefabOffline;

    [Header("Chunk Manager")]
    public ChunkGenerator chunkGenerator;
     [Header("Cinemachine")]
    public CinemachineCamera cinemachiCamera1;
    public CinemachineCamera cinemachiCamera2;

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
        var shooterGO = Instantiate(shooterPrefabOffline, new Vector3(-17, 7, 0), Quaternion.identity);
        // Spawn Melee at lower position
        var meleeGO = Instantiate(meleePrefabOffline, new Vector3(-17, 5, 0), Quaternion.identity);


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
        // Assign tracking targets for Cinemachine
        // if (cinemachiCamera1 != null)
        //     cinemachiCamera1.TrackingTarget = shooterGO.transform;

        // if (cinemachiCamera2 != null)
        //     cinemachiCamera2.TrackingTarget = meleeGO.transform;

        
    }

    // ------------------- Online -------------------
    private void SpawnOnlinePlayer(ulong clientId)
    {
        if (!IsServer) return;

        // Decide which prefab to spawn
        NetworkObject prefabToSpawn = (clientId == NetworkManager.Singleton.LocalClientId) 
            ? shooterPrefabOnline 
            : meleePrefabOnline;

        // Instantiate and spawn the player prefab
        var playerInstance = Instantiate(prefabToSpawn, new Vector3(-17, 5, 0), Quaternion.identity);
        playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

        // Update ChunkGenerator
        if (chunkGenerator != null)
            chunkGenerator.players = GetAllSpawnedPlayers();

        // Example: assign tracking target to cameras
        // if (clientId == NetworkManager.Singleton.LocalClientId && cinemachiCamera1 != null)
        //     cinemachiCamera1.TrackingTarget = playerInstance.transform;
        // else if (cinemachiCamera2 != null)
        //     cinemachiCamera2.TrackingTarget = playerInstance.transform;
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
