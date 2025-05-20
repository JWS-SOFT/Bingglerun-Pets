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
        // 현재 게임 상태 확인
        if (GameManager.Instance == null || GameManager.Instance.StateMachine == null)
        {
            Debug.LogError("GameOverUI - GameManager 또는 StateMachine이 null입니다.");
            return;
        }

        GameState currentState = GameManager.Instance.StateMachine.CurrentState;
        
        // 스토리 모드나 경쟁 모드 상태일 때만 ScoreManager 및 데이터 처리
        if (currentState == GameState.StoryInGame || currentState == GameState.CompetitionInGame)
        {
            // ScoreManager가 null인지 확인
            if (ScoreManager.Instance == null)
            {
                Debug.LogError("GameOverUI - ScoreManager.Instance가 null입니다.");
                return;
            }

            // PlayerDataManager 확인
            if (PlayerDataManager.Instance == null || PlayerDataManager.Instance.CurrentPlayerData == null)
            {
                Debug.LogError("GameOverUI - PlayerDataManager.Instance 또는 CurrentPlayerData가 null입니다.");
                return;
            }

            // 스코어와 기본 정보 가져오기
            totalScore = ScoreManager.Instance.GetScore();
            int currentStars = ScoreManager.Instance.GetStars();
            int currentCoins = ScoreManager.Instance.GetCoin();
            
            Debug.Log($"GameOverUI - 점수: {totalScore}, 별: {currentStars}개, 코인: {currentCoins}개");

            // 게임 상태에 따라 다른 처리
            if (currentState == GameState.StoryInGame)
            {
                // 스토리 모드인 경우: 별 점수 시스템 적용
                ProcessStoryModeResult(currentStars, currentCoins);
            }
            else // 경쟁 모드
            {
                // 경쟁 모드인 경우: 최고 점수 시스템 적용
                ProcessCompetitionModeResult(currentCoins);
            }

            // 공통 UI 업데이트
            if (totalScoreText != null)
                totalScoreText.text = totalScore.ToString();

            if (getGoldText != null)
                getGoldText.text = currentCoins.ToString();

            // 획득 재화 추가
            Debug.Log($"코인 추가 전 - 현재 골드: {PlayerDataManager.Instance.CurrentPlayerData.gold}, 총 수집 코인: {PlayerDataManager.Instance.CurrentPlayerData.totalCoinsCollected}");
            PlayerDataManager.Instance.AddGold(currentCoins);
            Debug.Log($"코인 추가 후 - 갱신된 골드: {PlayerDataManager.Instance.CurrentPlayerData.gold}, 총 수집 코인: {PlayerDataManager.Instance.CurrentPlayerData.totalCoinsCollected}");
        }
        else
        {
            // 인게임 상태가 아닐 때의 처리
            Debug.Log($"GameOverUI - 인게임 상태가 아닙니다. 현재 상태: {currentState}");
            
            // UI 초기화 - 값을 표시하지 않거나 기본값 표시
            if (totalScoreText != null)
                totalScoreText.text = "0";
            
            if (bestScoreText != null)
                bestScoreText.text = "0";
                
            if (getGoldText != null)
                getGoldText.text = "0";
        }
    }

    // 스토리 모드 결과 처리
    private void ProcessStoryModeResult(int currentStars, int currentCoins)
    {
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
    }

    // 경쟁 모드 결과 처리
    private void ProcessCompetitionModeResult(int currentCoins)
    {
        // 경쟁 모드 점수 업데이트 (기존 점수 시스템)
        PlayerDataManager.Instance.UpdateCompetitiveBestScore((int)totalScore);
        
        if (bestScoreText != null)
            bestScoreText.text = PlayerDataManager.Instance.CurrentPlayerData.competitiveBestScore.ToString();
    }
}
