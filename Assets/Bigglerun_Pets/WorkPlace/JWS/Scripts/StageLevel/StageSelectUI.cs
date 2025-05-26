using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class StageSelectUI : MonoBehaviour
{
    [Header("UI 설정")]
    [SerializeField] private Transform levelSelectPoint; // 버튼들이 생성될 위치
    [SerializeField] private GameObject levelButtonPrefab, startStagePopup, lockedButtonPopup; // 버튼 프리팹
    [SerializeField] private Button nextButton; // 다음 페이지 버튼
    [SerializeField] private Button prevButton; // 이전 페이지 버튼
    [SerializeField] private ScrollRect scrollRect; // 스크롤 컨트롤

    [Header("페이징 설정")]
    [SerializeField] private int poolSize = 10; // 한 페이지당 버튼 수
    [SerializeField] private int currentStage = 1; // 테스트용 현재 클리어한 스테이지
    [SerializeField] private int currentPage = 1; // 현재 페이지
    private List<GameObject> buttonPool = new();
    private PlayerData currentPlayerData;

    private void OnEnable()
    {
        currentPlayerData = PlayerDataManager.Instance.CurrentPlayerData;
        currentStage = currentPlayerData.highestStage;
        currentPage = Mathf.CeilToInt((float)currentStage / poolSize);
        startStagePopup.SetActive(false);
        lockedButtonPopup.SetActive(false);
        if (buttonPool.Count == 0)
            CreateButtonPool();

        UpdateStageButtons();
    }

    private void CreateButtonPool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            GameObject gob = Instantiate(levelButtonPrefab, levelSelectPoint);
            gob.SetActive(false);
            buttonPool.Add(gob);
        }
    }

    private void UpdateStageButtons()
    {
        int startIndex = (currentPage - 1) * poolSize;
        int maxStage = currentStage;

        for (int i = 0; i < poolSize; i++)
        {
            GameObject gob = buttonPool[i];
            int stageIndex = startIndex + i;

            gob.SetActive(true);

            bool isUnlocked = stageIndex < currentStage;
            bool isLastPlayed = stageIndex == currentStage - 1;

            gob.transform.GetChild(0).gameObject.SetActive(!isUnlocked);
            gob.transform.GetChild(1).gameObject.SetActive(isUnlocked);

            gob.transform.GetChild(0).GetComponent<ButtonLockedUI>().SetLockedButton(lockedButtonPopup);
            gob.transform.GetChild(1).GetComponent<LevelButtonUI>().SetStageLevelSetting(stageIndex, isLastPlayed, this);
        }

        int maxPage = Mathf.CeilToInt((float)maxStage / poolSize);
        nextButton.gameObject.SetActive(currentPage < maxPage);
        prevButton.gameObject.SetActive(currentPage > 1);

        CenterScrollToCurrentStage();
    }

    private void CenterScrollToCurrentStage()
    {
        int maxPage = Mathf.CeilToInt((float)currentStage / poolSize);

        if (maxPage <= 1) return;

        if (maxPage == currentPage)
        {
            int indexInPage = (currentStage - 1) % poolSize;

            if (poolSize <= 1)
            {
                scrollRect.verticalNormalizedPosition = 0f;
                return;
            }

            // 비율 계산 (0.0 ~ 1.0)
            float step = 1f / (poolSize - 1);
            float scrollValue = indexInPage * step;

            scrollRect.verticalNormalizedPosition = scrollValue;
        }
        else
        {
            scrollRect.verticalNormalizedPosition = 0f;
        }
    }

    public void OnClickNextPage()
    {
        currentPage++;
        UpdateStageButtons();
    }

    public void OnClickPrevPage()
    {
        currentPage--;
        UpdateStageButtons();
    }

    public void OnClickStartStage(int level)
    {
        if (startStagePopup.activeSelf) startStagePopup.SetActive(false);
        startStagePopup.GetComponent<StageStart>().SetStageGameStart(level);
        startStagePopup.SetActive(true);
    }
}
