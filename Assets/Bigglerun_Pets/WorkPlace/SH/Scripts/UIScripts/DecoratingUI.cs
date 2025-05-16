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

    [SerializeField] private GameObject accessaryPrefab;
    [SerializeField] private Transform contents;

    private int selectedItemIndex;

    private void Awake()
    {
        playerData = PlayerDataManager.Instance;
    }

    private void OnEnable()
    {
        PlayerDataManager.Instance.OnGoldChanged += SetGoldData;
        PlayerDataManager.Instance.OnDiamondChanged += SetDiamondData;
        PlayerDataManager.Instance.OnDecorationUnlocked += OnDecorationChanged;
        SetGoldData(playerData.CurrentPlayerData.gold);
        SetDiamondData(playerData.CurrentPlayerData.diamond);
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnGoldChanged -= SetGoldData;
        PlayerDataManager.Instance.OnDiamondChanged -= SetDiamondData;
        PlayerDataManager.Instance.OnDecorationUnlocked -= OnDecorationChanged;
    }

    private void Start()
    {
        AccessaryListLoad();
    }

    // 장식 아이템 해금 이벤트 처리
    private void OnDecorationChanged(string decorationId, bool unlocked)
    {
        if (unlocked)
        {
            RefreshDecorationList();
        }
    }

    // 장식 아이템 목록 새로고침
    private void RefreshDecorationList()
    {
        // 기존 목록 클리어 후 다시 로드
        ContentsClear();
        AccessaryListLoad();
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

        //Debug.Log("클릭된 버튼의 인덱스: " + selectedItemIndex);
        decoItemData = ShopManager.Instance.accessaryItemList[selectedItemIndex].GetComponent<AccessaryItem>().decoItemData;
    }

    private void ContentsClear()
    {
        // 기존 아이템 리스트 제거
        foreach (var item in ShopManager.Instance.accessaryItemList)
        {
            Destroy(item);
        }
        ShopManager.Instance.accessaryItemList.Clear();
        
        // 컨텐츠 영역의 모든 자식 오브젝트 제거
        for (int i = contents.childCount - 1; i >= 0; i--)
        {
            Destroy(contents.GetChild(i).gameObject);
        }
    }

    private void AccessaryListLoad()
    {
        for (int i = 0; i < ItemManager.AllDecorationItems.Count; i++)
        {
            GameObject itemPrefab = Instantiate(accessaryPrefab, contents);
            itemPrefab.GetComponent<AccessaryItem>().decoItemData = ItemManager.AllDecorationItems[i];
            ShopManager.Instance.accessaryItemList.Add(itemPrefab);
        }
        //Debug.Log($"리스트 아이템 개수 : {ShopManager.Instance.accessaryItemList.Count}");
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
