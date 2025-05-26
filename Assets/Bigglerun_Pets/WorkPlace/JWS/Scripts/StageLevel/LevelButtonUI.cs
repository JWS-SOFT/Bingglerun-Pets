using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    private int currentLevelIndex = 0;
    [SerializeField] private Material grayTint, currentButton;
    [SerializeField] private Image startButton;
    [SerializeField] private Image[] starTint;
    [SerializeField] private TextMeshProUGUI currentStageLeveltext;
    [SerializeField] private StageSelectUI stageSelectUI;

    public void SetStageLevelSetting(int level, bool current, StageSelectUI selectUI)
    {
        stageSelectUI = selectUI;
        currentLevelIndex = level + 1;
        currentStageLeveltext.text = currentLevelIndex.ToString();

                // 스테이지 데이터 가져오기
        StageData stageData = PlayerDataManager.Instance.GetStageData(currentLevelIndex.ToString());

        // 별 아이콘 표시
        int stars = (stageData != null) ? stageData.stars : 0;
        if (stars > 0)
        {
            for (int i = 0; i < stars; i++)
            {
                if (starTint[i] != null)
                {
                    starTint[i].material = null;
                }
            }
        }
        if (current)
        {
            transform.GetChild(0).gameObject.SetActive(true);
            transform.GetChild(transform.childCount - 1).gameObject.SetActive(true);
            startButton.material = currentButton;
        }
        else
        {
            transform.GetChild(0).gameObject.SetActive(false);
            transform.GetChild(transform.childCount - 1).gameObject.SetActive(false);
            startButton.material = null;
        }
    }

    public void ClickStageLevel()
    {
        stageSelectUI.OnClickStartStage(currentLevelIndex);
    }
}
