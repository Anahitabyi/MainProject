using UnityEngine;
using System.Collections;

public class NightAudio : MonoBehaviour
{
    public AudioSource nightAmbienceSource;
    public AudioSource dogBarkSource;
    public AudioSource owlHootSource;

    public AudioClip nightAmbienceClip;
    public AudioClip dogBarkClip;
    public AudioClip owlHootClip;

    private float dogTimer;
    private float owlTimer;

    void Start()
    {
        // start night ambience
        nightAmbienceSource.clip = nightAmbienceClip;
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
        return Random.Range(50f, 80f);
    }

    float GetRandomOwlDelay()
    {
        return Random.Range(30f, 50f);
    }
}
