using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

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

            if (keyTracker != null && keyTracker.getCurrentKey() >= 1) // change this if you require a specific number
            {
                Debug.Log("Player has at least one key. Opening door...");

                if (openDoorClip != null)
                {
                    Debug.Log("Playing door open sound.");
                    audioSource.PlayOneShot(openDoorClip);
                }

                StartCoroutine(FadeOutAndLoad(nextSceneName));
            }
            else
            {
                Debug.Log("Player does not have a key. Playing locked door sound.");

                if (lockedDoorClip != null)
                    audioSource.PlayOneShot(lockedDoorClip);

                if (messageText != null)
                    {
                        Debug.Log("Message text is set: " + messageText.name);
                        StartCoroutine(ShowMessage(lockedMessage, messageDuration));
                    }
                    else
                    {
                        Debug.LogWarning("Message text reference is NULL!");
                    }
            }
        }
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        Debug.Log("Starting fade-out transition.");

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

        Debug.Log("Fade complete. Loading scene: " + sceneName);
        SceneManager.LoadScene(sceneName);
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
