using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : MonoBehaviour
{
    public DecorationItemData decoItemData;

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI itemPriceText;

    private void OnEnable()
    {
        itemNameText.text = decoItemData.itemName.ToString();
        itemPriceText.text = decoItemData.goldPrice.ToString();
    }

    public void PurchaseButton()
    {
        bool result = ShopManager.Instance.TryBuyItem(decoItemData.itemId, useCash: false);
        Debug.Log($"아이템 구매 성공 여부 : {result}");
        UIManager.Instance.ExitPopup();
    }
}
