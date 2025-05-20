using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 스테이지 선택 화면 UI 컨트롤러
/// </summary>
public class StageSelectUIController : MonoBehaviour
{
    [Header("UI 참조")]
    [SerializeField] private Button backButton;
    [SerializeField] private TextMeshProUGUI totalStarsText;
    [SerializeField] private StageInfoUI stageInfoUI;
    
    [Header("스테이지 버튼")]
    [SerializeField] private List<StageButtonUI> stageButtons = new List<StageButtonUI>();
    
    /// <summary>
    /// 스테이지 버튼 초기화 및 이벤트 설정
    /// </summary>
    private void Start()
    {
        // 데이터 없으면 로드
        if (PlayerDataManager.Instance != null && !PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning("[StageSelectUIController] 플레이어 데이터가 로드되지 않았습니다.");
            // TODO: 로딩 화면 표시 또는 오류 처리
        }
        
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
            
        // 총 별 개수 표시
        UpdateTotalStars();
        
        // 스테이지 버튼 초기화
        InitializeStageButtons();
    }
    
    /// <summary>
    /// StageInfoUI 컴포넌트 찾기
    /// </summary>
    private StageInfoUI FindStageInfoUI()
    {
        // 1. FindObjectOfType으로 찾기 시도
        StageInfoUI infoUI = FindObjectOfType<StageInfoUI>();
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
        Canvas[] canvases = FindObjectsOfType<Canvas>();
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
        for (int i = 0; i < stageButtons.Count; i++)
        {
            StageButtonUI button = stageButtons[i];
            if (button != null)
            {
                // 스테이지 번호는 1부터 시작
                string stageId = (i + 1).ToString();
                button.Initialize(stageId, OnStageButtonClicked);
            }
        }
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
        foreach (StageButtonUI button in stageButtons)
        {
            if (button != null)
                button.RefreshData();
        }
        
        UpdateTotalStars();
    }
} 