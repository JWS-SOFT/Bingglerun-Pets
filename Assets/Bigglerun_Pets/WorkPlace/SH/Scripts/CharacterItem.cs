using UnityEngine;
using UnityEngine.UI;

public class CharacterItem : MonoBehaviour
{
    public string characterId;
    public Image characterImage;
    public int goldPrice;
    public int cashPrice;
}

public enum CharacterType
{
    dog,
    cat,
    hamster
}

public class CharacterData
{
    public string characterId;
    public int goldPrice;
    public int cashPrice;

}
