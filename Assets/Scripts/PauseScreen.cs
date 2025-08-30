
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System.Collections;
using Unity.Netcode;

public class PauseMenuManager : NetworkBehaviour
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
        //Debug.Log("PauseMenuManager Update running on " + (IsServer ? "Server" : "Client"));
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("ESC pressed locally");
            RequestTogglePauseServerRpc();
        }
    }


    // === Server RPCs (clients request actions) ===
    [ServerRpc(RequireOwnership = false)]
    private void RequestTogglePauseServerRpc(ServerRpcParams rpcParams = default)
    {
        TogglePauseAllClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestResumeServerRpc(ServerRpcParams rpcParams = default)
    {
        ResumeAllClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestRestartServerRpc(ServerRpcParams rpcParams = default)
    {
        RestartAllClientRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestQuitToMenuServerRpc(ServerRpcParams rpcParams = default)
    {
        QuitToMenuAllClientRpc();
    }

    // === Client RPCs (host tells everyone what to do) ===
    [ClientRpc]
    private void TogglePauseAllClientRpc()
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

    [ClientRpc]
    private void ResumeAllClientRpc()
    {
        if (resumeToggleClip != null)
            audioSource.PlayOneShot(resumeToggleClip);

        Time.timeScale = 1f;
        pausePanel.SetActive(false);
        isPaused = false;
    }

    [ClientRpc]
    private void RestartAllClientRpc()
    {
        if (resumeToggleClip != null)
            audioSource.PlayOneShot(resumeToggleClip);

        Time.timeScale = 1f;

        if (IsServer) // only host reloads scene
        {
            // Unsubscribe first to avoid double-calls
            SceneManager.sceneLoaded -= OnSceneLoadedRestart;
            SceneManager.sceneLoaded += OnSceneLoadedRestart;

            NetworkManager.Singleton.SceneManager.LoadScene("Level1", LoadSceneMode.Single);
        }
    }

    private void OnSceneLoadedRestart(Scene scene, LoadSceneMode mode)
    {
        // Reset player stats, scores, trackers
        LoadAndReset();

        // Spawn players via PlayerSpawnerTest
        var spawner = FindObjectOfType<PlayerSpawnerTest>();
        if (spawner != null && spawner.IsServer)
            spawner.SpawnAllSelectedPlayers();

        // Done, unsubscribe
        SceneManager.sceneLoaded -= OnSceneLoadedRestart;
    }

    [ClientRpc]
    private void QuitToMenuAllClientRpc()
    {
        if (resumeToggleClip != null)
            audioSource.PlayOneShot(resumeToggleClip);

        Time.timeScale = 1f;

        if (IsServer) // only host loads menu scene
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
        }
    }

    // === Local UI buttons call these ===
    public void Resume()
    {
        RequestResumeServerRpc();
    }

    public void RestartLevel()
    {
        RequestRestartServerRpc();
    }

    public void QuitToMainMenu()
    {
        RequestQuitToMenuServerRpc();
    }

    public void OpenSettings()
    {
        if (SettingsClip != null)
            audioSource.PlayOneShot(SettingsClip);
        pausePanel.SetActive(false);
        settingsPanel.SetActive(true);
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

    // === Reset player stats & score ===
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
        

        Debug.Log("[Restart] Scene fully loaded: " + SceneManager.GetActiveScene().name);
    }
}
