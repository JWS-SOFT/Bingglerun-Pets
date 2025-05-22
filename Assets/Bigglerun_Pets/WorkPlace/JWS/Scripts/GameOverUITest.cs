using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverUITest : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private GameObject[] rewardItems;
    [SerializeField] private float totalScore;

    private int currentStars, currentCoins;

    private void OnEnable()
    {
        totalScore = ScoreManager.Instance.GetScore();
        currentStars = ScoreManager.Instance.GetStars();
        currentCoins = ScoreManager.Instance.GetCoin();

        Debug.Log($"GameOverUI - 점수: {totalScore}, 별: {currentStars}개, 코인: {currentCoins}개");

        // 경쟁 모드 점수 업데이트 (기존 점수 시스템)
        PlayerDataManager.Instance.UpdateCompetitiveBestScore((int)totalScore);
        totalScoreText.text = totalScore.ToString();
        bestScoreText.text = "Best Scroe : " + PlayerDataManager.Instance.CurrentPlayerData.competitiveBestScore.ToString();

        // 스테이지 결과 저장 (별 점수 시스템)
        string currentStageId = GameDataManager.GetSelectedStageId();
        if (!string.IsNullOrEmpty(currentStageId))
        {
            Debug.Log($"스테이지 결과 저장 전 - 현재 스테이지: {currentStageId}, 현재 별: {currentStars}개");
            PlayerDataManager.Instance.UpdateStageResult(currentStageId, (int)totalScore, currentStars);

            // DB 저장 후 실제 값 확인
            StageData savedData = PlayerDataManager.Instance.GetStageData(currentStageId);
            int totalStars = PlayerDataManager.Instance.CurrentPlayerData.totalStars;
            Debug.Log($"스테이지 결과 저장 후 - 저장된 별: {savedData?.stars ?? 0}개, 총 별 개수: {totalStars}");
        }
        else
        {
            Debug.LogWarning("스테이지 ID를 찾을 수 없어 결과를 저장할 수 없습니다.");
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
