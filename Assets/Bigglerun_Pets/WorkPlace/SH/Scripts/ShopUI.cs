using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    private PlayerDataManager playerData;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerCash;

    [SerializeField] private Transform[] mainTab;

    public GameObject itemPrefab;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;

    [SerializeField] private List<Transform> accessaryItemList = new List<Transform>();
    [SerializeField] private Transform selectedItem;

    private void Awake()
    {
        playerData = PlayerDataManager.Instance;
    }

    private void OnEnable()
    {
        SetData();
    }

    public void SetData()
    {
        playerGold.text = playerData.CurrentPlayerData.gold.ToString();
        playerCash.text = playerData.CurrentPlayerData.diamond.ToString();
    }

    private void UpdateData()
    {
        playerGold.text = playerData.CurrentPlayerData.gold.ToString();
        playerCash.text = playerData.CurrentPlayerData.diamond.ToString();
    }

    public void SubTabSwitch(int index)
    {
        Debug.Log($"SubTab {index}로 변경");
    }

    public void MainTabSwitch(int index)
    {
        switch (index)
        {
            case 0:
                mainTab[0].gameObject.SetActive(true);
                mainTab[1].gameObject.SetActive(false);
                break;
            case 1:
                mainTab[0].gameObject.SetActive(false);
                mainTab[1].gameObject.SetActive(true);
                break;
        }
    }


}
