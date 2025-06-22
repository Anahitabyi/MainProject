using UnityEngine;
using UnityEngine.Audio;

public class BossSFX : MonoBehaviour
{
    public AudioClip deathSound;    
    public AudioClip attackSound;    
    public AudioClip alertSound;

    [Header("Spawn Sounds")]
    public AudioClip[] spawnSounds;  // Array of spawn sounds

    private AudioSource audioSource;

    [Header("Audio Mixer")]
    public AudioMixerGroup sfxMixerGroup;

    void Start()
    {
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (sfxMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = sfxMixerGroup;
        }
    }

    public void PlayDeathSound() => audioSource.PlayOneShot(deathSound);
    public void PlayAttackSound() {

        audioSource.PlayOneShot(attackSound, 0.7f);
    } 
    public void PlayAlertSound() => audioSource.PlayOneShot(alertSound);

    public void PlaySpawnSound()
    {
        if (spawnSounds != null && spawnSounds.Length > 0)
        {
            AudioClip clip = spawnSounds[Random.Range(0, spawnSounds.Length)];
            audioSource.PlayOneShot(clip, 0.5f);
        }
    }

    public void PlaySound(AudioClip clip, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (clip != null)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.PlayOneShot(clip);
            audioSource.pitch = 1f;
        }
    }
}