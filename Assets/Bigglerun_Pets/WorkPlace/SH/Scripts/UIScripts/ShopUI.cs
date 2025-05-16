using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    private PlayerDataManager playerData;
    [SerializeField] private TextMeshProUGUI playerGold;
    [SerializeField] private TextMeshProUGUI playerCash;

    [SerializeField] private Transform[] mainTab;

    [SerializeField] private GameObject accessaryPrefab;
    [SerializeField] private GameObject skinPrefab;
    [SerializeField] private Transform contents;
    public Image itemImage;
    public TextMeshProUGUI itemName;
    public TextMeshProUGUI itemPrice;

    //[SerializeField] private List<GameObject> accessaryItemList = new List<GameObject>();
    [SerializeField] private Button itemButton;
    [SerializeField] private int selectedItemIndex;

    public ConfirmUI confirmUI;

    private void Awake()
    {
        playerData = PlayerDataManager.Instance;
    }

    private void OnEnable()
    {
        PlayerDataManager.Instance.OnGoldChanged += SetGoldData;
        PlayerDataManager.Instance.OnDiamondChanged += SetDiamondData;
        PlayerDataManager.Instance.OnItemQuantityChanged += OnItemChanged;
        PlayerDataManager.Instance.OnDecorationUnlocked += OnDecorationChanged;
        
        SetGoldData(playerData.CurrentPlayerData.gold);
        SetDiamondData(playerData.CurrentPlayerData.diamond);
        
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnGoldChanged -= SetGoldData;
        PlayerDataManager.Instance.OnDiamondChanged -= SetDiamondData;
        PlayerDataManager.Instance.OnItemQuantityChanged -= OnItemChanged;
        PlayerDataManager.Instance.OnDecorationUnlocked -= OnDecorationChanged;
    }
    
    private void Start()
    {
        AccessaryListLoad();
        //confirmUI = UIManager.Instance.FindDirectChildByName("Confirm").GetComponent<ConfirmUI>();
        //SkinListLoad();
    }

    // 아이템 수량 변경 이벤트 처리
    private void OnItemChanged(string itemId, int quantity)
    {
        // 아이템 리스트 UI 업데이트가 필요한 경우 처리
        // 구매/사용한 아이템이 목록에 표시되는 아이템일 때 새로고침
        if (ItemManager.Instance.IsUsableItem(itemId) && mainTab[0].gameObject.activeSelf)
        {
            RefreshItemList();
        }
    }

    // 장식 아이템 해금 이벤트 처리
    private void OnDecorationChanged(string decorationId, bool unlocked)
    {
        // 장식 아이템이 해금되었을 때 목록 새로고침
        if (unlocked && mainTab[1].gameObject.activeSelf)
        {
            RefreshDecorationList();
        }
    }

    // 아이템 목록 새로고침
    private void RefreshItemList()
    {
        // 기존 목록 클리어 후 다시 로드
        ContentsClear();
        AccessaryListLoad();
    }

    // 장식 아이템 목록 새로고침
    private void RefreshDecorationList()
    {
        // 기존 목록 클리어 후 다시 로드
        ContentsClear();
        AccessaryListLoad();
    }

    // Gold Text 갱신
    private void SetGoldData(int gold)
    {
        playerGold.text = gold.ToString();
    }

    // Diamond Text 갱신
    private void SetDiamondData(int diamond)
    {
        playerCash.text = diamond.ToString();
    }

    // SubTab 카테고리 변경
    public void SubTabSwitch(int index)
    {
        Debug.Log($"SubTab {index}로 변경");
    }

    // MainTab 카테고리 변경
    public void MainTabSwitch(int index)
    {
        switch (index)
        {
            case 0:
                mainTab[0].gameObject.SetActive(true);
                mainTab[1].gameObject.SetActive(false);
                RefreshItemList();
                break;
            case 1:
                mainTab[0].gameObject.SetActive(false);
                mainTab[1].gameObject.SetActive(true);
                RefreshDecorationList();
                break;
        }
    }

    // 선택한 아이템을 confirmUI에 전달
    public void ItemSelect()
    {
        GameObject clickedObj = EventSystem.current.currentSelectedGameObject;

        // 부모인 Content Transform
        Transform parent = clickedObj.transform.parent;

        // 클릭된 버튼이 Content에서 몇 번째 자식인지 찾기
        selectedItemIndex = clickedObj.transform.GetSiblingIndex();

        //Debug.Log("클릭된 버튼의 인덱스: " + selectedItemIndex);
        var item = ShopManager.Instance.accessaryItemList[selectedItemIndex].GetComponent<AccessaryItem>();
        GameObject confirm = Instantiate(confirmUI.gameObject, UIManager.Instance.popup);
        confirm.SetActive(true);
        confirm.GetComponent<ConfirmUI>().decoItemData = item.decoItemData;
        //Debug.Log($"confirmUI.decoItemData.itemName = {confirm.GetComponent<ConfirmUI>().decoItemData?.itemName}");
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
        for(int i = 0; i < ItemManager.AllDecorationItems.Count; i++)
        {
            GameObject itemPrefab = Instantiate(accessaryPrefab, contents);
            itemPrefab.GetComponent<AccessaryItem>().decoItemData = ItemManager.AllDecorationItems[i];
            ShopManager.Instance.accessaryItemList.Add(itemPrefab);
        }
    }

    private void SkinListLoad()
    {
        // TODO
    }
}
