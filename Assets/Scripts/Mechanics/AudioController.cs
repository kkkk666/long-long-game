using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class AudioController : MonoBehaviour
{
    public AudioMixer MusicMixer;

    public AudioMixer sfxMixer;
    public Slider musicSlider;
    public Slider sfxSlider;
    
    // Minimum volume value in decibels (-80dB is essentially silent)
    private const float MIN_VOLUME_DB = -80f;
    
    // Default volume values (0.75 = 75% volume)
    private const float DEFAULT_MUSIC_VOLUME = 0.75f;
    private const float DEFAULT_SFX_VOLUME = 0.75f;
    
    void Start()
    {
        // Set initial slider values
        float musicVolume = PlayerPrefs.HasKey("MusicVolume") ? PlayerPrefs.GetFloat("MusicVolume") : DEFAULT_MUSIC_VOLUME;
        float sfxVolume = PlayerPrefs.HasKey("SFXVolume") ? PlayerPrefs.GetFloat("SFXVolume") : DEFAULT_SFX_VOLUME;
        
        // Initialize sliders and audio mixers
        musicSlider.value = musicVolume;
        sfxSlider.value = sfxVolume;
        
        SetMusicVolume(musicVolume);
        SetSFXVolume(sfxVolume);
        
        // Add listeners
        musicSlider.onValueChanged.AddListener(SetMusicVolume);
        sfxSlider.onValueChanged.AddListener(SetSFXVolume);
    }
    
    public void SetMusicVolume(float volume)
    {
        // Convert slider value (0 to 1) to decibels (-80dB to 0dB)
        float volumeDB = volume <= 0.001f ? MIN_VOLUME_DB : Mathf.Log10(volume) * 20f;
        MusicMixer.SetFloat("MusicVolume", volumeDB);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    public void SetSFXVolume(float volume)
    {
        // Convert slider value (0 to 1) to decibels (-80dB to 0dB)
        float volumeDB = volume <= 0.001f ? MIN_VOLUME_DB : Mathf.Log10(volume) * 20f;
        sfxMixer.SetFloat("SFXVolume", volumeDB);
        PlayerPrefs.SetFloat("SFXVolume", volume);
    }
}