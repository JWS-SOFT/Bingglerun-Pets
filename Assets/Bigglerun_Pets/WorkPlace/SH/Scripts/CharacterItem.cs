using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterItem : MonoBehaviour
{
    public CharacterData characterData;
}

public enum CharacterType
{
    dog,
    cat,
    hamster
}

[Serializable]
public class CharacterData
{
    public string characterId;
    public int goldPrice;
    public int cashPrice;
    public string iconPath;
}
