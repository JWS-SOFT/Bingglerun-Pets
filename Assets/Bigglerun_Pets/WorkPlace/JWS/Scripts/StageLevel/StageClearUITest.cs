using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageClearUITest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private GameObject[] rewardStars;
    [SerializeField] private Sprite goldStar;
    [SerializeField] private GameObject[] rewardItems;
    [SerializeField] private float totalScore;

    private int currentStars, currentCoins;

    private void OnValidate()
    {
        Animator[] animators = GetComponentsInChildren<Animator>(true);
        foreach (var animator in animators)
        {
            if (animator.updateMode != AnimatorUpdateMode.UnscaledTime)
                animator.updateMode = AnimatorUpdateMode.UnscaledTime;
        }
    }

    private void OnEnable()
    {
        totalScore = ScoreManager.Instance.GetScore();
        currentStars = ScoreManager.Instance.GetStars();
        currentCoins = ScoreManager.Instance.GetCoin();

        Debug.Log($"StageClearUI - 점수: {totalScore}, 별: {currentStars}개, 코인: {currentCoins}개");
        for (int i = 0; i < currentStars; i++)
        {
            rewardStars[i].transform.GetChild(1).GetComponent<Image>().sprite = goldStar;
        }

        // 현재 게임 상태에 따라 다른 베스트 스코어 처리
        GameState currentState = GameManager.Instance.StateMachine.CurrentState;

        if (currentState == GameState.CompetitionInGame)
        {
            // 경쟁 모드: competitiveBestScore만 업데이트
            PlayerDataManager.Instance.UpdateCompetitiveBestScore((int)totalScore);
            totalScoreText.text = totalScore.ToString();
            bestScoreText.text = "Best Score : " + PlayerDataManager.Instance.CurrentPlayerData.competitiveBestScore.ToString();
            Debug.Log($"경쟁모드 베스트 스코어 업데이트: {PlayerDataManager.Instance.CurrentPlayerData.competitiveBestScore}");
        }
        else if (currentState == GameState.StoryInGame)
        {
            // 스토리 모드: 스테이지별 베스트 스코어 처리
            string currentStageId = GameDataManager.GetSelectedStageId();
            if (!string.IsNullOrEmpty(currentStageId))
            {
                Debug.Log($"스테이지 결과 저장 전 - 현재 스테이지: {currentStageId}, 현재 별: {currentStars}개");
                PlayerDataManager.Instance.UpdateStageResult(currentStageId, (int)totalScore, currentStars);

                // 스테이지별 베스트 스코어 표시
                int stageBestScore = PlayerDataManager.Instance.GetStageBestScore(currentStageId);
                totalScoreText.text = totalScore.ToString();
                bestScoreText.text = "Best Score : " + stageBestScore.ToString();

                // DB 저장 후 실제 값 확인
                StageData savedData = PlayerDataManager.Instance.GetStageData(currentStageId);
                int totalStars = PlayerDataManager.Instance.CurrentPlayerData.totalStars;
                Debug.Log($"스테이지 결과 저장 후 - 저장된 별: {savedData?.stars ?? 0}개, 총 별 개수: {totalStars}, 스테이지 베스트: {stageBestScore}");
            }
            else
            {
                Debug.LogWarning("스테이지 ID를 찾을 수 없어 결과를 저장할 수 없습니다.");
                totalScoreText.text = totalScore.ToString();
                bestScoreText.text = "Best Score : 0";
            }

            // 클리어 정보 업데이트 하여 해금실시.
            if (int.TryParse(currentStageId, out int currentStageNumber))
            {
                int nextStageNumber = currentStageNumber + 1;
                PlayerDataManager.Instance.UnlockStage(nextStageNumber.ToString());
            }
            else
            {
                Debug.LogWarning($"currentStageId 파싱 실패: {currentStageId}");
            }
        }
        else
        {
            // 기타 상태에서는 기본 처리
            Debug.LogWarning($"예상하지 못한 게임 상태: {currentState}");
            totalScoreText.text = totalScore.ToString();
            bestScoreText.text = "Best Score : 0";
        }

        SetRewardUI();

        // 획득 재화 추가
        Debug.Log($"코인 추가 전 - 현재 골드: {PlayerDataManager.Instance.CurrentPlayerData.gold}, 총 수집 코인: {PlayerDataManager.Instance.CurrentPlayerData.totalCoinsCollected}");
        PlayerDataManager.Instance.AddGold(currentCoins);
        Debug.Log($"코인 추가 후 - 갱신된 골드: {PlayerDataManager.Instance.CurrentPlayerData.gold}, 총 수집 코인: {PlayerDataManager.Instance.CurrentPlayerData.totalCoinsCollected}");
    }

    private void SetRewardUI()
    {
        if (currentCoins > 0)
        {
            rewardItems[0].transform.GetChild(0).GetComponent<Image>().material = null;
            rewardItems[0].transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = currentCoins.ToString();
        }
    }

    public void ResetScore()
    {
        ScoreManager.Instance.ResetScore();
    }
}
