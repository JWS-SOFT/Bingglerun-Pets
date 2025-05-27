using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 친구 요청 아이템 UI 컨트롤러
/// </summary>
public class FriendRequestItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI timeText;
    [SerializeField] private TextMeshProUGUI levelText;      // 레벨 표시용 (선택사항)
    [SerializeField] private TextMeshProUGUI statusText;     // 온라인 상태 표시용 (선택사항)
    [SerializeField] private Button acceptButton;
    [SerializeField] private Button rejectButton;
    
    private FriendRequestData requestData;
    private Action<FriendRequestData> onAcceptClicked;
    private Action<FriendRequestData> onRejectClicked;
    
    private void Awake()
    {
        // UI 요소들이 할당되지 않은 경우 자동으로 찾기
        FindMissingUIElements();
    }
    
    /// <summary>
    /// 누락된 UI 요소들 자동 찾기
    /// </summary>
    private void FindMissingUIElements()
    {
        // 닉네임 텍스트 찾기
        if (nicknameText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text.name.ToLower().Contains("nickname") || 
                    text.name.ToLower().Contains("name") ||
                    text.name.ToLower().Contains("from"))
                {
                    nicknameText = text;
                    Debug.Log($"[FriendRequestItem] 닉네임 텍스트 자동 찾기 성공: {text.name}");
                    break;
                }
            }
            
            // 여전히 못 찾았으면 첫 번째 텍스트 사용
            if (nicknameText == null && texts.Length > 0)
            {
                nicknameText = texts[0];
                Debug.Log($"[FriendRequestItem] 첫 번째 텍스트를 닉네임으로 사용: {texts[0].name}");
            }
        }
        
        // 시간 텍스트 찾기
        if (timeText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text != nicknameText && 
                    (text.name.ToLower().Contains("time") || 
                     text.name.ToLower().Contains("date") ||
                     text.name.ToLower().Contains("ago")))
                {
                    timeText = text;
                    Debug.Log($"[FriendRequestItem] 시간 텍스트 자동 찾기 성공: {text.name}");
                    break;
                }
            }
        }
        
        // 수락 버튼 찾기
        if (acceptButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn.name.ToLower().Contains("accept") || 
                    btn.name.ToLower().Contains("yes") ||
                    btn.name.ToLower().Contains("ok") ||
                    btn.name.ToLower().Contains("수락"))
                {
                    acceptButton = btn;
                    Debug.Log($"[FriendRequestItem] 수락 버튼 자동 찾기 성공: {btn.name}");
                    break;
                }
            }
        }
        
        // 거절 버튼 찾기
        if (rejectButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                if (btn != acceptButton && 
                    (btn.name.ToLower().Contains("reject") || 
                     btn.name.ToLower().Contains("decline") ||
                     btn.name.ToLower().Contains("no") ||
                     btn.name.ToLower().Contains("거절")))
                {
                    rejectButton = btn;
                    Debug.Log($"[FriendRequestItem] 거절 버튼 자동 찾기 성공: {btn.name}");
                    break;
                }
            }
        }
        
        // 레벨 텍스트 찾기
        if (levelText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text != nicknameText && text != timeText && 
                    (text.name.ToLower().Contains("level") || 
                     text.name.ToLower().Contains("lv") ||
                     text.name.ToLower().Contains("레벨")))
                {
                    levelText = text;
                    Debug.Log($"[FriendRequestItem] 레벨 텍스트 자동 찾기 성공: {text.name}");
                    break;
                }
            }
        }
        
        // 상태 텍스트 찾기
        if (statusText == null)
        {
            TextMeshProUGUI[] texts = GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var text in texts)
            {
                if (text != nicknameText && text != timeText && text != levelText && 
                    (text.name.ToLower().Contains("status") || 
                     text.name.ToLower().Contains("online") ||
                     text.name.ToLower().Contains("state") ||
                     text.name.ToLower().Contains("상태")))
                {
                    statusText = text;
                    Debug.Log($"[FriendRequestItem] 상태 텍스트 자동 찾기 성공: {text.name}");
                    break;
                }
            }
        }
        
        Debug.Log($"[FriendRequestItem] UI 요소 상태 - 닉네임: {nicknameText != null}, 시간: {timeText != null}, 레벨: {levelText != null}, 상태: {statusText != null}, 수락: {acceptButton != null}, 거절: {rejectButton != null}");
    }
    
    /// <summary>
    /// 친구 요청 아이템 설정 (받은 요청용)
    /// </summary>
    public void Setup(FriendRequestData request, Action<FriendRequestData> acceptCallback, Action<FriendRequestData> rejectCallback)
    {
        Debug.Log($"[FriendRequestItem] Setup 호출 - 요청자: {request?.fromNickname}");
        
        requestData = request;
        onAcceptClicked = acceptCallback;
        onRejectClicked = rejectCallback;
        
        UpdateUI();
        SetupEvents();
    }
    
    /// <summary>
    /// 친구 요청 아이템 설정 (보낸 요청용)
    /// </summary>
    public void Setup(FriendRequestData request, Action<FriendRequestData> cancelCallback)
    {
        Debug.Log($"[FriendRequestItem] Setup 호출 (보낸 요청) - 받는 사람: {request?.toNickname}");
        
        requestData = request;
        onAcceptClicked = cancelCallback; // 취소 콜백을 수락 버튼에 연결
        onRejectClicked = null;
        
        UpdateUI();
        SetupEvents();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (requestData == null) 
        {
            Debug.LogWarning("[FriendRequestItem] requestData가 null입니다!");
            return;
        }
        
        Debug.Log($"[FriendRequestItem] UI 업데이트 - 타입: {requestData.requestType}, 닉네임: {requestData.fromNickname}");
        
        // 요청 타입에 따라 다른 UI 표시
        if (requestData.requestType == FriendRequestType.Received)
        {
            // 받은 요청: "From: 닉네임 (Lv.X)"
            if (nicknameText != null)
            {
                string displayText = $"From: {requestData.fromNickname}";
                if (requestData.fromUserLevel > 1)
                {
                    displayText += $" (Lv.{requestData.fromUserLevel})";
                }
                nicknameText.text = displayText;
                Debug.Log($"[FriendRequestItem] 받은 요청 닉네임 설정 완료: {displayText}");
            }
            
            // 레벨 정보 별도 표시 (levelText가 있는 경우)
            if (levelText != null)
            {
                levelText.text = $"Level {requestData.fromUserLevel}";
            }
            
            // 온라인 상태 표시
            UpdateUserStatus(requestData.fromUserIsOnline, requestData.fromUserLastLogin);
            
            // 수락/거절 버튼 표시
            if (acceptButton != null)
            {
                acceptButton.gameObject.SetActive(true);
                var acceptText = acceptButton.GetComponentInChildren<TextMeshProUGUI>();
                if (acceptText != null) acceptText.text = "Accept";
            }
            
            if (rejectButton != null)
            {
                rejectButton.gameObject.SetActive(true);
                var rejectText = rejectButton.GetComponentInChildren<TextMeshProUGUI>();
                if (rejectText != null) rejectText.text = "Reject";
            }
        }
        else if (requestData.requestType == FriendRequestType.Sent)
        {
            // 보낸 요청: "To: 닉네임 (Lv.X)"
            if (nicknameText != null)
            {
                string targetNickname = !string.IsNullOrEmpty(requestData.toNickname) ? requestData.toNickname : "Unknown";
                string displayText = $"To: {targetNickname}";
                if (requestData.toUserLevel > 1)
                {
                    displayText += $" (Lv.{requestData.toUserLevel})";
                }
                nicknameText.text = displayText;
                Debug.Log($"[FriendRequestItem] 보낸 요청 닉네임 설정 완료: {displayText}");
            }
            
            // 레벨 정보 별도 표시 (levelText가 있는 경우)
            if (levelText != null)
            {
                levelText.text = $"Level {requestData.toUserLevel}";
            }
            
            // 온라인 상태 표시
            UpdateUserStatus(requestData.toUserIsOnline, requestData.toUserLastLogin);
            
            // 취소 버튼만 표시
            if (acceptButton != null)
            {
                acceptButton.gameObject.SetActive(true);
                var acceptText = acceptButton.GetComponentInChildren<TextMeshProUGUI>();
                if (acceptText != null) acceptText.text = "Cancel";
            }
            
            if (rejectButton != null)
            {
                rejectButton.gameObject.SetActive(false);
            }
        }
        
        // 요청 시간 설정 (statusText가 별도로 있지 않은 경우에만)
        if (timeText != null && statusText == null)
        {
            // UpdateUserStatus에서 timeText에 상태 정보도 함께 표시됨
            Debug.Log("[FriendRequestItem] 시간 및 상태 정보는 UpdateUserStatus에서 처리됨");
        }
        else if (timeText != null && statusText != null)
        {
            // statusText가 별도로 있는 경우 timeText에는 요청 시간만 표시
            var requestTime = DateTimeOffset.FromUnixTimeSeconds(requestData.requestTime);
            var timeDiff = DateTimeOffset.UtcNow - requestTime;
            
            string timeString = "";
            if (timeDiff.TotalDays >= 1)
                timeString = $"{(int)timeDiff.TotalDays}d ago";
            else if (timeDiff.TotalHours >= 1)
                timeString = $"{(int)timeDiff.TotalHours}h ago";
            else if (timeDiff.TotalMinutes >= 1)
                timeString = $"{(int)timeDiff.TotalMinutes}m ago";
            else
                timeString = "Just now";
                
            timeText.text = timeString;
            Debug.Log($"[FriendRequestItem] 시간 설정 완료: {timeString}");
        }
        
        // UI 레이아웃 강제 업데이트
        Canvas.ForceUpdateCanvases();
    }
    
    /// <summary>
    /// 사용자 상태 업데이트 (온라인/오프라인, 마지막 접속시간)
    /// </summary>
    private void UpdateUserStatus(bool isOnline, long lastLoginTime)
    {
        if (statusText != null)
        {
            if (isOnline)
            {
                statusText.text = "Online";
                statusText.color = UnityEngine.Color.green;
            }
            else
            {
                // 마지막 접속시간 계산
                var lastLoginDateTime = DateTimeOffset.FromUnixTimeMilliseconds(lastLoginTime);
                var timeDiff = DateTimeOffset.UtcNow - lastLoginDateTime;
                
                string statusString = "";
                if (timeDiff.TotalDays >= 1)
                    statusString = $"Last seen {(int)timeDiff.TotalDays}d ago";
                else if (timeDiff.TotalHours >= 1)
                    statusString = $"Last seen {(int)timeDiff.TotalHours}h ago";
                else if (timeDiff.TotalMinutes >= 1)
                    statusString = $"Last seen {(int)timeDiff.TotalMinutes}m ago";
                else
                    statusString = "Last seen recently";
                
                statusText.text = statusString;
                statusText.color = UnityEngine.Color.gray;
            }
        }
        else if (timeText != null)
        {
            // statusText가 없으면 timeText에 상태 정보도 함께 표시
            var requestTime = DateTimeOffset.FromUnixTimeSeconds(requestData.requestTime);
            var timeDiff = DateTimeOffset.UtcNow - requestTime;
            
            string timeString = "";
            if (timeDiff.TotalDays >= 1)
                timeString = $"{(int)timeDiff.TotalDays}d ago";
            else if (timeDiff.TotalHours >= 1)
                timeString = $"{(int)timeDiff.TotalHours}h ago";
            else if (timeDiff.TotalMinutes >= 1)
                timeString = $"{(int)timeDiff.TotalMinutes}m ago";
            else
                timeString = "Just now";
            
            string statusString = isOnline ? "Online" : "Offline";
            timeText.text = $"{timeString} | {statusString}";
        }
    }
    
    /// <summary>
    /// 이벤트 설정
    /// </summary>
    private void SetupEvents()
    {
        if (acceptButton != null)
        {
            acceptButton.onClick.RemoveAllListeners();
            acceptButton.onClick.AddListener(OnAcceptButtonClicked);
            Debug.Log("[FriendRequestItem] 수락 버튼 이벤트 설정 완료");
        }
        else
        {
            Debug.LogWarning("[FriendRequestItem] acceptButton이 null입니다!");
        }
        
        if (rejectButton != null)
        {
            rejectButton.onClick.RemoveAllListeners();
            rejectButton.onClick.AddListener(OnRejectButtonClicked);
            Debug.Log("[FriendRequestItem] 거절 버튼 이벤트 설정 완료");
        }
        else
        {
            Debug.LogWarning("[FriendRequestItem] rejectButton이 null입니다!");
        }
    }
    
    /// <summary>
    /// 수락 버튼 클릭 처리
    /// </summary>
    private void OnAcceptButtonClicked()
    {
        Debug.Log($"[FriendRequestItem] 수락 버튼 클릭 - 요청자: {requestData?.fromNickname}");
        
        if (requestData == null)
        {
            Debug.LogError("[FriendRequestItem] requestData가 null입니다!");
            return;
        }
        
        if (onAcceptClicked == null)
        {
            Debug.LogError("[FriendRequestItem] onAcceptClicked 콜백이 null입니다!");
            return;
        }
        
        onAcceptClicked.Invoke(requestData);
    }
    
    /// <summary>
    /// 거절 버튼 클릭 처리
    /// </summary>
    private void OnRejectButtonClicked()
    {
        Debug.Log($"[FriendRequestItem] 거절 버튼 클릭 - 요청자: {requestData?.fromNickname}");
        
        if (requestData == null)
        {
            Debug.LogError("[FriendRequestItem] requestData가 null입니다!");
            return;
        }
        
        if (onRejectClicked == null)
        {
            Debug.LogError("[FriendRequestItem] onRejectClicked 콜백이 null입니다!");
            return;
        }
        
        onRejectClicked.Invoke(requestData);
    }
}