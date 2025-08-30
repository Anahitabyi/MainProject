using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using Unity.Netcode;

public class Collectible : NetworkBehaviour
{
    public enum CollectibleType
    {
        Food,
        Coin,
        Powerup,
        Health,
        Life
    }

    public CollectibleType type;
    public int scoreValue = 100;
    public int lifeValue = 1;
    public int healthValue = 1;
    public int powerupValue = 1;
    public float powerupDuration = 10f;

    public AudioClip healSound;
    public AudioClip coinPickupSound;
    public AudioClip healthPickupSound;
    public AudioClip lifePickupSound;
    public AudioMixerGroup sfxMixerGroup;

    public UniqueID uniqueID;

    void Awake()
    {
        if (CompareTag("Food")) type = CollectibleType.Food;
        if (CompareTag("Coin")) type = CollectibleType.Coin;
        if (CompareTag("Powerup")) type = CollectibleType.Powerup;
        if (CompareTag("Health")) type = CollectibleType.Health;
        if (CompareTag("Life")) type = CollectibleType.Life;

        uniqueID = GetComponent<UniqueID>();
    }

    void Start()
    {
        if (SaveTracker.Instance != null && SaveTracker.Instance.IsCollected(uniqueID.id))
        {
            gameObject.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsServer) return; // ✅ Only server handles collection
        if (!other.CompareTag("Player")) return;

        GameObject player = other.gameObject;
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        SaveTracker.Instance?.MarkCollected(uniqueID.id);

        switch (type)
        {
            case CollectibleType.Food:
                playerHealth?.AddHealth(healthValue);
                PlaySoundClientRpc();
                break;

            case CollectibleType.Coin:
                FindFirstObjectByType<ScoreManager>()?.AddScore(scoreValue);
                PlaySoundClientRpc();
                break;

            case CollectibleType.Health:
                playerHealth?.AddHealth(healthValue);
                PlaySoundClientRpc();
                break;

            case CollectibleType.Life:
                playerHealth?.AddLives(lifeValue);
                PlaySoundClientRpc();
                break;

            case CollectibleType.Powerup:
                DamageBoostHandler boostHandler = player.GetComponent<DamageBoostHandler>();
                if (boostHandler == null)
                    boostHandler = player.AddComponent<DamageBoostHandler>();

                boostHandler.ApplyBoost(player, powerupValue, powerupDuration);

                player.GetComponent<meleePlayerMovement>()?.weaponUIIndicator?.ShowForDuration(powerupDuration);
                player.GetComponent<playerMovement>()?.weaponUIIndicator?.ShowForDuration(powerupDuration);

                PlaySoundClientRpc();
                break;
        }

        // ✅ Despawn across network
        GetComponent<NetworkObject>().Despawn();
    }

    // ------------------- Networking -------------------

    [ClientRpc]
    private void PlaySoundClientRpc()
    {
        // This runs on all clients
        AudioClip clip = GetClipForType();
        if (clip != null)
            StartCoroutine(PlaySoundWithMixer(clip));
    }

    private AudioClip GetClipForType()
    {
        return type switch
        {
            CollectibleType.Food => healSound,
            CollectibleType.Coin => coinPickupSound,
            CollectibleType.Health => healthPickupSound,
            CollectibleType.Life => lifePickupSound,
            CollectibleType.Powerup => healSound,
            _ => null
        };
    }

    private IEnumerator PlaySoundWithMixer(AudioClip clip)
    {
        GameObject tempGO = new GameObject("TempAudio");
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.outputAudioMixerGroup = sfxMixerGroup;
        source.spatialBlend = 0f;
        source.Play();
        Destroy(tempGO, clip.length);
        yield return null;
    }
}
