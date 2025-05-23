using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

[System.Serializable]
public class LeaderboardManager : MonoBehaviour
{
    #region 싱글톤
    public static LeaderboardManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[LeaderboardManager] 이미 인스턴스가 존재합니다. 중복 제거.");
            Destroy(this);
            return;
        }

        Instance = this;
        Debug.Log("[LeaderboardManager] GameManager 컴포넌트로 초기화 완료");
    }

    // GameManager에 자동으로 LeaderboardManager 컴포넌트 추가
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void AutoAddToGameManager()
    {
        if (Instance != null) return;

        // GameManager 찾기
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            // LeaderboardManager 컴포넌트가 이미 있는지 확인
            LeaderboardManager existingComponent = gameManager.GetComponent<LeaderboardManager>();
            if (existingComponent == null)
            {
                // LeaderboardManager 컴포넌트 추가
                gameManager.gameObject.AddComponent<LeaderboardManager>();
                Debug.Log("[LeaderboardManager] GameManager에 자동으로 추가됨");
            }
        }
        else
        {
            Debug.LogWarning("[LeaderboardManager] GameManager를 찾을 수 없어 자동 추가 실패");
        }
    }
    #endregion

    [Header("Leaderboard Settings")]
    public int maxLeaderboardEntries = 50;     // 표시할 최대 리더보드 항목 수

    // 캐시된 리더보드 데이터
    private List<PlayerData> cachedLeaderboard = new List<PlayerData>();
    private bool isLeaderboardLoaded = false;

    /// <summary>
    /// 로비 상태인지 확인
    /// </summary>
    private bool IsInLobbyState()
    {
        if (GameManager.Instance?.StateMachine == null)
        {
            Debug.LogWarning("[LeaderboardManager] GameManager 또는 StateMachine이 없습니다.");
            return false;
        }

        bool isLobby = GameManager.Instance.StateMachine.CurrentState == GameState.Lobby;
        if (!isLobby)
        {
            Debug.Log($"[LeaderboardManager] 현재 게임 상태: {GameManager.Instance.StateMachine.CurrentState}, 로비 상태가 아니므로 리더보드 기능 비활성화");
        }
        return isLobby;
    }

    /// <summary>
    /// 리더보드 데이터 로드 (Firebase의 players 경로에서) - 로비 상태에서만 동작
    /// </summary>
    public async Task<List<PlayerData>> LoadLeaderboardAsync(int limit = 50)
    {
        // 로비 상태가 아니면 빈 리스트 반환
        if (!IsInLobbyState())
        {
            return new List<PlayerData>();
        }

        try
        {
#if FIREBASE_DATABASE
            if (FirebaseDatabase.Instance != null && FirebaseDatabase.Instance.IsInitialized)
            {
                // Firebase에서 모든 플레이어 데이터 가져오기
                Firebase.Database.FirebaseDatabase database = 
                    Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/");
                Firebase.Database.DatabaseReference playersRef = database.GetReference("players");
                
                Firebase.Database.DataSnapshot snapshot = await playersRef.GetValueAsync();
                
                List<PlayerData> allPlayers = new List<PlayerData>();
                
                if (snapshot.Exists)
                {
                    foreach (Firebase.Database.DataSnapshot child in snapshot.Children)
                    {
                        string json = child.GetRawJsonValue();
                        if (!string.IsNullOrEmpty(json))
                        {
                            try
                            {
                                PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
                                if (playerData != null && playerData.competitiveBestScore > 0)
                                {
                                    // Dictionary 초기화 (필요시)
                                    playerData.InitializeStagesFromList();
                                    playerData.InitializeItemsFromList();
                                    
                                    allPlayers.Add(playerData);
                                }
                            }
                            catch (Exception ex)
                            {
                                Debug.LogWarning($"[LeaderboardManager] 플레이어 데이터 파싱 실패: {ex.Message}");
                            }
                        }
                    }
                }
                
                // competitiveBestScore 기준으로 내림차순 정렬
                allPlayers.Sort((a, b) => b.competitiveBestScore.CompareTo(a.competitiveBestScore));
                
                // 제한된 수만큼 반환
                var topPlayers = allPlayers.Take(Math.Min(limit, maxLeaderboardEntries)).ToList();
                
                // 캐시 업데이트
                cachedLeaderboard = topPlayers;
                isLeaderboardLoaded = true;
                
                Debug.Log($"[LeaderboardManager] 리더보드 로드 완료: {topPlayers.Count}개 항목");
                return topPlayers;
            }
            else
            {
                Debug.LogError("[LeaderboardManager] Firebase Database가 초기화되지 않음");
            }
#else
            // 테스트 모드: 더미 데이터 생성
            await Task.Delay(500);
            
            if (cachedLeaderboard.Count == 0)
            {
                // 테스트용 더미 데이터 생성
                CreateTestData();
            }
            
            var sortedEntries = cachedLeaderboard
                .Where(p => p.competitiveBestScore > 0)
                .OrderByDescending(p => p.competitiveBestScore)
                .Take(limit)
                .ToList();
                
            Debug.Log($"[LeaderboardManager] 테스트 리더보드 로드: {sortedEntries.Count}개 항목");
            return sortedEntries;
#endif
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LeaderboardManager] 리더보드 로드 실패: {ex.Message}");
        }
        
        return new List<PlayerData>();
    }

    /// <summary>
    /// 플레이어의 순위 조회 - 로비 상태에서만 동작
    /// </summary>
    public async Task<int> GetPlayerRankAsync(string playerId)
    {
        // 로비 상태가 아니면 -1 반환
        if (!IsInLobbyState())
        {
            return -1;
        }

        try
        {
            var leaderboard = await LoadLeaderboardAsync(1000); // 충분히 많은 수로 로드
            
            for (int i = 0; i < leaderboard.Count; i++)
            {
                if (leaderboard[i].playerId == playerId)
                {
                    return i + 1; // 1부터 시작하는 순위
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[LeaderboardManager] 순위 조회 실패: {ex.Message}");
        }
        
        return -1; // 순위 없음
    }

    /// <summary>
    /// 현재 플레이어의 순위 조회 - 로비 상태에서만 동작
    /// </summary>
    public async Task<int> GetCurrentPlayerRankAsync()
    {
        // 로비 상태가 아니면 -1 반환
        if (!IsInLobbyState())
        {
            return -1;
        }

        if (PlayerDataManager.Instance?.CurrentPlayerData == null)
        {
            Debug.LogWarning("[LeaderboardManager] 현재 플레이어 데이터가 없습니다.");
            return -1;
        }

        return await GetPlayerRankAsync(PlayerDataManager.Instance.CurrentPlayerData.playerId);
    }

    /// <summary>
    /// 테스트용 더미 데이터 생성
    /// </summary>
    private void CreateTestData()
    {
        string[] testNames = { "김진우", "이수민", "박민준", "최서연", "정하늘", "강도윤", "윤지유", "임서준", "조민서", "한예준" };
        
        for (int i = 0; i < testNames.Length; i++)
        {
            PlayerData testPlayer = new PlayerData
            {
                playerId = $"test_player_{i}",
                nickname = testNames[i],
                competitiveBestScore = UnityEngine.Random.Range(50000, 999999),
                level = UnityEngine.Random.Range(1, 20),
                totalStars = UnityEngine.Random.Range(10, 100),
                lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - (i * 3600000) // 1시간씩 간격
            };
            
            cachedLeaderboard.Add(testPlayer);
        }
        
        // 점수 기준으로 정렬
        cachedLeaderboard.Sort((a, b) => b.competitiveBestScore.CompareTo(a.competitiveBestScore));
    }

    /// <summary>
    /// 캐시된 리더보드 데이터 가져오기 (즉시 반환) - 로비 상태에서만 동작
    /// </summary>
    public List<PlayerData> GetCachedLeaderboard()
    {
        if (!IsInLobbyState())
        {
            return new List<PlayerData>();
        }

        return cachedLeaderboard;
    }

    /// <summary>
    /// 리더보드 캐시 무효화
    /// </summary>
    public void InvalidateCache()
    {
        isLeaderboardLoaded = false;
        cachedLeaderboard.Clear();
        Debug.Log("[LeaderboardManager] 리더보드 캐시 무효화");
    }
} 