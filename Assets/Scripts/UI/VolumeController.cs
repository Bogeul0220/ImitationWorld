using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class VolumeController : MonoBehaviour
{
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private Slider sfxSlider;

    private void Start()
    {
        if(masterSlider != null)
        {
            masterSlider.value = SoundManager.Instance.GetMasterVolume();
            masterSlider.onValueChanged.AddListener(OnMasterVolumeChanged);
        }
        if(bgmSlider != null)
        {
            bgmSlider.value = SoundManager.Instance.GetBGMVolume();
            bgmSlider.onValueChanged.AddListener(OnBGMVolumeChanged);
        }
        if(sfxSlider != null)
        {
            sfxSlider.value = SoundManager.Instance.GetSFXVolume();
            sfxSlider.onValueChanged.AddListener(OnSFXVolumeChanged);
        }
    }

    private void OnMasterVolumeChanged(float value)
    {
        SoundManager.Instance.SetMasterVolume(value);
    }

    private void OnBGMVolumeChanged(float value)
    {
        SoundManager.Instance.SetBGMVolume(value);
    }

    private void OnSFXVolumeChanged(float value)
    {
        SoundManager.Instance.SetSFXVolume(value);
    }
}
