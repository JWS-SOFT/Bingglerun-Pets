using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 친구 목록 아이템 UI 컴포넌트
/// </summary>
public class FriendListItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI lastLoginText;
    [SerializeField] private Image characterIcon;
    [SerializeField] private Image onlineStatusIcon;
    [SerializeField] private Button compareButton;
    [SerializeField] private Button removeButton;
    [SerializeField] private GameObject onlineIndicator;

    // 이벤트 콜백들
    private Action<string> onRemoveClicked;
    private Action<string> onCompareClicked;
    
    // 현재 친구 정보
    private FriendInfo currentFriend;

    private void Awake()
    {
        // 버튼 이벤트 연결
        compareButton?.onClick.AddListener(OnCompareButtonClicked);
        removeButton?.onClick.AddListener(OnRemoveButtonClicked);
    }

    /// <summary>
    /// 친구 정보로 UI 설정
    /// </summary>
    public void Setup(FriendInfo friend, Action<string> removeCallback, Action<string> compareCallback)
    {
        currentFriend = friend;
        onRemoveClicked = removeCallback;
        onCompareClicked = compareCallback;

        UpdateUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (currentFriend == null) return;

        // 표시 이름
        if (displayNameText != null)
        {
            displayNameText.text = currentFriend.displayName;
        }

        // 최고 점수
        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best: {currentFriend.bestScore:N0}";
        }

        // 마지막 로그인 시간
        if (lastLoginText != null)
        {
            lastLoginText.text = GetLastLoginDisplayText();
        }

        // 온라인 상태
        if (onlineIndicator != null)
        {
            onlineIndicator.SetActive(currentFriend.isOnline);
        }

        if (onlineStatusIcon != null)
        {
            onlineStatusIcon.color = currentFriend.isOnline ? Color.green : Color.gray;
        }

        // 캐릭터 아이콘 (필요시 구현)
        if (characterIcon != null)
        {
            // TODO: characterId를 기반으로 아이콘 설정
            // characterIcon.sprite = GetCharacterSprite(currentFriend.characterId);
        }
    }

    /// <summary>
    /// 마지막 로그인 시간을 사용자 친화적인 텍스트로 변환
    /// </summary>
    private string GetLastLoginDisplayText()
    {
        if (currentFriend.isOnline)
        {
            return "Online";
        }

        if (currentFriend.lastLoginTime == 0)
        {
            return "Never";
        }

        // Unix timestamp를 DateTime으로 변환
        DateTime lastLogin = DateTimeOffset.FromUnixTimeSeconds(currentFriend.lastLoginTime).DateTime;
        DateTime now = DateTime.UtcNow;
        TimeSpan timeDiff = now - lastLogin;

        if (timeDiff.TotalMinutes < 1)
        {
            return "Just now";
        }
        else if (timeDiff.TotalHours < 1)
        {
            return $"{(int)timeDiff.TotalMinutes}m ago";
        }
        else if (timeDiff.TotalDays < 1)
        {
            return $"{(int)timeDiff.TotalHours}h ago";
        }
        else if (timeDiff.TotalDays < 7)
        {
            return $"{(int)timeDiff.TotalDays}d ago";
        }
        else
        {
            return lastLogin.ToString("MM/dd");
        }
    }

    /// <summary>
    /// 비교 버튼 클릭 이벤트
    /// </summary>
    private void OnCompareButtonClicked()
    {
        if (currentFriend != null && onCompareClicked != null)
        {
            onCompareClicked.Invoke(currentFriend.userId);
        }
    }

    /// <summary>
    /// 제거 버튼 클릭 이벤트
    /// </summary>
    private void OnRemoveButtonClicked()
    {
        if (currentFriend != null && onRemoveClicked != null)
        {
            onRemoveClicked.Invoke(currentFriend.userId);
        }
    }

    /// <summary>
    /// 친구 정보 새로고침
    /// </summary>
    public void RefreshFriendInfo(FriendInfo updatedFriend)
    {
        currentFriend = updatedFriend;
        UpdateUI();
    }
} 