using UnityEngine;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private string currentCharacter;
    public void CharacterChangeButton(string characterId)
    {
        PlayerDataManager.Instance.SelectCharacter(characterId);
        currentCharacter = characterId;
        Debug.Log($"현재 선택된 캐릭터 : {characterId}");
    }
}
