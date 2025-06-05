using UnityEngine;
using System.Collections.Generic;

public class ProximitySoundManager : MonoBehaviour
{
    public float waterRadius = 10f;
    public float waterfallRadius = 15f;

    private Transform listener;
    private List<AudioSource> waterSources = new List<AudioSource>();
    private List<AudioSource> waterfallSources = new List<AudioSource>();

    void Start()
    {
        listener = Camera.main?.GetComponent<AudioListener>()?.transform ?? transform;

        // Gather all Water AudioSources
        foreach (var go in GameObject.FindGameObjectsWithTag("Water"))
        {
            var src = go.GetComponent<AudioSource>();
            if (src) waterSources.Add(src);
        }
        foreach (var go in GameObject.FindGameObjectsWithTag("Waterfall"))
        {
            var src = go.GetComponent<AudioSource>();
            if (src) waterfallSources.Add(src);
        }
    }

    void Update()
    {
        Vector3 pos = listener.position;

        // Activate water sounds based on distance
        foreach (var src in waterSources)
            ManageSource(src, pos, waterRadius);

        foreach (var src in waterfallSources)
            ManageSource(src, pos, waterfallRadius);
    }

    private void ManageSource(AudioSource src, Vector3 listenerPos, float radius)
    {
        if (!src) return;

        float sqrDist = (src.transform.position - listenerPos).sqrMagnitude;
        float sqrRadius = radius * radius;

        if (sqrDist <= sqrRadius)
        {
            if (!src.isPlaying) src.Play();  // start looping spatial sound
        }
        else
        {
            if (src.isPlaying) src.Stop();
        }
    }
}