using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyHud : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI userName;

    private void Start()
    {
        PlayerDataManager.Instance.OnNicknameChanged += RenewalNickname;
        if (PlayerDataManager.Instance != null && userName != null)
        {
            userName.text = PlayerDataManager.Instance.CurrentPlayerData.nickname;
        }
    }

    private void OnDisable()
    {
        PlayerDataManager.Instance.OnNicknameChanged -= RenewalNickname;
    }

    private void RenewalNickname(string newName)
    {
        if(userName != null)
        {
            userName.text = newName;
        }
    }
}
