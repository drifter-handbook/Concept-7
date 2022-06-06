using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsMenu : MonoBehaviour
{
    public Slider MasterVolumeSlider;
    public Slider MusicVolumeSlider;
    public Slider SFXVolumeSlider;

    public AudioMixer AudioMixer;

    public AudioSource testSFX; //plays when user adjusts sfx

    public float MaxVolume = 1; //as defined in the audio mixer
    public float MinVolume = -20;

    void Start()
    {
        if(PlayerPrefs.HasKey("masterVolume")){
            MasterVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("masterVolume"));
        }
        if(PlayerPrefs.HasKey("musicVolume")){
            MusicVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("musicVolume"));
        }
        if(PlayerPrefs.HasKey("sfxVolume")){
            SFXVolumeSlider.SetValueWithoutNotify(PlayerPrefs.GetFloat("sfxVolume"));
        }
        UpdateVolume();
    }


    public void SetVolume(int type){
        switch(type){
            case 0:{
                PlayerPrefs.SetFloat("masterVolume", GetSliderPercent(MasterVolumeSlider)); break;
            } 
            case 1: PlayerPrefs.SetFloat("musicVolume", GetSliderPercent(MusicVolumeSlider)); break;
            case 2:{
                PlayerPrefs.SetFloat("sfxVolume", GetSliderPercent(SFXVolumeSlider));
                if(sfxCooldown <= 0){
                    testSFX.Play();
                    sfxCooldown = 0.3f;
                } 
                break;
            } 
            default: break;
        }
        UpdateVolume();
    }

    public float GetClampedVolume(float percent){
        return (percent*-MinVolume)+MinVolume;
    }


    float sfxCooldown = 0;
    public void UpdateVolume(){
        if(PlayerPrefs.HasKey("masterVolume")){
            AudioMixer.SetFloat("MasterVolume", GetClampedVolume(PlayerPrefs.GetFloat("masterVolume")));
            if(PlayerPrefs.GetFloat("masterVolume") == 0){
                 AudioMixer.SetFloat("MasterVolume", -80);
            }
        }
        if(PlayerPrefs.HasKey("musicVolume")){
            AudioMixer.SetFloat("MusicVolume", GetClampedVolume(PlayerPrefs.GetFloat("musicVolume")));
            if(PlayerPrefs.GetFloat("musicVolume") == 0){
                 AudioMixer.SetFloat("MusicVolume", -80);
            }
        }
        if(PlayerPrefs.HasKey("sfxVolume")){
            AudioMixer.SetFloat("SFXVolume", GetClampedVolume(PlayerPrefs.GetFloat("sfxVolume")));
            if(PlayerPrefs.GetFloat("sfxVolume") == 0){
                 AudioMixer.SetFloat("SFXVolume", -80);
            }
        }
    }

    public void Update(){
        if(sfxCooldown > 0){
            sfxCooldown -= Time.deltaTime;
        }
    }

    public float GetSliderPercent(Slider slider){
        return (slider.value - slider.minValue)/slider.maxValue;
    }

}
