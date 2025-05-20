using System.Collections.Generic;
using UnityEngine;

public class ItemTest : MonoBehaviour
{
    [SerializeField] private string itemId = "item001";
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        InitTestData();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) //아이템 필터링 테스트
        {
            //var shopItems = ShopManager.Instance.GetFilteredUsableItems();
            //Debug.Log("[상점 - 전체 소모 아이템 목록]");
            //foreach (var item in shopItems)
            //    Debug.Log($"{item.itemName} | Gold: {item.goldPrice}, Cash: {item.cashPrice}");

            ItemManager.Instance.SelectPreGameItem(itemId);
            Debug.Log($"{itemId} 초기 아이템 설정");
        }

        if (Input.GetKeyDown(KeyCode.Alpha2)) //골드 구매 테스트
        {
            bool result = ShopManager.Instance.TryBuyItem("item001", useCash: false);
            Debug.Log("골드 구매 성공 여부: " + result);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3)) //캐시 구매 테스트
        {
            bool result = ShopManager.Instance.TryBuyItem("item002", useCash: true);
            Debug.Log("캐시 구매 성공 여부: " + result);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4)) //아이템 사용 테스트
        {
            //ItemManager.Instance.UseUsableItem("item001");
            Debug.Log(ItemManager.Instance.GetSelectedPreGameItem().itemName);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5)) //인벤토리 내 아이템 필터링
        {
            var inventoryItems = ItemManager.Instance.GetFilteredUsableItems(inventoryOnly: true);
            Debug.Log("[인벤토리 보유 아이템]");
            foreach (var item in inventoryItems)
                Debug.Log($"{item.itemName} (보유량: {ItemManager.Instance.GetUsableItemCount(item.itemId)})");
        }

        if (Input.GetKeyDown(KeyCode.Alpha6)) //상점 - 골드 아이템 필터링
        {
            var goldItems = ShopManager.Instance.GetFilteredUsableItems(goldOnly: true);
            Debug.Log("[상점 - 골드 전용 아이템 목록]");
            foreach (var item in goldItems)
                Debug.Log($"{item.itemName} | Gold: {item.goldPrice}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha7)) //상점 - 캐시 아이템 필터링
        {
            var cashItems = ShopManager.Instance.GetFilteredUsableItems(cashOnly: true);
            Debug.Log("[상점 - 캐시 전용 아이템 목록]");
            foreach (var item in cashItems)
                Debug.Log($"{item.itemName} | Cash: {item.cashPrice}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha8)) //상점 - 오름차순 정렬
        {
            var sortedItems = ShopManager.Instance.GetFilteredUsableItems(sortByPriceAsc: true);
            Debug.Log("[상점 - 가격 오름차순 정렬]");
            foreach (var item in sortedItems)
                Debug.Log($"{item.itemName} | G:{item.goldPrice} / C:{item.cashPrice}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha9)) //상점 - 데코 골드 아이템 필터링
        {
            var decoGoldItems = ShopManager.Instance.GetFilteredDecorationItems(goldOnly: true);
            Debug.Log("[상점 - 데코 골드 아이템 목록]");
            foreach (var item in decoGoldItems)
                Debug.Log($"{item.itemName} | Gold: {item.goldPrice}");
        }

        if (Input.GetKeyDown(KeyCode.Alpha0)) //상점 - 데코 캐시 아이템 필터링
        {
            var decoCashItems = ShopManager.Instance.GetFilteredDecorationItems(cashOnly: true);
            Debug.Log("[상점 - 데코 캐시 아이템 목록]");
            foreach (var item in decoCashItems)
                Debug.Log($"{item.itemName} | Cash: {item.cashPrice}");
        }



        if (Input.GetKeyDown(KeyCode.Q))    //데코아이템 구매
        {
            bool result = ShopManager.Instance.TryBuyItem("hat001", useCash: false);
            Debug.Log("데코 아이템 구매 성공 여부: " + result);
        }

        if(Input.GetKeyDown(KeyCode.W))     //데코아이템 장착
        {
            string itemId = "hat001";
            bool unlocked = ItemManager.Instance.IsUnlockedDecoration(itemId);
            if (unlocked)
            {
                ItemManager.Instance.EquipDecoration(itemId);
                var deco = ItemManager.Instance.GetEquippedDecoration(DecorationType.Hat);
                Debug.Log($"데코 아이템 장착됨: {deco.itemName}");
            }
            else
            {
                Debug.Log("데코 아이템 언락되지 않음");
            }
        }

        if (Input.GetKeyDown(KeyCode.E)) //장착 해제
        {
            ItemManager.Instance.UnequipDecoration(DecorationType.Hat);
        }

        if (Input.GetKeyDown(KeyCode.R))    //장착된 데코 아이템 검색
        {
            string itemId = ItemManager.Instance.GetEquippedDecorationId(DecorationType.Hat);

            Debug.Log($"Hat에 장착된 아이템 {itemId}");
        }
    }

    private void InitTestData()
    {
        //ItemManager.Instance.usableItemList = new List<ItemData>
        //{
        //    new ItemData { itemId = "item001", itemName = "Booster", useTiming = ItemUseTiming.PreGame, effectType = ItemEffectType.Booster, goldPrice = 500, cashPrice = 0, showInShop = true },
        //    new ItemData { itemId = "item002", itemName = "Heart", useTiming = ItemUseTiming.PreGame, effectType = ItemEffectType.Heart, goldPrice = 0, cashPrice = 10, showInShop = true },
        //    new ItemData { itemId = "item003", itemName = "SkillUp", useTiming = ItemUseTiming.PreGame, effectType = ItemEffectType.SkillUp, goldPrice = 800, cashPrice = 0, showInShop = true },
        //    new ItemData { itemId = "item004", itemName = "Invincible", useTiming = ItemUseTiming.InGame, effectType = ItemEffectType.Invincible, goldPrice = 1000, cashPrice = 5, showInShop = false },
        //    new ItemData { itemId = "item005", itemName = "Debug Only", useTiming = ItemUseTiming.PreGame, effectType = ItemEffectType.SkillUp, goldPrice = 0, cashPrice = 0, showInShop = false },
        //};

        //ItemManager.Instance.decoItemList = new List<DecorationItemData>
        //{
        //    new DecorationItemData { itemId = "hat001", itemName = "Red Hat", type = DecorationType.Hat, goldPrice = 1000, cashPrice = 0, showInShop = true },
        //    new DecorationItemData { itemId = "body001", itemName = "Blue Body", type = DecorationType.Body, goldPrice = 0, cashPrice = 15, showInShop = true },
        //    new DecorationItemData { itemId = "shoes001", itemName = "Green Shoes", type = DecorationType.Shoes, goldPrice = 800, cashPrice = 5, showInShop = true },
        //};

        //보유 아이템 추가 (초기 인벤토리 상태 설정)
        ItemManager.Instance.AddUsableItems("item001", 2);
        ItemManager.Instance.AddUsableItems("item002", 2);
        ItemManager.Instance.AddUsableItems("item003", 5);
    }
}
