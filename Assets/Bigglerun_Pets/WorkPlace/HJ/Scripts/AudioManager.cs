using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public static class SoundEvents
{
    public static Action<BGMType> OnPlayBGM;
    public static Action OnStopBGM;
    public static Action<SFXType> OnPlaySFX;
    public static Action<bool> OnMuteStateChanged;

    //public static Action OnJump;
    //public static Action OnLand;
    //public static Action OnItem;
    //public static Action OnClickUI;
    //public static Action OnFailStairs;
    //public static Action OnFailRun;
    //public static Action OnSkillDog;
    //public static Action OnSkillCat;
    //public static Action OnSkillHamster;
    //public static Action OnTakeDamage;
    //public static Action OnGameOver;
    //public static Action OnStageClear;
}

public enum BGMType
{
    Title,          //타이틀
    Lobby,          //로비
    StoryInGame,    //스토리 모드 인게임
    CompetitionInGame, //경쟁 모드 인게임
    Result          //결과
}

public enum SFXType
{
    //계단, 런 공통
    Jump, Land, Item, Click,
    //계단
    Fail_Stairs,
    //런
    Fail_Run, Skill_Dog, Skill_Cat, Skill_Hamster,
    //피해, 결과
    HPDamage,GameOver,StageClear
}

public class AudioManager : MonoBehaviour
{
    [Header("오디오믹서")]
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private string masterVolumeParam = "MasterVolume";
    [SerializeField] private string bgmVolumeParam = "BGMVolume";
    [SerializeField] private string sfxVolumeParam = "SFXVolume";

    [Header("오디오소스")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource[] sfxSources;

    [Header("BGM 클립 테이블 (동일한 인덱스로 매핑)")]
    [SerializeField] private BGMType[] bgmClipKeys;
    [SerializeField] private AudioClip[] bgmClips;

    [Header("SFX 클립 테이블 (동일한 인덱스로 매핑)")]
    [SerializeField] private SFXType[] sfxClipKeys;
    [SerializeField] private AudioClip[] sfxClips;

    private Dictionary<BGMType, AudioClip> bgmClipMap;
    private Dictionary<SFXType, List<AudioClip>> sfxClipMap;
    private int sfxIndex = 0;

    [Header("볼륨 셋팅")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float bgmVolume = 1f;
    [Range(0f, 1f)] public float sfxVolume = 1f;
    [SerializeField] private bool isMuted = false;

    #region Singleton
    public static AudioManager Instance;

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        
        // GameManager의 자식이 아닌 경우에만 DontDestroyOnLoad 적용
        if (transform.parent == null || transform.parent.GetComponent<GameManager>() == null)
        {
            DontDestroyOnLoad(gameObject);
            Debug.Log("[AudioManager] 독립적인 AudioManager - DontDestroyOnLoad 적용");
        }
        else
        {
            Debug.Log("[AudioManager] GameManager 자식 객체로서 AudioManager 초기화");
        }

        InitBGMClipMap();
        InitSFXClipMap();
        ApplyVolume();
    }
    #endregion

    #region Events
    private void OnEnable()
    {
        // 델리게이트에 맞는 메서드 참조를 사용
        SoundEvents.OnPlayBGM += HandlePlayBGM;
        SoundEvents.OnStopBGM += StopBGM;
        SoundEvents.OnPlaySFX += PlaySFX;

        //SoundEvents.OnJump += () => PlaySFX(SFXType.Jump);
        //SoundEvents.OnLand += () => PlaySFX(SFXType.Land);
        //SoundEvents.OnItem += () => PlaySFX(SFXType.Item);
        //SoundEvents.OnClickUI += () => PlaySFX(SFXType.Click);
        //SoundEvents.OnFailStairs += () => PlaySFX(SFXType.Fail_Stairs);
        //SoundEvents.OnFailRun += () => PlaySFX(SFXType.Fail_Run);
        //SoundEvents.OnSkillDog += () => PlaySFX(SFXType.Skill_Dog);
        //SoundEvents.OnSkillCat += () => PlaySFX(SFXType.Skill_Cat);
        //SoundEvents.OnSkillHamster += () => PlaySFX(SFXType.Skill_Hamster);
        //SoundEvents.OnTakeDamage += () => PlaySFX(SFXType.HPDamage);
        //SoundEvents.OnGameOver += () => PlaySFX(SFXType.GameOver);
        //SoundEvents.OnStageClear += () => PlaySFX(SFXType.StageClear);
    }

    private void OnDisable()
    {
        // 델리게이트에 맞는 메서드 참조 해제
        SoundEvents.OnPlayBGM -= HandlePlayBGM;
        SoundEvents.OnStopBGM -= StopBGM;
        SoundEvents.OnPlaySFX -= PlaySFX;

        //SoundEvents.OnJump -= () => PlaySFX(SFXType.Jump);
        //SoundEvents.OnLand -= () => PlaySFX(SFXType.Land);
        //SoundEvents.OnItem -= () => PlaySFX(SFXType.Item);
        //SoundEvents.OnClickUI -= () => PlaySFX(SFXType.Click);
        //SoundEvents.OnFailStairs -= () => PlaySFX(SFXType.Fail_Stairs);
        //SoundEvents.OnFailRun -= () => PlaySFX(SFXType.Fail_Run);
        //SoundEvents.OnSkillDog -= () => PlaySFX(SFXType.Skill_Dog);
        //SoundEvents.OnSkillCat -= () => PlaySFX(SFXType.Skill_Cat);
        //SoundEvents.OnSkillHamster -= () => PlaySFX(SFXType.Skill_Hamster);
        //SoundEvents.OnTakeDamage -= () => PlaySFX(SFXType.HPDamage);
        //SoundEvents.OnGameOver -= () => PlaySFX(SFXType.GameOver);
        //SoundEvents.OnStageClear -= () => PlaySFX(SFXType.StageClear);
    }
    #endregion

    // 이벤트 핸들러 메서드 - Action<BGMType> 델리게이트와 일치하는 시그니처
    private void HandlePlayBGM(BGMType type)
    {
        PlayBGM(type);
    }

    //전체 음소거(옵션 UI에서 호출)
    public void Mute(bool isMute)
    {
        isMuted = isMute;
        audioMixer.SetFloat(masterVolumeParam, isMute ? -80f : ToDecibels(masterVolume));
        
        // UI 및 DB에 뮤트 상태 변경 알림
        SoundEvents.OnMuteStateChanged?.Invoke(isMuted);
    }

    //전체 볼륨 제어(옵션 UI에서 호출)
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        
        bool wasMuted = isMuted;
        if (isMuted && volume > 0)
        {
            isMuted = false;
            
            // 뮤트 상태가 변경되었을 때 UI 및 DB에 알림
            if (wasMuted != isMuted)
            {
                SoundEvents.OnMuteStateChanged?.Invoke(isMuted);
            }
        }
        
        if (!isMuted)
        {
            audioMixer.SetFloat(masterVolumeParam, ToDecibels(masterVolume));
        }
    }

    //BGM 볼륨 제어(옵션 UI에서 호출)
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        
        if (!isMuted)
        {
            audioMixer.SetFloat(bgmVolumeParam, ToDecibels(bgmVolume));
        }
    }

    //SFX 볼륨 제어(옵션 UI에서 호출)
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        
        if (!isMuted)
        {
            audioMixer.SetFloat(sfxVolumeParam, ToDecibels(sfxVolume));
        }
    }

    // 현재 뮤트 상태 반환 (UI에서 사용)
    public bool IsMuted()
    {
        return isMuted;
    }

    //BGM 재생
    public void PlayBGM(BGMType type, bool loop = true)
    {
        if (!bgmClipMap.ContainsKey(type))
        {
            Debug.LogWarning($"AudioManager 재생할 BGM 클립이 없음: {type}");
            return;
        }

        AudioClip clip = bgmClipMap[type];
        if (bgmSource.clip == clip && bgmSource.isPlaying) return;

        bgmSource.clip = clip;
        bgmSource.loop = loop;
        bgmSource.Play();
    }

    //BGM 정지
    public void StopBGM()
    {
        bgmSource.Stop();
        bgmSource.clip = null;
    }

    //SFX 재생
    public void PlaySFX(SFXType type)
    {
        if (!sfxClipMap.ContainsKey(type) || sfxClipMap[type].Count == 0)
        {
            Debug.LogWarning($"AudioManager 재생할 SFX 클립이 없음: {type}");
            return;
        }

        var clipList = sfxClipMap[type];
        var clip = clipList[UnityEngine.Random.Range(0, clipList.Count)];

        var source = sfxSources[sfxIndex];
        source.clip = clip;
        source.Play();

        sfxIndex = (sfxIndex + 1) % sfxSources.Length;
    }

    //오디오클립 매핑 초기화
    private void InitBGMClipMap()
    {
        bgmClipMap = new Dictionary<BGMType, AudioClip>();

        for (int i = 0; i < Mathf.Min(bgmClipKeys.Length, bgmClips.Length); i++)
        {
            if (!bgmClipMap.ContainsKey(bgmClipKeys[i]))
                bgmClipMap.Add(bgmClipKeys[i], bgmClips[i]);
        }
    }

    private void InitSFXClipMap()
    {
        sfxClipMap = new Dictionary<SFXType, List<AudioClip>>();

        for(int i = 0; i < Mathf.Min(sfxClipKeys.Length, sfxClips.Length); i++)
        {
            if (!sfxClipMap.ContainsKey(sfxClipKeys[i]))
                sfxClipMap[sfxClipKeys[i]] = new List<AudioClip>();

            sfxClipMap[sfxClipKeys[i]].Add(sfxClips[i]);
        }
    } 

    //볼륨을 데시벨로 변환(오디오믹서에서 이용)
    private float ToDecibels(float volume)
    {
        return Mathf.Log10(Mathf.Max(volume, 0.0001f)) * 20f;
    }

    //볼륨 적용(초기화)
    private void ApplyVolume()
    {
        if (!isMuted)
        {
            SetMasterVolume(masterVolume);
            SetBGMVolume(bgmVolume);
            SetSFXVolume(sfxVolume);
        }
        else
        {
            audioMixer.SetFloat(masterVolumeParam, -80f);
        }
    }
}