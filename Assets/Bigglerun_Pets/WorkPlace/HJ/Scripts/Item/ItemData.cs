using System;
using UnityEngine;

[Serializable]
public class ItemData
{
    public string itemId;
    public string itemName;
    public ItemUseTiming useTiming;
    public ItemEffectType effectType;
    public string iconPath;
    public bool showInShop = true;

    public int goldPrice;
    public int cashPrice;
}

[Serializable]
public class DecorationItemData
{
    public string itemId;
    public string itemName;
    public string iconPath;
    public string prefabPath;   //캐릭터에 장착되는 모델
    public DecorationType type;
    public bool showInShop = true;

    public int goldPrice;
    public int cashPrice;
}

public enum ItemUseTiming
{
    PreGame,
    InGame
}

public enum ItemEffectType
{
    Booster,    //초반 부스터
    SkillUp,    //스킬횟수 추가
    Heart,      //목숨 1회 추가
    Invincible  //일정 시간 무적
}

public enum DecorationType
{
    Hat,
    Body,
    Shoes
}

public enum ItemType
{
    Decoration,
    Skin,
    UsableItem
}
