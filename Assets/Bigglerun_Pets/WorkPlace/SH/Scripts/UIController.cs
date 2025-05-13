using UnityEngine;
using UnityEngine.UIElements;

public class UIController : MonoBehaviour
{
    public void TogglePopup(string uiName = "")
    {
        if(uiName != "")
        {
            UIManager.Instance.TogglePopupUI(uiName);
        }
    }

    public void ExitButton(Transform button)
    {
        Debug.Log($"{button.name} ExitButton 눌림");
        UIManager.Instance.ExitPopup();
    }
}
