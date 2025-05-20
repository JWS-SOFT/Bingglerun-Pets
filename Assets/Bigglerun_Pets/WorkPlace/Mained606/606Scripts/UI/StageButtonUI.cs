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
        
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning($"[StageButtonUI] 스테이지 {stageId} 데이터 로드 실패: PlayerDataManager가 초기화되지 않았거나 데이터가 로드되지 않음");
            return;
        }
            
        StageData stageData = PlayerDataManager.Instance.GetStageData(stageId);
        
        if (stageData != null)
        {
            isUnlocked = stageData.isUnlocked;
            stars = stageData.stars;
            highScore = stageData.highScore;
        }
        else if (stageId == "1") // 첫 스테이지는 기본적으로 해금
        {
            isUnlocked = true;
            stars = 0;
            highScore = 0;
        }
        else
        {
            // 이전 스테이지가 클리어되었으면 현재 스테이지 해금
            int prevStageNum = int.Parse(stageId) - 1;
            StageData prevStageData = PlayerDataManager.Instance.GetStageData(prevStageNum.ToString());
            
            if (prevStageData != null && prevStageData.stars > 0)
            {
                isUnlocked = true;
                PlayerDataManager.Instance.UnlockStage(stageId);
            }
            else
            {
                isUnlocked = false;
            }
            
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
        // 파이어베이스에서 최신 데이터 로드
        LoadStageData();
        
        // UI가 제대로 갱신되었는지 확인 로그
        Debug.Log($"[StageButtonUI] 스테이지 {stageId} 데이터 새로고침 - 별 개수: {stars}, 잠금 상태: {isUnlocked}");
    }
} 