using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameModeSelector : MonoBehaviour
{
    public void ChooseLocalCoop()
    {
        Debug.Log("Starting Local Coop...");

        // Start host for local play
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost();
        }

        // Load the main menu scene
        SceneManager.LoadScene("MainMenu");
    }

    public void HostGame()
    {
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost();
        }

        NetworkManager.Singleton.SceneManager.LoadScene("Lobby", LoadSceneMode.Single);
    }

    public void JoinGame()
    {
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartClient();
        }
    }

    public void BackButton()
    {
        SceneManager.LoadScene("Signup");
    }
}