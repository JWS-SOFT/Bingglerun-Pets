using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InGameHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI floorText;
    [SerializeField] private TextMeshProUGUI coinText;
    [SerializeField] private TextMeshProUGUI distanceText;

    [SerializeField] private TextMeshProUGUI currentScoreText;
    [SerializeField] private Sprite fullHeartImage;
    [SerializeField] private Sprite brokenHeartImage;
    [SerializeField] private GameObject heart;

    private void Start()
    {
        PlayerManager.Instance.OnTakeDamage += LoseHeart;
        HeartInit();
    }

    private void Update()
    {
        if (ScoreManager.Instance != null)
        {
            int currentScore = ScoreManager.Instance.GetScore();
            currentScoreText.text = $"Current Score: {currentScore}";
        }
    }
    private void OnDisable()
    {
        PlayerManager.Instance.OnTakeDamage -= LoseHeart;
    }

    private void HeartInit()
    {
        for(int i = 0; i<PlayerManager.Instance.GetMaxLife(); i++)
        {
            heart.transform.GetChild(i).gameObject.SetActive(true);
        }
    }

    private void LoseHeart()
    {
        int index = PlayerManager.Instance.GetCurrentLife() - 1;
        heart.transform.GetChild(index).GetComponent<Image>().sprite = brokenHeartImage;
    }
}
