using UnityEngine;
using UnityEngine.SceneManagement;


public class StartMenuUI : MonoBehaviour
{
    public GameObject startOptionsPanel;
    public string firstLevelSceneName = "Level1";

    public GameObject otherButton1; // assign in Inspector
    public GameObject otherButton2; // assign in Inspector
    public GameObject startButton;  // optional: assign if you're manually hiding Start

    private void Start()
    {
        startOptionsPanel.SetActive(false);
    }

    public void OnStartButtonPressed()
    {
        startOptionsPanel.SetActive(true);

        if (startButton != null)
            startButton.SetActive(false);

        if (otherButton1 != null)
            otherButton1.SetActive(false);

        if (otherButton2 != null)
            otherButton2.SetActive(false);
    }

    public void OnNewGamePressed()
    {
        playerStatsManager.Instance.ResetAllStats();
        SaveTracker.Instance.collectedIDs.Clear();
        SaveTracker.Instance.defeatedEnemyIDs.Clear();
        SaveSystem.DeleteSave();

        SceneManager.LoadScene(firstLevelSceneName);
    }

    public void OnLoadGamePressed()
    {
        if (SaveSystem.SaveExists())
        {
            GameSaveController.Instance.LoadFromFile();
        }
        else
        {
            Debug.Log("No saved game found.");
        }
    }
    public void OnBackToStartScreen()
{
    // Hide the options panel
    startOptionsPanel.SetActive(false);

    // Show Start and other buttons again
    if (startButton != null)
        startButton.SetActive(true);

    if (otherButton1 != null)
        otherButton1.SetActive(true);

    if (otherButton2 != null)
        otherButton2.SetActive(true);
}

}
