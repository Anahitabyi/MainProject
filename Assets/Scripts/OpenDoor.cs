using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class OpenDoor : MonoBehaviour
{
    public TMP_Text _text;
    KeyTracker keyTracker;

    public AudioClip openDoorSound;
    public AudioClip lockedDoorSound;
    public AudioSource audioSource;
    public AudioMixerGroup sfxMixerGroup;

    private bool _playerInTrigger = false;
    private Coroutine subtitleCoroutine;

    private void Start()
    {
        keyTracker = FindFirstObjectByType<KeyTracker>();

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
            UpdateButtonText(keyTracker.getCurrentKey());
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            _playerInTrigger = false;

            // Hide text and stop subtitle if playing
            if (_text != null)
                _text.gameObject.SetActive(false);

            if (subtitleCoroutine != null)
            {
                StopCoroutine(subtitleCoroutine);
                subtitleCoroutine = null;
            }
        }
    }

    void UpdateButtonText(int currentKey)
    {
        if (_text != null)
        {
            _text.text = $"Keys: {currentKey}/4"; // Show the number of keys when in door zone
            _text.gameObject.SetActive(true);
        }
    }

    public void OpenLockedDoor()
    {
        int currentKey = keyTracker.getCurrentKey();

        if (currentKey >= 4)
        {
            if (openDoorSound != null) // Open door (with or without sound)
            {
                audioSource.PlayOneShot(openDoorSound);
                StartCoroutine(WaitAndLoadScene(openDoorSound.length));
            }
            else
            {
                SceneManager.LoadScene("MainMenu");
            }
        }
        else // Not enough keys, won't let player in. 
        {
            if (lockedDoorSound != null)
                audioSource.PlayOneShot(lockedDoorSound);

            string message = $"Insufficient keys! {currentKey}/4 acquired";

            // If subtitle already showing, stop it so we can restart with new message
            if (subtitleCoroutine != null)
                StopCoroutine(subtitleCoroutine);

            subtitleCoroutine = StartCoroutine(ShowSubtitle(message, 2f));
        }
    }

    private IEnumerator ShowSubtitle(string message, float duration) // Method to say that we don't have enough keys.
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
        SceneManager.LoadScene("MainMenu");
    }
}
