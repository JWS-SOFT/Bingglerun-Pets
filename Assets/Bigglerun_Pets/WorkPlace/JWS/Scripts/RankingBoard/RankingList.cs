using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScore, playerName, rank;
    [SerializeField] private Image avataImage;

    public void SetRankList(string score, string name, int rank)
    {
        highScore.text = score;
        playerName.text = name;
        this.rank.text = "#" + (rank + 1).ToString();
        avataImage.sprite = null;
    }
}
