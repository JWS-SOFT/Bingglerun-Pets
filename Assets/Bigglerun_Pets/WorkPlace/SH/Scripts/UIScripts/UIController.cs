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
}
