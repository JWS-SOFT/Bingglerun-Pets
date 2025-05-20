using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

/// <summary>
/// 스테이지 버튼 UI 컴포넌트
/// </summary>
public class StageButtonUI : MonoBehaviour
{
    [Header("필수 컴포넌트")]
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI stageNumberText;
    
    [Header("잠금 관련")]
    [SerializeField] private GameObject lockIcon;
    
    [Header("별 아이콘")]
    [SerializeField] private GameObject[] starIcons; // 최대 3개의 별 아이콘
    
    // 스테이지 ID
    [HideInInspector] public string stageId;
    
    // 스테이지 데이터
    private bool isUnlocked;
    private int stars;
    private int highScore;
    
    // 이벤트 콜백
    private System.Action<string> onClickCallback;
    
    private void Awake()
    {
        Debug.Log($"[StageButtonUI] Awake 호출됨");
        
        // 버튼 컴포넌트 없으면 자동 찾기
        if (button == null)
            button = GetComponent<Button>();
            
        // 텍스트 컴포넌트 없으면 간소화된 방법으로 찾기
        if (stageNumberText == null)
        {
            // 방법 1: 직접 자식에서 찾기
            stageNumberText = GetComponentInChildren<TextMeshProUGUI>(true);
            
            // 방법 2: 특정 이름 패턴으로 찾기
            if (stageNumberText == null)
            {
                Transform[] allChildren = GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name.Contains("StageName") || child.name.Contains("Number") || 
                        child.name.Contains("Text"))
                    {
                        TextMeshProUGUI textComp = child.GetComponent<TextMeshProUGUI>();
                        if (textComp != null)
                        {
                            stageNumberText = textComp;
                            Debug.Log($"[StageButtonUI] 텍스트 컴포넌트 찾음: {child.name}");
                            break;
                        }
                    }
                }
            }
            
            if (stageNumberText != null)
                Debug.Log($"[StageButtonUI] 텍스트 컴포넌트 자동 찾기 성공");
            else
                Debug.LogWarning("[StageButtonUI] 텍스트 컴포넌트를 찾을 수 없습니다!");
        }
        
        button.onClick.AddListener(OnButtonClick);
    }
    
    /// <summary>
    /// 스테이지 버튼 초기화
    /// </summary>
    public void Initialize(string id, System.Action<string> callback)
    {
        stageId = id;
        onClickCallback = callback;
        
        Debug.Log($"[StageButtonUI] 스테이지 {id} 초기화 시작");
        
        // 스테이지 텍스트 설정
        UpdateStageText();
            
        // 파이어베이스에서 데이터 로드
        LoadStageData();
    }
    
    /// <summary>
    /// 스테이지 텍스트 업데이트
    /// </summary>
    private void UpdateStageText()
    {
        if (stageNumberText != null)
        {
            // 스테이지 ID 형식으로 텍스트 생성
            string displayText = $"Stage {stageId}";
            stageNumberText.text = displayText;
            
            // 텍스트 속성 설정
            stageNumberText.enableAutoSizing = true;
            stageNumberText.fontSizeMin = 12;
            stageNumberText.fontSizeMax = 36;
            stageNumberText.color = Color.black;
            
            Debug.Log($"[StageButtonUI] 스테이지 {stageId}의 텍스트를 '{displayText}'로 설정함");
        }
        else
        {
            Debug.LogWarning($"[StageButtonUI] 스테이지 {stageId}의 텍스트 컴포넌트가 연결되지 않았습니다!");
            
            // 컴포넌트 자동 검색 시도
            stageNumberText = GetComponentInChildren<TextMeshProUGUI>();
            if (stageNumberText != null)
            {
                string displayText = $"Stage {stageId}";
                stageNumberText.text = displayText;
                stageNumberText.color = Color.black;
                Debug.Log($"[StageButtonUI] 스테이지 {stageId}의 텍스트 컴포넌트를 자동으로 찾아 설정함");
            }
        }
    }
    
    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClick()
    {
        if (!isUnlocked)
        {
            Debug.Log($"스테이지 {stageId}가 잠겨있습니다.");
            return;
        }
        
        Debug.Log($"[StageButtonUI] 스테이지 {stageId} 버튼 클릭됨");
        onClickCallback?.Invoke(stageId);
    }
    
    /// <summary>
    /// 스테이지 데이터 로드
    /// </summary>
    private void LoadStageData()
    {
        Debug.Log($"[StageButtonUI] 스테이지 {stageId} 데이터 로드 시작");
        
        // PlayerDataManager 및 데이터 로드 상태 확인
        if (PlayerDataManager.Instance == null)
        {
            Debug.LogError($"[StageButtonUI] 스테이지 {stageId} 데이터 로드 실패: PlayerDataManager.Instance가 null입니다.");
            isUnlocked = stageId == "1"; // 첫 스테이지는 기본 해금
            stars = 0;
            highScore = 0;
            UpdateUI();
            return;
        }
            
        if (!PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning($"[StageButtonUI] 스테이지 {stageId} 데이터 로드 실패: 플레이어 데이터가 로드되지 않았습니다.");
            isUnlocked = stageId == "1"; // 첫 스테이지는 기본 해금
            stars = 0;
            highScore = 0;
            UpdateUI();
            return;
        }
        
        try
        {
            // 스테이지 데이터 Dictionary 확인
            if (PlayerDataManager.Instance.CurrentPlayerData.storyStages == null)
            {
                Debug.LogWarning($"[StageButtonUI] 스테이지 {stageId} 데이터 로드 실패: storyStages Dictionary가 null입니다.");
                
                // Dictionary 재초기화 시도
                if (PlayerDataManager.Instance.CurrentPlayerData.storyStagesList != null && 
                    PlayerDataManager.Instance.CurrentPlayerData.storyStagesList.Count > 0)
                {
                    Debug.Log($"[StageButtonUI] storyStages Dictionary 재초기화 시도");
                    PlayerDataManager.Instance.CurrentPlayerData.InitializeStagesFromList();
                }
                else
                {
                    Debug.LogWarning($"[StageButtonUI] 스테이지 리스트도 비어있어 재초기화할 수 없습니다.");
                }
            }
                
            StageData stageData = PlayerDataManager.Instance.GetStageData(stageId);
            
            Debug.Log($"[StageButtonUI] 스테이지 {stageId} GetStageData 결과: {(stageData != null ? "데이터 있음" : "데이터 없음")}");
            
            if (stageData != null)
            {
                isUnlocked = stageData.isUnlocked;
                stars = stageData.stars;
                highScore = stageData.highScore;
                Debug.Log($"[StageButtonUI] 스테이지 {stageId} 데이터 로드 완료 - 잠금 상태: {isUnlocked}, 별 개수: {stars}, 최고점수: {highScore}");
            }
            else if (stageId == "1") // 첫 스테이지는 기본적으로 해금
            {
                isUnlocked = true;
                stars = 0;
                highScore = 0;
                Debug.Log($"[StageButtonUI] 첫 번째 스테이지는 기본적으로 해금 상태로 설정합니다.");
                
                // 스테이지 1 데이터 생성 및 저장
                if (PlayerDataManager.Instance.CurrentPlayerData.storyStages != null)
                {
                    if (!PlayerDataManager.Instance.CurrentPlayerData.storyStages.ContainsKey("1"))
                    {
                        Debug.Log($"[StageButtonUI] 스테이지 1 데이터가 없어 생성합니다.");
                        PlayerDataManager.Instance.UnlockStage("1");
                    }
                }
            }
            else
            {
                // 이전 스테이지가 클리어되었으면 현재 스테이지 해금
                int prevStageNum = int.Parse(stageId) - 1;
                StageData prevStageData = PlayerDataManager.Instance.GetStageData(prevStageNum.ToString());
                
                Debug.Log($"[StageButtonUI] 이전 스테이지 {prevStageNum} 데이터: {(prevStageData != null ? $"별 {prevStageData.stars}개" : "데이터 없음")}");
                
                if (prevStageData != null && prevStageData.stars > 0)
                {
                    isUnlocked = true;
                    Debug.Log($"[StageButtonUI] 이전 스테이지가 클리어되어({prevStageData.stars}★) 스테이지 {stageId}를 해금합니다.");
                    PlayerDataManager.Instance.UnlockStage(stageId);
                }
                else
                {
                    isUnlocked = false;
                    Debug.Log($"[StageButtonUI] 이전 스테이지가 클리어되지 않아 스테이지 {stageId}는 잠금 상태입니다.");
                }
                
                stars = 0;
                highScore = 0;
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[StageButtonUI] 스테이지 {stageId} 데이터 로드 중 오류 발생: {ex.Message}");
            
            // 오류 발생 시 기본값 설정
            isUnlocked = stageId == "1"; // 첫 스테이지는 기본 해금
            stars = 0;
            highScore = 0;
        }
        
        UpdateUI();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        Debug.Log($"[StageButtonUI] 스테이지 {stageId} UI 업데이트 시작 - 잠금 상태: {isUnlocked}, 별 개수: {stars}");
        
        // 텍스트 업데이트
        UpdateStageText();
        
        // 잠금 아이콘 업데이트
        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);
        
        // 별 아이콘 업데이트
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].SetActive(i < stars);
                }
            }
        }
        
        // 버튼 인터랙션 업데이트
        button.interactable = isUnlocked;
    }
    
    /// <summary>
    /// 데이터 갱신
    /// </summary>
    public void RefreshData()
    {
        Debug.Log($"[StageButtonUI] 스테이지 {stageId} 데이터 새로고침 시작");
        
        // 데이터가 로드되지 않았거나 로딩 중이면 무시
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded || PlayerDataManager.Instance.IsLoading)
        {
            Debug.LogWarning($"[StageButtonUI] 스테이지 {stageId} 데이터 새로고침 실패: 데이터가 로드되지 않았거나 로딩 중입니다.");
            return;
        }
        
        // 파이어베이스에서 최신 데이터 로드
        LoadStageData();
        
        // UI가 제대로 갱신되었는지 확인 로그
        Debug.Log($"[StageButtonUI] 스테이지 {stageId} 데이터 새로고침 완료 - 별 개수: {stars}, 잠금 상태: {isUnlocked}");
    }
} 