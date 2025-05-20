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

        PlayerDataManager.Instance.UpdateCompetitiveBestScore((int)totalScore);
        totalScoreText.text = totalScore.ToString();
        bestScoreText.text = PlayerDataManager.Instance.CurrentPlayerData.competitiveBestScore.ToString();

        getGoldText.text = ScoreManager.Instance.GetCoin().ToString();

        // 획득 재화 추가
        PlayerDataManager.Instance.AddGold(ScoreManager.Instance.GetCoin());
    }
}
