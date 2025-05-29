using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class PauseMenuManager : MonoBehaviour
{
    public GameObject pauseMenuUI;    // Reference to your Canvas
    public GameObject pausePanel;     // Resume / Settings / Quit
    public GameObject settingsPanel;  // Settings menu
    private bool isPaused = false;
    
    public AudioClip pauseToggleClip;
    public AudioClip resumeToggleClip;
    public AudioClip SettingsClip;
    private AudioSource audioSource;

    public AudioMixer audioMixer;
    public AudioMixerGroup sfxGroup;
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
        pausePanel.SetActive(false);
        isPaused = false;
    }
    
    public void RestartLevel()
    {
        if (resumeToggleClip != null)
            audioSource.PlayOneShot(resumeToggleClip);
        Time.timeScale = 1f;
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
        PlayerHealth[] players = GameObject.FindObjectsByType<PlayerHealth>(FindObjectsSortMode.None);
        foreach (PlayerHealth p in players)
        {
            p.setHealth(9);
        }
        ScoreManager.Instance.ResetScore();
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
        SceneManager.LoadScene("MainMenu"); // Change to your actual main menu scene
    }
    public void BackToPause()
    {
        settingsPanel.SetActive(false);
        pausePanel.SetActive(true);
        if (SettingsClip != null)
            audioSource.PlayOneShot(SettingsClip);
    }
}
