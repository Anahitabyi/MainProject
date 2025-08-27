using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class GameModeSelector : MonoBehaviour
{
    // Assign these scene names in the Inspector
    [Header("Scene Names")]
    [SerializeField] private string mainMenuScene = "MainMenu";
    [SerializeField] private string signupScene = "Signup";

    public void ChooseLocalCoop()
    {
        Debug.Log("Starting Local Coop...");

        // Start host for local play
        if (!NetworkManager.Singleton.IsListening)
        {
            NetworkManager.Singleton.StartHost();
        }

        // Load the main menu scene
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ChooseOnline()
    {
        Debug.Log("Going to Signup...");

        // Go to signup scene first
        SceneManager.LoadScene(signupScene);
    }
}