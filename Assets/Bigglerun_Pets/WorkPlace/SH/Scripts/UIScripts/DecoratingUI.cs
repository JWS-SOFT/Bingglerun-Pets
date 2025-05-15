using TMPro;
using UnityEngine;

public class DecoratingUI : MonoBehaviour
{
    private PlayerDataManager playerData;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerCash;

    private void Start()
    {
        playerData = PlayerDataManager.Instance;

        SetData();
    }
    public void SetData()
    {
        playerGold.text = playerData.CurrentPlayerData.gold.ToString();
        playerCash.text = playerData.CurrentPlayerData.diamond.ToString();
    }
}
