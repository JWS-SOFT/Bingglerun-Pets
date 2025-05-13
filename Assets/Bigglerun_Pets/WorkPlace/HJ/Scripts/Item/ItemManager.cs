using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    [Header("소모 아이템")]
    [SerializeField] private List<ItemData> usableItemList;   //데이터 베이스에 따라 추후 수정
    private Dictionary<string, int> ownedUsableItems = new Dictionary<string, int>();

    [Header("데코 아이템")]
    [SerializeField] private List<DecorationItemData> decoItemList; //데이터 베이스에 따라 추후 수정
    private HashSet<string> unlockedDecorationIds = new HashSet<string>();

    //선택된 스타트 아이템
    private ItemData selectedPreGameItem = null;
    public ItemData SelectedPreGameItem => selectedPreGameItem;

    //읽기전용 변수
    public static IReadOnlyList<ItemData> AllUsableItems => Instance.usableItemList;
    public static IReadOnlyList<DecorationItemData> AllDecorationItems => Instance.decoItemList;

    #region Singleton
    public static ItemManager Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeItems();
        }
        else
            Destroy(gameObject);
    }
    #endregion

    //아이템 유효성 검사
    public bool IsUsableItem(string itemId) => GetUsableItemById(itemId) != null;
    public bool IsDecorationItem(string itemId) => GetDecorationById(itemId) != null;

    //아이템 추가(상점에서 아이템 구매, 아이템 습득시 호출)
    public void AddUsableItems(string itemId, int amount = 1)
    {
        ItemData item = GetUsableItemById(itemId);

        if (item == null) return;

        if(ownedUsableItems.ContainsKey(itemId))
            ownedUsableItems[itemId] += amount;
        else
            ownedUsableItems[itemId] = amount;
    }

    //초기 아이템 선택(UI에서 초기 아이템 선택시 호출)
    public void SelectPreGameItem(string itemId)
    {
        if (!ownedUsableItems.TryGetValue(itemId, out int count) || count <= 0) return;

        ItemData item = GetUsableItemById(itemId);
        if (item == null || item.useTiming != ItemUseTiming.PreGame) return;

        if (SelectedPreGameItem?.itemId == itemId)
            selectedPreGameItem = null; //기존 아이템 선택시 해제
        else
            selectedPreGameItem = item;     
    }

    //선택된 스타트 아이템 사용(게임 스타트시 호출함)
    public void UseSelectedPreGameItem()
    {
        if(selectedPreGameItem == null) return;

        UseUsableItem(selectedPreGameItem.itemId);
    }

    //아이템 사용(인게임 아이템 습득시 호출할 함수, 게임 스타트시 호출됨)
    public void UseUsableItem(string itemId)
    {
        ItemData item = GetUsableItemById(itemId);
        if(item == null) return;

        if(item.useTiming == ItemUseTiming.PreGame) ownedUsableItems[itemId]--;

        ApplyItemEffect(item);
    }

    //아이템 갯수(인벤토리 UI에서 호출)
    public int GetUsableItemCount(string itemId)
    {
        return ownedUsableItems.TryGetValue(itemId, out int count) ? count : 0;
    }

    //소모용 아이템 필터링(인벤토리UI, 상점에서 이용)
    public List<ItemData> GetFilteredUsableItems(bool inventoryOnly = false, ItemUseTiming ? timing = null, bool showInShopOnly = false)
    {
        var items = usableItemList;

        if (inventoryOnly)
            items = items.Where(i => GetUsableItemCount(i.itemId) > 0).ToList();

        if (timing.HasValue)
            items = items.Where(i => i.useTiming == timing.Value).ToList();

        if (showInShopOnly)
            items = items.Where(i => i.showInShop).ToList();

        return items;
    }

    //데코 아이템 필터링(인벤토리UI, 상점에서 이용)
    public List<DecorationItemData> GetFilteredDecorationItems(bool inventoryOnly = false, DecorationType? type = null, bool showInShopOnly = false)
    {
        var items = decoItemList;

        if (inventoryOnly)
            items = items.Where(d => unlockedDecorationIds.Contains(d.itemId)).ToList();

        if (type.HasValue)
            items = items.Where(d => d.type == type.Value).ToList();

        if (showInShopOnly)
            items = items.Where(d => d.showInShop).ToList();

        return items;
    }

    //데코 아이템 해금(상점에서 구매시, 조건 달성시 호출)
    public void UnlockDecoration(string itemId)
    {
        unlockedDecorationIds.Add(itemId);
    }

    //데코 아이템 해금 여부(상점 UI에서 호출)
    public bool IsUnlockedDecoration(string itemId)
    {
        return unlockedDecorationIds.Contains(itemId);
    }    

    //가격 가져오기(상점, UI에서 사용)
    public int GetGoldPrice(string itemId)
    {
        var item = GetUsableItemById(itemId);
        var deco = GetDecorationById(itemId);
        return item?.goldPrice ?? deco?.goldPrice ?? -1;
    }

    //캐쉬 가격 가져오기(상점, UI에서 사용)
    public int GetCashPrice(string itemId)
    {
        var item = GetUsableItemById(itemId);
        var deco = GetDecorationById(itemId);
        return item?.cashPrice ?? deco?.cashPrice ?? -1;
    }

    //캐쉬 아이템 여부(상점? 쓸지 안쓸지 모름)
    public bool IsCashItem(string itemId) => GetCashPrice(itemId) > 0;

    #region Private Method
    //아이템 아이디로 찾기
    private ItemData GetUsableItemById(string itemId)
    {
        return usableItemList.Find(i => i.itemId == itemId);
    }

    //데코 아이템 아이디로 찾기
    private DecorationItemData GetDecorationById(string itemId)
    {
        return decoItemList.Find(d => d.itemId == itemId);
    }

    //아이템 효과 적용
    private void ApplyItemEffect(ItemData item)
    {
        switch (item.effectType)
        {
            case ItemEffectType.Booster:
                Debug.Log("부스터 적용");
                break;
            case ItemEffectType.SkillUp:
                Debug.Log("스킬 횟수 +1");
                break;
            case ItemEffectType.Heart:
                Debug.Log("목숨 1회 구제");
                break;
            case ItemEffectType.Invincible:
                Debug.Log("무적");
                break;
        }
    }

    private void InitializeItems()
    {
        usableItemList = ItemLoader.LoadUsableItemData();
        decoItemList = ItemLoader.LoadDecorationItemData();
    }

    #endregion
}
