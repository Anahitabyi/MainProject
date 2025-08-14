using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerSpawnerTest : NetworkBehaviour
{
    [Header("Player Prefabs")]
    public NetworkObject shooterPrefab;
    public NetworkObject meleePrefab;

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
    }

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

        // Assign keyboard schemes **only for players on the local machine**
        var playerInput = playerInstance.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            // Host uses Right keyboard, client uses Left keyboard
            if (clientId == NetworkManager.Singleton.LocalClientId && NetworkManager.Singleton.IsHost)
            {
                playerInput.SwitchCurrentControlScheme("KeyboardLeft", Keyboard.current);
            }
            else
            {
                playerInput.SwitchCurrentControlScheme("KeyboardRight", Keyboard.current);
            }
        }
    }
}
