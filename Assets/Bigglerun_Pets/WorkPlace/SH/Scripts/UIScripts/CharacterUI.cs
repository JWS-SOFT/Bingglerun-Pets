using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.UI;

public class CharacterUI : MonoBehaviour
{
    [SerializeField] private string currentCharacter;

    [SerializeField] private List<Button> buttonList = new List<Button>();
    [SerializeField] private List<Toggle> toggleList = new List<Toggle>();
    [SerializeField] private List<string> characterNames = new List<string>();

    public string SelectedCharacterName { get; private set; } = null;

    private void OnEnable()
    {
        currentCharacter = PlayerDataManager.Instance.CurrentPlayerData.currentCharacter;
        if(currentCharacter == null)
        {
            currentCharacter = "dog";
            PlayerDataManager.Instance.SelectCharacter("dog");
        }
    }

    private void Start()
    {
        for (int i = 0; i < buttonList.Count; i++)
        {
            int index = i;
            buttonList[i].onClick.AddListener(() => OnToggleChanged(index));
        }
    }

    private void OnToggleChanged(int changedIndex)
    {
        if (!toggleList[changedIndex].isOn)
        {
            toggleList[changedIndex].isOn = true;
            // 다른 토글은 해제
            for (int i = 0; i < toggleList.Count; i++)
            {
                if (i != changedIndex)
                    toggleList[i].isOn = false;
            }
        }
        SelectedCharacterName = characterNames[changedIndex];
        PlayerDataManager.Instance.SelectCharacter(SelectedCharacterName);
        Debug.Log($"현재 선택된 캐릭터 : {SelectedCharacterName}");
    }
}
