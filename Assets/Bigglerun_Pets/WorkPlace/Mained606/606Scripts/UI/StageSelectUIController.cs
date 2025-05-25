using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

/// <summary>
/// 스테이지 선택 화면 UI 컨트롤러
/// </summary>
public class StageSelectUIController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI totalStarsText;
    [SerializeField] private StageInfoUI stageInfoUI;
    
    // [Header("스테이지 버튼")]
    // [SerializeField] private List<StageButtonUI> stageButtons = new List<StageButtonUI>();
    
    [Header("설정")]
    [SerializeField] private float initializationDelay = 0.2f;
    [SerializeField] private bool autoRefreshOnStart = true;
    
    // 초기화 완료 여부
    private bool isInitialized = false;
    
    private void Awake()
    {
        // StageInfoUI 참조가 없는 경우 자동 찾기
        if (stageInfoUI == null)
        {
            stageInfoUI = FindStageInfoUI();
        }
        
        // StageInfoUI가 있으면 처음에는 비활성화
        if (stageInfoUI != null && stageInfoUI.gameObject.activeSelf)
        {
            Debug.Log("[StageSelectUIController] StageInfoUI를 비활성화합니다 - 버튼 클릭 시에만 표시되도록 설정");
            stageInfoUI.gameObject.SetActive(false);
        }
        
        // 뒤로가기 버튼 이벤트 설정
        if (backButton != null)
            backButton.onClick.AddListener(OnBackButtonClicked);
            
        // PlayerDataManager의 OnDataLoaded 이벤트 구독
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataLoaded += OnPlayerDataLoaded;
        }
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataLoaded -= OnPlayerDataLoaded;
        }
    }
    
    /// <summary>
    /// 스테이지 버튼 초기화 및 이벤트 설정
    /// </summary>
    private void Start()
    {
        if (autoRefreshOnStart)
        {
            // 약간의 지연 후 초기화 진행 (데이터 로드 대기 위함)
            StartCoroutine(InitializeWithDelay());
        }
    }
    
    /// <summary>
    /// 지연 후 초기화 진행
    /// </summary>
    private IEnumerator InitializeWithDelay()
    {
        yield return new WaitForSeconds(initializationDelay);
        
        // 데이터 로드 확인
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError("[StageSelectUIController] PlayerDataManager.Instance가 null입니다.");
            yield break;
        }
        
        if (!PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning("[StageSelectUIController] 플레이어 데이터가 로드되지 않았습니다. 데이터 로드를 시도합니다.");
            
            // Firebase에서 사용자 ID 가져오기
            if (FirebaseManager.Instance != null && FirebaseManager.Instance.IsAuthenticated)
            {
                string userId = FirebaseManager.Instance.UserId;
                
                // await 대신 콜백 방식으로 변경
                StartCoroutine(LoadPlayerDataRoutine(userId));
            }
            else
            {
                Debug.LogError("[StageSelectUIController] Firebase 로그인 상태 확인 필요");
                yield break;
            }
        }
        else
        {
            // Dictionary 초기화 확인
            EnsureDictionariesInitialized();
            
            // 데이터가 이미 로드된 경우 바로 초기화 진행
            InitializeUI();
        }
    }
    
    /// <summary>
    /// Dictionary 초기화 확인 및 재초기화
    /// </summary>
    private void EnsureDictionariesInitialized()
    {
        if (PlayerDataManager.Instance != null && 
            PlayerDataManager.Instance.IsDataLoaded && 
            PlayerDataManager.Instance.CurrentPlayerData != null)
        {
            var playerData = PlayerDataManager.Instance.CurrentPlayerData;
            
            // storyStages Dictionary 초기화 확인
            if (playerData.storyStages == null && playerData.storyStagesList != null && playerData.storyStagesList.Count > 0)
            {
                Debug.Log("[StageSelectUIController] 스테이지 데이터 Dictionary 재초기화");
                playerData.InitializeStagesFromList();
            }
            
            // 스테이지 데이터가 비어있는 경우 스테이지 1을 기본값으로 추가
            if (playerData.storyStages == null || playerData.storyStages.Count == 0)
            {
                Debug.Log("[StageSelectUIController] 스테이지 데이터가 비어있어 기본값 생성");
                playerData.storyStages = new Dictionary<string, StageData> {
                    { "1", new StageData { stageId = "1", stars = 0, highScore = 0, isUnlocked = true } }
                };
                playerData.UpdateListFromDictionary();
            }
        }
    }
    
    /// <summary>
    /// 플레이어 데이터 로드를 위한 별도 코루틴
    /// </summary>
    private IEnumerator LoadPlayerDataRoutine(string userId)
    {
        bool loadCompleted = false;
        bool loadSuccess = false;
        
        // 비동기 작업 시작
        var task = PlayerDataManager.Instance.LoadPlayerDataAsync(userId);
        
        // 작업이 완료될 때까지 대기
        while (!loadCompleted)
        {
            if (task.IsCompleted)
            {
                loadCompleted = true;
                loadSuccess = task.Result;
            }
            yield return null;
        }
        
        if (!loadSuccess)
        {
            Debug.LogError("[StageSelectUIController] 플레이어 데이터 로드 실패");
            // TODO: 오류 메시지 표시
            yield break;
        }
        
        // Dictionary 초기화 확인
        EnsureDictionariesInitialized();
        
        // 성공적으로 로드되었으면 UI 초기화
        InitializeUI();
    }
    
    /// <summary>
    /// UI 초기화 (스테이지 버튼 및 총 별 개수)
    /// </summary>
    private void InitializeUI()
    {
        // 총 별 개수 표시
        UpdateTotalStars();
        
        // 스테이지 버튼 초기화
        // InitializeStageButtons();
        
        isInitialized = true;
        Debug.Log("[StageSelectUIController] UI 초기화 완료");
    }
    
    /// <summary>
    /// StageInfoUI 컴포넌트 찾기
    /// </summary>
    private StageInfoUI FindStageInfoUI()
    {
        // 1. FindObjectOfType으로 찾기 시도
        StageInfoUI infoUI = FindFirstObjectByType<StageInfoUI>();
        if (infoUI != null)
        {
            return infoUI;
        }
        
        // 2. 이름으로 게임오브젝트 찾기
        GameObject infoUIObj = GameObject.Find("StageInfoUI");
        if (infoUIObj != null)
        {
            infoUI = infoUIObj.GetComponent<StageInfoUI>();
            if (infoUI != null)
            {
                return infoUI;
            }
        }
        
        // 3. Canvas 하위에서 찾기 시도 (성능 개선)
        Canvas[] canvases = FindObjectsByType<Canvas>(FindObjectsSortMode.None);
        foreach (Canvas canvas in canvases)
        {
            // Canvas의 직접적인 자식들 중에서 찾기
            for (int i = 0; i < canvas.transform.childCount; i++)
            {
                Transform child = canvas.transform.GetChild(i);
                if (child.name.Contains("StageInfo"))
                {
                    infoUI = child.GetComponent<StageInfoUI>();
                    if (infoUI != null)
                    {
                        return infoUI;
                    }
                }
                
                // 자식의 자식 중에서 찾기
                for (int j = 0; j < child.childCount; j++)
                {
                    Transform grandchild = child.GetChild(j);
                    if (grandchild.name.Contains("StageInfo"))
                    {
                        infoUI = grandchild.GetComponent<StageInfoUI>();
                        if (infoUI != null)
                        {
                            return infoUI;
                        }
                    }
                }
            }
        }
        
        Debug.LogWarning("[StageSelectUIController] StageInfoUI를 찾을 수 없습니다!");
        return null;
    }
    
    /// <summary>
    /// 스테이지 버튼 초기화
    /// </summary>
    private void InitializeStageButtons()
    {
        // 플레이어 데이터가 없으면 초기화 중단
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogError("[StageSelectUIController] 플레이어 데이터 없이 스테이지 버튼을 초기화할 수 없습니다.");
            return;
        }
        
        // for (int i = 0; i < stageButtons.Count; i++)
        // {
        //     StageButtonUI button = stageButtons[i];
        //     if (button != null)
        //     {
        //         // 스테이지 번호는 1부터 시작
        //         string stageId = (i + 1).ToString();
        //         button.Initialize(stageId, OnStageButtonClicked);
        //     }
        // }
    }
    
    /// <summary>
    /// 총 별 개수 업데이트
    /// </summary>
    private void UpdateTotalStars()
    {
        if (totalStarsText != null && PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            totalStarsText.text = $"Total Stars: {PlayerDataManager.Instance.CurrentPlayerData.totalStars}";
        }
        else if (totalStarsText != null)
        {
            totalStarsText.text = "Total Stars: 0";
        }
    }
    
    /// <summary>
    /// 스테이지 버튼 클릭 이벤트
    /// </summary>
    private void OnStageButtonClicked(string stageId)
    {
        Debug.Log($"[StageSelectUIController] 스테이지 {stageId} 선택됨");
        
        // 스테이지 정보 UI가 없으면 자동으로 찾기
        if (stageInfoUI == null)
        {
            stageInfoUI = FindStageInfoUI();
        }
        
        // 스테이지 정보 UI 표시
        if (stageInfoUI != null)
        {
            Debug.Log($"[StageSelectUIController] StageInfoUI.ShowStageInfo({stageId}) 호출");
            stageInfoUI.ShowStageInfo(stageId);
        }
        else
        {
            Debug.LogError("[StageSelectUIController] StageInfoUI 참조를 찾을 수 없습니다. 인스펙터에서 참조를 확인하세요.");
        }
    }
    
    /// <summary>
    /// 뒤로가기 버튼 클릭 이벤트
    /// </summary>
    private void OnBackButtonClicked()
    {
        Debug.Log("[StageSelectUIController] 뒤로가기 버튼 클릭");
        
        // 로비로 돌아가기
        GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
    }
    
    /// <summary>
    /// 모든 스테이지 버튼 데이터 새로고침
    /// </summary>
    public void RefreshAllStageButtons()
    {
        // 초기화가 안된 상태라면 초기화 먼저 진행
        if (!isInitialized)
        {
            InitializeUI();
            return;
        }
        
        // 플레이어 데이터 확인
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning("[StageSelectUIController] 플레이어 데이터가 로드되지 않아 새로고침을 건너뜁니다.");
            return;
        }
        
        // // 각 버튼 새로고침
        // foreach (StageButtonUI button in stageButtons)
        // {
        //     if (button != null)
        //         button.RefreshData();
        // }
        
        UpdateTotalStars();
        Debug.Log("[StageSelectUIController] 모든 스테이지 버튼 새로고침 완료");
    }
    
    // 플레이어 데이터 로드 완료 시 호출될 이벤트 핸들러
    private void OnPlayerDataLoaded()
    {
        Debug.Log("[StageSelectUIController] 플레이어 데이터 로드 완료 이벤트 수신");
        
        // Dictionary 초기화 확인
        EnsureDictionariesInitialized();
        
        // UI 초기화 또는 새로고침
        if (!isInitialized)
        {
            InitializeUI();
        }
        else
        {
            RefreshAllStageButtons();
        }
    }
} 