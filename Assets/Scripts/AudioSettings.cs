using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class AudioSettings : MonoBehaviour
{
    public AudioMixer audioMixer;

    public Slider sfxSlider;
    public Slider musicSlider;

    public Button sfxMuteButton;
    public Button musicMuteButton;

    public Image sfxMuteIcon;
    public Image musicMuteIcon;

    public Sprite MutedSprite;
    public Sprite UnmutedSprite;

    private float lastSFXVolume = 0.5f;
    private float lastMusicVolume = 0.5f;

    private bool isSFXMuted = false;
    private bool isMusicMuted = false;

    void Start()
    {
        float savedSFX = PlayerPrefs.GetFloat("SFXVolume", 0.5f);
        float savedMusic = PlayerPrefs.GetFloat("MusicVolume", 0.5f);

        lastSFXVolume = savedSFX;
        lastMusicVolume = savedMusic;

        sfxSlider.value = savedSFX;
        musicSlider.value = savedMusic;

        ApplyVolume("SFXVolume", savedSFX);
        ApplyVolume("MusicVolume", savedMusic);

        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
        musicSlider.onValueChanged.AddListener(SetMusicVolume);

        sfxMuteButton.onClick.AddListener(ToggleSFXMute);
        musicMuteButton.onClick.AddListener(ToggleMusicMute);
    }

    private void ApplyVolume(string parameter, float value)
    {
        if (value <= 0.0001f)
            audioMixer.SetFloat(parameter, -80f);
        else
            audioMixer.SetFloat(parameter, Mathf.Log10(value) * 20f);
    }

    public void SetSFXVolume(float value)
    {
        lastSFXVolume = value;
        PlayerPrefs.SetFloat("SFXVolume", value);

        if (!isSFXMuted)
            ApplyVolume("SFXVolume", value);
    }

    public void SetMusicVolume(float value)
    {
        lastMusicVolume = value;
        PlayerPrefs.SetFloat("MusicVolume", value);

        if (!isMusicMuted)
            ApplyVolume("MusicVolume", value);
    }

    public void ToggleSFXMute()
    {
        isSFXMuted = !isSFXMuted;

        if (isSFXMuted)
        {
            audioMixer.SetFloat("SFXVolume", -80f);
            sfxMuteIcon.sprite = MutedSprite;
        }
        else
        {
            ApplyVolume("SFXVolume", lastSFXVolume);
            sfxSlider.value = lastSFXVolume;
            SetSFXVolume(lastSFXVolume); // Manually call this
            sfxMuteIcon.sprite = UnmutedSprite;
        }
    }

    public void ToggleMusicMute()
    {
        isMusicMuted = !isMusicMuted;

        if (isMusicMuted)
        {
            audioMixer.SetFloat("MusicVolume", -80f);
            musicMuteIcon.sprite = MutedSprite;
        }
        else
        {
            ApplyVolume("MusicVolume", lastMusicVolume);
            musicSlider.value = lastMusicVolume;
            SetMusicVolume(lastMusicVolume); // Manually call this
            musicMuteIcon.sprite = UnmutedSprite;
        }
    }

}
