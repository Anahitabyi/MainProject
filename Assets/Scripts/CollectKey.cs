using UnityEngine;
using UnityEngine.Audio;

public class CollectKey : MonoBehaviour
{
    private KeyTracker keyTracker;
    public AudioSource audioSource;
    public AudioClip collectKeySound;
    public AudioMixerGroup sfxMixerGroup;

    private void Start()
    {
        keyTracker = FindFirstObjectByType<KeyTracker>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            keyTracker.GotKey();

            if (collectKeySound != null)
            {
                audioSource.PlayOneShot(collectKeySound);
                // Destroy after sound length to let it play
                Destroy(gameObject, collectKeySound.length);
            }
            else
            {
                Destroy(gameObject); // No sound, destroy immediately
            }
        }
    }
}