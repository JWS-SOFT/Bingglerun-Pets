using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ItemManager : MonoBehaviour
{
    public static ItemManager Instance;

    [Header("소모 아이템")]
    [SerializeField] private List<ItemData> itemList;   //데이터 베이스에 따라 추후 수정
    private Dictionary<string, int> ownedItems = new Dictionary<string, int>();

    private ItemData selectedPreGameItem = null;
    public ItemData SelectedPreGameItem
    {
        get { return selectedPreGameItem; }
        private set { selectedPreGameItem = value; }
    }

    [Header("데코 아이템")]
    [SerializeField] private List<DecorationItemData> decoItemList; //데이터 베이스에 따라 추후 수정
    private HashSet<string> unlockedDecorationIds = new HashSet<string>();

    public static List<ItemData> ItemDataBase => Instance.itemList; //데이터 베이스에 따라 추후 수정
    public static List<DecorationItemData> DeciDecorationItemDataBase => Instance.decoItemList;


    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    //아이템 추가
    public void AddItems(string itemId, int amount = 1)
    {
        ItemData item = GetItem(itemId);

        if (item == null) return;

        if(ownedItems.ContainsKey(itemId))
            ownedItems[itemId] += amount;
        else
            ownedItems[itemId] = amount;
    }

    //초기 선택용 아이템
    public void SelectPreGameItem(string itemId)
    {
        if (!ownedItems.TryGetValue(itemId, out int count) || count <= 0) return;

        ItemData item = GetItem(itemId);
        if (item == null || item.uesTiming != ItemUseTiming.PreGame) return;

        if (SelectedPreGameItem?.itemId == itemId)
        {
            SelectedPreGameItem = null; //기존 아이템 선택시 해제
        }
        else
        {
            SelectedPreGameItem = item;
        }        
    }

    //스타트 아이템 사용
    public void UsePreGateItem(string itemId)
    {

    }

    //인게임 아이템 사용
    public void UseInGameId(string itemId)
    {

    }
    
    //아이템 효과 적용
    private void ApplyItemEffect(ItemData item)
    {
        
    }

    //아이템 아이디로 찾기
    private ItemData GetItem(string itemId)
    {
        return ItemDataBase.Find(i => i.itemId == itemId);
    }
}
