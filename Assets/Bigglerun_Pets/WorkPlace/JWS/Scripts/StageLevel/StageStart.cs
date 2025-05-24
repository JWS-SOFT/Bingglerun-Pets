using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageStart : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageLeveltext, score;
    [SerializeField] private int currentStageLevel;
    [SerializeField] private Image[] starIcons;

    public void SetStageGameStart(int level)
    {
        currentStageLevel = level;
        // 스테이지 번호 표시
        stageLeveltext.text = "Stage #" + currentStageLevel.ToString();

        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning("[StageInfoUI] 플레이어 데이터가 로드되지 않았습니다.");
            return;
        }

        // 스테이지 데이터 가져오기
        StageData stageData = PlayerDataManager.Instance.GetStageData(currentStageLevel.ToString());

        // 별 아이콘 표시
        int stars = (stageData != null) ? stageData.stars : 0;
        if (stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                if (starIcons[i] != null)
                {
                    starIcons[i].material = null;
                }
            }
        }

        if (stageData.highScore > 0) score.text = stageData.highScore.ToString();
    }

    public void StageGameStart()
    {
        // 게임 데이터 저장
        GameDataManager.SetSelectedStageId(currentStageLevel.ToString());
        
        // 인게임 씬으로 전환 (스토리 모드)
        UIManager.Instance.ShowLoadingScreen(true, "Loading Game...");
        GameManager.Instance.LoadGameScene(currentStageLevel.ToString(), false); // false = 스토리 모드
        
        // UI 닫기
        gameObject.SetActive(false);
    }
}
