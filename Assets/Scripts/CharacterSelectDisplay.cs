using Unity.Netcode;
using UnityEngine;

public class CharacterSelectDisplay : NetworkBehaviour
{
    public NetworkList<CharacterSelection> players;
    [SerializeField] private PlayCard[] playerCards;
    public static CharacterSelectDisplay instance { get; private set; }

    private void Awake()
    {
        players = new NetworkList<CharacterSelection>();
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public override void OnNetworkSpawn()
    {
        if (IsClient)
        {
            players.OnListChanged += handlePlayerStateChange;
        }
        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += handleClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += handleClientDisconnected;
        }
    }

    private void handleClientConnected(ulong clientId)
    {
        players.Add(new CharacterSelection(clientId, -1));
    }

    private void handleClientDisconnected(ulong clientId)
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

    private void handlePlayerStateChange(NetworkListEvent<CharacterSelection> e)
    {
        for (int i = 0; i < playerCards.Length; i++)
        {
            if (players.Count > 1)
            {
                playerCards[i].updateDisplay(players[i]);
            }
            else
            {
                playerCards[i].disableDisplay();
            }
        }
    }

    public void select(int characterId)
    {
        SelectServerRpc(characterId);
    }

    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == serverRpcParams.Receive.SenderClientId)
            {
                players[i] = new CharacterSelection(players[i].clientId, characterId);
            }
        }
    }
}
