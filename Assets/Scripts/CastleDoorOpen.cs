using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;
using Unity.Netcode;

public class CastleDoorOpen : MonoBehaviour
{
    [Header("Scene Transition")]
    public string nextSceneName = "Level2";
    public Image fadeImage;
    public float fadeDuration = 5f;

    [Header("Audio")]
    public AudioClip openDoorClip;
    public AudioClip lockedDoorClip;
    public AudioMixerGroup sfxMixerGroup;

    [Header("UI")]
    public TMP_Text messageText;
    public string lockedMessage = "You need a key to open this door!";
    public float messageDuration = 2f;

    private AudioSource audioSource;
    private bool playerInRange = false;
    private KeyTracker keyTracker;

    private void Start()
    {
        keyTracker = FindFirstObjectByType<KeyTracker>();
        if (keyTracker == null)
        {
            Debug.LogWarning("KeyTracker not found in the scene!");
        }

        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = sfxMixerGroup;

        Debug.Log("CastleDoorOpen initialized.");
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.Return))
        {
            Debug.Log("Return key pressed while player is in range.");
            if (openDoorClip != null)
            {
                Debug.Log("Playing door open sound.");
                audioSource.PlayOneShot(openDoorClip);
            }

            // Only the server tells everyone to fade
            if (NetworkManager.Singleton.IsServer)
            {
                StartFadeClientRpc();
            }
        }
    }

    [ClientRpc]
    private void StartFadeClientRpc()
    {
        // Run fade on all clients
        StartCoroutine(FadeOutCoroutine());
    }

    private IEnumerator FadeOutCoroutine()
    {
        fadeImage.gameObject.SetActive(true);
        float elapsed = 0f;
        Color startColor = fadeImage.color;
        startColor.a = 0f;
        Color targetColor = startColor;
        targetColor.a = 1f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeImage.color = Color.Lerp(startColor, targetColor, elapsed / fadeDuration);
            yield return null;
        }

        // Only the server actually changes the scene, after letting clients finish their fade
        if (NetworkManager.Singleton.IsServer)
        {
            // Wait a frame to make sure the RPC was processed by clients
            yield return null;
            NetworkManager.Singleton.SceneManager.LoadScene(nextSceneName, LoadSceneMode.Single);
        }
    }



    private IEnumerator ShowMessage(string message, float duration)
    {
        Debug.Log("Showing locked door message: " + message);

        messageText.gameObject.SetActive(true);  // Ensure GameObject is active
        messageText.text = message;
        messageText.enabled = true;

        yield return new WaitForSeconds(duration);

        messageText.enabled = false;
        messageText.gameObject.SetActive(false); // Optional: hide it again
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Player entered door trigger zone.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Player exited door trigger zone.");
        }
    }
}
