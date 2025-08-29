using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;

public class StartMenuUI : NetworkBehaviour
{
    public GameObject startOptionsPanel;
    public string firstLevelSceneName = "Level1";

    public GameObject otherButton1;
    public GameObject otherButton2;
    public GameObject startButton;

    private void Start()
    {
        if (startOptionsPanel != null)
            startOptionsPanel.SetActive(false);
    }
    
    public void OnStartButtonPressed()
    {
        if (IsHost)
        {
            // Host tells everyone to show the start options panel
            ShowStartOptionsClientRpc();

            if (startButton != null)
                startButton.SetActive(false);

            if (otherButton1 != null)
                otherButton1.SetActive(false);

            if (otherButton2 != null)
                otherButton2.SetActive(false);
        }
    }

    [ClientRpc]
    private void ShowStartOptionsClientRpc()
    {
        if (startOptionsPanel != null)
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
        if (IsHost)
        {
            ResetGameData();
            // Load the scene for all players
            NetworkManager.SceneManager.LoadScene(firstLevelSceneName, LoadSceneMode.Single);
        }
    }

    // === Called by UI Button ===
    public void OnLoadGamePressed()
    {
        if (IsHost)
        {
            if (SaveSystem.SaveExists())
            {
                GameSaveController.Instance.LoadFromFile();
                NetworkManager.SceneManager.LoadScene(firstLevelSceneName, LoadSceneMode.Single);
            }
            else
            {
                Debug.Log("No saved game found.");
            }
        }
    }

    // === Called by UI Button ===
    public void OnBackToStartScreen()
    {
        if (IsHost)
        {
            BackToStartScreenClientRpc();
        }
    }

    [ClientRpc]
    private void BackToStartScreenClientRpc()
    {
        if (startOptionsPanel != null)
            startOptionsPanel.SetActive(false);

        if (startButton != null)
            startButton.SetActive(true);

        if (otherButton1 != null)
            otherButton1.SetActive(true);

        if (otherButton2 != null)
            otherButton2.SetActive(true);
    }

    private void ResetGameData()
    {
        if (playerStatsManager.Instance != null)
            playerStatsManager.Instance.ResetAllStats();

        if (SaveTracker.Instance != null)
        {
            SaveTracker.Instance.collectedIDs.Clear();
            SaveTracker.Instance.defeatedEnemyIDs.Clear();
            SaveTracker.Instance.ClearChunks();
        }

        SaveSystem.DeleteSave();
    }
}
