using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    private PlayerDataManager playerData;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerCash;

    public GameObject itemPrefab;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;

    [SerializeField] private List<Transform> accessaryItemList = new List<Transform>();
    [SerializeField] private Transform selectedItem;


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
