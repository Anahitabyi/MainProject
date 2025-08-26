// === PauseMenuManager.cs ===
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI;
    public GameObject pausePanel;
    public GameObject settingsPanel;
    private bool isPaused = false;

    public AudioClip pauseToggleClip;
    public AudioClip resumeToggleClip;
    public AudioClip SettingsClip;
    private AudioSource audioSource;

    public AudioMixer audioMixer;
    public AudioMixerGroup sfxGroup;

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxGroup;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        if (!isPaused)
        {
            if (pauseToggleClip != null)
                audioSource.PlayOneShot(pauseToggleClip);
        }
        else
        {
            if (resumeToggleClip != null)
                audioSource.PlayOneShot(resumeToggleClip);
        }

        isPaused = !isPaused;
        pausePanel.SetActive(isPaused);
        settingsPanel.SetActive(false);
        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void Resume()
    {
        if (resumeToggleClip != null)
            audioSource.PlayOneShot(resumeToggleClip);

        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
    }

    public void RestartLevel()
    {
        if (resumeToggleClip != null)
            audioSource.PlayOneShot(resumeToggleClip);

        Time.timeScale = 1f;
        LoadAndReset();
    }

    private void LoadAndReset()
    {
        PlayerHealth[] players = GameObject.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (PlayerHealth p in players)
        {
            p.SetHealth(9);
        }
        Transform spawn = GameObject.Find("PlayerSpawn")?.transform;
        if (spawn != null)
        {
            var meleePlayer = GameObject.FindAnyObjectByType<meleePlayerMovement>()?.gameObject;
            var rangedPlayer = GameObject.FindAnyObjectByType<playerMovement>()?.gameObject;

            if (meleePlayer != null) meleePlayer.transform.position = spawn.position;
            if (rangedPlayer != null) rangedPlayer.transform.position = spawn.position;
        }
        else
        {
            Debug.LogWarning("PlayerSpawn object not found in scene!");
        }
        if (ScoreManager.Instance != null)
            ScoreManager.Instance.ResetScore();

        if (playerStatsManager.Instance != null)
            playerStatsManager.Instance.ResetAllStats();

        if (SaveTracker.Instance != null)
        SaveTracker.Instance.ClearAll();

        SceneManager.LoadScene("Level1");

        Debug.Log("[Restart] Scene fully loaded: " + SceneManager.GetActiveScene().name);

       
    }

    public void OpenSettings()
    {
        if (SettingsClip != null)
            audioSource.PlayOneShot(SettingsClip);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
    }

    public void QuitToMainMenu()
    {
        if (resumeToggleClip != null)
            audioSource.PlayOneShot(resumeToggleClip);
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void BackToPause()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        if (SettingsClip != null)
            audioSource.PlayOneShot(SettingsClip);
    }

    public void SaveGame()
    {
        Debug.Log("save button clicked!");
        StartCoroutine(DelayedSaveCoroutine());
    }

    private IEnumerator DelayedSaveCoroutine()
    {
        yield return null;

        if (GameSaveController.Instance != null)
        {
            GameSaveController.Instance.SaveToFile();

            SaveFeedback feedback = FindFirstObjectByType<SaveFeedback>();
            if (feedback != null)
                feedback.Show("Game Saved!");
        }
        else
        {
            Debug.LogWarning("GameSaveController not found!");
        }
    }
}
