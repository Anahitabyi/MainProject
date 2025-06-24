using UnityEngine;
using UnityEngine.SceneManagement;

public class StartMenuUI : MonoBehaviour
{
    public GameObject startOptionsPanel;
    public string firstLevelSceneName = "Level1";

    public GameObject otherButton1;
    public GameObject otherButton2;
    public GameObject startButton;

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
        // Reset all relevant data
        playerStatsManager.Instance.ResetAllStats();

        if (SaveTracker.Instance != null)
        {
            SaveTracker.Instance.collectedIDs.Clear();
            SaveTracker.Instance.defeatedEnemyIDs.Clear();
            SaveTracker.Instance.ClearChunks(); // ✅ Important!
        }

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
        startOptionsPanel.SetActive(false);

        if (startButton != null)
            startButton.SetActive(true);

        if (otherButton1 != null)
            otherButton1.SetActive(true);

        if (otherButton2 != null)
            otherButton2.SetActive(true);
    }
}
