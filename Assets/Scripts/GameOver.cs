using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // Called when Restart button is clicked
    public void RestartGame()
    {
        Debug.Log("Restart button pressed");

        /*PlayerHealth[] players = GameObject.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (PlayerHealth p in players)
        {
            p.setHealth(9);
        }
        ScoreManager.Instance.ResetScore();*/
        SceneManager.LoadScene("Level1");
    }

    // Called when Exit button is clicked
    public void ExitToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}