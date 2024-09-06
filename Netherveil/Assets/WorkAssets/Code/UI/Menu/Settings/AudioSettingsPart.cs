using UnityEngine;
using UnityEngine.UI;

public class AudioSettingsPart : MenuPart
{
    [Header("Audio Settings")]
    [SerializeField] private Slider mainVolumeSlider;
    [SerializeField] private Slider SFXVolumeSlider;
    [SerializeField] private Slider AmbienceVolumeSlider;
    [SerializeField] private Slider MusicVolumeSlider;

    private void Start()
    {
        ResetSliders();
    }

    public void ChangeMainVolume(float value)
    {
        AudioManager.Instance.masterVolumeBarValue = value;
    }

    public void ChangeSFXVolume(float value)
    {
        AudioManager.Instance.soundsFXVolumeBarValue = value;
    }

    public void ChangeMusicVolume(float value)
    {
        AudioManager.Instance.musicVolumeBarValue = value;
    }

    public void ChangeAmbienceVolume(float value)
    {
        AudioManager.Instance.ambiencesVolumeBarValue = value;
    }

    public void ResetAllVolumes()
    {
        AudioManager.Instance.masterVolumeBarValue = 0.5f;
        AudioManager.Instance.soundsFXVolumeBarValue = 0.5f;
        AudioManager.Instance.musicVolumeBarValue = 0.5f;
        AudioManager.Instance.ambiencesVolumeBarValue = 0.5f;

        ResetSliders();
    }

    private void ResetSliders()
    {
        mainVolumeSlider.value = AudioManager.Instance.masterVolumeBarValue;
        SFXVolumeSlider.value = AudioManager.Instance.soundsFXVolumeBarValue;
        MusicVolumeSlider.value = AudioManager.Instance.musicVolumeBarValue;
        AmbienceVolumeSlider.value = AudioManager.Instance.ambiencesVolumeBarValue;
    }
}
