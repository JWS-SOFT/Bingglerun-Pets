using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
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
}
