using TMPro;
using UnityEngine;

public class GameOverUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI totalScoreText;

    [SerializeField] private float totalScore;

    private void OnEnable()
    {
        if (PlayerManager.PlayMode)
        {
            totalScore = PlayerManager.Instance.currentPlayerDistance;
        }
        else
        {
            totalScore = PlayerManager.Instance.currentPlayerFloor;
        }

        totalScoreText.text = totalScore.ToString();
    }
}
