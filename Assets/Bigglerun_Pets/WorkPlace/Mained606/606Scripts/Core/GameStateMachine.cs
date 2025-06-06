using UnityEngine;
using System.Threading.Tasks;
using System;

/// <summary>
/// 게임의 전체 상태 전환을 제어하는 클래스
/// </summary>
public class GameStateMachine : MonoBehaviour
{
    // 상태 변경 이벤트
    public static event Action<GameState> OnStateChanged;
    
    [SerializeField] private GameState currentState = GameState.None;

    public GameState CurrentState 
    { 
        get { return currentState; } 
        private set 
        { 
            if (currentState != value)
            {
                currentState = value;
                OnStateChanged?.Invoke(currentState);
            }
        }
    }

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        Debug.Log($"[GameStateMachine] 상태 전환: {CurrentState} → {newState}");

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Init:
                GameManager.Instance.InitializeFirebase();
                break;

            case GameState.Title:
                // 타이틀 상태일 때 계정 정보 확인 및 오디오 설정 동기화 후 BGM 재생
                SyncAudioSettingsBasedOnAccount(BGMType.Title);
                UIManager.Instance.ShowTitleUI();
                
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.StopHeartRecoveryTimer();
                }
                break;

            case GameState.Lobby:
                UIManager.Instance.ShowLobbyUI();
                // 로비 BGM 재생
                PlayBGM(BGMType.Lobby);
                // 로비 상태 - 리더보드 기능 자동 활성화 (IsInLobbyState에서 체크)
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.CheckAndRecoverHearts();
                    HeartSystem.Instance.StartHeartRecoveryTimer();
                }
                break;

            // case GameState.ModeSelect:
            //     UIManager.Instance.ShowModeSelectUI();
            //     break;

            case GameState.StoryStageSelect:
                // 스테이지 선택 진입 시 최신 플레이어 데이터 로드
                if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsAuthenticated && PlayerDataManager.Instance != null)
                {
                    Debug.Log("[GameStateMachine] 스테이지 선택 씬 진입 - 최신 데이터 로드 시작");
                    string userId = FirebaseManager.Instance.UserId;

                    // 데이터 로드 완료 이벤트를 일회성으로 구독
                    System.Action dataLoadedHandler = null;
                    dataLoadedHandler = () => {
                        Debug.Log("[GameStateMachine] 플레이어 데이터 로드 완료 - UI 초기화 시작");
                        UIManager.Instance.ShowStoryStageSelectUI();
                        if (HeartSystem.Instance != null)
                        {
                            HeartSystem.Instance.CheckAndRecoverHearts();
                            HeartSystem.Instance.StartHeartRecoveryTimer();
                        }
                        
                        // 이벤트 구독 해제
                        PlayerDataManager.Instance.OnDataLoaded -= dataLoadedHandler;
                    };
                    
                    // 이벤트 구독 및 데이터 로드 시작
                    PlayerDataManager.Instance.OnDataLoaded += dataLoadedHandler;
                    _ = PlayerDataManager.Instance.LoadPlayerDataAsync(userId);
                }
                else
                {
                    // Firebase 인증이 없는 경우 기본 UI만 표시
                    UIManager.Instance.ShowStoryStageSelectUI();
                    if (HeartSystem.Instance != null)
                    {
                        HeartSystem.Instance.CheckAndRecoverHearts();
                        HeartSystem.Instance.StartHeartRecoveryTimer();
                    }
                }
                
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                break;

            case GameState.CompetitiveSetup:
                UIManager.Instance.ShowCompetitiveSetupUI();
                
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.StopHeartRecoveryTimer();
                }
                break;

            // 스토리 모드 인게임
            case GameState.StoryInGame:
                UIManager.Instance.HideAll();
                // 스토리 모드 인게임 관련 초기화 작업
                PlayBGM(BGMType.StoryInGame);
                
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.StopHeartRecoveryTimer();
                }
                break;

            // 경쟁 모드 인게임
            case GameState.CompetitionInGame:
                UIManager.Instance.HideAll();
                // 경쟁 모드 인게임 관련 초기화 작업
                PlayBGM(BGMType.CompetitionInGame);
                
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.StopHeartRecoveryTimer();
                }
                break;

            case GameState.Result:
                UIManager.Instance.ShowResultUI();
                
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.StopHeartRecoveryTimer();
                }
                break;

            case GameState.Loading:
                // 로딩 처리
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.StopHeartRecoveryTimer();
                }
                break;

            case GameState.Pause:
                UIManager.Instance.ShowPauseMenu();
                
                // 리더보드 캐시 무효화 (로비가 아닌 상태)
                if (LeaderboardManager.Instance != null)
                {
                    LeaderboardManager.Instance.InvalidateCache();
                }
                if (HeartSystem.Instance != null)
                {
                    HeartSystem.Instance.StopHeartRecoveryTimer();
                }
                break;
        }
    }

    private void ExitState(GameState state)
    {
        // 상태 종료 시 필요한 정리 로직
    }
    
    // 계정 정보에 따라 오디오 설정을 동기화하는 메서드
    private async void SyncAudioSettingsBasedOnAccount(BGMType bgmType = BGMType.Title)
    {
        Debug.Log($"[GameStateMachine] 오디오 설정 동기화 시작. BGM 타입: {bgmType}");
        
        // AudioManager 존재 여부 확인
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[GameStateMachine] AudioManager.Instance가 null입니다. 오디오 설정을 적용할 수 없습니다.");
            return;
        }

        // FirebaseManager 존재 여부 확인
        if (FirebaseManager.Instance == null)
        {
            Debug.LogError("[GameStateMachine] FirebaseManager.Instance가 null입니다. 기본 오디오 설정을 적용합니다.");
            ApplyDefaultAudioSettings();
            PlayBGM(bgmType);
            return;
        }
        
        // 인증된 사용자가 있는지 확인
        if (FirebaseManager.Instance.IsAuthenticated)
        {
            Debug.Log("[GameStateMachine] 계정 정보가 있습니다. 오디오 설정을 동기화합니다.");
            
            // PlayerDataManager 존재 여부 확인
            if (PlayerDataManager.Instance == null)
            {
                Debug.LogError("[GameStateMachine] PlayerDataManager.Instance가 null입니다. 기본 오디오 설정을 적용합니다.");
                ApplyDefaultAudioSettings();
                PlayBGM(bgmType);
                return;
            }
            
            // 플레이어 데이터가 로드되었는지 확인
            if (PlayerDataManager.Instance.IsDataLoaded && PlayerDataManager.Instance.CurrentPlayerData != null)
            {
                // 플레이어 데이터에서 오디오 설정 적용
                SyncAudioWithUserData(PlayerDataManager.Instance.CurrentPlayerData);
            }
            else
            {
                // 데이터 로드 후 오디오 설정 적용
                string userId = FirebaseManager.Instance.UserId;
                Debug.Log($"[GameStateMachine] 플레이어 데이터 로드 시도. 유저 ID: {userId}");
                
                bool success = await PlayerDataManager.Instance.LoadPlayerDataAsync(userId);
                
                if (success && PlayerDataManager.Instance.CurrentPlayerData != null)
                {
                    Debug.Log("[GameStateMachine] 플레이어 데이터 로드 성공. 오디오 설정 적용.");
                    SyncAudioWithUserData(PlayerDataManager.Instance.CurrentPlayerData);
                }
                else
                {
                    // 데이터 로드 실패 시 기본 설정 적용
                    Debug.LogWarning("[GameStateMachine] 플레이어 데이터 로드 실패. 기본 설정 적용.");
                    ApplyDefaultAudioSettings();
                }
            }
        }
        else
        {
            Debug.Log("[GameStateMachine] 계정 정보가 없습니다. 기본 오디오 설정을 적용합니다.");
            ApplyDefaultAudioSettings();
        }
        
        // 오디오 설정 적용 완료 후 BGM 재생 (계정 여부와 상관없이 항상 재생)
        Debug.Log($"[GameStateMachine] 오디오 설정 적용 완료. BGM 재생: {bgmType}");
        PlayBGM(bgmType);
    }

    // BGM 재생 메서드 - SoundEvents.OnPlayBGM 또는 직접 AudioManager 호출
    private void PlayBGM(BGMType bgmType)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[GameStateMachine] AudioManager.Instance가 null입니다. BGM을 재생할 수 없습니다.");
            return;
        }

        // SoundEvents.OnPlayBGM이 null이 아닌지 확인
        if (SoundEvents.OnPlayBGM != null)
        {
            SoundEvents.OnPlayBGM.Invoke(bgmType);
        }
        else
        {
            Debug.LogWarning("[GameStateMachine] SoundEvents.OnPlayBGM이 null입니다. 직접 AudioManager.PlayBGM을 호출합니다.");
            // 직접 AudioManager 호출
            AudioManager.Instance.PlayBGM(bgmType);
        }
    }

    // 사용자 데이터로 오디오 설정 동기화
    private void SyncAudioWithUserData(PlayerData playerData)
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[GameStateMachine] AudioManager.Instance가 null입니다. 오디오 설정을 적용할 수 없습니다.");
            return;
        }

        if (playerData.volumeList != null && playerData.volumeList.Count >= 3)
        {
            // 무한 이벤트 루프 방지를 위해 이전 뮤트 상태를 저장
            bool previousMuteState = AudioManager.Instance.IsMuted();
            bool newMuteState = !playerData.soundEnabled;
            
            // 볼륨 리스트: [0]=마스터, [1]=BGM, [2]=SFX
            AudioManager.Instance.SetMasterVolume(playerData.volumeList[0]);
            AudioManager.Instance.SetBGMVolume(playerData.volumeList[1]);
            AudioManager.Instance.SetSFXVolume(playerData.volumeList[2]);
            
            // 사운드 활성화 설정 (뮤트 상태가 변경된 경우에만 적용)
            if (previousMuteState != newMuteState)
            {
                AudioManager.Instance.Mute(newMuteState);
            }
            
            Debug.Log($"[GameStateMachine] 오디오 설정 동기화 완료: 마스터={playerData.volumeList[0]}, BGM={playerData.volumeList[1]}, SFX={playerData.volumeList[2]}, 사운드활성화={playerData.soundEnabled}");
        }
        else
        {
            Debug.LogWarning("[GameStateMachine] 사용자 데이터에 볼륨 설정이 없습니다. 기본 설정을 적용합니다.");
            ApplyDefaultAudioSettings();
        }
    }

    // 기본 오디오 설정 적용
    private void ApplyDefaultAudioSettings()
    {
        if (AudioManager.Instance == null)
        {
            Debug.LogError("[GameStateMachine] AudioManager.Instance가 null입니다. 기본 오디오 설정을 적용할 수 없습니다.");
            return;
        }

        // 기본 볼륨 값
        float defaultVolume = 1.0f;
        
        AudioManager.Instance.SetMasterVolume(defaultVolume);
        AudioManager.Instance.SetBGMVolume(defaultVolume);
        AudioManager.Instance.SetSFXVolume(defaultVolume);
        AudioManager.Instance.Mute(false);
        
        Debug.Log("[GameStateMachine] 기본 오디오 설정 적용 완료");
    }
}