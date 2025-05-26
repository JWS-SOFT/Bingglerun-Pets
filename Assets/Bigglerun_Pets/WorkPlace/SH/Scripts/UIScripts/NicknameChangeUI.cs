using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NicknameChangeUI : MonoBehaviour
{
    [SerializeField] private TMP_InputField inputField;
    
    public void NicknameChange()
    {
        Debug.Log($"닉네임 변경 : {inputField.text}");
        PlayerDataManager.Instance.SetNickname(inputField.text);
    }
}
