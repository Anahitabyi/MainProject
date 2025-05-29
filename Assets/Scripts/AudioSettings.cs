using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Slider sfxSlider;

    void Start()
    {
        float volume;
        audioMixer.GetFloat("SFXVolume", out volume);
        sfxSlider.value = Mathf.Pow(10, volume / 20); // Convert from dB to linear
    }

    public void SetSFXVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", Mathf.Log10(value) * 20);
    }
}