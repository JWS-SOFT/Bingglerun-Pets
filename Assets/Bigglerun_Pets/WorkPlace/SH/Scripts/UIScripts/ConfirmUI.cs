using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ConfirmUI : MonoBehaviour
{
    public DecorationItemData decoItemData;

    [SerializeField] private Image itemImage;
    [SerializeField] private TextMeshProUGUI itemNameText;
    [SerializeField] private TextMeshProUGUI goldPriceText;
    [SerializeField] private TextMeshProUGUI diamondPriceText;
    [SerializeField] private Toggle useDiamondsToggle;

    private void Start()
    {
        itemNameText.text = decoItemData.itemName.ToString();
        goldPriceText.text = decoItemData.goldPrice.ToString();
        diamondPriceText.text = decoItemData.cashPrice.ToString();

        //Debug.Log($"itemName : {decoItemData.itemName}");
        //Debug.Log($"itemPrice : {decoItemData.goldPrice}");
    }

    public void PurchaseButton()
    {
        bool result = ShopManager.Instance.TryBuyItem(decoItemData.itemId, useCash: useDiamondsToggle.isOn);
        Debug.Log($"아이템 구매 성공 여부 : {result}");
        //UIManager.Instance.ExitPopup();
        Destroy(gameObject);
    }

    public void CancelButton()
    {
        Destroy(gameObject);
    }
}
