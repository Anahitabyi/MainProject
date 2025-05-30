using UnityEngine;
using UnityEngine.Audio;

public class ProximityAudio : MonoBehaviour
{
    [Header("Audio Clips")]
    public AudioClip waterClip;
    public AudioClip waterfallClip;

    [Header("Audio Settings")]
    public float maxDistance = 10f;
    public float minVolume = 0f;
    public float maxVolume = 1f;
    public AudioMixerGroup outputMixerGroup;

    private AudioSource waterSource;
    private AudioSource waterfallSource;

    void Start()
    {
        waterSource = CreateAudioSource(waterClip);
        waterfallSource = CreateAudioSource(waterfallClip);
    }

    void Update()
    {
        waterSource.volume = CalculateVolumeForTag("Water");
        waterfallSource.volume = CalculateVolumeForTag("Waterfall");
    }

    AudioSource CreateAudioSource(AudioClip clip)
    {
        AudioSource source = gameObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.loop = true;
        source.playOnAwake = false;
        source.spatialBlend = 0f;
        if (outputMixerGroup != null)
            source.outputAudioMixerGroup = outputMixerGroup;
        source.Play();
        return source;
    }

    float CalculateVolumeForTag(string tag)
    {
        GameObject[] objs = GameObject.FindGameObjectsWithTag(tag);
        if (objs.Length == 0) return 0f;

        float closestDist = Mathf.Infinity;
        Vector2 playerPos = new Vector2(transform.position.x, transform.position.y);

        foreach (GameObject obj in objs)
        {
            Vector2 objPos = new Vector2(obj.transform.position.x, obj.transform.position.y);
            float dist = Vector2.Distance(playerPos, objPos);
            if (dist < closestDist)
                closestDist = dist;
        }

        if (closestDist <= maxDistance)
        {
            float t = Mathf.Clamp01(closestDist / maxDistance);
            return Mathf.Lerp(maxVolume, minVolume, t);
        }

        return 0f;
    }
}