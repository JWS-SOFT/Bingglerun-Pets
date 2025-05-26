using System;
using System;
using System.Collections.Generic;

/// <summary>
/// 친구 데이터 구조
/// </summary>
[Serializable]
public class FriendData
{
    public string userId;
    public string nickname;
    public bool isOnline;
    public long lastLoginTime;
    public int level;
    public string profileImageUrl;
    
    public FriendData()
    {
        userId = "";
        nickname = "";
        isOnline = false;
        lastLoginTime = 0;
        level = 1;
        profileImageUrl = "";
    }
    
    public FriendData(string userId, string nickname, bool isOnline = false, int level = 1)
    {
        this.userId = userId;
        this.nickname = nickname;
        this.isOnline = isOnline;
        this.lastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.level = level;
        this.profileImageUrl = "";
    }
}

/// <summary>
/// 친구 요청 데이터 구조
/// </summary>
[Serializable]
public class FriendRequestData
{
    public string requestId;
    public string fromUserId;
    public string fromNickname;
    public string toUserId;
    public string toNickname;  // 받는 사람 닉네임 추가
    public long requestTime;
    public FriendRequestStatus status;
    public FriendRequestType requestType;  // UI 표시용 요청 타입
    
    public FriendRequestData()
    {
        requestId = "";
        fromUserId = "";
        fromNickname = "";
        toUserId = "";
        toNickname = "";
        requestTime = 0;
        status = FriendRequestStatus.Pending;
        requestType = FriendRequestType.Received;
    }
    
    public FriendRequestData(string fromUserId, string fromNickname, string toUserId, string toNickname = "")
    {
        this.requestId = Guid.NewGuid().ToString();
        this.fromUserId = fromUserId;
        this.fromNickname = fromNickname;
        this.toUserId = toUserId;
        this.toNickname = toNickname;
        this.requestTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        this.status = FriendRequestStatus.Pending;
        this.requestType = FriendRequestType.Received; // 기본값, 나중에 설정됨
    }
}

/// <summary>
/// 친구 요청 상태
/// </summary>
public enum FriendRequestStatus
{
    Pending,    // 대기 중
    Accepted,   // 수락됨
    Rejected    // 거절됨
}

/// <summary>
/// 친구 요청 타입 (UI 표시용)
/// </summary>
public enum FriendRequestType
{
    Received,   // 받은 요청
    Sent        // 보낸 요청
}

/// <summary>
/// 사용자 검색 결과 데이터
/// </summary>
[Serializable]
public class UserSearchResult
{
    public string userId;
    public string nickname;
    public int level;
    public bool isOnline;
    public bool isFriend;
    public bool hasPendingRequest;
    
    public UserSearchResult()
    {
        userId = "";
        nickname = "";
        level = 1;
        isOnline = false;
        isFriend = false;
        hasPendingRequest = false;
    }
}

/// <summary>
/// 친구 목록 응답 데이터
/// </summary>
[Serializable]
public class FriendListResponse
{
    public List<FriendData> friends;
    public bool success;
    public string errorMessage;
    
    public FriendListResponse()
    {
        friends = new List<FriendData>();
        success = false;
        errorMessage = "";
    }
}

/// <summary>
/// 친구 요청 목록 응답 데이터
/// </summary>
[Serializable]
public class FriendRequestListResponse
{
    public List<FriendRequestData> requests;
    public bool success;
    public string errorMessage;
    
    public FriendRequestListResponse()
    {
        requests = new List<FriendRequestData>();
        success = false;
        errorMessage = "";
    }
}