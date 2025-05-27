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
    [Header("UI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI heartCountText;
    [SerializeField] private TextMeshProUGUI recoveryTimeText;
    [SerializeField] private Slider heartProgressSlider;
    [SerializeField] private Button addHeartButton; // 하트 구매/추가 버튼
    
    [Header("하트 아이콘")]
    [SerializeField] private Transform heartIconContainer;
    [SerializeField] private GameObject heartIconPrefab;
    [SerializeField] private int maxDisplayHearts = 5; // 표시할 최대 하트 아이콘 개수
    
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
        if (addHeartButton != null)
        {
            addHeartButton.onClick.AddListener(OnAddHeartButtonClicked);
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
        // 하트 개수 텍스트 업데이트
        if (heartCountText != null)
        {
            int maxHearts = GetMaxHearts();
            heartCountText.text = $"{currentHearts}/{maxHearts}";
        }
        
        // 하트 아이콘 업데이트
        UpdateHeartIcons(currentHearts);
        
        // 회복 시간 표시 업데이트
        UpdateRecoveryTimeDisplay();
        
        // 진행 바 업데이트
        UpdateProgressSlider();
    }
    
    /// <summary>
    /// 하트 아이콘 업데이트
    /// </summary>
    private void UpdateHeartIcons(int currentHearts)
    {
        if (heartIconContainer == null || heartIconPrefab == null)
            return;
        
        int maxHearts = GetMaxHearts();
        int displayHearts = Mathf.Min(maxHearts, maxDisplayHearts);
        
        // 기존 아이콘 제거
        for (int i = heartIconContainer.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(heartIconContainer.GetChild(i).gameObject);
        }
        
        // 새 아이콘 생성
        for (int i = 0; i < displayHearts; i++)
        {
            GameObject heartIcon = Instantiate(heartIconPrefab, heartIconContainer);
            
            // 하트 상태에 따라 표시 변경
            bool isFilled = i < currentHearts;
            UpdateHeartIconState(heartIcon, isFilled);
        }
        
        // 최대 표시 개수를 초과하는 경우 "+" 표시
        if (maxHearts > maxDisplayHearts)
        {
            // TODO: "+" 아이콘 또는 텍스트 추가
        }
    }
    
    /// <summary>
    /// 개별 하트 아이콘 상태 업데이트
    /// </summary>
    private void UpdateHeartIconState(GameObject heartIcon, bool isFilled)
    {
        // Image 컴포넌트의 알파값 조정 또는 다른 스프라이트 사용
        Image heartImage = heartIcon.GetComponent<Image>();
        if (heartImage != null)
        {
            heartImage.color = isFilled ? Color.white : new Color(1f, 1f, 1f, 0.3f);
        }
    }
    
    /// <summary>
    /// 회복 시간 표시 업데이트
    /// </summary>
    private void UpdateRecoveryTimeDisplay()
    {
        if (recoveryTimeText == null)
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
                    recoveryTimeText.text = "FULL";
                    yield break;
                }
                
                // HeartSystem이 있으면 사용, 없으면 직접 계산
                TimeSpan timeUntilNext = CalculateTimeUntilNextRecovery();
                
                if (timeUntilNext.TotalSeconds <= 0)
                {
                    recoveryTimeText.text = "Ready!";
                    // 하트 회복 체크 (HeartSystem이 있으면 호출)
                    CheckHeartRecovery();
                }
                else
                {
                    recoveryTimeText.text = $"{timeUntilNext.Minutes:D2}:{timeUntilNext.Seconds:D2}";
                }
            }
            else
            {
                recoveryTimeText.text = "--:--";
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
                    playerData.heart++;
                    playerData.lastHeartRecoveryTime = currentTime;
                    
                    // 이벤트 발생
                    PlayerDataManager.Instance.OnHeartChanged?.Invoke(playerData.heart);
                    
                    // 데이터 저장
                    _ = PlayerDataManager.Instance.SavePlayerDataAsync();
                }
            }
        }
    }
    
    /// <summary>
    /// 진행 바 업데이트
    /// </summary>
    private void UpdateProgressSlider()
    {
        if (heartProgressSlider == null)
            return;
        
        if (!PlayerDataManager.Instance.IsDataLoaded)
        {
            heartProgressSlider.value = 0;
            return;
        }
        
        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        int maxHearts = GetMaxHearts();
        
        if (playerData.heart >= maxHearts)
        {
            heartProgressSlider.value = 1f;
            return;
        }
        
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long recoveryIntervalMilliseconds = 30 * 60 * 1000; // 30분
        long timeSinceLastRecovery = currentTime - playerData.lastHeartRecoveryTime;
        
        float progress = (float)timeSinceLastRecovery / recoveryIntervalMilliseconds;
        heartProgressSlider.value = Mathf.Clamp01(progress);
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
    /// 하트 추가 버튼 클릭 이벤트
    /// </summary>
    private void OnAddHeartButtonClicked()
    {
        // TODO: 하트 구매 또는 광고 시청 로직 구현
        Debug.Log("[HeartUI] 하트 추가 버튼 클릭됨");
        
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