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
        totalScore = ScoreManager.Instance.GetScore();
        int currentStars = ScoreManager.Instance.GetStars();
        int currentCoins = ScoreManager.Instance.GetCoin();
        
        Debug.Log($"GameOverUI - 점수: {totalScore}, 별: {currentStars}개, 코인: {currentCoins}개");

        // 현재 게임 상태에 따라 다른 베스트 스코어 처리
        GameState currentState = GameManager.Instance.StateMachine.CurrentState;
        
        if (currentState == GameState.CompetitionInGame)
        {
            // 경쟁 모드: competitiveBestScore만 업데이트
            PlayerDataManager.Instance.UpdateCompetitiveBestScore((int)totalScore);
            totalScoreText.text = totalScore.ToString();
            bestScoreText.text = PlayerDataManager.Instance.CurrentPlayerData.competitiveBestScore.ToString();
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
                bestScoreText.text = stageBestScore.ToString();
                
                // DB 저장 후 실제 값 확인
                StageData savedData = PlayerDataManager.Instance.GetStageData(currentStageId);
                int totalStars = PlayerDataManager.Instance.CurrentPlayerData.totalStars;
                Debug.Log($"스테이지 결과 저장 후 - 저장된 별: {savedData?.stars ?? 0}개, 총 별 개수: {totalStars}, 스테이지 베스트: {stageBestScore}");
            }
            else
            {
                Debug.LogWarning("스테이지 ID를 찾을 수 없어 결과를 저장할 수 없습니다.");
                totalScoreText.text = totalScore.ToString();
                bestScoreText.text = "0";
            }
        }
        else
        {
            // 기타 상태에서는 기본 처리
            Debug.LogWarning($"예상하지 못한 게임 상태: {currentState}");
            totalScoreText.text = totalScore.ToString();
            bestScoreText.text = "0";
        }

        getGoldText.text = currentCoins.ToString();

        // 획득 재화 추가
        Debug.Log($"코인 추가 전 - 현재 골드: {PlayerDataManager.Instance.CurrentPlayerData.gold}, 총 수집 코인: {PlayerDataManager.Instance.CurrentPlayerData.totalCoinsCollected}");
        PlayerDataManager.Instance.AddGold(currentCoins);
        Debug.Log($"코인 추가 후 - 갱신된 골드: {PlayerDataManager.Instance.CurrentPlayerData.gold}, 총 수집 코인: {PlayerDataManager.Instance.CurrentPlayerData.totalCoinsCollected}");
    }
}
