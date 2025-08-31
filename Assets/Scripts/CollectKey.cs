using UnityEngine;
using UnityEngine.Audio;
using Unity.Netcode;

public class CollectKey : MonoBehaviour
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
        Debug.Log($"[CollectKey] Start called for key {keyIndex}");

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
            Debug.Log("[CollectKey] AudioSource added dynamically.");
        }

        // If already collected, destroy key immediately 
        if (keyTracker != null && keyTracker.HasKey(keyIndex))
        {
            Debug.Log($"[CollectKey] Key {keyIndex} already collected, destroying.");
            Destroy(gameObject);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[CollectKey] Trigger entered by {other.name} for key {keyIndex}");

        if (!NetworkManager.Singleton.IsServer)
        {
            Debug.Log("[CollectKey] Not server, ignoring key collection.");
            return;
        }

        if (other.CompareTag("Player"))
        {
            Debug.Log($"[CollectKey] Player collided with key {keyIndex}, calling ServerRpc.");
            TryCollectKeyServerRpc(keyIndex);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void TryCollectKeyServerRpc(int index, ServerRpcParams rpcParams = default)
    {
        Debug.Log($"[CollectKey] ServerRpc received for key {index}");

        if (keyTracker != null && !keyTracker.HasKey(index))
        {
            Debug.Log($"[CollectKey] Server: Key {index} is new, collecting...");
            keyTracker.GotKey(index);

            Debug.Log($"[CollectKey] Server: Calling ClientRpc to destroy/play sound for key {index}");
            CollectKeyClientRpc();
        }
        else
        {
            Debug.Log($"[CollectKey] Server: Key {index} was already collected or keyTracker missing.");
        }
    }

    // Called on all clients to play the sound and destroy the key
    [ClientRpc]
    private void CollectKeyClientRpc()
    {
        Debug.Log($"[CollectKey] ClientRpc called for key {keyIndex}");

        if (collectKeySound != null)
        {
            Debug.Log($"[CollectKey] Playing collect sound for key {keyIndex}");
            audioSource.PlayOneShot(collectKeySound);
            Destroy(gameObject, collectKeySound.length);
        }
        else
        {
            Debug.Log($"[CollectKey] No sound set, just destroying key {keyIndex}");
            Destroy(gameObject);
        }
    }
}
