using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingsManager : MonoBehaviour
{
    [Header("Audio Mixer")]
    public AudioMixer mainMixer;

    [Header("UI References")]
    public GameObject optionsPanel;
    public Slider masterSlider;
    public Slider musicSlider;
    public Slider sfxSlider;
    public Toggle fullscreenToggle;

    private void Start()
    {
        LoadSettings();
    }

    public void SetMasterVolume(float value)
    {
        // Công thức chuyển từ Slider (0.0001 -> 1) sang Decibel (-80 -> 0)
        // Log10 giúp thanh trượt mượt tai hơn
        float dB = Mathf.Log10(value) * 20;
        mainMixer.SetFloat("MasterVol", dB);
        PlayerPrefs.SetFloat("MasterVol", value); 
    }

    public void SetMusicVolume(float value)
    {
        float dB = Mathf.Log10(value) * 20;
        mainMixer.SetFloat("MusicVol", dB);
        PlayerPrefs.SetFloat("MusicVol", value);
    }

    public void SetSFXVolume(float value)
    {
        float dB = Mathf.Log10(value) * 20;
        mainMixer.SetFloat("SFXVol", dB);
        PlayerPrefs.SetFloat("SFXVol", value);
    }

    // --- PHẦN MÀN HÌNH ---
    public void SetFullscreen(bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        // Lưu 1 là True, 0 là False
        PlayerPrefs.SetInt("Fullscreen", isFullscreen ? 1 : 0);
    }
    public void CloseOptionsPanel()
    {
        optionsPanel.SetActive(false);
    }
    // --- LOAD DỮ LIỆU ---
    private void LoadSettings()
    {
        // Load Volume (Mặc định là 1 - Max volume)
        float masterVol = PlayerPrefs.GetFloat("MasterVol", 1f);
        float musicVol = PlayerPrefs.GetFloat("MusicVol", 1f);
        float sfxVol = PlayerPrefs.GetFloat("SFXVol", 1f);

        // Update UI Slider
        if (masterSlider) masterSlider.value = masterVol;
        if (musicSlider) musicSlider.value = musicVol;
        if (sfxSlider) sfxSlider.value = sfxVol;

        // Apply vào Mixer ngay lập tức
        SetMasterVolume(masterVol);
        SetMusicVolume(musicVol);
        SetSFXVolume(sfxVol);

        // Load Fullscreen (Mặc định là true)
        bool isFullscreen = PlayerPrefs.GetInt("Fullscreen", 1) == 1;
        if (fullscreenToggle) fullscreenToggle.isOn = isFullscreen;
        Screen.fullScreen = isFullscreen;
    }

}