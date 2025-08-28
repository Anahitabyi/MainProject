using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

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
    
    [ServerRpc (RequireOwnership = false)]
    private void SelectServerRpc(int characterId, ServerRpcParams serverRpcParams = default)
    {
        // Prevent duplicate picks
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].characterId == characterId)
            {
                return; // character already chosen, ignore
            }
        }

        // Assign to this client
        for (int i = 0; i < players.Count; i++)
        {
            if (players[i].clientId == serverRpcParams.Receive.SenderClientId)
            {
                players[i] = new CharacterSelection(players[i].clientId, characterId);
            }
        }
    }


    public void backButton()
    {
        SceneManager.LoadScene("Signup");
    }

    public void startButton()
    {
        if (!IsHost) 
        {
            Debug.Log("Only host can start the game.");
            return;
        }

        foreach (var player in players)
        {
            if (player.characterId == -1)
            {
                Debug.Log("Not all players have chosen a character!");
                return;
            }
        }

        Debug.Log("Loading MainMenu for all players...");
        NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

}
