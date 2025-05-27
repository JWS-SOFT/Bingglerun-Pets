using System;
using UnityEngine;

/// <summary>
/// 하트 시스템을 관리하는 클래스
/// 하트 소모, 회복, 시간 계산 등을 담당
/// </summary>
public class HeartSystem : MonoBehaviour
{
    #region 싱글톤
    public static HeartSystem Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    [Header("하트 시스템 설정")]
    // Firebase에서 가져올 실시간 설정값들 (기본값)
    private int heartRecoveryIntervalMinutes = 30; // 하트 회복 간격 (분)
    private int defaultMaxHearts = 5; // 기본 최대 하트 개수
    private int absoluteMaxHearts = 99; // 절대 최대 하트 개수

    // 이벤트
    public event Action<int> OnHeartChanged;
    public event Action<int, int> OnHeartRecovered; // (현재 하트, 회복된 하트 수)
    public event Action<TimeSpan> OnNextRecoveryTimeUpdated;

    private void Start()
    {
        // PlayerDataManager 이벤트 구독
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataLoaded += OnPlayerDataLoaded;
        }
        
        // 설정값 초기화
        InitializeSettings();
    }

    private void OnDestroy()
    {
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataLoaded -= OnPlayerDataLoaded;
        }
    }

    private void OnPlayerDataLoaded()
    {
        // 플레이어 데이터가 로드되면 하트 회복 체크
        CheckAndRecoverHearts();
    }
    
    /// <summary>
    /// 설정값 초기화 (Firebase 연동)
    /// </summary>
    private void InitializeSettings()
    {
        Debug.Log($"[HeartSystem] 기본 설정값 사용 - 회복간격: {heartRecoveryIntervalMinutes}분, 최대하트: {defaultMaxHearts}");
        
        // Firebase에서 실시간 설정값 로드 (비동기)
        _ = LoadSettingsFromFirebase();
    }
    
    /// <summary>
    /// Firebase에서 하트 시스템 설정값 로드
    /// </summary>
    private async System.Threading.Tasks.Task LoadSettingsFromFirebase()
    {
        try
        {
            // Firebase Database에서 게임 설정 로드
            var gameSettings = await FirebaseDatabase.Instance.LoadGameSettingsAsync();
            
            if (gameSettings != null)
            {
                // 하트 관련 설정 업데이트
                if (gameSettings.ContainsKey("heartRecoveryIntervalMinutes"))
                {
                    heartRecoveryIntervalMinutes = (int)gameSettings["heartRecoveryIntervalMinutes"];
                }
                
                if (gameSettings.ContainsKey("defaultMaxHearts"))
                {
                    defaultMaxHearts = (int)gameSettings["defaultMaxHearts"];
                }
                
                if (gameSettings.ContainsKey("absoluteMaxHearts"))
                {
                    absoluteMaxHearts = (int)gameSettings["absoluteMaxHearts"];
                }
                
                Debug.Log($"[HeartSystem] Firebase 설정 로드 완료 - 회복간격: {heartRecoveryIntervalMinutes}분, 최대하트: {defaultMaxHearts}");
            }
            else
            {
                Debug.LogWarning("[HeartSystem] Firebase에서 게임 설정을 찾을 수 없습니다. 기본값을 사용합니다.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[HeartSystem] Firebase 설정 로드 실패: {ex.Message}");
        }
    }

    /// <summary>
    /// 하트 회복 체크 및 실행
    /// </summary>
    public void CheckAndRecoverHearts()
    {
        if (!PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning("[HeartSystem] 플레이어 데이터가 로드되지 않았습니다.");
            return;
        }

        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        // 하트가 이미 최대치인 경우 회복 시간 업데이트만 하고 리턴
        if (playerData.heart >= GetMaxHearts())
        {
            playerData.lastHeartRecoveryTime = currentTime;
            OnHeartChanged?.Invoke(playerData.heart);
            return;
        }

        // 마지막 회복 시간이 0인 경우 (새 플레이어) 현재 시간으로 설정
        if (playerData.lastHeartRecoveryTime == 0)
        {
            playerData.lastHeartRecoveryTime = currentTime;
            OnHeartChanged?.Invoke(playerData.heart);
            return;
        }

        // 경과 시간 계산
        long elapsedMilliseconds = currentTime - playerData.lastHeartRecoveryTime;
        long recoveryIntervalMilliseconds = heartRecoveryIntervalMinutes * 60 * 1000;

        // 회복할 하트 개수 계산
        int heartsToRecover = (int)(elapsedMilliseconds / recoveryIntervalMilliseconds);

        if (heartsToRecover > 0)
        {
            int maxHearts = GetMaxHearts();
            int newHeartCount = Mathf.Min(playerData.heart + heartsToRecover, maxHearts);
            int actualRecovered = newHeartCount - playerData.heart;

            if (actualRecovered > 0)
            {
                playerData.heart = newHeartCount;
                
                // 회복된 하트 수만큼 시간 업데이트
                playerData.lastHeartRecoveryTime += actualRecovered * recoveryIntervalMilliseconds;
                
                Debug.Log($"[HeartSystem] 하트 회복: {actualRecovered}개, 현재 하트: {playerData.heart}/{maxHearts}");
                
                OnHeartRecovered?.Invoke(playerData.heart, actualRecovered);
                OnHeartChanged?.Invoke(playerData.heart);
                
                // 데이터 저장
                _ = PlayerDataManager.Instance.SavePlayerDataAsync();
            }
        }
    }

    /// <summary>
    /// 하트 소모 시도
    /// </summary>
    /// <param name="amount">소모할 하트 개수</param>
    /// <returns>소모 성공 여부</returns>
    public bool TryConsumeHearts(int amount = 1)
    {
        if (!PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.LogWarning("[HeartSystem] 플레이어 데이터가 로드되지 않았습니다.");
            return false;
        }

        var playerData = PlayerDataManager.Instance.CurrentPlayerData;

        if (playerData.heart >= amount)
        {
            playerData.heart -= amount;
            
            // 하트가 최대치 미만이 되었을 때 회복 시간 설정
            if (playerData.heart < GetMaxHearts() && playerData.lastHeartRecoveryTime == 0)
            {
                playerData.lastHeartRecoveryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            
            Debug.Log($"[HeartSystem] 하트 소모: {amount}개, 현재 하트: {playerData.heart}");
            
            OnHeartChanged?.Invoke(playerData.heart);
            
            // 데이터 저장
            _ = PlayerDataManager.Instance.SavePlayerDataAsync();
            
            return true;
        }

        Debug.Log($"[HeartSystem] 하트 부족: 필요 {amount}개, 현재 {playerData.heart}개");
        return false;
    }

    /// <summary>
    /// 하트 추가 (구매, 보상 등)
    /// </summary>
    /// <param name="amount">추가할 하트 개수</param>
    public void AddHearts(int amount)
    {
        if (!PlayerDataManager.Instance.IsDataLoaded || amount <= 0)
        {
            return;
        }

        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        int maxHearts = GetMaxHearts();
        int newHeartCount = Mathf.Min(playerData.heart + amount, maxHearts);
        int actualAdded = newHeartCount - playerData.heart;

        if (actualAdded > 0)
        {
            playerData.heart = newHeartCount;
            
            // 하트가 최대치가 되면 회복 시간 리셋
            if (playerData.heart >= maxHearts)
            {
                playerData.lastHeartRecoveryTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            }
            
            Debug.Log($"[HeartSystem] 하트 추가: {actualAdded}개, 현재 하트: {playerData.heart}/{maxHearts}");
            
            OnHeartChanged?.Invoke(playerData.heart);
            
            // 데이터 저장
            _ = PlayerDataManager.Instance.SavePlayerDataAsync();
        }
    }

    /// <summary>
    /// 현재 하트 개수 반환
    /// </summary>
    public int GetCurrentHearts()
    {
        if (!PlayerDataManager.Instance.IsDataLoaded)
            return 0;

        return PlayerDataManager.Instance.CurrentPlayerData.heart;
    }

    /// <summary>
    /// 최대 하트 개수 반환
    /// </summary>
    public int GetMaxHearts()
    {
        if (!PlayerDataManager.Instance.IsDataLoaded)
            return defaultMaxHearts;

        // 플레이어 데이터에서 최대 하트 개수를 가져오되, 절대 최대치를 넘지 않도록 함
        return Mathf.Min(PlayerDataManager.Instance.CurrentPlayerData.maxHearts, absoluteMaxHearts);
    }

    /// <summary>
    /// 다음 하트 회복까지 남은 시간 반환
    /// </summary>
    public TimeSpan GetTimeUntilNextRecovery()
    {
        if (!PlayerDataManager.Instance.IsDataLoaded)
            return TimeSpan.Zero;

        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        
        // 하트가 이미 최대치인 경우
        if (playerData.heart >= GetMaxHearts())
            return TimeSpan.Zero;

        long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long recoveryIntervalMilliseconds = heartRecoveryIntervalMinutes * 60 * 1000;
        long nextRecoveryTime = playerData.lastHeartRecoveryTime + recoveryIntervalMilliseconds;
        long timeUntilRecovery = nextRecoveryTime - currentTime;

        if (timeUntilRecovery <= 0)
            return TimeSpan.Zero;

        return TimeSpan.FromMilliseconds(timeUntilRecovery);
    }

    /// <summary>
    /// 하트가 가득 찰 때까지 남은 시간 반환
    /// </summary>
    public TimeSpan GetTimeUntilFullRecovery()
    {
        if (!PlayerDataManager.Instance.IsDataLoaded)
            return TimeSpan.Zero;

        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        int maxHearts = GetMaxHearts();
        
        if (playerData.heart >= maxHearts)
            return TimeSpan.Zero;

        int heartsNeeded = maxHearts - playerData.heart;
        long recoveryIntervalMilliseconds = heartRecoveryIntervalMinutes * 60 * 1000;
        
        // 다음 회복까지의 시간 + 나머지 하트들의 회복 시간
        TimeSpan timeUntilNext = GetTimeUntilNextRecovery();
        long additionalTime = (heartsNeeded - 1) * recoveryIntervalMilliseconds;
        
        return timeUntilNext.Add(TimeSpan.FromMilliseconds(additionalTime));
    }

    /// <summary>
    /// 하트 회복 간격 설정 (분 단위)
    /// </summary>
    public void SetHeartRecoveryInterval(int minutes)
    {
        heartRecoveryIntervalMinutes = Mathf.Max(1, minutes);
        Debug.Log($"[HeartSystem] 하트 회복 간격 변경: {heartRecoveryIntervalMinutes}분");
    }

    /// <summary>
    /// 현재 하트 회복 간격 반환 (분 단위)
    /// </summary>
    public int GetHeartRecoveryInterval()
    {
        return heartRecoveryIntervalMinutes;
    }

    /// <summary>
    /// 실시간으로 설정값 업데이트 (Firebase에서 변경된 경우)
    /// </summary>
    public async void RefreshSettingsFromFirebase()
    {
        await LoadSettingsFromFirebase();
        
        // 설정 변경 후 하트 회복 재계산
        CheckAndRecoverHearts();
        
        Debug.Log("[HeartSystem] Firebase 설정 새로고침 완료");
    }
    
    /// <summary>
    /// 현재 설정값들을 반환 (디버그/UI용)
    /// </summary>
    public (int recoveryInterval, int defaultMax, int absoluteMax) GetCurrentSettings()
    {
        return (heartRecoveryIntervalMinutes, defaultMaxHearts, absoluteMaxHearts);
    }
    
    /// <summary>
    /// 하트 시스템 상태 디버그 출력
    /// </summary>
    [ContextMenu("Debug Heart System")]
    public void DebugHeartSystem()
    {
        if (!PlayerDataManager.Instance.IsDataLoaded)
        {
            Debug.Log("[HeartSystem] 플레이어 데이터가 로드되지 않았습니다.");
            return;
        }

        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        Debug.Log($"[HeartSystem] 현재 하트: {playerData.heart}/{GetMaxHearts()}");
        Debug.Log($"[HeartSystem] 마지막 회복 시간: {DateTimeOffset.FromUnixTimeMilliseconds(playerData.lastHeartRecoveryTime)}");
        Debug.Log($"[HeartSystem] 다음 회복까지: {GetTimeUntilNextRecovery()}");
        Debug.Log($"[HeartSystem] 완전 회복까지: {GetTimeUntilFullRecovery()}");
        Debug.Log($"[HeartSystem] 현재 설정 - 회복간격: {heartRecoveryIntervalMinutes}분, 기본최대: {defaultMaxHearts}, 절대최대: {absoluteMaxHearts}");
    }
} 