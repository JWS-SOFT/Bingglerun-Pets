using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

/// <summary>
/// 리더보드 디버깅 및 관리용 스크립트
/// 삭제된 플레이어 제거, 캐시 관리 등의 기능 제공
/// </summary>
public class LeaderboardDebugger : MonoBehaviour
{
    [Header("Debug UI")]
    public Button forceRefreshButton;
    public Button removeDeletedPlayerButton;
    public Button clearCacheButton;
    public TextMeshProUGUI statusText;
    public TMP_InputField playerIdInput;
    public Button removeSpecificPlayerButton;

    [Header("Debug Info")]
    public TextMeshProUGUI debugInfoText;

    private void Start()
    {
        SetupButtons();
        UpdateDebugInfo();
    }

    private void SetupButtons()
    {
        if (forceRefreshButton != null)
        {
            forceRefreshButton.onClick.AddListener(ForceRefreshLeaderboard);
        }

        if (removeDeletedPlayerButton != null)
        {
            removeDeletedPlayerButton.onClick.AddListener(RemoveKnownDeletedPlayer);
        }

        if (clearCacheButton != null)
        {
            clearCacheButton.onClick.AddListener(ClearLeaderboardCache);
        }

        if (removeSpecificPlayerButton != null)
        {
            removeSpecificPlayerButton.onClick.AddListener(RemoveSpecificPlayer);
        }
    }

    /// <summary>
    /// 리더보드 강제 새로고침
    /// </summary>
    public void ForceRefreshLeaderboard()
    {
        UpdateStatus("리더보드 강제 새로고침 중...");
        
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.InvalidateCache();
            
            // UI Controller를 통해 새로고침
            UIController uiController = FindObjectOfType<UIController>();
            if (uiController != null)
            {
                uiController.ForceRefreshLeaderboard();
                UpdateStatus("강제 새로고침 완료!");
            }
            else
            {
                UpdateStatus("UIController를 찾을 수 없습니다.");
            }
        }
        else
        {
            UpdateStatus("LeaderboardManager를 찾을 수 없습니다.");
        }
        
        UpdateDebugInfo();
    }

    /// <summary>
    /// 알려진 삭제된 플레이어 제거
    /// </summary>
    public void RemoveKnownDeletedPlayer()
    {
        string deletedPlayerId = "vwVfT7pu92dYR6Fmh4YmTE2N4bR2";
        UpdateStatus($"삭제된 플레이어 {deletedPlayerId} 제거 중...");
        
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.RemovePlayerFromCache(deletedPlayerId);
            
            // UI에서도 제거
            LeaderboardUIController leaderboardUI = FindObjectOfType<LeaderboardUIController>();
            if (leaderboardUI != null)
            {
                leaderboardUI.RemovePlayerFromLeaderboard(deletedPlayerId);
            }
            
            UpdateStatus($"플레이어 {deletedPlayerId} 제거 완료!");
        }
        else
        {
            UpdateStatus("LeaderboardManager를 찾을 수 없습니다.");
        }
        
        UpdateDebugInfo();
    }

    /// <summary>
    /// 특정 플레이어 제거 (입력 필드 사용)
    /// </summary>
    public void RemoveSpecificPlayer()
    {
        if (playerIdInput != null && !string.IsNullOrEmpty(playerIdInput.text))
        {
            string playerId = playerIdInput.text.Trim();
            UpdateStatus($"플레이어 {playerId} 제거 중...");
            
            if (LeaderboardManager.Instance != null)
            {
                LeaderboardManager.Instance.RemovePlayerFromCache(playerId);
                
                // UI에서도 제거
                LeaderboardUIController leaderboardUI = FindObjectOfType<LeaderboardUIController>();
                if (leaderboardUI != null)
                {
                    leaderboardUI.RemovePlayerFromLeaderboard(playerId);
                }
                
                UpdateStatus($"플레이어 {playerId} 제거 완료!");
                playerIdInput.text = "";
            }
            else
            {
                UpdateStatus("LeaderboardManager를 찾을 수 없습니다.");
            }
        }
        else
        {
            UpdateStatus("플레이어 ID를 입력해주세요.");
        }
        
        UpdateDebugInfo();
    }

    /// <summary>
    /// 리더보드 캐시 완전 삭제
    /// </summary>
    public void ClearLeaderboardCache()
    {
        UpdateStatus("리더보드 캐시 삭제 중...");
        
        if (LeaderboardManager.Instance != null)
        {
            LeaderboardManager.Instance.InvalidateCache();
            UpdateStatus("캐시 삭제 완료!");
        }
        else
        {
            UpdateStatus("LeaderboardManager를 찾을 수 없습니다.");
        }
        
        UpdateDebugInfo();
    }

    /// <summary>
    /// 상태 텍스트 업데이트
    /// </summary>
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
        Debug.Log($"[LeaderboardDebugger] {message}");
    }

    /// <summary>
    /// 디버그 정보 업데이트
    /// </summary>
    private void UpdateDebugInfo()
    {
        if (debugInfoText == null) return;

        string info = "=== 리더보드 디버그 정보 ===\n";
        
        if (LeaderboardManager.Instance != null)
        {
            var cachedData = LeaderboardManager.Instance.GetCachedLeaderboard();
            info += $"캐시된 플레이어 수: {cachedData.Count}\n";
            
            if (cachedData.Count > 0)
            {
                info += "상위 5명:\n";
                for (int i = 0; i < Mathf.Min(5, cachedData.Count); i++)
                {
                    var player = cachedData[i];
                    info += $"{i + 1}. {player.nickname} ({player.playerId}) - {player.competitiveBestScore}점\n";
                }
            }
            
            // 문제가 된 플레이어가 캐시에 있는지 확인
            bool hasDeletedPlayer = cachedData.Exists(p => p.playerId == "vwVfT7pu92dYR6Fmh4YmTE2N4bR2");
            info += $"\n삭제된 플레이어 vwVfT7pu92dYR6Fmh4YmTE2N4bR2 캐시 상태: {(hasDeletedPlayer ? "존재함 ⚠️" : "없음 ✅")}\n";
        }
        else
        {
            info += "LeaderboardManager 없음\n";
        }
        
        info += $"\n현재 게임 상태: {(GameManager.Instance?.StateMachine?.CurrentState.ToString() ?? "Unknown")}\n";
        info += $"업데이트 시간: {System.DateTime.Now:HH:mm:ss}";
        
        debugInfoText.text = info;
    }

    /// <summary>
    /// 버튼으로 호출할 수 있는 디버그 정보 업데이트
    /// </summary>
    public void RefreshDebugInfo()
    {
        UpdateDebugInfo();
        UpdateStatus("디버그 정보 새로고침 완료");
    }

    private void Update()
    {
        // 5초마다 자동으로 디버그 정보 업데이트
        if (Time.frameCount % 300 == 0) // 60fps 기준 5초
        {
            UpdateDebugInfo();
        }
    }
} 