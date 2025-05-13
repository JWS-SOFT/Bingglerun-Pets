using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

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
                // UI 매니저에 상태 변경 알림
                UIManager.Instance.HandleGameStateChange(state);
                break;

            case GameState.Lobby:
                // 현재 씬이 LobbyScene이 아니라면 씬 전환
                if (SceneManager.GetActiveScene().name != "606TestLobby")
                {
                    GameManager.Instance.SceneFader.LoadScene("606TestLobby");
                    // 씬 로드 완료 후 UI 처리를 위한 코루틴 시작
                    StartCoroutine(WaitForSceneLoad(() => UIManager.Instance.HandleGameStateChange(state)));
                }
                else
                {
                    // 이미 로비 씬인 경우 바로 UI 처리
                    UIManager.Instance.HandleGameStateChange(state);
                }
                break;

            case GameState.ModeSelect:
                UIManager.Instance.HandleGameStateChange(state);
                break;

            case GameState.StoryStageSelect:
                UIManager.Instance.HandleGameStateChange(state);
                break;

            case GameState.CompetitiveSetup:
                UIManager.Instance.HandleGameStateChange(state);
                break;

            case GameState.InGame:
                // UI 처리 후 씬 전환
                UIManager.Instance.HandleGameStateChange(state);
                
                // 현재 씬이 InGame이 아니라면 씬 전환
                if (SceneManager.GetActiveScene().name != "InGame")
                {
                    GameManager.Instance.SceneFader.LoadScene("InGame");
                }
                break;

            case GameState.Result:
                UIManager.Instance.HandleGameStateChange(state);
                break;

            case GameState.Loading:
                // 로딩 처리
                break;

            case GameState.Pause:
                UIManager.Instance.HandleGameStateChange(state);
                break;
        }
    }

    /// <summary>
    /// 씬 로드 완료를 기다린 후 콜백 실행
    /// </summary>
    private IEnumerator WaitForSceneLoad(System.Action onComplete)
    {
        // 한 프레임 대기 (씬 로드 완료 확인을 위해)
        yield return null;
        
        // 추가 대기를 위한 시간 설정 (필요한 경우 조정)
        yield return new WaitForSeconds(0.1f);
        
        // 콜백 실행
        onComplete?.Invoke();
        
        Debug.Log("[GameStateMachine] 씬 로드 후 UI 업데이트 완료");
    }

    private void ExitState(GameState state)
    {
        // 필요시 상태 종료 처리 추가
    }
}