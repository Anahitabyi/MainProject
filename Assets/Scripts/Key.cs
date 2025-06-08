using UnityEngine;
using UnityEngine.Audio;

public class Key : MonoBehaviour
{
    private KeyTracker keyTracker;
    public AudioSource audioSource;
    public AudioClip collectKeySound;
    public AudioMixerGroup sfxMixerGroup;

    private bool collected = false; // Prevent multiple pickups

    private void Start()
    {
        keyTracker = FindFirstObjectByType<KeyTracker>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (collected) return; // Already collected, do nothing
        if (!other.CompareTag("Player")) return;

        collected = true; // Prevent re-triggering

        keyTracker.GotKey();

        if (collectKeySound != null)
        {
            audioSource.PlayOneShot(collectKeySound);
            Destroy(gameObject, collectKeySound.length);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
