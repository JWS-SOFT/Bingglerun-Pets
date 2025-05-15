using TMPro;
using UnityEngine;

public class DecoratingUI : MonoBehaviour
{
    private PlayerDataManager playerData;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerCash;

    private void Awake()
    {
        playerData = PlayerDataManager.Instance;
    }

    private void OnEnable()
    {
        PlayerDataManager.Instance.OnGoldChanged += SetGoldData;
        PlayerDataManager.Instance.OnDiamondChanged += SetDiamondData;
        SetGoldData(playerData.CurrentPlayerData.gold);
        SetDiamondData(playerData.CurrentPlayerData.diamond);
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnGoldChanged -= SetGoldData;
        PlayerDataManager.Instance.OnDiamondChanged -= SetDiamondData;
    }

    private void SetGoldData(int gold)
    {
        playerGold.text = gold.ToString();

    }

    private void SetDiamondData(int diamond)
    {
        playerCash.text = diamond.ToString();
    }
}
