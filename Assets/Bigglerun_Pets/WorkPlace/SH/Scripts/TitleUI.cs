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
        // 씬 이름에 따라 적절한 게임 상태로 변경
        switch (sceneName)
        {
            case "LobbyScene":
                // 로비씬으로 전환 시 GameState.Lobby로 상태 변경
                // 이제 씬 전환은 GameStateMachine에서 처리됨
                GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
                break;
                
            case "TitleScene":
                GameManager.Instance.StateMachine.ChangeState(GameState.Title);
                break;
                
            case "InGame":
                GameManager.Instance.StateMachine.ChangeState(GameState.InGame);
                break;
                
            default:
                // 매핑되지 않은 씬은 기존 방식으로 직접 로드
                Debug.Log($"[TitleUI] 상태 매핑이 없는 씬으로 직접 전환: {sceneName}");
                GameManager.Instance.SceneFader.LoadScene(sceneName);
                break;
        }
    }
}
