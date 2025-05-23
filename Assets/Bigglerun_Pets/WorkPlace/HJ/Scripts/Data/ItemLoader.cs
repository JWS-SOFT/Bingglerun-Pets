using System.Collections.Generic;
using UnityEngine;

public static class ItemLoader
{
    public static List<ItemData> LoadUsableItemData()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/ItemData");
        return JsonUtilityWrapper.FromJsonList<ItemData>(json.text);
    }

    public static List<DecorationItemData> LoadDecorationItemData()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/DecorationItemData");
        return JsonUtilityWrapper.FromJsonList<DecorationItemData>(json.text);
    }

    public static List<CharacterData> LoadCharacterData()
    {
        TextAsset json = Resources.Load<TextAsset>("Data/CharacterData");
        return JsonUtilityWrapper.FromJsonList<CharacterData>(json.text);
    }
}
