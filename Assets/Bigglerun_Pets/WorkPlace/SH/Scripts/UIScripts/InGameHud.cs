using TMPro;
using UnityEngine;

public class InGameHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI distanceText;

    [SerializeField] private TextMeshProUGUI currentScoreText;

    private void Update()
    {
        if (ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.GetScore();
            currentScoreText.text = $"Current Score: {currentScore}";
        }
    }
}
