using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// 친구 요청 아이템 UI 컴포넌트
/// </summary>
public class FriendRequestItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI displayNameText;
    [SerializeField] private TextMeshProUGUI requestTimeText;
    [SerializeField] private Image characterIcon;
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    [SerializeField] private Button cancelButton;
    [SerializeField] private GameObject receivedRequestButtons;
    [SerializeField] private GameObject sentRequestButtons;

    // 이벤트 콜백들
    private Action<string> onAcceptClicked;
    private Action<string> onRejectClicked;
    private Action<string> onCancelClicked;
    
    // 현재 요청 정보
    private FriendRequest currentRequest;
    private bool isReceivedRequest;

    private void Awake()
    {
        // 버튼 이벤트 연결
        acceptButton?.onClick.AddListener(OnAcceptButtonClicked);
        rejectButton?.onClick.AddListener(OnRejectButtonClicked);
        cancelButton?.onClick.AddListener(OnCancelButtonClicked);
    }

    /// <summary>
    /// 받은 친구 요청으로 UI 설정
    /// </summary>
    public void SetupReceivedRequest(FriendRequest request, Action<string> acceptCallback, Action<string> rejectCallback)
    {
        currentRequest = request;
        isReceivedRequest = true;
        onAcceptClicked = acceptCallback;
        onRejectClicked = rejectCallback;

        UpdateUI();
    }

    /// <summary>
    /// 보낸 친구 요청으로 UI 설정
    /// </summary>
    public void SetupSentRequest(FriendRequest request, Action<string> cancelCallback)
    {
        currentRequest = request;
        isReceivedRequest = false;
        onCancelClicked = cancelCallback;

        UpdateUI();
    }

    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (currentRequest == null) return;

        // 표시 이름
        if (displayNameText != null)
        {
            if (isReceivedRequest)
            {
                displayNameText.text = currentRequest.fromDisplayName;
            }
            else
            {
                // 보낸 요청의 경우 상대방 이름을 표시해야 하지만, 현재 구조에서는 fromDisplayName만 있음
                displayNameText.text = $"To: {GetTargetDisplayName()}";
            }
        }

        // 요청 시간
        if (requestTimeText != null)
        {
            requestTimeText.text = GetRequestTimeDisplayText();
        }

        // 캐릭터 아이콘 (필요시 구현)
        if (characterIcon != null)
        {
            // TODO: 캐릭터 아이콘 설정
        }

        // 버튼 그룹 표시/숨김
        if (receivedRequestButtons != null)
        {
            receivedRequestButtons.SetActive(isReceivedRequest);
        }

        if (sentRequestButtons != null)
        {
            sentRequestButtons.SetActive(!isReceivedRequest);
        }
    }

    /// <summary>
    /// 요청 시간을 사용자 친화적인 텍스트로 변환
    /// </summary>
    private string GetRequestTimeDisplayText()
    {
        if (currentRequest.requestTime == 0)
        {
            return "Unknown";
        }

        // Unix timestamp를 DateTime으로 변환
        DateTime requestTime = DateTimeOffset.FromUnixTimeSeconds(currentRequest.requestTime).DateTime;
        DateTime now = DateTime.UtcNow;
        TimeSpan timeDiff = now - requestTime;

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
            return requestTime.ToString("MM/dd");
        }
    }

    /// <summary>
    /// 대상 사용자의 표시 이름 가져오기 (보낸 요청용)
    /// </summary>
    private string GetTargetDisplayName()
    {
        // 실제로는 toUserId를 기반으로 사용자 이름을 가져와야 함
        // 현재는 간단히 User ID의 일부만 표시
        if (!string.IsNullOrEmpty(currentRequest.toUserId))
        {
            return currentRequest.toUserId.Length > 8 ? 
                   currentRequest.toUserId.Substring(0, 8) + "..." : 
                   currentRequest.toUserId;
        }
        return "Unknown";
    }

    /// <summary>
    /// 수락 버튼 클릭 이벤트
    /// </summary>
    private void OnAcceptButtonClicked()
    {
        if (currentRequest != null && onAcceptClicked != null && isReceivedRequest)
        {
            onAcceptClicked.Invoke(currentRequest.requestId);
        }
    }

    /// <summary>
    /// 거절 버튼 클릭 이벤트
    /// </summary>
    private void OnRejectButtonClicked()
    {
        if (currentRequest != null && onRejectClicked != null && isReceivedRequest)
        {
            onRejectClicked.Invoke(currentRequest.requestId);
        }
    }

    /// <summary>
    /// 취소 버튼 클릭 이벤트
    /// </summary>
    private void OnCancelButtonClicked()
    {
        if (currentRequest != null && onCancelClicked != null && !isReceivedRequest)
        {
            onCancelClicked.Invoke(currentRequest.requestId);
        }
    }

    /// <summary>
    /// 요청 정보 새로고침
    /// </summary>
    public void RefreshRequest(FriendRequest updatedRequest)
    {
        currentRequest = updatedRequest;
        UpdateUI();
    }

    /// <summary>
    /// 요청 상태가 변경되었을 때 UI 업데이트
    /// </summary>
    public void OnRequestStatusChanged(FriendRequestStatus newStatus)
    {
        if (currentRequest != null)
        {
            currentRequest.status = newStatus;
            
            // 상태가 변경되면 아이템을 비활성화하거나 삭제할 수 있음
            if (newStatus != FriendRequestStatus.Pending)
            {
                // 요청이 처리되었으므로 아이템을 페이드 아웃하거나 즉시 제거
                gameObject.SetActive(false);
            }
        }
    }
} 