using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// 친구 시스템 관리자
/// Firebase Database와 연동하여 친구 관련 기능을 제공
/// </summary>
public class FriendManager : MonoBehaviour
{
    public static FriendManager Instance { get; private set; }
    
    // 이벤트
    public event Action<List<FriendData>> OnFriendListUpdated;
    public event Action<List<FriendRequestData>> OnFriendRequestsUpdated;
    public event Action<string> OnFriendRequestSent;
    public event Action<string, bool> OnFriendRequestResponded;
    public event Action<string> OnFriendRemoved;
    public event Action<string> OnError;
    
    // 캐시된 데이터
    private List<FriendData> cachedFriends = new List<FriendData>();
    private List<FriendRequestData> cachedRequests = new List<FriendRequestData>();
    
    // 상태
    public bool IsInitialized { get; private set; }
    
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
    
    private async void Start()
    {
        await InitializeAsync();
    }
    
    /// <summary>
    /// 친구 매니저 초기화
    /// </summary>
    public async Task InitializeAsync()
    {
        try
        {
            Debug.Log("[FriendManager] 초기화 시작");
            
            // Firebase 초기화 대기
            if (FirebaseManager.Instance != null)
            {
                while (!FirebaseManager.Instance.IsInitialized)
                {
                    await Task.Delay(100);
                }
                
                // 로그인 상태 변경 이벤트 구독
                FirebaseManager.Instance.OnLoginStateChanged += OnLoginStateChanged;
                
                // 현재 로그인되어 있다면 친구 데이터 로드
                if (FirebaseManager.Instance.IsAuthenticated)
                {
                    await LoadFriendDataAsync();
                }
            }
            
            IsInitialized = true;
            Debug.Log("[FriendManager] 초기화 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 초기화 실패: {e.Message}");
            OnError?.Invoke($"Friend system initialization failed: {e.Message}");
        }
    }
    
    /// <summary>
    /// 로그인 상태 변경 처리
    /// </summary>
    private async void OnLoginStateChanged(bool isLoggedIn)
    {
        if (isLoggedIn)
        {
            await LoadFriendDataAsync();
        }
        else
        {
            ClearCachedData();
        }
    }
    
    /// <summary>
    /// 캐시된 데이터 초기화
    /// </summary>
    private void ClearCachedData()
    {
        cachedFriends.Clear();
        cachedRequests.Clear();
        OnFriendListUpdated?.Invoke(cachedFriends);
        OnFriendRequestsUpdated?.Invoke(cachedRequests);
    }    
    /// <summary>
    /// 친구 데이터 로드
    /// </summary>
    public async Task LoadFriendDataAsync()
    {
        try
        {
            if (!FirebaseManager.Instance.IsAuthenticated)
            {
                Debug.LogWarning("[FriendManager] 로그인되지 않은 상태에서 친구 데이터 로드 시도");
                return;
            }
            
            string userId = FirebaseManager.Instance.UserId;
            Debug.Log($"[FriendManager] 친구 데이터 로드 시작: {userId}");
            
            // 친구 목록 로드
            await LoadFriendListAsync();
            
            // 친구 요청 목록 로드
            await LoadFriendRequestsAsync();
            
            Debug.Log("[FriendManager] 친구 데이터 로드 완료");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 친구 데이터 로드 실패: {e.Message}");
            OnError?.Invoke($"Failed to load friend data: {e.Message}");
        }
    }
    
    /// <summary>
    /// 친구 목록 로드
    /// </summary>
    public async Task LoadFriendListAsync()
    {
        try
        {
            string userId = FirebaseManager.Instance.UserId;
            var friendsData = await FirebaseDatabase.Instance.GetFriendsAsync(userId);
            
            if (friendsData.success)
            {
                cachedFriends = friendsData.friends;
                OnFriendListUpdated?.Invoke(cachedFriends);
                Debug.Log($"[FriendManager] 친구 목록 로드 완료: {cachedFriends.Count}명");
            }
            else
            {
                Debug.LogWarning($"[FriendManager] 친구 목록 로드 실패: {friendsData.errorMessage}");
                OnError?.Invoke(friendsData.errorMessage);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 친구 목록 로드 예외: {e.Message}");
            OnError?.Invoke($"Failed to load friends: {e.Message}");
        }
    }
    
    /// <summary>
    /// 친구 요청 목록 로드
    /// </summary>
    public async Task LoadFriendRequestsAsync()
    {
        try
        {
            string userId = FirebaseManager.Instance.UserId;
            var requestsData = await FirebaseDatabase.Instance.GetFriendRequestsAsync(userId);
            
            if (requestsData.success)
            {
                cachedRequests = requestsData.requests;
                OnFriendRequestsUpdated?.Invoke(cachedRequests);
                Debug.Log($"[FriendManager] 친구 요청 목록 로드 완료: {cachedRequests.Count}개");
            }
            else
            {
                Debug.LogWarning($"[FriendManager] 친구 요청 목록 로드 실패: {requestsData.errorMessage}");
                OnError?.Invoke(requestsData.errorMessage);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 친구 요청 목록 로드 예외: {e.Message}");
            OnError?.Invoke($"Failed to load friend requests: {e.Message}");
        }
    }    
    /// <summary>
    /// 닉네임으로 사용자 검색
    /// </summary>
    public async Task<UserSearchResult> SearchUserByNicknameAsync(string nickname)
    {
        try
        {
            if (string.IsNullOrEmpty(nickname))
            {
                return new UserSearchResult();
            }
            
            Debug.Log($"[FriendManager] 사용자 검색: {nickname}");
            
            var result = await FirebaseDatabase.Instance.SearchUserByNicknameAsync(nickname);
            
            if (result != null && !string.IsNullOrEmpty(result.userId))
            {
                string currentUserId = FirebaseManager.Instance.UserId;
                
                // 자기 자신을 검색한 경우
                if (result.userId == currentUserId)
                {
                    Debug.Log("[FriendManager] 자기 자신을 검색했습니다.");
                    return new UserSearchResult(); // 빈 결과 반환
                }
                
                // 이미 친구인지 확인
                result.isFriend = cachedFriends.Exists(f => f.userId == result.userId);
                
                // 대기 중인 친구 요청이 있는지 확인 (보낸 요청과 받은 요청 모두 확인)
                result.hasPendingRequest = cachedRequests.Exists(r => 
                    (r.fromUserId == currentUserId && r.toUserId == result.userId) ||
                    (r.fromUserId == result.userId && r.toUserId == currentUserId));
                
                Debug.Log($"[FriendManager] 사용자 검색 완료: {result.nickname}, 친구여부: {result.isFriend}, 요청대기: {result.hasPendingRequest}");
            }
            
            return result ?? new UserSearchResult();
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 사용자 검색 실패: {e.Message}");
            OnError?.Invoke($"Failed to search user: {e.Message}");
            return new UserSearchResult();
        }
    }
    
    /// <summary>
    /// 친구 요청 보내기
    /// </summary>
    public async Task<bool> SendFriendRequestAsync(string targetUserId, string targetNickname)
    {
        try
        {
            if (!FirebaseManager.Instance.IsAuthenticated)
            {
                OnError?.Invoke("Please login first");
                return false;
            }
            
            string fromUserId = FirebaseManager.Instance.UserId;
            
            // PlayerDataManager가 초기화되지 않았거나 닉네임이 없는 경우 처리
            string fromNickname = "Unknown";
            if (PlayerDataManager.Instance != null && 
                PlayerDataManager.Instance.IsDataLoaded && 
                PlayerDataManager.Instance.CurrentPlayerData != null &&
                !string.IsNullOrEmpty(PlayerDataManager.Instance.CurrentPlayerData.nickname))
            {
                fromNickname = PlayerDataManager.Instance.CurrentPlayerData.nickname;
            }
            else
            {
                Debug.LogWarning("[FriendManager] PlayerDataManager가 초기화되지 않았거나 닉네임이 설정되지 않음");
                OnError?.Invoke("Please set your nickname first");
                return false;
            }
            
            Debug.Log($"[FriendManager] 친구 요청 전송: {fromNickname} -> {targetNickname}");
            
            var requestData = new FriendRequestData(fromUserId, fromNickname, targetUserId, targetNickname);
            bool success = await FirebaseDatabase.Instance.SendFriendRequestAsync(requestData);
            
            if (success)
            {
                OnFriendRequestSent?.Invoke($"Friend request sent to {targetNickname}");
                await LoadFriendRequestsAsync(); // 요청 목록 새로고침
                Debug.Log("[FriendManager] 친구 요청 전송 완료");
                return true;
            }
            else
            {
                OnError?.Invoke("Failed to send friend request");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 친구 요청 전송 실패: {e.Message}");
            OnError?.Invoke($"Failed to send friend request: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 친구 요청 취소
    /// </summary>
    public async Task<bool> CancelFriendRequestAsync(string requestId, string toUserId, string toNickname)
    {
        try
        {
            if (!FirebaseManager.Instance.IsAuthenticated)
            {
                OnError?.Invoke("Please login first");
                return false;
            }
            
            Debug.Log($"[FriendManager] 친구 요청 취소: {toNickname}");
            
            bool success = await FirebaseDatabase.Instance.CancelFriendRequestAsync(requestId, toUserId);
            
            if (success)
            {
                OnFriendRequestSent?.Invoke($"Friend request to {toNickname} cancelled");
                await LoadFriendRequestsAsync(); // 요청 목록 새로고침
                Debug.Log("[FriendManager] 친구 요청 취소 완료");
                return true;
            }
            else
            {
                OnError?.Invoke("Failed to cancel friend request");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 친구 요청 취소 실패: {e.Message}");
            OnError?.Invoke($"Failed to cancel friend request: {e.Message}");
            return false;
        }
    }    
    /// <summary>
    /// 친구 요청 응답 (수락/거절)
    /// </summary>
    public async Task<bool> RespondToFriendRequestAsync(string requestId, bool accept)
    {
        try
        {
            Debug.Log($"[FriendManager] 친구 요청 응답: {requestId}, 수락: {accept}");
            
            bool success = await FirebaseDatabase.Instance.RespondToFriendRequestAsync(requestId, accept);
            
            if (success)
            {
                string message = accept ? "Friend request accepted" : "Friend request rejected";
                OnFriendRequestResponded?.Invoke(message, accept);
                
                // 데이터 새로고침
                await LoadFriendRequestsAsync();
                if (accept)
                {
                    await LoadFriendListAsync(); // 수락한 경우 친구 목록도 새로고침
                }
                
                Debug.Log("[FriendManager] 친구 요청 응답 완료");
                return true;
            }
            else
            {
                OnError?.Invoke("Failed to respond to friend request");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 친구 요청 응답 실패: {e.Message}");
            OnError?.Invoke($"Failed to respond to friend request: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 친구 삭제
    /// </summary>
    public async Task<bool> RemoveFriendAsync(string friendUserId, string friendNickname)
    {
        try
        {
            Debug.Log($"[FriendManager] 친구 삭제: {friendNickname}");
            
            string userId = FirebaseManager.Instance.UserId;
            bool success = await FirebaseDatabase.Instance.RemoveFriendAsync(userId, friendUserId);
            
            if (success)
            {
                OnFriendRemoved?.Invoke($"Removed {friendNickname} from friends");
                await LoadFriendListAsync(); // 친구 목록 새로고침
                Debug.Log("[FriendManager] 친구 삭제 완료");
                return true;
            }
            else
            {
                OnError?.Invoke("Failed to remove friend");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FriendManager] 친구 삭제 실패: {e.Message}");
            OnError?.Invoke($"Failed to remove friend: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// 캐시된 친구 목록 반환
    /// </summary>
    public List<FriendData> GetCachedFriends()
    {
        return new List<FriendData>(cachedFriends);
    }
    
    /// <summary>
    /// 캐시된 친구 요청 목록 반환
    /// </summary>
    public List<FriendRequestData> GetCachedFriendRequests()
    {
        return new List<FriendRequestData>(cachedRequests);
    }
    
    private void OnDestroy()
    {
        if (FirebaseManager.Instance != null)
        {
            FirebaseManager.Instance.OnLoginStateChanged -= OnLoginStateChanged;
        }
    }
}