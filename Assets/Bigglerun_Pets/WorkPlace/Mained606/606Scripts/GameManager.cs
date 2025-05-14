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

    [SerializeField] private String LobbySceneName;

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
        // gameObject.AddComponent<AudioManager>();
        // gameObject.AddComponent<InputManager>();
        gameObject.AddComponent<FirebaseManager>(); // 이건 나중에 초기화 시도
        
        // 데이터 관련 매니저 추가
        gameObject.AddComponent<FirebaseDatabase>();
        gameObject.AddComponent<PlayerDataManager>();

        // TODO: 필요 시 다른 매니저도 연결
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
            Debug.LogError("[GameManager] Firebase 로그인 실패 계정 정보 없음");
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
}