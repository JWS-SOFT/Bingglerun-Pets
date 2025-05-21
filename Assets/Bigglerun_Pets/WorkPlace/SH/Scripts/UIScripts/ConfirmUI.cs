using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : MonoBehaviour
{
    public ItemType itemType;
    public DecorationItemData decoItemData;
    public ItemData itemData;

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI goldPriceText;
    [SerializeField] private TextMeshProUGUI diamondPriceText;
    [SerializeField] private Toggle useDiamondsToggle;

    private void Start()
    {
        switch (itemType)
        {
            case ItemType.Decoration:
                itemNameText.text = decoItemData.itemName.ToString();
                goldPriceText.text = decoItemData.goldPrice.ToString();
                diamondPriceText.text = decoItemData.cashPrice.ToString();
                break;
            case ItemType.Skin:
                break;
            case ItemType.UsableItem:
                itemNameText.text = itemData.itemName.ToString();
                goldPriceText.text = itemData.goldPrice.ToString();
                diamondPriceText.text = itemData.cashPrice.ToString();
                break;
        }

        //Debug.Log($"itemName : {decoItemData.itemName}");
        //Debug.Log($"itemPrice : {decoItemData.goldPrice}");
    }

    public void PurchaseButton()
    {
        bool result;
        switch (itemType)
        {
            case ItemType.Decoration:
                result = ShopManager.Instance.TryBuyItem(decoItemData.itemId, useCash: useDiamondsToggle.isOn);
                break;
            case ItemType.Skin:
                result = false;
                break;
            case ItemType.UsableItem:
                result = ShopManager.Instance.TryBuyItem(itemData.itemId, useCash: useDiamondsToggle.isOn);
                break;
            default:
                result = false;
                break;
        }
        
        Debug.Log($"아이템 구매 성공 여부 : {result}");
        //UIManager.Instance.ExitPopup();
        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
