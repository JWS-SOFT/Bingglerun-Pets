using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private Transform[] mainTab;
    [SerializeField] private List<Slider> volumeSlider;
    [SerializeField] private List<float> volumeValue = new List<float>();

    [SerializeField] private Toggle allMute;

    private void Awake()
    {
        InitValue();
    }

    private void OnEnable()
    {
        SetVolume();
        allMute.isOn = !PlayerDataManager.Instance.CurrentPlayerData.soundEnabled;
    }

    private void InitValue()
    {
        for(int i = 0; i< volumeSlider.Count; i++)
        {
            volumeValue.Add(volumeSlider[i].value);
        }
    }

    public void MainTabSwitch(int index)
    {
        switch (index)
        {
            case 0:
                mainTab[0].gameObject.SetActive(true);
                mainTab[1].gameObject.SetActive(false);
                break;
            case 1:
                mainTab[0].gameObject.SetActive(false);
                mainTab[1].gameObject.SetActive(true);
                break;
        }
    }

    public void ControlVolume(int index)
    {
        //Debug.Log($"인덱스 : {index}번째 볼륨 조절");
        float value = volumeSlider[index].value;
        volumeValue[index] = value;
    }

    public void SetVolume()
    {
        var volumeList = PlayerDataManager.Instance.CurrentPlayerData.volumeList;

        if (volumeList != null && volumeList.Count >= volumeSlider.Count)
        {
            for (int i = 0; i < volumeSlider.Count; i++)
            {
                volumeSlider[i].value = volumeList[i];
                volumeValue[i] = volumeList[i];
                AudioManager.Instance.SetMasterVolume(volumeValue[0]);
                AudioManager.Instance.SetBGMVolume(volumeValue[1]);
                AudioManager.Instance.SetSFXVolume(volumeValue[2]);
            }
        }
        else
        {
            volumeList.Clear();
            for (int i = 0; i < volumeSlider.Count; i++)
            {
                volumeList.Add(1f); // 기본값
                volumeSlider[i].value = 1f;
            }
        }

        AudioManager.Instance.Mute(allMute.isOn);
    }

    public void SettingButton()
    {
        AudioManager.Instance.SetMasterVolume(volumeValue[0]);
        AudioManager.Instance.SetBGMVolume(volumeValue[1]);
        AudioManager.Instance.SetSFXVolume(volumeValue[2]);
        PlayerDataManager.Instance.SetVolume(volumeValue);
    }

    public void AllMute()
    {
        AudioManager.Instance.Mute(allMute.isOn);
        PlayerDataManager.Instance.SetSoundEnabled(!allMute.isOn);
        if (!allMute.isOn)
        {
            SetVolume();
        }
    }
}
