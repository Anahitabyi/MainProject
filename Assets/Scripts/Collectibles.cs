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
        Life // ✅ New collectible type
    }

    public collectibleType type;
    public int scoreValue = 100;
    public int lifevalue = 1;
    public int healthValue = 1;
    public int powerupValue = 0;
    public AudioClip healSound;
    public AudioClip coinPickupSound;
    public AudioClip healthPickupSound;
    public AudioClip lifePickupSound; // ✅ Optional new sound for life
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

        PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();

        switch (type)
        {
            case collectibleType.Food:
                if (playerHealth != null)
                {
                    playerHealth.AddHealth(healthValue);
                }
                if (healSound != null)
                {
                    StartCoroutine(PlaySoundWithMixer(healSound));
                    //Debug.Log("Heal sound played.");
                }
                Destroy(gameObject);
                break;

            case collectibleType.Coin:
                var scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddScore(scoreValue);
                    //Debug.Log("Score added: " + scoreValue);
                }
                if (coinPickupSound != null)
                {
                    StartCoroutine(PlaySoundWithMixer(coinPickupSound));
                    //Debug.Log("Coin pickup sound played.");
                }
                Destroy(gameObject);
                break;
            case collectibleType.Health:
                if (playerHealth != null)
                {
                    playerHealth.AddHealth(healthValue); // ✅ Correctly adds to current health
                }
                if (healthPickupSound != null)
                {
                    StartCoroutine(PlaySoundWithMixer(healthPickupSound));
                    //Debug.Log("Health pickup sound played.");
                }
                Destroy(gameObject);
                break;

            case collectibleType.Life:
                if (playerHealth != null)
                {
                    playerHealth.AddLives(lifevalue);
                }
                if (lifePickupSound != null)
                {
                    StartCoroutine(PlaySoundWithMixer(lifePickupSound));
                    //Debug.Log("Life pickup sound played.");
                }
                Destroy(gameObject);
                break;
            case collectibleType.Powerup:
                meleePlayerMovement meleePlayer = other.GetComponent<meleePlayerMovement>();
                playerMovement rangedPlayer = other.GetComponent<playerMovement>();

                if (meleePlayer != null)
                {
                    meleePlayer.attackDamage += powerupValue;
                    Debug.Log("Melee player powerup applied. New attack damage: " + meleePlayer.attackDamage);
                }
                else if (rangedPlayer != null)
                {
                    rangedPlayer.attackDamage += powerupValue;
                    Debug.Log("Ranged player powerup applied. New attack damage: " + rangedPlayer.attackDamage);
                }

                if (healSound != null)
                {
                    StartCoroutine(PlaySoundWithMixer(healSound));
                }

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
