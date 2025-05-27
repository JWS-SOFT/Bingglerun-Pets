using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 친구 목록 아이템 UI 컨트롤러
/// </summary>
public class FriendListItem : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI nicknameText;
    [SerializeField] private TextMeshProUGUI levelText;
    [SerializeField] private TextMeshProUGUI statusText;
    [SerializeField] private Image onlineIndicator;
    [SerializeField] private Button removeButton;
    
    [Header("Colors")]
    [SerializeField] private Color onlineColor = Color.green;
    [SerializeField] private Color offlineColor = Color.gray;
    
    private FriendData friendData;
    private Action<FriendData> onRemoveClicked;
    
    /// <summary>
    /// 친구 아이템 설정
    /// </summary>
    public void Setup(FriendData friend, Action<FriendData> removeCallback)
    {
        friendData = friend;
        onRemoveClicked = removeCallback;
        
        UpdateUI();
        SetupEvents();
    }
    
    /// <summary>
    /// UI 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (friendData == null) return;
        
        // 닉네임 설정
        if (nicknameText != null)
            nicknameText.text = friendData.nickname;
        
        // 레벨 설정
        if (levelText != null)
            levelText.text = $"Lv.{friendData.level}";
        
        // 온라인 상태 설정
        if (statusText != null)
            statusText.text = friendData.isOnline ? "Online" : "Offline";
        
        // 온라인 인디케이터 설정
        if (onlineIndicator != null)
        {
            onlineIndicator.color = friendData.isOnline ? onlineColor : offlineColor;
        }
    }
    
    /// <summary>
    /// 이벤트 설정
    /// </summary>
    private void SetupEvents()
    {
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveButtonClicked);
        }
    }
    
    /// <summary>
    /// 삭제 버튼 클릭 처리
    /// </summary>
    private void OnRemoveButtonClicked()
    {
        onRemoveClicked?.Invoke(friendData);
    }
}