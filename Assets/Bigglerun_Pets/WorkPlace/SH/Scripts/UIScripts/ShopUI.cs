using System;
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
    [SerializeField] private GameObject startItemPrefab;
    [SerializeField] private GameObject characterPrefab;
    [SerializeField] private int characterCount;

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
            RefreshDecorationList();
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
        ItemListLoad();
    }

    // 장식 아이템 목록 새로고침
    private void RefreshDecorationList()
    {
        // 기존 목록 클리어 후 다시 로드
        ContentsClear();
        AccessaryListLoad();
    }

    private void RefreshCharacterList()
    {
        ContentsClear();
        CharacterListLoad();
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
        SwitchingTab(index);
        switch (index)
        {
            case 0:
                RefreshDecorationList();
                break;
            case 1:
                //RefreshDecorationList();
                break;
            case 2:
                RefreshItemList();
                break;
            case 3:
                RefreshCharacterList();
                break;
        }
    }

    private void SwitchingTab(int index)
    {
        for(int i = 0; i < mainTab.Length; i++)
        {
            if (i == index)
            {
                mainTab[i].gameObject.SetActive(true);
            }
            else
            {
                mainTab[i].gameObject.SetActive(false);
            }
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

        if (ShopManager.Instance.accessaryItemList.Count > selectedItemIndex && ShopManager.Instance.accessaryItemList[selectedItemIndex] != null)
        {
            var go = ShopManager.Instance.accessaryItemList[selectedItemIndex];
            if (go != null)
            {
                AccessaryItem accessaryItem = go.GetComponent<AccessaryItem>();
                if (accessaryItem != null)
                {
                    GameObject confirm = Instantiate(confirmUI.gameObject, UIManager.Instance.popup);
                    confirm.SetActive(true);
                    confirm.GetComponent<ConfirmUI>().decoItemData = accessaryItem.decoItemData;
                    confirm.GetComponent<ConfirmUI>().itemType = ItemType.Decoration;
                    return;
                }
            }
        }

        if (ShopManager.Instance.startItemList.Count > selectedItemIndex && ShopManager.Instance.startItemList[selectedItemIndex] != null)
        {
            var go = ShopManager.Instance.startItemList[selectedItemIndex];
            if (go != null)
            {
                StartItem startItem = go.GetComponent<StartItem>();
                if (startItem != null)
                {
                    GameObject confirm = Instantiate(confirmUI.gameObject, UIManager.Instance.popup);
                    confirm.SetActive(true);
                    confirm.GetComponent<ConfirmUI>().itemData = startItem.itemData;
                    confirm.GetComponent<ConfirmUI>().itemType = ItemType.UsableItem;
                    return;
                }
            }
        }

        if (ShopManager.Instance.characterItemList.Count > selectedItemIndex && ShopManager.Instance.characterItemList[selectedItemIndex] != null)
        {
            var go = ShopManager.Instance.characterItemList[selectedItemIndex];
            if (go != null)
            {
                CharacterItem characterItem = go.GetComponent<CharacterItem>();
                if (characterItem != null)
                {
                    GameObject confirm = Instantiate(confirmUI.gameObject, UIManager.Instance.popup);
                    confirm.SetActive(true);
                    confirm.GetComponent<ConfirmUI>().characterItemData = characterItem.characterData;
                    confirm.GetComponent<ConfirmUI>().itemType = ItemType.Character;
                    return;
                }
            }
        }

    }

    private void ContentsClear()
    {
        // 액세서리 아이템 리스트 정리
        foreach (var item in ShopManager.Instance.accessaryItemList)
        {
            Destroy(item);
        }
        ShopManager.Instance.accessaryItemList.Clear();

        // 스타트 아이템 리스트 정리
        foreach (var item in ShopManager.Instance.startItemList)
        {
            Destroy(item);
        }
        ShopManager.Instance.startItemList.Clear();

        foreach(var item in ShopManager.Instance.characterItemList)
        {
            Destroy(item);
        }
        ShopManager.Instance.characterItemList.Clear();

        // 콘텐츠 영역 자식 오브젝트 정리
        for (int i = contents.childCount - 1; i >= 0; i--)
        {
            Destroy(contents.GetChild(i).gameObject);
        }
    }

    // 데코레이션 아이템 리스트 로드
    private void AccessaryListLoad()
    {
        for(int i = 0; i < ItemManager.AllDecorationItems.Count; i++)
        {
            GameObject itemPrefab = Instantiate(accessaryPrefab, contents);
            itemPrefab.GetComponent<AccessaryItem>().decoItemData = ItemManager.AllDecorationItems[i];
            ShopManager.Instance.accessaryItemList.Add(itemPrefab);
        }
    }

    // 스킨 아이템 리스트 로드
    private void SkinListLoad()
    {
        // TODO
    }

    // 시작 아이템 리스트 로드
    private void ItemListLoad()
    {
        var itemList = ItemManager.Instance.GetFilteredUsableItems(showInShopOnly: true);
        for (int i = 0; i < itemList.Count; i++)
        {
            GameObject itemPrefab = Instantiate(startItemPrefab, contents);
            itemPrefab.GetComponent<StartItem>().itemData = itemList[i];
            ShopManager.Instance.startItemList.Add(itemPrefab);
        }
    }

    // 캐릭터 아이템 리스트 로드
    private void CharacterListLoad()
    {
        for (int i = 0; i < ItemManager.AllCharacterItems.Count; i++)
        {
            GameObject itemPrefab = Instantiate(characterPrefab, contents);
            itemPrefab.GetComponent<CharacterItem>().characterData = ItemManager.AllCharacterItems[i];
            ShopManager.Instance.characterItemList.Add(itemPrefab);
        }
        //for (int i = 0; i < characterCount; i++)
        //{
        //    GameObject itemPrefab = Instantiate(characterPrefab, contents);
        //    CharacterType type = (CharacterType)i;
        //    itemPrefab.GetComponent<CharacterItem>().characterId = type.ToString();
        //    itemPrefab.GetComponent<CharacterItem>().goldPrice = 100 * i;
        //    itemPrefab.GetComponent<CharacterItem>().cashPrice = 50 * i;
        //    ShopManager.Instance.characterItemList.Add(itemPrefab);
        //}
    }
}
