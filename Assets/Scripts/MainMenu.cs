using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public GameObject settingsPanel;

    public void StartGame()
    {
        Debug.Log("Start button pressed");

        /*PlayerHealth[] players = GameObject.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (PlayerHealth p in players)
        {
            p.setHealth(9);
        }
        ScoreManager.Instance.ResetScore();*/
        // Load first level
        NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
    }

    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    public void ExitGame()
    {
        Debug.Log("Quit Game");
        Application.Quit(); // i said inshallah ke this will work in build
    }
}