using UnityEngine;

public class TitleUI : MonoBehaviour
{
    public void TogglePopupUI(string uiName)
    {
        Transform ui = FindDirectChildByName(uiName);

        if (ui != null)
        {
            ui.gameObject.SetActive(!ui.gameObject.activeSelf);
        }
    }

    private Transform FindDirectChildByName(string uiName)
    {
        Transform popup = UIManager.Instance.popup;
        foreach (Transform child in popup)
        {
            if (child.name == uiName)
                return child;
        }
        Debug.Log($"{uiName} 라는 이름을 가진 UI가 존재하지 않습니다.");
        return null;
    }

    public void ExitPopupUI()
    {
        Transform popup = UIManager.Instance.popup;
        foreach(Transform child in popup)
        {
            if (child.gameObject.activeSelf)
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void MoveScene(string sceneName)
    {
        GameManager.Instance.SceneFader.LoadScene(sceneName);
    }
}
