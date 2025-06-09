using UnityEngine;
using UnityEngine.Audio;

public class ProximityAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip waterfall;
    public AudioClip water;
    public AudioMixerGroup SfxMixerGroup;

    public enum WaterType { Water, Waterfall }
    public WaterType type;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.outputAudioMixerGroup = SfxMixerGroup;
        audioSource.loop = true;
        audioSource.spatialBlend = 1f; // Fully 3D

        switch (type)
        {
            case WaterType.Water:
                audioSource.clip = water;
                break;
            case WaterType.Waterfall:
                audioSource.clip = waterfall;
                break;
        }

        audioSource.Play();
    }
}