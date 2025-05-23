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
        switch(sceneName)
        {
            case "TitleScene":
                GameManager.Instance.StateMachine.ChangeState(GameState.Title);
                break;
            case "LobbyScene":
                GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
                break;
            case "StoryStageSelectScene":
                GameManager.Instance.StateMachine.ChangeState(GameState.StoryStageSelect);
                break;
            case "StoryGameScene01":
                GameManager.Instance.StateMachine.ChangeState(GameState.StoryInGame);
                break;
            case "CompetitiveGameScene01":
                GameManager.Instance.StateMachine.ChangeState(GameState.CompetitionInGame);
                break;
        }
    }

    public void UpdateData()
    {

    }

    /// <summary>
    /// 리더보드 UI 열기
    /// </summary>
    public void OpenLeaderboard()
    {
        TogglePopup("LeaderboardUI");
    }

    /// <summary>
    /// 리더보드 새로고침 (리더보드 UI가 열려있을 때 호출)
    /// </summary>
    public void RefreshLeaderboard()
    {
        // 현재 활성화된 리더보드 UI 컨트롤러 찾기
        LeaderboardUIController leaderboardController = FindObjectOfType<LeaderboardUIController>();
        if (leaderboardController != null)
        {
            leaderboardController.RefreshLeaderboard();
        }
        else
        {
            Debug.LogWarning("[UIController] LeaderboardUIController를 찾을 수 없습니다.");
        }
    }
}
