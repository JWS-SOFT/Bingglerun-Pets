using System;
using UnityEngine;

/// <summary>
/// 게임 전반을 초기화하고 상태 머신 및 각 매니저를 연결하는 중심 클래스
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameStateMachine StateMachine { get; private set; }
    public SceneFader SceneFader { get; private set; }
    [HideInInspector] public int currentPlayStage = 1;

    [SerializeField] private String LobbySceneName;
    [SerializeField] private String GameSceneName;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(transform.root.gameObject);
        
        InitializeManagers();
    }

    /// <summary>
    /// 게임 시작 시 호출되는 초기 매니저 연결
    /// </summary>
    private void InitializeManagers()
    {
        // 상태 머신
        StateMachine = gameObject.AddComponent<GameStateMachine>();

        // 씬 페이더
        SceneFader = gameObject.AddComponent<SceneFader>();

        // 핵심 매니저 연결
        gameObject.AddComponent<UIManager>();
        
        // AudioManager 확인 및 처리
        AudioManager existingAudioManager = GetComponentInChildren<AudioManager>();
        if (existingAudioManager == null)
        {
            // 오디오 매니저가 없는 경우에만 추가
            gameObject.AddComponent<AudioManager>();
        }
        else
        {
            // 이미 존재하는 오디오 매니저가 있으므로 사용
            Debug.Log("[GameManager] 기존 AudioManager를 사용합니다.");
        }
        
        gameObject.AddComponent<InputManager>();
        gameObject.AddComponent<FirebaseManager>(); // 이건 나중에 초기화 시도

        // 데이터 관련 매니저 추가
        gameObject.AddComponent<FirebaseDatabase>();
        gameObject.AddComponent<PlayerDataManager>();

        // 게임 기능 매니저들
        gameObject.AddComponent<LeaderboardManager>(); // 리더보드 매니저 추가
        gameObject.AddComponent<ItemManager>();
        gameObject.AddComponent<ShopManager>();
        gameObject.AddComponent<ScoreManager>();
        gameObject.AddComponent<HeartSystem>(); // 하트 시스템 추가
    }

    private void Start()
    {
        // 초기 상태로 진입
        StateMachine.ChangeState(GameState.Init);
    }

    /// <summary>
    /// 파이어베이스 인증을 초기화하고, 성공 시 타이틀로 전환
    /// </summary>
    public async void InitializeFirebase()
    {
        var firebase = FirebaseManager.Instance;

        bool success = await firebase.InitializeAndLoginAsync();

        if (success)
            StateMachine.ChangeState(GameState.Title);
        else
        {
            Debug.LogError("[GameManager] Firebase 로그인 실패 계정 정보 없음");
            // 로그인 실패해도 타이틀 상태로 전환
            StateMachine.ChangeState(GameState.Title);
        }
    }
    
    /// <summary>
    /// 플레이어 데이터를 로드하고 로비 씬으로 전환
    /// </summary>
    public async void LoadPlayerDataAndGoToLobby()
    {
        var firebase = FirebaseManager.Instance;
        
        if (!firebase.IsAuthenticated)
        {
            Debug.LogError("[GameManager] 로그인되지 않은 상태로 로비 전환 시도");
            return;
        }
        
        UIManager.Instance.ShowLoadingScreen(true, "Loading data...");
        
        bool success = await PlayerDataManager.Instance.LoadPlayerDataAsync(firebase.UserId);
        
        UIManager.Instance.ShowLoadingScreen(false);
        
        if (success)
        {
            Debug.Log("[GameManager] 플레이어 데이터 로드 성공, 로비로 전환합니다.");
            SceneFader.LoadScene(LobbySceneName);
            StateMachine.ChangeState(GameState.Lobby);
        }
        else
        {
            Debug.LogError("[GameManager] 플레이어 데이터 로드 실패");
            // 오류 메시지 표시 또는 재시도 로직 추가
        }
    }
    
    /// <summary>
    /// 게임 씬을 로드하고 게임 상태로 전환
    /// </summary>
    public void LoadGameScene(string stageId, bool isCompetitionMode = false)
    {
        if (string.IsNullOrEmpty(stageId))
        {
            Debug.LogError("[GameManager] 스테이지 ID가 없습니다.");
            return;
        }
        
        // 하트 소모 체크
        if (!TryConsumeHeartForGame())
        {
            Debug.LogWarning("[GameManager] 하트가 부족하여 게임을 시작할 수 없습니다.");
            // UI에서 하트 부족 알림 표시
            if (UIManager.Instance != null)
            {
                // 하트 부족 팝업 표시 (구현 필요)
                ShowInsufficientHeartsPopup();
            }
            return;
        }
        
        // 게임 데이터 저장
        GameDataManager.SetSelectedStageId(stageId);
        
        // 게임 씬 로드
        Debug.Log($"[GameManager] 게임 씬 로드 시작: 스테이지 {stageId}, 경쟁 모드: {isCompetitionMode}");
        SceneFader.LoadScene(GameSceneName);
        
        // 모드에 따라 다른 게임 상태로 전환
        if (isCompetitionMode)
        {
            StateMachine.ChangeState(GameState.CompetitionInGame);
        }
        else
        {
            StateMachine.ChangeState(GameState.StoryInGame);
        }
    }
    
    /// <summary>
    /// 게임 시작을 위한 하트 소모 시도
    /// </summary>
    private bool TryConsumeHeartForGame()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            return PlayerDataManager.Instance.TrySpendHeart(1);
        }
        
        Debug.LogWarning("[GameManager] PlayerDataManager가 초기화되지 않았습니다.");
        return false;
    }
    
    /// <summary>
    /// 하트 부족 팝업 표시
    /// </summary>
    private void ShowInsufficientHeartsPopup()
    {
        // TODO: 하트 부족 팝업 UI 구현
        // 현재는 로그로만 표시
        Debug.Log("[GameManager] 하트 부족 팝업 표시 (UI 구현 필요)");
        
        // 임시로 간단한 알림 표시
        if (HeartSystem.Instance != null)
        {
            var timeUntilNext = HeartSystem.Instance.GetTimeUntilNextRecovery();
            if (timeUntilNext.TotalSeconds > 0)
            {
                Debug.Log($"[GameManager] 다음 하트 회복까지: {timeUntilNext.Minutes}분 {timeUntilNext.Seconds}초");
            }
        }
    }
}