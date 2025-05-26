using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 친구 정보를 담는 데이터 클래스
/// </summary>
[Serializable]
public class FriendInfo
{
    public string userId;           // 친구의 사용자 ID
    public string displayName;      // 표시될 이름
    public string email;           // 이메일 (선택적)
    public int bestScore;          // 최고 점수
    public long lastLoginTime;     // 마지막 로그인 시간 (Unix timestamp)
    public string characterId;     // 현재 사용 중인 캐릭터 ID
    public bool isOnline;          // 현재 온라인 상태
    
    public FriendInfo()
    {
        userId = "";
        displayName = "";
        email = "";
        bestScore = 0;
        lastLoginTime = 0;
        characterId = "";
        isOnline = false;
    }
    
    public FriendInfo(string userId, string displayName, int bestScore = 0)
    {
        this.userId = userId;
        this.displayName = displayName;
        this.bestScore = bestScore;
        this.email = "";
        this.lastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.characterId = "";
        this.isOnline = true;
    }
}

/// <summary>
/// 친구 요청 정보를 담는 데이터 클래스
/// </summary>
[Serializable]
public class FriendRequest
{
    public string requestId;        // 요청 고유 ID
    public string fromUserId;       // 요청한 사용자 ID
    public string fromDisplayName;  // 요청한 사용자 이름
    public string toUserId;         // 요청받은 사용자 ID
    public long requestTime;        // 요청 시간 (Unix timestamp)
    public FriendRequestStatus status; // 요청 상태
    
    public FriendRequest()
    {
        requestId = "";
        fromUserId = "";
        fromDisplayName = "";
        toUserId = "";
        requestTime = 0;
        status = FriendRequestStatus.Pending;
    }
    
    public FriendRequest(string fromUserId, string fromDisplayName, string toUserId)
    {
        this.requestId = System.Guid.NewGuid().ToString();
        this.fromUserId = fromUserId;
        this.fromDisplayName = fromDisplayName;
        this.toUserId = toUserId;
        this.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.status = FriendRequestStatus.Pending;
    }
}

/// <summary>
/// 친구 요청 상태
/// </summary>
public enum FriendRequestStatus
{
    Pending,    // 대기 중
    Accepted,   // 수락됨
    Rejected,   // 거절됨
    Cancelled   // 취소됨
}

/// <summary>
/// 사용자의 친구 관련 데이터
/// </summary>
[Serializable]
public class UserFriendData
{
    [SerializeField] private List<FriendInfo> friendsList = new List<FriendInfo>();
    [SerializeField] private List<FriendRequest> sentRequestsList = new List<FriendRequest>(); // 보낸 요청들
    [SerializeField] private List<FriendRequest> receivedRequestsList = new List<FriendRequest>(); // 받은 요청들
    
    // Dictionary for quick access (runtime only)
    [NonSerialized] private Dictionary<string, FriendInfo> friendsDict;
    [NonSerialized] private Dictionary<string, FriendRequest> sentRequestsDict;
    [NonSerialized] private Dictionary<string, FriendRequest> receivedRequestsDict;
    
    // Properties
    public List<FriendInfo> Friends => friendsList;
    public List<FriendRequest> SentRequests => sentRequestsList;
    public List<FriendRequest> ReceivedRequests => receivedRequestsList;
    
    public Dictionary<string, FriendInfo> FriendsDict
    {
        get
        {
            if (friendsDict == null) InitializeFriendsFromList();
            return friendsDict;
        }
    }
    
    public Dictionary<string, FriendRequest> SentRequestsDict
    {
        get
        {
            if (sentRequestsDict == null) InitializeSentRequestsFromList();
            return sentRequestsDict;
        }
    }
    
    public Dictionary<string, FriendRequest> ReceivedRequestsDict
    {
        get
        {
            if (receivedRequestsDict == null) InitializeReceivedRequestsFromList();
            return receivedRequestsDict;
        }
    }
    
    /// <summary>
    /// List에서 Dictionary로 초기화
    /// </summary>
    public void InitializeFriendsFromList()
    {
        friendsDict = new Dictionary<string, FriendInfo>();
        foreach (var friend in friendsList)
        {
            if (!string.IsNullOrEmpty(friend.userId))
            {
                friendsDict[friend.userId] = friend;
            }
        }
    }
    
    public void InitializeSentRequestsFromList()
    {
        sentRequestsDict = new Dictionary<string, FriendRequest>();
        foreach (var request in sentRequestsList)
        {
            if (!string.IsNullOrEmpty(request.requestId))
            {
                sentRequestsDict[request.requestId] = request;
            }
        }
    }
    
    public void InitializeReceivedRequestsFromList()
    {
        receivedRequestsDict = new Dictionary<string, FriendRequest>();
        foreach (var request in receivedRequestsList)
        {
            if (!string.IsNullOrEmpty(request.requestId))
            {
                receivedRequestsDict[request.requestId] = request;
            }
        }
    }
    
    /// <summary>
    /// Dictionary에서 List로 업데이트 (저장 전)
    /// </summary>
    public void UpdateListFromDictionary()
    {
        if (friendsDict != null)
        {
            friendsList = new List<FriendInfo>(friendsDict.Values);
        }
        
        if (sentRequestsDict != null)
        {
            sentRequestsList = new List<FriendRequest>(sentRequestsDict.Values);
        }
        
        if (receivedRequestsDict != null)
        {
            receivedRequestsList = new List<FriendRequest>(receivedRequestsDict.Values);
        }
    }
    
    /// <summary>
    /// 친구 추가
    /// </summary>
    public void AddFriend(FriendInfo friend)
    {
        if (FriendsDict.ContainsKey(friend.userId)) return;
        
        FriendsDict[friend.userId] = friend;
        friendsList.Add(friend);
    }
    
    /// <summary>
    /// 친구 제거
    /// </summary>
    public void RemoveFriend(string userId)
    {
        if (!FriendsDict.ContainsKey(userId)) return;
        
        FriendsDict.Remove(userId);
        friendsList.RemoveAll(f => f.userId == userId);
    }
    
    /// <summary>
    /// 친구 요청 추가 (보낸 요청)
    /// </summary>
    public void AddSentRequest(FriendRequest request)
    {
        if (SentRequestsDict.ContainsKey(request.requestId)) 
        {
            Debug.Log($"[UserFriendData] 이미 존재하는 보낸 요청: {request.requestId}");
            return;
        }
        
        SentRequestsDict[request.requestId] = request;
        sentRequestsList.Add(request);
        Debug.Log($"[UserFriendData] 보낸 요청 추가: {request.requestId} -> {request.toUserId}");
    }
    
    /// <summary>
    /// 친구 요청 추가 (받은 요청)
    /// </summary>
    public void AddReceivedRequest(FriendRequest request)
    {
        if (ReceivedRequestsDict.ContainsKey(request.requestId)) 
        {
            Debug.Log($"[UserFriendData] 이미 존재하는 받은 요청: {request.requestId}");
            return;
        }
        
        ReceivedRequestsDict[request.requestId] = request;
        receivedRequestsList.Add(request);
        Debug.Log($"[UserFriendData] 받은 요청 추가: {request.requestId} from {request.fromUserId}");
    }
    
    /// <summary>
    /// 요청 제거
    /// </summary>
    public void RemoveRequest(string requestId, bool isSent)
    {
        if (isSent)
        {
            SentRequestsDict.Remove(requestId);
            sentRequestsList.RemoveAll(r => r.requestId == requestId);
        }
        else
        {
            ReceivedRequestsDict.Remove(requestId);
            receivedRequestsList.RemoveAll(r => r.requestId == requestId);
        }
    }
    
    /// <summary>
    /// 친구인지 확인
    /// </summary>
    public bool IsFriend(string userId)
    {
        return FriendsDict.ContainsKey(userId);
    }
    
    /// <summary>
    /// 이미 요청을 보냈는지 확인
    /// </summary>
    public bool HasSentRequestTo(string userId)
    {
        return sentRequestsList.Any(r => r.toUserId == userId && r.status == FriendRequestStatus.Pending);
    }
    
    /// <summary>
    /// 해당 사용자로부터 요청을 받았는지 확인
    /// </summary>
    public bool HasReceivedRequestFrom(string userId)
    {
        return receivedRequestsList.Any(r => r.fromUserId == userId && r.status == FriendRequestStatus.Pending);
    }

    /// <summary>
    /// 친구 목록 설정 (외부에서 로드한 데이터로 설정)
    /// </summary>
    public void SetFriendsList(List<FriendInfo> friends)
    {
        friendsList.Clear();
        if (friends != null)
        {
            friendsList.AddRange(friends);
        }
        InitializeFriendsFromList();
    }

    /// <summary>
    /// 보낸 요청 목록 설정 (외부에서 로드한 데이터로 설정)
    /// </summary>
    public void SetSentRequestsList(List<FriendRequest> requests)
    {
        sentRequestsList.Clear();
        if (requests != null)
        {
            sentRequestsList.AddRange(requests);
        }
        InitializeSentRequestsFromList();
    }

    /// <summary>
    /// 받은 요청 목록 설정 (외부에서 로드한 데이터로 설정)
    /// </summary>
    public void SetReceivedRequestsList(List<FriendRequest> requests)
    {
        receivedRequestsList.Clear();
        if (requests != null)
        {
            receivedRequestsList.AddRange(requests);
        }
        InitializeReceivedRequestsFromList();
    }

    /// <summary>
    /// 친구 목록 초기화
    /// </summary>
    public void ClearFriends()
    {
        friendsList.Clear();
        if (friendsDict != null)
        {
            friendsDict.Clear();
        }
    }

    /// <summary>
    /// 보낸 요청 목록 초기화
    /// </summary>
    public void ClearSentRequests()
    {
        sentRequestsList.Clear();
        if (sentRequestsDict != null)
        {
            sentRequestsDict.Clear();
        }
    }

    /// <summary>
    /// 받은 요청 목록 초기화
    /// </summary>
    public void ClearReceivedRequests()
    {
        receivedRequestsList.Clear();
        if (receivedRequestsDict != null)
        {
            receivedRequestsDict.Clear();
        }
    }
}

/// <summary>
/// 사용자 검색을 위한 공개 프로필 정보
/// </summary>
[Serializable]
public class UserPublicProfile
{
    public string userId;
    public string displayName;
    public int bestScore;
    public string characterId;
    public long lastLoginTime;
    public bool isSearchable; // 검색 허용 여부
    
    public UserPublicProfile()
    {
        userId = "";
        displayName = "";
        bestScore = 0;
        characterId = "";
        lastLoginTime = 0;
        isSearchable = true;
    }
} 