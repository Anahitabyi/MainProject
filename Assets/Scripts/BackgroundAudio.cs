using UnityEngine;
using System.Collections;
using UnityEngine.Audio;

public class BackgroundAudio : MonoBehaviour
{
    public AudioSource nightAmbienceSource;
    public AudioSource dogBarkSource;
    public AudioSource owlHootSource;

    public AudioClip nightAmbienceClip;
    public AudioClip dogBarkClip;
    public AudioClip owlHootClip;

    private float dogTimer;
    private float owlTimer;

    public AudioMixerGroup musicMixerGroup;

    void Awake()
    {
        // Ensure AudioSources exist
        if (nightAmbienceSource == null)
        {
            nightAmbienceSource = gameObject.AddComponent<AudioSource>();
            nightAmbienceSource.outputAudioMixerGroup = musicMixerGroup;
        }

        if (dogBarkSource == null)
        {
            dogBarkSource = gameObject.AddComponent<AudioSource>();
            dogBarkSource.outputAudioMixerGroup = musicMixerGroup;
        }

        if (owlHootSource == null)
        {
            owlHootSource = gameObject.AddComponent<AudioSource>();
            owlHootSource.outputAudioMixerGroup = musicMixerGroup;
        }
    }

    void Start()
    {
        // start night ambience
        nightAmbienceSource.clip = nightAmbienceClip;
        owlHootSource.clip = owlHootClip;
        dogBarkSource.clip = dogBarkClip;
        nightAmbienceSource.loop = true;
        nightAmbienceSource.Play();

        // initialize timers
        dogTimer = GetRandomDogDelay();
        owlTimer = GetRandomOwlDelay();
    }

    void Update()
    {
        // dog audio
        dogTimer -= Time.deltaTime;
        if (dogTimer <= 0f)
        {
            dogBarkSource.PlayOneShot(dogBarkClip);
            dogTimer = GetRandomDogDelay();
        }

        // owl audio
        owlTimer -= Time.deltaTime;
        if (owlTimer <= 0f)
        {
            owlHootSource.PlayOneShot(owlHootClip);
            owlTimer = GetRandomOwlDelay();
        }
    }

    float GetRandomDogDelay()
    {
        return Random.Range(40f, 70f);
    }

    float GetRandomOwlDelay()
    {
        return Random.Range(25f, 50f);
    }
}
