using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 검색 결과 아이템 UI 컴포넌트
/// </summary>
public class SearchResultItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private TextMeshProUGUI bestScoreText;
    [SerializeField] private TextMeshProUGUI lastLoginText;
    [SerializeField] private Image characterIcon;
    [SerializeField] private Button actionButton;
    [SerializeField] private TextMeshProUGUI actionButtonText;
    [SerializeField] private Image statusIcon;

    // 이벤트 콜백
    private Action<string, string> onSendRequestClicked;
    
    // 현재 프로필 정보
    private UserPublicProfile currentProfile;
    private bool isFriend;
    private bool hasSentRequest;
    private bool hasReceivedRequest;

    private void Awake()
    {
        // 버튼 이벤트 연결
        actionButton?.onClick.AddListener(OnActionButtonClicked);
    }

    /// <summary>
    /// 검색 결과로 UI 설정
    /// </summary>
    public void Setup(UserPublicProfile profile, bool isFriend, bool hasSentRequest, bool hasReceivedRequest, Action<string, string> sendRequestCallback)
    {
        currentProfile = profile;
        this.isFriend = isFriend;
        this.hasSentRequest = hasSentRequest;
        this.hasReceivedRequest = hasReceivedRequest;
        onSendRequestClicked = sendRequestCallback;

        UpdateUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (currentProfile == null) return;

        // 표시 이름
        if (displayNameText != null)
        {
            displayNameText.text = currentProfile.displayName;
        }

        // 최고 점수
        if (bestScoreText != null)
        {
            bestScoreText.text = $"Best: {currentProfile.bestScore:N0}";
        }

        // 마지막 로그인 시간
        if (lastLoginText != null)
        {
            lastLoginText.text = GetLastLoginDisplayText();
        }

        // 캐릭터 아이콘 (필요시 구현)
        if (characterIcon != null)
        {
            // TODO: characterId를 기반으로 아이콘 설정
            // characterIcon.sprite = GetCharacterSprite(currentProfile.characterId);
        }

        // 액션 버튼 및 상태 아이콘 업데이트
        UpdateActionButton();
        UpdateStatusIcon();
    }

    /// <summary>
    /// 액션 버튼 상태 업데이트
    /// </summary>
    private void UpdateActionButton()
    {
        if (actionButton == null || actionButtonText == null) return;

        if (isFriend)
        {
            // 이미 친구인 경우
            actionButton.interactable = false;
            actionButtonText.text = "Friend";
            actionButtonText.color = Color.green;
        }
        else if (hasSentRequest)
        {
            // 요청을 보낸 경우
            actionButton.interactable = false;
            actionButtonText.text = "Sent";
            actionButtonText.color = Color.yellow;
        }
        else if (hasReceivedRequest)
        {
            // 요청을 받은 경우
            actionButton.interactable = false;
            actionButtonText.text = "Pending";
            actionButtonText.color = Color.cyan;
        }
        else
        {
            // 요청 가능한 경우
            actionButton.interactable = true;
            actionButtonText.text = "Add Friend";
            actionButtonText.color = Color.white;
        }
    }

    /// <summary>
    /// 상태 아이콘 업데이트
    /// </summary>
    private void UpdateStatusIcon()
    {
        if (statusIcon == null) return;

        if (isFriend)
        {
            statusIcon.color = Color.green;
        }
        else if (hasSentRequest)
        {
            statusIcon.color = Color.yellow;
        }
        else if (hasReceivedRequest)
        {
            statusIcon.color = Color.cyan;
        }
        else
        {
            statusIcon.color = Color.gray;
        }
    }

    /// <summary>
    /// 마지막 로그인 시간을 사용자 친화적인 텍스트로 변환
    /// </summary>
    private string GetLastLoginDisplayText()
    {
        if (currentProfile.lastLoginTime == 0)
        {
            return "Never";
        }

        // Unix timestamp를 DateTime으로 변환
        DateTime lastLogin = DateTimeOffset.FromUnixTimeSeconds(currentProfile.lastLoginTime).DateTime;
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
    /// 액션 버튼 클릭 이벤트
    /// </summary>
    private void OnActionButtonClicked()
    {
        if (currentProfile != null && onSendRequestClicked != null && !isFriend && !hasSentRequest && !hasReceivedRequest)
        {
            onSendRequestClicked.Invoke(currentProfile.userId, currentProfile.displayName);
        }
    }

    /// <summary>
    /// 관계 상태 새로고침
    /// </summary>
    public void RefreshRelationshipStatus()
    {
        if (FriendSystemManager.Instance != null && currentProfile != null)
        {
            isFriend = FriendSystemManager.Instance.IsFriend(currentProfile.userId);
            hasSentRequest = FriendSystemManager.Instance.HasSentRequestTo(currentProfile.userId);
            hasReceivedRequest = FriendSystemManager.Instance.HasReceivedRequestFrom(currentProfile.userId);

            UpdateActionButton();
            UpdateStatusIcon();
        }
    }

    /// <summary>
    /// 프로필 정보 새로고침
    /// </summary>
    public void RefreshProfile(UserPublicProfile updatedProfile)
    {
        currentProfile = updatedProfile;
        UpdateUI();
    }
} 