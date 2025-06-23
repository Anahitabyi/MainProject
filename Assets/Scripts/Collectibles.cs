using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class Collectible : MonoBehaviour
{
    public enum collectibleType
    {
        Food,
        Coin,
        Powerup,
        Health,
        Life
    }

    public collectibleType type;
    public int scoreValue = 100;
    public int lifevalue = 1;
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
        if (CompareTag("Food")) type = collectibleType.Food;
        if (CompareTag("Coin")) type = collectibleType.Coin;
        if (CompareTag("Powerup")) type = collectibleType.Powerup;
        if (CompareTag("Health")) type = collectibleType.Health;
        if (CompareTag("Life")) type = collectibleType.Life;

        uniqueID = GetComponent<UniqueID>();
    }

    void Start()
    {
        if (SaveTracker.Instance != null && SaveTracker.Instance.IsCollected(uniqueID.id))
        {
            gameObject.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject player = other.gameObject;
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        SaveTracker.Instance?.MarkCollected(uniqueID.id); // ✅ Mark as collected before doing anything

        switch (type)
        {
            case collectibleType.Food:
                playerHealth?.AddHealth(healthValue);
                PlaySound(healSound);
                break;

            case collectibleType.Coin:
                FindFirstObjectByType<ScoreManager>()?.AddScore(scoreValue);
                PlaySound(coinPickupSound);
                break;

            case collectibleType.Health:
                playerHealth?.AddHealth(healthValue);
                PlaySound(healthPickupSound);
                break;

            case collectibleType.Life:
                playerHealth?.AddLives(lifevalue);
                PlaySound(lifePickupSound);
                break;

            case collectibleType.Powerup:
                DamageBoostHandler boostHandler = player.GetComponent<DamageBoostHandler>();
                if (boostHandler == null)
                    boostHandler = player.AddComponent<DamageBoostHandler>();

                boostHandler.ApplyBoost(player, powerupValue, powerupDuration);

                player.GetComponent<meleePlayerMovement>()?.weaponUIIndicator?.ShowForDuration(powerupDuration);
                player.GetComponent<playerMovement>()?.weaponUIIndicator?.ShowForDuration(powerupDuration);

                PlaySound(healSound);
                break;
        }

        Destroy(gameObject);
    }

    void PlaySound(AudioClip clip)
    {
        if (clip == null) return;
        StartCoroutine(PlaySoundWithMixer(clip));
    }

    IEnumerator PlaySoundWithMixer(AudioClip clip)
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
