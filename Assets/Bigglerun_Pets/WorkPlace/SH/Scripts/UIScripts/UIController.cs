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

        //스토리모드 셀렉트 상태일 땐 그대로 스토리모드 셀렉트 상태로 두고
        //경쟁모드 셀렉트 상태일 땐 경쟁모드 셀렉트 상태로 둔다.
        if(GameManager.Instance.StateMachine.CurrentState != GameState.StoryStageSelect)
        {
            GameManager.Instance.StateMachine.ChangeState(GameState.CompetitiveSetup);
        }
    }

    public void ExitButton()
    {
        UIManager.Instance.ExitPopup();
    }

    public void StoryGameMoveScene(string sceneName)
    {
        GameManager.Instance.StateMachine.ChangeState(GameState.StoryStageSelect);
        GameManager.Instance.SceneFader.LoadScene(sceneName);
    }

    public void CompetitiveGameMoveScene(string sceneName)
    {
        GameManager.Instance.StateMachine.ChangeState(GameState.CompetitionInGame);
        GameManager.Instance.SceneFader.LoadScene(sceneName);
    }

    public void MoveScene(string sceneName)
    {
        GameManager.Instance.SceneFader.LoadScene(sceneName);
    }

    public void UpdateData()
    {

    }
}
