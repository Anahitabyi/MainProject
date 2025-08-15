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
    if (!IsServer) return;

    // Decide which prefab to spawn
    NetworkObject prefabToSpawn = (clientId == NetworkManager.Singleton.LocalClientId) ? shooterPrefab : meleePrefab;

    // Instantiate and spawn the player prefab
    var playerInstance = Instantiate(prefabToSpawn, new Vector3(-17, 5, 0), Quaternion.identity);
    playerInstance.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);

    // Get all currently spawned players
    // Transform[] allPlayers = GetAllSpawnedPlayers();

    // NetworkObject ownerNetObj = null;
    // PlayerInput ownerInput = null;

    // // First loop: disable all other PlayerInputs and find the owner
    // foreach (var playerTransform in allPlayers)
    // {
    //     var netObj = playerTransform.GetComponent<NetworkObject>();
    //     var playerInput = playerTransform.GetComponent<PlayerInput>();
    //     if (playerInput == null) continue;

    //     if (netObj.IsOwner)
    //     {
    //         ownerNetObj = netObj;     // store owner reference
    //         ownerInput = playerInput; // store owner PlayerInput
    //         playerInput.enabled = true; // temporarily enable for now
    //     }
    //     else
    //     {
    //         playerInput.enabled = false; // disable all others
    //     }
    // }

    // // Now assign control scheme for the owner player
    // if (ownerInput != null)
    // {
    //     string controlScheme = NetworkManager.Singleton.IsHost ? "KeyboardLeft" : "KeyboardRight";
    //     ownerInput.SwitchCurrentControlScheme(controlScheme, Keyboard.current);
    // }

    // Update ChunkGenerator
    if (chunkGenerator != null)
        chunkGenerator.players = GetAllSpawnedPlayers();
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
