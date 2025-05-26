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
        
        Debug.Log($"[FriendRequestItem] UI 요소 상태 - 닉네임: {nicknameText != null}, 시간: {timeText != null}, 수락: {acceptButton != null}, 거절: {rejectButton != null}");
    }
    
    /// <summary>
    /// 친구 요청 아이템 설정
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
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (requestData == null) 
        {
            Debug.LogWarning("[FriendRequestItem] requestData가 null입니다!");
            return;
        }
        
        Debug.Log($"[FriendRequestItem] UI 업데이트 - 닉네임: {requestData.fromNickname}");
        
        // 닉네임 설정
        if (nicknameText != null)
        {
            nicknameText.text = requestData.fromNickname;
            Debug.Log($"[FriendRequestItem] 닉네임 설정 완료: {requestData.fromNickname}");
        }
        else
        {
            Debug.LogWarning("[FriendRequestItem] nicknameText가 null입니다!");
        }
        
        // 요청 시간 설정
        if (timeText != null)
        {
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