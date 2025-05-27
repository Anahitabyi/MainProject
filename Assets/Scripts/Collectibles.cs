using System.Collections;
using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum collectibleType
    {
        Food,
        Coin,
        Powerup
    }
    public collectibleType type;
    public int scoreValue = 100;
    public int healValue = 1;
    public AudioClip healSound;
    public AudioClip coinPickupSound;

    void Awake()
    {
        // set the type based on the gameobject's tag
        if (CompareTag("Food"))
        {
            type = collectibleType.Food;
        }
        if (CompareTag("Coin"))
        {
            type = collectibleType.Coin;
        }
        if (CompareTag("Powerup"))
        {
            type = collectibleType.Coin;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        switch (type)
        {
            case collectibleType.Food: //if the collectible type is a food then heal the player
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.heal(healValue);
                }
                if (healSound != null)
                {
                    StartCoroutine(PlaySoundAtVolume(healSound, 1.5f));
                    Debug.Log("Heal sound played. If you don't hear it you're deaf.");
                }
                Destroy(gameObject);
                break;
            case collectibleType.Coin: //add score if the type is a coin
                var scoreManager = FindFirstObjectByType<ScoreManager>();
                if (scoreManager != null)
                {
                    scoreManager.AddScore(scoreValue);
                    Debug.Log("Score added: " + scoreValue);
                }

                if (coinPickupSound != null)
                {
                    StartCoroutine(PlaySoundAtVolume(coinPickupSound, 1.5f));
                    Debug.Log("Coin pickup sound played. If you don't hear it you're deaf.");
                }
                Destroy(gameObject);
                break;
            case collectibleType.Powerup: //give the player the powerup
                //TODO
                break;
        }
    }
    IEnumerator PlaySoundAtVolume(AudioClip clip, float volume)
    {
        GameObject tempGO = new GameObject("TempAudio");
        AudioSource source = tempGO.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = volume; // e.g. 1.5f for louder
        source.spatialBlend = 0f; // 2D sound
        source.Play();
        Destroy(tempGO, clip.length);
        yield return null;
    }
}
