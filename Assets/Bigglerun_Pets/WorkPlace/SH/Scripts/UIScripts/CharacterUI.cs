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
        // 플레이어 데이터에서 현재캐릭터 로드
        currentCharacter = PlayerDataManager.Instance.CurrentPlayerData.currentCharacter;

        // 없거나 비어있으면 기본 캐릭터를 dog로 세팅
        if (string.IsNullOrEmpty(currentCharacter))
        {
            currentCharacter = "dog";
            PlayerDataManager.Instance.SelectCharacter("dog");
        }

        // 해당 캐릭터가 인덱스 몇번째인지 검사
        int index = characterNames.IndexOf(currentCharacter);

        if (index >= 0 && index < toggleList.Count)
        {
            // 해당 인덱스의 토글을 On으로 설정하고, 나머지는 Off
            for (int i = 0; i < toggleList.Count; i++)
            {
                toggleList[i].isOn = (i == index);
            }
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
