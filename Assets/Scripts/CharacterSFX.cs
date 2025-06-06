using UnityEngine;
using UnityEngine.Audio;

public class CharacterSFX : MonoBehaviour
{
    [Header("Audio Source")]
    public AudioSource audioSource;

    [Header("Sounds")]
    public AudioClip jumpSound;
    public AudioClip landSound;
    public AudioClip hurtSound;
    public AudioClip deathSound;
    public AudioClip attackSound;

    [Header("Audio Mixer")]
    public AudioMixerGroup sfxMixerGroup;

    void Awake()
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

    // === Called by Animation Events ===

    public void PlayJumpSound()
    {
        PlaySound(jumpSound, 0.95f, 1.05f);
    }

    public void PlayLandSound()
    {
        PlaySound(landSound, 0.95f, 1.05f);
    }

    public void PlayHurtSound()
    {
        PlaySound(hurtSound);
    }

    public void PlayDeathSound()
    {
        PlaySound(deathSound);
    }

    public void PlayAttackSound()
    {
        PlaySound(attackSound);
    }

    // === Internal helper ===
    private void PlaySound(AudioClip clip, float pitchMin = 1f, float pitchMax = 1f)
    {
        if (clip != null)
        {
            audioSource.pitch = Random.Range(pitchMin, pitchMax);
            audioSource.PlayOneShot(clip);
            audioSource.pitch = 1f; // Reset pitch
        }
    }
}
