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
            
        // 텍스트 컴포넌트 없으면 정확한 경로로 자동 찾기
        if (stageNumberText == null)
        {
            // 방법 1: 정확한 경로로 찾기 (Viewport > Content > StageName)
            Transform viewport = transform.Find("Viewport");
            if (viewport != null)
            {
                Transform content = viewport.Find("Content");
                if (content != null)
                {
                    Transform stageName = content.Find("StageName");
                    if (stageName != null)
                    {
                        stageNumberText = stageName.GetComponent<TextMeshProUGUI>();
                        if (stageNumberText != null)
                            Debug.Log($"[StageButtonUI] 정확한 경로로 텍스트 컴포넌트를 찾음: {stageName.name}");
                    }
                }
            }
            
            // 방법 2: 계층 구조 전체에서 "StageName" 이름의 오브젝트 찾기
            if (stageNumberText == null)
            {
                Transform[] allChildren = GetComponentsInChildren<Transform>(true); // true: 비활성화된 오브젝트도 포함
                foreach (Transform child in allChildren)
                {
                    if (child.name == "StageName")
                    {
                        TextMeshProUGUI textComp = child.GetComponent<TextMeshProUGUI>();
                        if (textComp != null)
                        {
                            stageNumberText = textComp;
                            Debug.Log($"[StageButtonUI] StageName 오브젝트에서 텍스트 컴포넌트를 찾음: {child.name}");
                            break;
                        }
                    }
                }
            }
            
            // 방법 3: 다양한 이름 패턴으로 찾기
            if (stageNumberText == null)
            {
                Transform[] allChildren = GetComponentsInChildren<Transform>(true);
                foreach (Transform child in allChildren)
                {
                    if (child.name.Contains("Number") || child.name.Contains("number") || 
                        child.name.Contains("StageName") || child.name.Contains("Text"))
                    {
                        TextMeshProUGUI textComp = child.GetComponent<TextMeshProUGUI>();
                        if (textComp != null)
                        {
                            stageNumberText = textComp;
                            Debug.Log($"[StageButtonUI] TextMeshProUGUI 컴포넌트를 이름으로 찾음: {child.name}");
                            break;
                        }
                    }
                }
            }
            
            if (stageNumberText != null)
                Debug.Log($"[StageButtonUI] 텍스트 컴포넌트 자동 찾기 성공: {stageNumberText.gameObject.name}");
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
        
        if (stageNumberText != null)
        {
            // 스테이지 ID만 표시하는 대신 "Stage X" 형식으로 표시
            string displayText = $"Stage {id}";
            stageNumberText.text = displayText;
            
            // 텍스트가 잘 보이도록 텍스트 속성 조정
            stageNumberText.enableAutoSizing = true;
            stageNumberText.fontSizeMin = 12;
            stageNumberText.fontSizeMax = 36;
            stageNumberText.color = Color.black;
            
            Debug.Log($"[StageButtonUI] 스테이지 {id}의 텍스트 설정 완료: '{displayText}', 텍스트 컴포넌트: {stageNumberText.gameObject.name}");
        }
        else
        {
            Debug.LogError($"[StageButtonUI] 스테이지 {id}의 텍스트 컴포넌트가 연결되지 않았습니다!");
            
            // 컴포넌트 자동 검색 시도
            stageNumberText = GetComponentInChildren<TextMeshProUGUI>();
            if (stageNumberText != null)
            {
                // 스테이지 ID만 표시하는 대신 "Stage X" 형식으로 표시
                string displayText = $"Stage {id}";
                stageNumberText.text = displayText;
                stageNumberText.color = Color.black;
                Debug.Log($"[StageButtonUI] 스테이지 {id}의 텍스트 컴포넌트를, 자동으로 찾아 설정함: {stageNumberText.gameObject.name}");
            }
        }
            
        // 파이어베이스에서 데이터 로드
        LoadStageData();
    }
    
    /// <summary>
    /// 버튼 클릭 이벤트
    /// </summary>
    private void OnButtonClick()
    {
        Debug.Log($"[StageButtonUI] 스테이지 {stageId} 버튼 클릭 - isUnlocked: {isUnlocked}");
        
        if (!isUnlocked)
        {
            Debug.Log($"[StageButtonUI] 스테이지 {stageId}가 잠겨있어 이벤트를 처리하지 않습니다.");
            
            // 디버깅용: 잠긴 스테이지도 일시적으로 정보 표시 허용
            Debug.Log($"[StageButtonUI] 디버깅: 잠긴 스테이지도 정보 표시");
            onClickCallback?.Invoke(stageId);
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
        
        // 텍스트 업데이트 확인
        if (stageNumberText != null)
        {
            // 스테이지 ID 형식으로 텍스트 생성
            string expectedText = $"Stage {stageId}";
            
            // 텍스트가 비었거나 예상과 다르면 다시 설정
            if (string.IsNullOrEmpty(stageNumberText.text) || stageNumberText.text != expectedText)
            {
                stageNumberText.text = expectedText;
                
                // 텍스트 속성 다시 확인
                stageNumberText.color = Color.black;
                stageNumberText.enableAutoSizing = true;
                stageNumberText.fontSizeMin = 12;
                stageNumberText.fontSizeMax = 36;
                
                Debug.Log($"[StageButtonUI] 스테이지 {stageId}의 텍스트를 '{expectedText}'로 다시 설정함");
            }
            else
            {
                Debug.Log($"[StageButtonUI] 스테이지 {stageId}의 현재 텍스트: {stageNumberText.text}");
            }
            
            // TextMeshPro 컴포넌트를 사용하는지 확인
            if (stageNumberText.GetType() != typeof(TextMeshProUGUI))
            {
                Debug.LogWarning($"[StageButtonUI] 스테이지 {stageId}가 TextMeshProUGUI 컴포넌트를 사용하지 않음: {stageNumberText.GetType().Name}");
            }
        }
        else
        {
            Debug.LogError($"[StageButtonUI] 스테이지 {stageId}의 텍스트 컴포넌트가 null입니다!");
            
            // 컴포넌트 자동 검색 다시 시도
            Transform stageName = transform.Find("Viewport/Content/StageName");
            if (stageName != null)
            {
                stageNumberText = stageName.GetComponent<TextMeshProUGUI>();
                if (stageNumberText != null)
                {
                    stageNumberText.text = $"Stage {stageId}";
                    Debug.Log($"[StageButtonUI] 스테이지 {stageId}의 텍스트 컴포넌트를 UpdateUI에서 다시 찾음");
                }
            }
        }
        
        // 잠금 아이콘
        if (lockIcon != null)
            lockIcon.SetActive(!isUnlocked);
            
        // 버튼 활성화/비활성화만 설정 (색상 변경 없음)
        if (button != null)
        {
            button.interactable = isUnlocked;
        }
        
        // 별 아이콘
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (i < starIcons.Length)
                {
                    // 새로운 구조 적용
                    Transform starTransform = starIcons[i].transform;
                    
                    // 별 하위 오브젝트 찾기
                    Transform emptyStar = starTransform.Find("EmptyStar");
                    Transform fullStar = starTransform.Find("FullStar");
                    Transform starOutline = starTransform.Find("StarOutline");
                    
                    // 별 획득 여부에 따라 하위 오브젝트 활성화/비활성화
                    bool isStarEarned = (i < stars);
                    
                    // EmptyStar와 FullStar가 모두 있는 경우
                    if (emptyStar != null && fullStar != null)
                    {
                        emptyStar.gameObject.SetActive(!isStarEarned);
                        fullStar.gameObject.SetActive(isStarEarned);
                    }
                    // StarOutline이 있는 경우 (잠금 상태 표시용)
                    else if (starOutline != null)
                    {
                        starOutline.gameObject.SetActive(!isUnlocked);
                    }
                    // 하위 구조가 없는 경우 예전 방식으로 처리
                    else
                    {
                        starIcons[i].SetActive(i < stars);
                    }
                }
            }
        }
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