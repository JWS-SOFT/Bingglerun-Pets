using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI getGoldText;

    [SerializeField] private float totalScore;

    private void OnEnable()
    {
        //if (PlayerManager.PlayMode)
        //{
        //    totalScore = PlayerManager.Instance.currentPlayerDistance;
        //}
        //else
        //{
        //    totalScore = PlayerManager.Instance.currentPlayerFloor;
        //}
        totalScore = ScoreManager.Instance.GetScore();

        // 경쟁 모드 점수 업데이트 (기존 점수 시스템)
        PlayerDataManager.Instance.UpdateCompetitiveBestScore((int)totalScore);
        totalScoreText.text = totalScore.ToString();
        bestScoreText.text = PlayerDataManager.Instance.CurrentPlayerData.competitiveBestScore.ToString();

        // 스테이지 결과 저장 (별 점수 시스템)
        string currentStageId = GameDataManager.GetSelectedStageId();
        if (!string.IsNullOrEmpty(currentStageId))
        {
            int starsEarned = ScoreManager.Instance.GetStars();
            Debug.Log($"게임 클리어 결과 저장: 스테이지 {currentStageId}, 점수 {(int)totalScore}, 별 {starsEarned}개");
            PlayerDataManager.Instance.UpdateStageResult(currentStageId, (int)totalScore, starsEarned);
        }
        else
        {
            Debug.LogWarning("스테이지 ID를 찾을 수 없어 결과를 저장할 수 없습니다.");
        }

        getGoldText.text = ScoreManager.Instance.GetCoin().ToString();

        // 획득 재화 추가
        PlayerDataManager.Instance.AddGold(ScoreManager.Instance.GetCoin());
    }
}
