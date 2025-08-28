using Unity.Netcode;
using UnityEngine;

public class CharacterSelectManager : NetworkBehaviour
{
    public static CharacterSelectManager Instance { get; private set; }
    public NetworkList<CharacterSelection> players;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        players = new NetworkList<CharacterSelection>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        players.Add(new CharacterSelection(clientId, -1));
    }

    private void OnClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == clientId)
            {
                players.RemoveAt(i);
                break;
            }
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void SelectServerRpc(int characterId, ServerRpcParams rpcParams = default)
    {
        // prevent duplicates
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].characterId == characterId)
                return;
        }

        // assign
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == rpcParams.Receive.SenderClientId)
            {
                players[i] = new CharacterSelection(players[i].clientId, characterId);
            }
        }
    }
}