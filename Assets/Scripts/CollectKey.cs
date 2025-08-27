using UnityEngine;
using UnityEngine.Audio;
using Unity.Netcode;

public class CollectKey : NetworkBehaviour
{
    private KeyTracker keyTracker;
    public AudioSource audioSource;
    public AudioClip collectKeySound;
    public AudioMixerGroup sfxMixerGroup;

    // The unique index for this key 
    public int keyIndex = 0;

    private void Start()
    {
        keyTracker = KeyTracker.Instance;

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }

        // If already collected, destroy key immediately 
        if (keyTracker != null && keyTracker.HasKey(keyIndex))
        {
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return; // Only server handles collection for sync

        if (other.CompareTag("Player"))
        {
            if (keyTracker != null && !keyTracker.HasKey(keyIndex))
            {
                keyTracker.GotKey(keyIndex);

                // Inform all clients to play sound and destroy the key object
                CollectKeyClientRpc();
            }
        }
    }

    // Called on all clients to play the sound and destroy the key
    [ClientRpc]
    private void CollectKeyClientRpc()
    {
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