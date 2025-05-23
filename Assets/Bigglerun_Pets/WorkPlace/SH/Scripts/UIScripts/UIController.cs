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

    /// <summary>
    /// 리더보드 강제 새로고침 (캐시 무효화 후 새로 로드)
    /// </summary>
    public void ForceRefreshLeaderboard()
    {
        // 현재 활성화된 리더보드 UI 컨트롤러 찾기
        LeaderboardUIController leaderboardController = FindObjectOfType<LeaderboardUIController>();
        if (leaderboardController != null)
        {
            leaderboardController.ForceRefreshLeaderboard();
            Debug.Log("[UIController] 리더보드 강제 새로고침을 시작했습니다.");
        }
        else
        {
            Debug.LogWarning("[UIController] LeaderboardUIController를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 특정 플레이어를 리더보드에서 제거 (관리용)
    /// </summary>
    public void RemovePlayerFromLeaderboard(string playerId)
    {
        if (string.IsNullOrEmpty(playerId))
        {
            Debug.LogWarning("[UIController] 플레이어 ID가 비어있습니다.");
            return;
        }

        // 현재 활성화된 리더보드 UI 컨트롤러 찾기
        LeaderboardUIController leaderboardController = FindObjectOfType<LeaderboardUIController>();
        if (leaderboardController != null)
        {
            leaderboardController.RemovePlayerFromLeaderboard(playerId);
            Debug.Log($"[UIController] 플레이어 {playerId} 제거 요청을 보냈습니다.");
        }
        else
        {
            // 리더보드가 열려있지 않다면 LeaderboardManager에서 직접 제거
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.RemovePlayerFromCache(playerId);
                Debug.Log($"[UIController] LeaderboardManager 캐시에서 플레이어 {playerId}를 제거했습니다.");
            }
        }
    }

    /// <summary>
    /// 삭제된 특정 플레이어를 리더보드에서 제거하기 위한 편의 메서드
    /// </summary>
    public void RemoveDeletedPlayer()
    {
        // 문제가 된 플레이어 ID를 직접 제거
        string deletedPlayerId = "vwVfT7pu92dYR6Fmh4YmTE2N4bR2";
        RemovePlayerFromLeaderboard(deletedPlayerId);
        
        // 강제 새로고침도 함께 수행
        ForceRefreshLeaderboard();
        
        Debug.Log($"[UIController] 삭제된 플레이어 {deletedPlayerId}를 제거하고 리더보드를 강제 새로고침했습니다.");
    }
}
