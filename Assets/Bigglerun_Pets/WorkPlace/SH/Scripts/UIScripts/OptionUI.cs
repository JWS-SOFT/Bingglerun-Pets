using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

public class OptionUI : MonoBehaviour
{
    [SerializeField] private Toggle[] mainTabToggles;
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
        Debug.Log("OptionUI OnEnable 호출됨");
        SetVolume();
        allMute.isOn = !PlayerDataManager.Instance.CurrentPlayerData.soundEnabled;

        for (int i = 0; i < mainTabToggles.Length; i++)
        {
            int index = i;  // 클로저 문제 방지
            mainTabToggles[i].onValueChanged.AddListener((isOn) =>
            {
                if (isOn)
                {
                    MainTabSwitch(index);
                }
            });
        }

        // 오디오 매니저의 뮤트 상태 변경 이벤트 구독
        SoundEvents.OnMuteStateChanged += OnMuteStateChanged;
    }
    
    private void OnDisable()
    {
        for (int i = 0; i < mainTabToggles.Length; i++)
        {
            mainTabToggles[i].onValueChanged.RemoveAllListeners();
        }
        // 오디오 매니저의 뮤트 상태 변경 이벤트 구독 해제
        SoundEvents.OnMuteStateChanged -= OnMuteStateChanged;
    }
    
    // 뮤트 상태가 변경되었을 때 호출되는 이벤트 핸들러
    private void OnMuteStateChanged(bool isMuted)
    {
        Debug.Log($"OptionUI: 뮤트 상태 변경됨 - {isMuted}");
        
        // UI 토글 상태 업데이트 (이벤트 루프 방지를 위해 UI 이벤트 발생 없이 직접 값만 설정)
        if (allMute.isOn != isMuted)
        {
            allMute.SetIsOnWithoutNotify(isMuted);
        }
        
        // DB 상태 업데이트
        PlayerDataManager.Instance.SetSoundEnabled(!isMuted);
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
        float value = volumeSlider[index].value;
        volumeValue[index] = value;
        
        // volumeList 확인 및 동기화
        var volumeList = PlayerDataManager.Instance.CurrentPlayerData.volumeList;
        if (volumeList == null)
        {
            volumeList = new List<float>();
            PlayerDataManager.Instance.CurrentPlayerData.volumeList = volumeList;
        }
        
        // volumeList 크기 확장
        while (volumeList.Count <= index)
        {
            volumeList.Add(1f); // 기본값으로 확장
        }
        
        // 변경된 인덱스에 맞는, 해당하는 볼륨만 설정
        switch (index)
        {
            case 0: // 마스터 볼륨
                AudioManager.Instance.SetMasterVolume(value);
                volumeList[0] = value;
                break;
            case 1: // BGM 볼륨
                AudioManager.Instance.SetBGMVolume(value);
                volumeList[1] = value;
                break;
            case 2: // SFX 볼륨
                AudioManager.Instance.SetSFXVolume(value);
                volumeList[2] = value;
                break;
        }
        
        // volumeValue가 정확히 최신 상태인지 확인
        for (int i = 0; i < Mathf.Min(volumeList.Count, volumeValue.Count); i++)
        {
            volumeValue[i] = volumeList[i];
        }
        
        // 변경된 전체 볼륨 리스트 저장
        Debug.Log($"ControlVolume({index}) - 볼륨 변경: {string.Join(", ", volumeList)}");
        PlayerDataManager.Instance.SetVolume(volumeValue);
    }

    public void SetVolume()
    {
        var volumeList = PlayerDataManager.Instance.CurrentPlayerData.volumeList;
        Debug.Log($"SetVolume 호출됨 - volumeList: {(volumeList != null ? string.Join(", ", volumeList) : "null")}, Count: {volumeList?.Count}");

        // volumeList가 비어있거나 null인 경우 초기화
        if (volumeList == null)
        {
            Debug.LogWarning("volumeList가 null입니다. 새 리스트를 생성합니다.");
            volumeList = new List<float>();
            PlayerDataManager.Instance.CurrentPlayerData.volumeList = volumeList;
        }

        // volumeList의 크기가 volumeSlider.Count보다 작으면 필요한 만큼 확장
        while (volumeList.Count < volumeSlider.Count)
        {
            // DB에 저장된 볼륨 값이 부족한 경우, 저장된 마지막 값 또는 기본값(1f) 사용
            float defaultValue = volumeList.Count > 0 ? volumeList[volumeList.Count - 1] : 1f;
            volumeList.Add(defaultValue);
            Debug.Log($"volumeList 확장: 인덱스 {volumeList.Count - 1}에 {defaultValue} 추가");
        }

        // volumeList와 슬라이더 동기화 및 볼륨 설정
        for (int i = 0; i < volumeSlider.Count; i++)
        {
            // 슬라이더에 DB 값 설정
            volumeSlider[i].value = volumeList[i];
            volumeValue[i] = volumeList[i];
            Debug.Log($"Slider[{i}] 값 설정: {volumeValue[i]}");
        }
        
        // 볼륨 값 한 번에 적용
        AudioManager.Instance.SetMasterVolume(volumeValue[0]);
        AudioManager.Instance.SetBGMVolume(volumeValue[1]);
        AudioManager.Instance.SetSFXVolume(volumeValue[2]);
        Debug.Log($"볼륨 적용됨 - Master:{volumeValue[0]}, BGM:{volumeValue[1]}, SFX:{volumeValue[2]}");

        // 저장된 값이 변경되었으면 DB에 업데이트
        if (volumeList.Count != volumeSlider.Count || AnyValueDifferent(volumeList, volumeValue))
        {
            Debug.Log("볼륨 값이 변경되어 DB에 업데이트합니다.");
            PlayerDataManager.Instance.SetVolume(volumeValue);
        }

        AudioManager.Instance.Mute(allMute.isOn);
    }

    // volumeList와 volumeValue의 값이 다른지 확인하는 헬퍼 함수
    private bool AnyValueDifferent(List<float> a, List<float> b)
    {
        if (a.Count != b.Count) return true;
        
        for (int i = 0; i < a.Count; i++)
        {
            if (Math.Abs(a[i] - b[i]) > 0.0001f) return true;
        }
        
        return false;
    }

    public void SettingButton()
    {
        // 현재 볼륨 값 로그 출력 (디버깅용)
        Debug.Log($"볼륨 설정 - Master:{volumeValue[0]}, BGM:{volumeValue[1]}, SFX:{volumeValue[2]}");
        
        // 볼륨 값 적용
        AudioManager.Instance.SetMasterVolume(volumeValue[0]);
        AudioManager.Instance.SetBGMVolume(volumeValue[1]);
        AudioManager.Instance.SetSFXVolume(volumeValue[2]);
        
        // DB에 저장
        PlayerDataManager.Instance.SetVolume(volumeValue);
    }

    public void AllMute()
    {
        AudioManager.Instance.Mute(allMute.isOn);
        
        // 뮤트 해제 시 볼륨 적용
        if (!allMute.isOn)
        {
            SetVolume();
        }
        
        // AudioManager에서 이벤트를 발생시키므로 여기서는 DB 업데이트 코드 제거
        // AudioManager가 OnMuteStateChanged 이벤트를 발생시키면 OnMuteStateChanged 메서드에서 처리
    }
}
