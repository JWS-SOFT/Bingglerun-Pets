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

    private void Start()
    {
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

    public void SetVolume(int index)
    {
        Debug.Log($"인덱스 : {index}번째 볼륨 조절");
        float value = volumeSlider[index].value;
        //volumeValue[index] = value;
    }

    public void SettingButton()
    {
        AudioManager.Instance.masterVolume = volumeValue[0];
        AudioManager.Instance.bgmVolume = volumeValue[1];
        AudioManager.Instance.sfxVolume = volumeValue[2];
        PlayerDataManager.Instance.SetVolume(volumeValue);
    }
}
