using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    public int PlayerGold
    {
        get => PlayerDataManager.Instance.CurrentPlayerData.gold;
    }

    private int PlayerCash
    {
        get => PlayerDataManager.Instance.CurrentPlayerData.diamond;
    }

    public List<GameObject> accessaryItemList = new List<GameObject>();
    public List<GameObject> startItemList = new List<GameObject>();
    public List<GameObject> characterItemList = new List<GameObject>();

    #region Singleton
    public static ShopManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    //아이템 구매(상점 UI에서 적용)
    public bool TryBuyItem(string itemId, bool useCash = false, int amount = 1)
    {
        if (amount <= 0) return false;

        if (ItemManager.Instance.IsDecorationItem(itemId))
        {
            if (ItemManager.Instance.IsUnlockedDecoration(itemId))
            {
                //Debug.Log("이미 해금된 데코 아이템");
                return false;
            }
        }

        if (useCash)
        {
            int cashPrice = ItemManager.Instance.GetCashPrice(itemId) * amount;

            if (cashPrice > 0 && PlayerCash >= cashPrice)
            {
                if (PlayerDataManager.Instance.TrySpendDiamond(cashPrice))
                {
                    GiveItem(itemId, amount);
                    return true;
                }
            }
        }
        else
        {
            int goldPrice = ItemManager.Instance.GetGoldPrice(itemId) * amount;

            if (goldPrice > 0 && PlayerGold >= goldPrice)
            {
                if (PlayerDataManager.Instance.TrySpendGold(goldPrice))
                {
                    GiveItem(itemId, amount);
                    return true;
                }
            }
        }

        return false;
    }

    //아이템 가격에 따라 필터링(캐쉬 전용, 골드 전용, 가격 오름차순으로 정렬)
    public List<ItemData> GetFilteredUsableItems(bool cashOnly = false, bool goldOnly = false, bool sortByPriceAsc = false)
    {
        var items = ItemManager.Instance.GetFilteredUsableItems(timing: ItemUseTiming.PreGame, showInShopOnly: true);

        if(cashOnly)
            items = items.Where(i => i.cashPrice > 0).ToList();
        else if(goldOnly)
            items = items.Where(i => i.goldPrice > 0).ToList();

        if(sortByPriceAsc)
        {
            if(cashOnly)
                items = items.OrderBy(i => i.cashPrice).ToList();
            else
                items = items.OrderBy(i => i.goldPrice).ToList();
        }

        return items;
    }

    //데코 아이템 가격에 따라 필터링(캐쉬 전용, 골드 전용, 가격 오름차순으로 정렬, 이미 해금된건 제외)
    public List<DecorationItemData> GetFilteredDecorationItems(DecorationType? type = null, bool cashOnly = false, bool goldOnly = false, bool sortByPriceAsc = false, bool excludeUnlocked = true)
    {
        var items = ItemManager.Instance.GetFilteredDecorationItems(type: type, showInShopOnly: true);

        if (cashOnly)
            items = items.Where(i => i.cashPrice > 0).ToList();
        else if (goldOnly)
            items = items.Where(i => i.goldPrice > 0).ToList();

        if(excludeUnlocked)
            items = items.Where(i => !ItemManager.Instance.IsUnlockedDecoration(i.itemId)).ToList();

        if (sortByPriceAsc)
        {
            if (cashOnly)
                items = items.OrderBy(i => i.cashPrice).ToList();
            else
                items = items.OrderBy(i => i.goldPrice).ToList();
        }

        return items;
    }

    //아이템 주기
    private void GiveItem(string itemId, int amount = 1)
    {
        if(ItemManager.Instance.IsUsableItem(itemId))
        {
            ItemManager.Instance.AddUsableItems(itemId, amount);
        }
        else if(ItemManager.Instance.IsDecorationItem(itemId))
        {
            ItemManager.Instance.UnlockDecoration(itemId);
        }
    }
}
