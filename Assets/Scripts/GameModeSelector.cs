using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameModeSelector : MonoBehaviour
{
    public static GameModeSelector Instance;

    [SerializeField] private bool isOnline = false; // ✅ false = offline/local, true = online

    private void Awake()
    {
        // Singleton setup so this persists across scenes
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChooseLocalCoop()
    {
        Debug.Log("Starting Local Coop...");
        isOnline = false;

        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost(); // still use host but no real clients will join
        }

        SceneManager.LoadScene("Level1");
    }

    public void HostGame()
    {
        Debug.Log("Hosting Online Game...");
        isOnline = true;

        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost();
        }

        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        Debug.Log("Joining Online Game...");
        isOnline = true;

        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    public void BackButton()
    {
        SceneManager.LoadScene("Signup");
    }

    public bool IsOnline()
    {
        return isOnline;
    }
}