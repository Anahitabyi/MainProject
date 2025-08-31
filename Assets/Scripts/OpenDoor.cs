using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class OpenDoor : NetworkBehaviour
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
        if (!IsOwner) return; // Only the player controlling the object can send input

        if (_playerInTrigger && Input.GetKeyDown(KeyCode.Return))
        {
            TryOpenDoorServerRpc();
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && IsOwner)
        {
            _playerInTrigger = true;
            UpdateButtonText();
        }
    }

    public void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && IsOwner)
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

    [ServerRpc(RequireOwnership = false)]
    private void TryOpenDoorServerRpc(ServerRpcParams rpcParams = default)
    {
        if (keyTracker == null)
        {
            Debug.LogWarning("KeyTracker instance not found on server!");
            return;
        }

        int currentKeyCount = keyTracker.GetCurrentKeyCount();

        if (currentKeyCount >= requiredKeys)
        {
            // Open the door and notify clients
            OpenDoorClientRpc();
        }
        else
        {
            // Notify the client that keys are insufficient
            ShowLockedDoorClientRpc(currentKeyCount);
        }
    }

    [ClientRpc]
    private void OpenDoorClientRpc()
    {
        if (openDoorSound != null)
        {
            audioSource.PlayOneShot(openDoorSound);
            StartCoroutine(WaitAndLoadScene(openDoorSound.length));
        }
        else
        {
            if (IsServer)
                NetworkManager.Singleton.SceneManager.LoadScene("Level3", LoadSceneMode.Single);
        }
    }

    [ClientRpc]
    private void ShowLockedDoorClientRpc(int currentKeyCount)
    {
        if (lockedDoorSound != null)
            audioSource.PlayOneShot(lockedDoorSound);

        string message = $"Insufficient keys! {currentKeyCount}/{requiredKeys} acquired";

        if (subtitleCoroutine != null)
            StopCoroutine(subtitleCoroutine);

        subtitleCoroutine = StartCoroutine(ShowSubtitle(message, 2f));
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
        if (IsServer)
            NetworkManager.Singleton.SceneManager.LoadScene("Level3", LoadSceneMode.Single);
    }
}
