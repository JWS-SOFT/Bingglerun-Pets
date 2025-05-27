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
        // PlayerDataManager 이벤트 구독
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnHeartChanged += UpdateHeartDisplay;
            PlayerDataManager.Instance.OnDataLoaded += OnPlayerDataLoaded;
        }
        
        // HeartSystem 이벤트 구독
        if (HeartSystem.Instance != null)
        {
            HeartSystem.Instance.OnHeartChanged += UpdateHeartDisplay;
            HeartSystem.Instance.OnHeartRecovered += OnHeartRecovered;
        }
        
        // 버튼 이벤트 연결
        if (purchaseButton != null)
        {
            purchaseButton.onClick.AddListener(OnPurchaseButtonClicked);
        }
        
        // 초기 UI 업데이트
        UpdateHeartDisplay();
    }
    
    private void OnDestroy()
    {
        // PlayerDataManager 이벤트 구독 해제
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnHeartChanged -= UpdateHeartDisplay;
            PlayerDataManager.Instance.OnDataLoaded -= OnPlayerDataLoaded;
        }
        
        // HeartSystem 이벤트 구독 해제
        if (HeartSystem.Instance != null)
        {
            HeartSystem.Instance.OnHeartChanged -= UpdateHeartDisplay;
            HeartSystem.Instance.OnHeartRecovered -= OnHeartRecovered;
        }
        
        // 코루틴 정리
        if (recoveryTimerCoroutine != null)
        {
            StopCoroutine(recoveryTimerCoroutine);
        }
    }
    
    private void OnPlayerDataLoaded()
    {
        UpdateHeartDisplay();
    }
    
    /// <summary>
    /// 하트 회복 이벤트 핸들러
    /// </summary>
    private void OnHeartRecovered(int currentHearts, int recoveredAmount)
    {
        Debug.Log($"[HeartUI] 하트 회복됨: +{recoveredAmount}, 현재: {currentHearts}");
        
        // 회복 애니메이션이나 이펙트를 여기서 추가할 수 있음
        // TODO: 하트 회복 시각적 피드백 구현
        
        UpdateHeartDisplay(currentHearts);
    }
    
    /// <summary>
    /// 하트 표시 업데이트 (매개변수 없는 버전)
    /// </summary>
    private void UpdateHeartDisplay()
    {
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            int currentHearts = PlayerDataManager.Instance.CurrentPlayerData.heart;
            UpdateHeartDisplay(currentHearts);
        }
    }
    
    /// <summary>
    /// 하트 표시 업데이트
    /// </summary>
    private void UpdateHeartDisplay(int currentHearts)
    {
        // 찬스 이미지 상태 업데이트 (하트가 1개라도 있으면 YesChance 활성화)
        bool hasHearts = currentHearts > 0;
        
        if (yesChanceImage != null)
            yesChanceImage.SetActive(hasHearts);
            
        if (noChanceImage != null)
            noChanceImage.SetActive(!hasHearts);
        
        // 남은 하트 수 표시
        if (chanceCountText != null)
        {
            chanceCountText.text = currentHearts.ToString();
        }
        
        // 회복 시간 표시 업데이트
        UpdateRecoveryTimeDisplay();
    }
    
    /// <summary>
    /// 회복 시간 표시 업데이트
    /// </summary>
    private void UpdateRecoveryTimeDisplay()
    {
        if (timerText == null)
            return;
        
        // 기존 코루틴 정지
        if (recoveryTimerCoroutine != null)
        {
            StopCoroutine(recoveryTimerCoroutine);
        }
        
        // 새 코루틴 시작
        recoveryTimerCoroutine = StartCoroutine(UpdateRecoveryTimer());
    }
    
    /// <summary>
    /// 회복 시간 타이머 코루틴
    /// </summary>
    private IEnumerator UpdateRecoveryTimer()
    {
        while (true)
        {
            if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
            {
                var playerData = PlayerDataManager.Instance.CurrentPlayerData;
                int maxHearts = GetMaxHearts();
                
                if (playerData.heart >= maxHearts)
                {
                    timerText.text = "FULL";
                    yield break;
                }
                
                // HeartSystem이 있으면 사용, 없으면 직접 계산
                TimeSpan timeUntilNext = CalculateTimeUntilNextRecovery();
                
                if (timeUntilNext.TotalSeconds <= 0)
                {
                    timerText.text = "Ready!";
                    // 하트 회복 체크 (HeartSystem이 있으면 호출)
                    CheckHeartRecovery();
                }
                else
                {
                    timerText.text = $"{timeUntilNext.Minutes:D2}:{timeUntilNext.Seconds:D2}";
                }
            }
            else
            {
                timerText.text = "--:--";
            }
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// 다음 하트 회복까지 시간 계산
    /// </summary>
    private TimeSpan CalculateTimeUntilNextRecovery()
    {
        // HeartSystem이 있으면 사용
        if (HeartSystem.Instance != null)
        {
            return HeartSystem.Instance.GetTimeUntilNextRecovery();
        }
        
        // HeartSystem이 없는 경우 직접 계산 (하위 호환성)
        if (!PlayerDataManager.Instance.IsDataLoaded)
            return TimeSpan.Zero;
        
        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long recoveryIntervalMilliseconds = 30 * 60 * 1000; // 30분 (기본값)
        long nextRecoveryTime = playerData.lastHeartRecoveryTime + recoveryIntervalMilliseconds;
        long timeUntilRecovery = nextRecoveryTime - currentTime;
        
        if (timeUntilRecovery <= 0)
            return TimeSpan.Zero;
        
        return TimeSpan.FromMilliseconds(timeUntilRecovery);
    }
    
    /// <summary>
    /// 하트 회복 체크
    /// </summary>
    private void CheckHeartRecovery()
    {
        // HeartSystem이 있으면 사용
        if (HeartSystem.Instance != null)
        {
            HeartSystem.Instance.CheckAndRecoverHearts();
            return;
        }
        
        // HeartSystem이 없는 경우 직접 처리 (하위 호환성)
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            var playerData = PlayerDataManager.Instance.CurrentPlayerData;
            long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long recoveryIntervalMilliseconds = 30 * 60 * 1000; // 30분 (기본값)
            
            if (currentTime >= playerData.lastHeartRecoveryTime + recoveryIntervalMilliseconds)
            {
                int maxHearts = GetMaxHearts();
                if (playerData.heart < maxHearts)
                {
                    // PlayerDataManager의 RefillHeart 메서드를 사용하여 하트 추가
                    // 이 메서드가 내부적으로 이벤트 발생과 데이터 저장을 처리함
                    PlayerDataManager.Instance.RefillHeart(1);
                }
            }
        }
    }
    
    /// <summary>
    /// 최대 하트 개수 반환
    /// </summary>
    private int GetMaxHearts()
    {
        // HeartSystem이 있으면 사용
        if (HeartSystem.Instance != null)
        {
            return HeartSystem.Instance.GetMaxHearts();
        }
        
        // HeartSystem이 없는 경우 직접 계산 (하위 호환성)
        if (PlayerDataManager.Instance != null && PlayerDataManager.Instance.IsDataLoaded)
        {
            return PlayerDataManager.Instance.CurrentPlayerData.maxHearts;
        }
        return 5; // 기본값
    }
    
    /// <summary>
    /// 하트 구매 버튼 클릭 이벤트
    /// </summary>
    private void OnPurchaseButtonClicked()
    {
        // TODO: 하트 구매 또는 광고 시청 로직 구현
        Debug.Log("[HeartUI] 하트 구매 버튼 클릭됨");
        
        // 임시로 하트 1개 추가 (테스트용)
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
    }
} 