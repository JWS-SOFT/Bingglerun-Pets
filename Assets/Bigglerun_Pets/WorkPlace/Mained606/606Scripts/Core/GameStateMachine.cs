using UnityEngine;

/// <summary>
/// 게임의 전체 상태 전환을 제어하는 클래스
/// </summary>
public class GameStateMachine : MonoBehaviour
{
    public GameState CurrentState { get; private set; } = GameState.None;

    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState)
            return;

        Debug.Log($"[GameStateMachine] 상태 전환: {CurrentState} → {newState}");

        ExitState(CurrentState);
        CurrentState = newState;
        EnterState(CurrentState);
    }

    private void EnterState(GameState state)
    {
        switch (state)
        {
            case GameState.Init:
                GameManager.Instance.InitializeFirebase();
                break;

            case GameState.Title:
                UIManager.Instance.ShowTitleUI();
                break;

            case GameState.Lobby:
                UIManager.Instance.ShowLobbyUI();
                break;

            case GameState.ModeSelect:
                UIManager.Instance.ShowModeSelectUI();
                break;

            case GameState.StoryStageSelect:
                UIManager.Instance.ShowStoryStageSelectUI();
                break;

            case GameState.CompetitiveSetup:
                UIManager.Instance.ShowCompetitiveSetupUI();
                break;

            case GameState.InGame:
                UIManager.Instance.HideAll();
                GameManager.Instance.SceneFader.LoadScene("InGame");
                break;

            case GameState.Result:
                UIManager.Instance.ShowResultUI();
                break;

            case GameState.Loading:
                // 로딩 처리
                break;

            case GameState.Pause:
                UIManager.Instance.ShowPauseMenu();
                break;
        }
    }

    private void ExitState(GameState state)
    {
        // 상태 종료 시 필요한 정리 로직
    }
}