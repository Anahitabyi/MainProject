using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class OpenDoor : MonoBehaviour
{
    public TMP_Text _text;
    private KeyTracker keyTracker;

    public AudioClip openDoorSound;
    public AudioClip lockedDoorSound;
    public AudioSource audioSource;
    public AudioMixerGroup sfxMixerGroup;

    private bool _playerInTrigger = false;
    private Coroutine subtitleCoroutine;

    private const int requiredKeys = 4;

    private void Start()
    {
        keyTracker = KeyTracker.Instance;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    private void Update()
    {
        if (_playerInTrigger && Input.GetKeyDown(KeyCode.Return))
        {
            OpenLockedDoor();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = true;
            UpdateButtonText();
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = false;

            if (_text != null)
                _text.gameObject.SetActive(false);

            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
                subtitleCoroutine = null;
            }
        }
    }

    void UpdateButtonText()
    {
        if (_text != null && keyTracker != null)
        {
            int currentKeyCount = keyTracker.GetCurrentKeyCount();
            _text.text = $"Keys: {currentKeyCount}/{requiredKeys}";
            _text.gameObject.SetActive(true);
        }
    }

    public void OpenLockedDoor()
    {
        if (keyTracker == null)
        {
            Debug.LogWarning("KeyTracker instance not found!");
            return;
        }

        int currentKeyCount = keyTracker.GetCurrentKeyCount();

        if (currentKeyCount >= requiredKeys)
        {
            if (openDoorSound != null)
            {
                audioSource.PlayOneShot(openDoorSound);
                StartCoroutine(WaitAndLoadScene(openDoorSound.length));
            }
            else
            {
                SceneManager.LoadScene("Level3");
            }
        }
        else
        {
            if (lockedDoorSound != null)
                audioSource.PlayOneShot(lockedDoorSound);

            string message = $"Insufficient keys! {currentKeyCount}/{requiredKeys} acquired";

            if (subtitleCoroutine != null)
                StopCoroutine(subtitleCoroutine);

            subtitleCoroutine = StartCoroutine(ShowSubtitle(message, 2f));
        }
    }

    private IEnumerator ShowSubtitle(string message, float duration)
    {
        if (_text != null)
        {
            _text.text = message;
            _text.gameObject.SetActive(true);
            yield return new WaitForSeconds(duration);
            _text.gameObject.SetActive(false);
            subtitleCoroutine = null;
        }
    }

    private IEnumerator WaitAndLoadScene(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        SceneManager.LoadScene("Level3");
    }
}