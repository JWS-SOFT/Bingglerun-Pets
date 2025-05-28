using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 하트 UI를 관리하는 클래스
/// 하트 개수, 회복 시간 등을 표시
/// </summary>
public class HeartUI : MonoBehaviour
{
    [Header("찬스 UI 컴포넌트")]
    [SerializeField] private GameObject yesChanceImage;  // 하트가 있을 때 활성화
    [SerializeField] private GameObject noChanceImage;   // 하트가 없을 때 활성화
    [SerializeField] private TextMeshProUGUI chanceCountText;  // 남은 하트 수 표시
    [SerializeField] private TextMeshProUGUI timerText;        // 회복 시간 표시
    [SerializeField] private Button purchaseButton;           // 하트 구매 버튼
    
    private Coroutine recoveryTimerCoroutine;
    
    private void Start()
    {
        // HeartSystem 이벤트 구독
        if (HeartSystem.Instance != null)
        {
            HeartSystem.Instance.OnHeartChanged += UpdateHeartDisplay;
            HeartSystem.Instance.OnHeartRecovered += OnHeartRecovered;
            HeartSystem.Instance.OnNextRecoveryTimeUpdated += UpdateRecoveryTimeDisplay;
        }
        
        // 버튼 이벤트 연결
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        }
        
        // 초기 UI 업데이트
        UpdateHeartDisplay();
        UpdateRecoveryTimeDisplay(HeartSystem.Instance != null ? HeartSystem.Instance.GetTimeUntilNextRecovery() : TimeSpan.Zero);
    }
    
    private void OnDestroy()
    {
        // HeartSystem 이벤트 구독 해제
        if (HeartSystem.Instance != null)
        {
            HeartSystem.Instance.OnHeartChanged -= UpdateHeartDisplay;
            HeartSystem.Instance.OnHeartRecovered -= OnHeartRecovered;
            HeartSystem.Instance.OnNextRecoveryTimeUpdated -= UpdateRecoveryTimeDisplay;
        }
    }
    
    private void OnHeartRecovered(int currentHearts, int recoveredAmount)
    {
        Debug.Log($"[HeartUI] 하트 회복됨: +{recoveredAmount}, 현재: {currentHearts}");
        UpdateHeartDisplay(currentHearts);
    }
    
    private void UpdateHeartDisplay()
    {
        if (HeartSystem.Instance != null)
        {
            int currentHearts = HeartSystem.Instance.GetCurrentHearts();
            UpdateHeartDisplay(currentHearts);
        }
    }
    
    private void UpdateHeartDisplay(int currentHearts)
    {
        bool hasHearts = currentHearts > 0;
        if (yesChanceImage != null)
            yesChanceImage.SetActive(hasHearts);
        if (noChanceImage != null)
            noChanceImage.SetActive(!hasHearts);
        if (chanceCountText != null)
            chanceCountText.text = currentHearts.ToString();
    }
    
    // HeartSystem의 OnNextRecoveryTimeUpdated 이벤트에서 호출됨
    private void UpdateRecoveryTimeDisplay(TimeSpan timeUntilNext)
    {
        if (timerText == null)
            return;
        if (HeartSystem.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
        {
            timerText.text = "--:--";
            return;
        }
        int maxHearts = HeartSystem.Instance.GetMaxHearts();
        int currentHearts = HeartSystem.Instance.GetCurrentHearts();
        if (currentHearts >= maxHearts)
        {
            timerText.text = "FULL";
        }
        else if (timeUntilNext.TotalSeconds <= 0)
        {
            timerText.text = "Ready!";
        }
        else
        {
            timerText.text = $"{timeUntilNext.Minutes:D2}:{timeUntilNext.Seconds:D2}";
        }
    }
    
    private void OnPurchaseButtonClicked()
    {
        Debug.Log("[HeartUI] 하트 구매 버튼 클릭됨");
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.RefillHeart(1);
        }
    }
    
    /// <summary>
    /// 하트 UI 강제 업데이트
    /// </summary>
    public void ForceUpdateDisplay()
    {
        UpdateHeartDisplay();
        UpdateRecoveryTimeDisplay(HeartSystem.Instance != null ? HeartSystem.Instance.GetTimeUntilNextRecovery() : TimeSpan.Zero);
    }
} 