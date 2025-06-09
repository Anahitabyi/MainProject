using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    // Called when Restart button is clicked
    public void RestartGame()
    {
        Debug.Log("Restart button pressed");

        if (playerStatsManager.Instance != null)
        {
            playerStatsManager.Instance.ResetAllStats();
        }

        SceneManager.LoadScene("Level1");
    }

    // Called when Exit button is clicked
    public void ExitToMainMenu()
{
    if (playerStatsManager.Instance != null)
    {
        playerStatsManager.Instance.ResetAllStats();
    }

    SceneManager.LoadScene("MainMenu");
}
}