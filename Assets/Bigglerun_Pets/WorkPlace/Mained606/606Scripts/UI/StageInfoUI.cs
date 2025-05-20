using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스테이지 정보 팝업 UI
/// </summary>
public class StageInfoUI : MonoBehaviour
{
    [Header("스테이지 정보")]
    [SerializeField] private TextMeshProUGUI stageTitleText;
    [SerializeField] private TextMeshProUGUI highScoreText;
    
    [Header("별 아이콘")]
    [SerializeField] private GameObject[] starIcons; // 최대 3개의 별 아이콘
    
    [Header("버튼")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button closeButton;
    
    // 현재 선택된 스테이지 ID
    private string selectedStageId;
    
    private void Awake()
    {
        if (startButton != null)
            startButton.onClick.AddListener(OnStartButtonClicked);
            
        if (closeButton != null)
            closeButton.onClick.AddListener(OnCloseButtonClicked);
    }
    
    /// <summary>
    /// 스테이지 정보 표시
    /// </summary>
    public void ShowStageInfo(string stageId)
    {
        selectedStageId = stageId;
        
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning("[StageInfoUI] 플레이어 데이터가 로드되지 않았습니다.");
            return;
        }
        
        // 스테이지 데이터 가져오기
        StageData stageData = PlayerDataManager.Instance.GetStageData(stageId);
        
        // 스테이지 번호 표시
        if (stageTitleText != null)
        {
            stageTitleText.text = $"Stage Name : {stageId}";
        }
        else
        {
            Debug.LogError("[StageInfoUI] stageTitleText가 할당되지 않았습니다. Inspector에서 확인해주세요.");
        }
            
        // 별 아이콘 표시
        int stars = (stageData != null) ? stageData.stars : 0;
        
        if (starIcons != null)
        {
            for (int i = 0; i < starIcons.Length; i++)
            {
                if (starIcons[i] != null)
                {
                    // 새로운 구조 적용
                    Transform starTransform = starIcons[i].transform;
                    
                    // 별 하위 오브젝트 찾기
                    Transform emptyStar = starTransform.Find("EmptyStar");
                    Transform fullStar = starTransform.Find("FullStar");
                    
                    // 별 획득 여부에 따라 하위 오브젝트 활성화/비활성화
                    bool isStarEarned = (i < stars);
                    
                    // EmptyStar와 FullStar가 모두 있는 경우
                    if (emptyStar != null && fullStar != null)
                    {
                        emptyStar.gameObject.SetActive(!isStarEarned);
                        fullStar.gameObject.SetActive(isStarEarned);
                        
                        // 부모 오브젝트는 항상 활성화
                        starIcons[i].SetActive(true);
                    }
                    // 하위 구조가 없는 경우 예전 방식으로 처리
                    else
                    {
                        starIcons[i].SetActive(i < stars);
                    }
                }
            }
        }
        
        // 최고 점수 표시 (경쟁모드에서만 사용하므로 highScoreText가 null이어도 됨)
        if (highScoreText != null)
        {
            int highScore = (stageData != null) ? stageData.highScore : 0;
            
            if (highScore > 0)
                highScoreText.text = $"최고 점수: {highScore.ToString("N0")}";
            else
                highScoreText.text = "최고 점수: -";
        }
        
        // UI 표시
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 시작 버튼 클릭 이벤트
    /// </summary>
    private void OnStartButtonClicked()
    {
        // 게임 데이터 저장
        GameDataManager.SetSelectedStageId(selectedStageId);
        
        // 인게임 씬으로 전환 (스토리 모드)
        UIManager.Instance.ShowLoadingScreen(true, "Loading Game...");
        GameManager.Instance.LoadGameScene(selectedStageId, false); // false = 스토리 모드
        
        // UI 닫기
        gameObject.SetActive(false);
    }
    
    /// <summary>
    /// 닫기 버튼 클릭 이벤트
    /// </summary>
    private void OnCloseButtonClicked()
    {
        gameObject.SetActive(false);
    }
}

/// <summary>
/// 인게임에서 사용할 게임 데이터 관리
/// </summary>
public static class GameDataManager
{
    private static string selectedStageId;
    
    /// <summary>
    /// 선택된 스테이지 ID 설정
    /// </summary>
    public static void SetSelectedStageId(string stageId)
    {
        selectedStageId = stageId;
    }
    
    /// <summary>
    /// 선택된 스테이지 ID 가져오기
    /// </summary>
    public static string GetSelectedStageId()
    {
        return selectedStageId;
    }
} 