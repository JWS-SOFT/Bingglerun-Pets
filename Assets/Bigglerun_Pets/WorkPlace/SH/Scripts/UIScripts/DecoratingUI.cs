using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using static UnityEditor.Progress;

public class DecoratingUI : MonoBehaviour
{
    private DecorationItemData decoItemData;
    private PlayerDataManager playerData;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerCash;

    private int selectedItemIndex;

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

    private void ItemSelect()
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;

        // 부모인 Content Transform
        Transform parent = clickedObj.transform.parent;

        // 클릭된 버튼이 Content에서 몇 번째 자식인지 찾기
        selectedItemIndex = clickedObj.transform.GetSiblingIndex();

        Debug.Log("클릭된 버튼의 인덱스: " + selectedItemIndex);
        decoItemData = parent.GetChild(selectedItemIndex).GetComponent<ShItem>().decoItemData;
    }

    public void EquipButton()
    {
        ItemSelect();
        bool unlocked = ItemManager.Instance.IsUnlockedDecoration(decoItemData.itemId);
        if (unlocked)
        {
            ItemManager.Instance.EquipDecoration(decoItemData.itemId);
            DecorationItemData deco;
            switch (decoItemData.type)
            {
                case DecorationType.Hat:
                    deco = ItemManager.Instance.GetEquippedDecoration(DecorationType.Hat);
                    break;
                case DecorationType.Body:
                    deco = ItemManager.Instance.GetEquippedDecoration(DecorationType.Body);
                    break;
                case DecorationType.Shoes:
                    deco = ItemManager.Instance.GetEquippedDecoration(DecorationType.Shoes);
                    break;
                default:
                    deco = ItemManager.Instance.GetEquippedDecoration(DecorationType.Hat);
                    break;
            }
            Debug.Log($"데코 아이템 장착됨: {deco.itemName}");
        }
        else
        {
            Debug.Log("데코 아이템 언락되지 않음");
        }
    }
}
