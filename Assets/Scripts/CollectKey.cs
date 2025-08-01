using UnityEngine;
using UnityEngine.Audio;

public class CollectKey : MonoBehaviour
{
    private KeyTracker keyTracker;
    public AudioSource audioSource;
    public AudioClip collectKeySound;
    public AudioMixerGroup sfxMixerGroup;

    private string keyID;

    private void Start()
    {
        keyTracker = KeyTracker.Instance;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        // Get the UniqueID component and fetch the id
        var uniqueID = GetComponent<UniqueID>();
        if (uniqueID == null)
        {
            Debug.LogError($"No UniqueID component on {gameObject.name}!");
            keyID = gameObject.name; // fallback
        }
        else
        {
            keyID = uniqueID.id;
        }

        // If already collected, destroy key immediately (skip showing it again)
        if (keyTracker != null && keyTracker.HasKey(keyID))
        {
            Destroy(gameObject);
        }
    }

    public void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (keyTracker != null && !keyTracker.HasKey(keyID))
            {
                keyTracker.GotKey(keyID);

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
    }
}
