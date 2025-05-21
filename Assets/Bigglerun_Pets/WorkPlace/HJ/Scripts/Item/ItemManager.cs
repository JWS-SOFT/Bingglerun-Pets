using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static UnityEditor.Progress;

public class ItemManager : MonoBehaviour
{
    [Header("소모 아이템")]
    [SerializeField] private List<ItemData> usableItemList;
    private Dictionary<string, int> ownedUsableItems = new Dictionary<string, int>();

    [Header("데코 아이템")]
    [SerializeField] private List<DecorationItemData> decoItemList;
    private HashSet<string> unlockedDecorationIds = new HashSet<string>();
    private Dictionary<DecorationType, string> equippedDecorationIds = new Dictionary<DecorationType, string>();

    //선택된 스타트 아이템
    [SerializeField] private ItemData selectedPreGameItem = null;

    //아이템 이펙트
    private Dictionary<ItemEffectType, Action> itemEffectActions;

    //읽기전용 변수
    public static IReadOnlyList<ItemData> AllUsableItems => Instance.usableItemList;
    public static IReadOnlyList<DecorationItemData> AllDecorationItems => Instance.decoItemList;

    #region Singleton
    public static ItemManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeItems();
        InitializeItemEffects();
    }
    #endregion

    // PlayerDataManager 이벤트 구독
    private void OnEnable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataLoaded += SyncWithPlayerData;
        }
    }

    private void OnDisable()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataLoaded -= SyncWithPlayerData;
        }
    }

    // PlayerDataManager와 데이터 동기화
    public void SyncWithPlayerData()
    {
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
            return;

        var playerData = PlayerDataManager.Instance.CurrentPlayerData;

        // 소모 아이템 동기화
        ownedUsableItems.Clear();
        if (playerData.items != null)
        {
            foreach (var item in playerData.items)
            {
                ownedUsableItems[item.Key] = item.Value;
            }
        }

        // 장식 아이템 동기화
        unlockedDecorationIds.Clear();
        if (playerData.unlockedDecorations != null)
        {
            foreach (var decorationId in playerData.unlockedDecorations)
            {
                unlockedDecorationIds.Add(decorationId);
            }
        }

        // 장착 아이템 동기화
        equippedDecorationIds.Clear();
        if (!string.IsNullOrEmpty(playerData.equippedHat))
            equippedDecorationIds[DecorationType.Hat] = playerData.equippedHat;
        if (!string.IsNullOrEmpty(playerData.equippedBody))
            equippedDecorationIds[DecorationType.Body] = playerData.equippedBody;
        if (!string.IsNullOrEmpty(playerData.equippedShoes))
            equippedDecorationIds[DecorationType.Shoes] = playerData.equippedShoes;

        // 선택된 시작 아이템 설정
        if (!string.IsNullOrEmpty(playerData.selectedPreGameItem))
        {
            selectedPreGameItem = GetUsableItemById(playerData.selectedPreGameItem);
        }
        else
        {
            selectedPreGameItem = null;
        }

        Debug.Log("[ItemManager] 플레이어 데이터와 동기화 완료");
    }

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

        // PlayerDataManager에 저장
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            PlayerDataManager.Instance.AddItem(itemId, amount);
        }
    }

    //초기 아이템 선택(UI에서 초기 아이템 선택시 호출)
    public void SelectPreGameItem(string itemId)
    {
        if (!ownedUsableItems.TryGetValue(itemId, out int count) || count <= 0) return;
        
        ItemData item = GetUsableItemById(itemId);
        if (item == null || item.useTiming != ItemUseTiming.PreGame) return;

        if (selectedPreGameItem?.itemId == itemId)
            selectedPreGameItem = null; //기존 아이템 선택시 해제
        else
            selectedPreGameItem = item;

        // PlayerDataManager에 저장
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            PlayerDataManager.Instance.SelectPreGameItem(selectedPreGameItem?.itemId ?? "");
        }
    }

    //초기 아이템 해제(창이 꺼지는 경우)
    public void PreGameItemInit()
    {
        selectedPreGameItem = null;
        // PlayerDataManager에 저장
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            PlayerDataManager.Instance.SelectPreGameItem(selectedPreGameItem?.itemId ?? "");
        }
    }

    //선택된 스타트 아이템 사용(게임 스타트시 호출함)
    public void UseSelectedPreGameItem()
    {
        if(selectedPreGameItem != null)
        {
            UseUsableItem(selectedPreGameItem.itemId);

            selectedPreGameItem = null;
            PlayerDataManager.Instance.SelectPreGameItem(null);
        }
    }

    //현재 선택된 아이템 가져오기
    public ItemData GetSelectedPreGameItem()
    {
        return selectedPreGameItem;
    }

    //특정 아이템이 선택되었는지 확인
    public bool IsSelectedPreGameItem(string itemId)
    {
        return selectedPreGameItem != null && selectedPreGameItem.itemId == itemId;
    }

    //아이템 사용(인게임 아이템 습득시 호출할 함수, 게임 스타트시 호출됨)
    public void UseUsableItem(string itemId)
    {
        ItemData item = GetUsableItemById(itemId);
        if(item == null) return;

        if(item.useTiming == ItemUseTiming.PreGame)
        {
            if(ownedUsableItems.ContainsKey(itemId))
            {
                ownedUsableItems[itemId]--;

                // PlayerDataManager에서도 아이템 사용
                if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
                {
                    PlayerDataManager.Instance.TryUseItem(itemId);
                }
            }
        }

        ApplyItemEffect(item);
    }

    //아이템 갯수(인벤토리 UI에서 호출)
    public int GetUsableItemCount(string itemId)
    {
        return ownedUsableItems.TryGetValue(itemId, out int count) ? count : 0;
    }

    //데코 장착
    public void EquipDecoration(string itemId)
    {
        var deco = GetDecorationById(itemId);
        if (deco == null || !IsUnlockedDecoration(itemId)) return;

        if (equippedDecorationIds.TryGetValue(deco.type, out var equippedId) && equippedId == itemId)
        {
            Debug.Log($"{deco.type}에 이미 {itemId}가 장착되어 있음");
            return;
        }

        equippedDecorationIds[deco.type] = itemId;

        // PlayerDataManager에도 장착 상태 저장
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            switch (deco.type)
            {
                case DecorationType.Hat:
                    PlayerDataManager.Instance.EquipHat(itemId);
                    break;
                case DecorationType.Body:
                    PlayerDataManager.Instance.EquipBody(itemId);
                    break;
                case DecorationType.Shoes:
                    PlayerDataManager.Instance.EquipShoes(itemId);
                    break;
            }
        }
    }

    //데코 장착 해제
    public void UnequipDecoration(DecorationType type)
    {
        if(equippedDecorationIds.ContainsKey(type))
        {
            equippedDecorationIds.Remove(type);
            Debug.Log($"{type} 장착 해제");

            // PlayerDataManager에도 장착 해제 상태 저장
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
            {
                switch (type)
                {
                    case DecorationType.Hat:
                        PlayerDataManager.Instance.EquipHat("");
                        break;
                    case DecorationType.Body:
                        PlayerDataManager.Instance.EquipBody("");
                        break;
                    case DecorationType.Shoes:
                        PlayerDataManager.Instance.EquipShoes("");
                        break;
                }
            }
        }
        else
        {
            Debug.Log($"{type}에 장착된 아이템 없음");
        }
    }

    //타입으로 장착된 데코 아이템 아이디 가져오기
    public string GetEquippedDecorationId(DecorationType type)
    {
        return equippedDecorationIds.TryGetValue(type, out string id) ? id : null;
    }

    //타입으로 장착된 데코 아이템 가져오기
    public DecorationItemData GetEquippedDecoration(DecorationType type)
    {
        string id = GetEquippedDecorationId(type);
        return id != null ? GetDecorationById(id) : null;
    }

    //소모용 아이템 필터링(인벤토리UI, 상점에서 이용)
    public List<ItemData> GetFilteredUsableItems(bool inventoryOnly = false, ItemUseTiming? timing = null, bool showInShopOnly = false)
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

        // PlayerDataManager에도 해금 상태 저장
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            PlayerDataManager.Instance.UnlockDecoration(itemId);
        }
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
        if(itemEffectActions.TryGetValue(item.effectType, out var effectAction))
        {
            effectAction.Invoke();
            Debug.Log($"{item.effectType} 아이템 효과 적용");
        }
        else
        {
            Debug.Log($"{item.effectType} 아이템 효과 없음");
        }
    }

    //아이템 리스트 초기화
    private void InitializeItems()
    {
        usableItemList = ItemLoader.LoadUsableItemData();
        decoItemList = ItemLoader.LoadDecorationItemData();

        // 게임 매니저 초기화가 완료된 후 PlayerDataManager와 동기화
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            SyncWithPlayerData();
        }
    }

    //아이템 효과 초기화
    private void InitializeItemEffects()
    {
        itemEffectActions = new Dictionary<ItemEffectType, Action>
        {
            {
                ItemEffectType.Booster,
                () => PlayerManager.Instance.StartBooster(1.5f)
            },
            {
                ItemEffectType.SkillUp,
                () => PlayerManager.Instance.AddSkillCount(1)
            },
            {
                ItemEffectType.Heart,
                () => PlayerManager.Instance.AddLife(1)
            },
            {
                ItemEffectType.Invincible,
                () => PlayerManager.Instance.SetInvincible(3f)
            }
        };
    }
}
