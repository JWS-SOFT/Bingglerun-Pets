using UnityEngine;

public class ButtonLockedUI : MonoBehaviour
{
    private GameObject lockedPopup;

    public void SetLockedButton(GameObject popup)
    {
        lockedPopup = popup;
    }

    public void ClickButton()
    {
        if (lockedPopup.activeSelf) lockedPopup.SetActive(false);
        lockedPopup.SetActive(true);
    }
}
