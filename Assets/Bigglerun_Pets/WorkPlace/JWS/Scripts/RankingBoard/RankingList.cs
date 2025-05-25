using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RankingList : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI highScore, playerName, rank;
    [SerializeField] private Image avataImage;

    public void SetRankList(string score, string name, int rank, bool player)
    {
        highScore.text = score;
        playerName.text = name;
        this.rank.text = "#" + (rank + 1).ToString();
        avataImage.sprite = null;
        if (player)
        {
            transform.GetComponent<Image>().color = Color.green;
            transform.GetChild(0).GetComponent<Image>().color = Color.green;
        }
    }
}
