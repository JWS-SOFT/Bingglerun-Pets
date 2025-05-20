using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    public TextMeshProUGUI heart;

    private void Start()
    {
        if(heart != null)
        {
            heart.text = PlayerDataManager.Instance.CurrentPlayerData.heart.ToString();
        }
        
    }

    public void TogglePopup(string uiName = "")
    {
        UIManager.Instance.TogglePopupUI(uiName);
    }

    public void ExitButton()
    {
        UIManager.Instance.ExitPopup();
    }

    public void MoveScene(string sceneName)
    {
        GameManager.Instance.SceneFader.LoadScene(sceneName);
    }

    public void UpdateData()
    {

    }
}
