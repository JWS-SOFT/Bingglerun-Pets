using UnityEngine;

public class ItemData
{
    public string itemId;
    public string itemName;
    public ItemUseTiming uesTiming;
    public EffectType effectType;
    public Sprite icon;
    public bool CanBying;

    public int goldPrice;
    public int cashPrice;
}

public class DecorationItemData
{
    public string id;
    public string name;
    public string icon;
    public GameObject prefab;   //캐릭터에 장착되는 모델
    public DecorationType type;
    public bool isUnlocked = false;

    public int goldPrice;
    public int cashPrice;
}

public enum ItemUseTiming
{
    PreGame,
    InGame
}

public enum EffectType
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
    Accessory
}
