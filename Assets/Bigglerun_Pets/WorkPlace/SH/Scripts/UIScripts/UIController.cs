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
        GameManager.Instance.StateMachine.ChangeState(GameState.CompetitiveSetup);
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
        GameManager.Instance.StateMachine.ChangeState(GameState.CompetitiveSetup);
        GameManager.Instance.SceneFader.LoadScene(sceneName);
    }

    public void UpdateData()
    {

    }
}
