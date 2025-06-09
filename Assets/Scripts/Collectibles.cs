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
    public int powerupValue = 1;              // How much damage to add
    public float powerupDuration = 10f;       // Duration in seconds

    public AudioClip healSound;
    public AudioClip coinPickupSound;
    public AudioClip healthPickupSound;
    public AudioClip lifePickupSound;
    public AudioMixerGroup sfxMixerGroup;

    void Awake()
    {
        if (CompareTag("Food")) type = collectibleType.Food;
        if (CompareTag("Coin")) type = collectibleType.Coin;
        if (CompareTag("Powerup")) type = collectibleType.Powerup;
        if (CompareTag("Health")) type = collectibleType.Health;
        if (CompareTag("Life")) type = collectibleType.Life;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        GameObject player = other.gameObject;
        PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();

        switch (type)
        {
            case collectibleType.Food:
                if (playerHealth != null)
                    playerHealth.AddHealth(healthValue);

                if (healSound != null)
                    StartCoroutine(PlaySoundWithMixer(healSound));

                Destroy(gameObject);
                break;

            case collectibleType.Coin:
                var scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                    scoreManager.AddScore(scoreValue);

                if (coinPickupSound != null)
                    StartCoroutine(PlaySoundWithMixer(coinPickupSound));

                Destroy(gameObject);
                break;

            case collectibleType.Health:
                if (playerHealth != null)
                    playerHealth.AddHealth(healthValue);

                if (healthPickupSound != null)
                    StartCoroutine(PlaySoundWithMixer(healthPickupSound));

                Destroy(gameObject);
                break;

            case collectibleType.Life:
                if (playerHealth != null)
                    playerHealth.AddLives(lifevalue);

                if (lifePickupSound != null)
                    StartCoroutine(PlaySoundWithMixer(lifePickupSound));

                Destroy(gameObject);
                break;

            case collectibleType.Powerup:
                // Use the DamageBoostHandler component
                DamageBoostHandler boostHandler = player.GetComponent<DamageBoostHandler>();
                if (boostHandler == null)
                    boostHandler = player.AddComponent<DamageBoostHandler>();

                boostHandler.ApplyBoost(player, powerupValue, powerupDuration);

                // Show powerup UI effect (if player has it)
                meleePlayerMovement meleePlayer = player.GetComponent<meleePlayerMovement>();
                playerMovement rangedPlayer = player.GetComponent<playerMovement>();

                if (meleePlayer != null && meleePlayer.weaponUIIndicator != null)
                    meleePlayer.weaponUIIndicator.ShowForDuration(powerupDuration);

                if (rangedPlayer != null && rangedPlayer.weaponUIIndicator != null)
                    rangedPlayer.weaponUIIndicator.ShowForDuration(powerupDuration);

                if (healSound != null)
                    StartCoroutine(PlaySoundWithMixer(healSound));

                Destroy(gameObject);
                break;
        }
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
