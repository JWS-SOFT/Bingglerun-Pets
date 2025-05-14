using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public static class SoundEvents
{
    public static Action<BGMType> OnPlayBGM;
    public static Action OnStopBGM;
    public static Action<SFXType> OnPlaySFX;

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
    InGame_Stairs,  //인게임 계단
    InGame_Run,     //인게임 런
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
        DontDestroyOnLoad(gameObject);

        InitBGMClipMap();
        InitSFXClipMap();
        ApplyVolume();
    }
    #endregion

    #region Events
    private void OnEnable()
    {
        SoundEvents.OnPlayBGM += (type) => PlayBGM(type);
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
        SoundEvents.OnPlayBGM -= (type) => PlayBGM(type);
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

    //전체 음소거(옵션 UI에서 호출)
    public void Mute(bool isMute)
    {
        audioMixer.SetFloat(masterVolumeParam, isMute ? -80f : ToDecibels(masterVolume));
    }

    //전체 볼륨 제어(옵션 UI에서 호출)
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        audioMixer.SetFloat(masterVolumeParam, ToDecibels(masterVolume));
    }

    //BGM 볼륨 제어(옵션 UI에서 호출)
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        audioMixer.SetFloat(bgmVolumeParam, ToDecibels(bgmVolume));
    }

    //SFX 볼륨 제어(옵션 UI에서 호출)
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        audioMixer.SetFloat(sfxVolumeParam, ToDecibels(sfxVolume));
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
        SetMasterVolume(masterVolume);
        SetBGMVolume(bgmVolume);
        SetSFXVolume(sfxVolume);
    }
}