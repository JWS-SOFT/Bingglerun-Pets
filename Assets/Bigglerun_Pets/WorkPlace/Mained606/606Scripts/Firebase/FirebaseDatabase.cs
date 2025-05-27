using System;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Firebase Realtime Database와 통신하기 위한 매니저 클래스
/// </summary>
public class FirebaseDatabase : MonoBehaviour
{
    #region 싱글톤
    public static FirebaseDatabase Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeFirebaseDatabase();
    }
    #endregion
    
    // Firebase 초기화 상태
    public bool IsInitialized { get; private set; }
    
    // Firebase SDK 참조 (조건부 컴파일용)
#if FIREBASE_DATABASE
    private Firebase.Database.FirebaseDatabase database;
    private Firebase.Database.DatabaseReference databaseReference;
#endif

    // 더미 데이터 (Firebase SDK가 없을 때 사용)
    private Dictionary<string, string> mockDatabase = new Dictionary<string, string>();
    
    /// <summary>
    /// Firebase Database 초기화
    /// </summary>
    private void InitializeFirebaseDatabase()
    {
#if FIREBASE_DATABASE
        try
        {
            // URL 직접 지정하여 데이터베이스 인스턴스 가져오기
            database = Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/");
            databaseReference = database.RootReference;
            
            IsInitialized = true;
            Debug.Log("[FirebaseDatabase] Firebase Database 초기화 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] Firebase Database 초기화 중 오류 발생: {ex.Message}");
            IsInitialized = false;
        }
#else
        // Firebase SDK가 없을 때 테스트용 초기화
        Debug.LogWarning("[FirebaseDatabase] Firebase SDK가 없습니다. 테스트용 DB를 사용합니다.");
        IsInitialized = true;
#endif
    }
    
    /// <summary>
    /// Firebase에서 플레이어 데이터 로드
    /// </summary>
    public async Task<PlayerData> LoadPlayerDataAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID가 없습니다.");
            return null;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return null;
        }
        
        try
        {
            // Realtime Database에서 플레이어 데이터 가져오기
            Firebase.Database.DataSnapshot snapshot = 
                await databaseReference.Child("players").Child(userId).GetValueAsync();
                
            if (snapshot.Exists)
            {
                // JSON 데이터 가져오기
                string json = snapshot.GetRawJsonValue();
                
                // JSON을 PlayerData 객체로 변환
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
                
                // Dictionary 초기화
                if (playerData != null)
                {
                    // 스테이지 데이터 Dictionary 초기화
                    playerData.InitializeStagesFromList();
                    
                    // 아이템 데이터 Dictionary 초기화
                    playerData.InitializeItemsFromList();
                    
                    // StageData의 additionalData Dictionary 초기화
                    if (playerData.storyStages != null)
                    {
                        foreach (var stageData in playerData.storyStages.Values)
                        {
                            stageData.InitializeAdditionalDataFromList();
                        }
                    }
                }
                
                Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 로드했습니다.");
                return playerData;
            }
            else
            {
                Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터가 없습니다.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 로드 중 오류 발생: {ex.Message}");
            return null;
        }
#else
        // 테스트용 더미 로드
        await Task.Delay(500); // 네트워크 지연 시뮬레이션
        
        if (mockDatabase.TryGetValue(userId, out string json))
        {
            Debug.Log($"[FirebaseDatabase] 테스트 데이터 로드: {userId}");
            PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
            
            // Dictionary 초기화
            if (playerData != null)
            {
                // 스테이지 데이터 Dictionary 초기화
                playerData.InitializeStagesFromList();
                
                // 아이템 데이터 Dictionary 초기화
                playerData.InitializeItemsFromList();
                
                // StageData의 additionalData Dictionary 초기화
                if (playerData.storyStages != null)
                {
                    foreach (var stageData in playerData.storyStages.Values)
                    {
                        stageData.InitializeAdditionalDataFromList();
                    }
                }
            }
            
            return playerData;
        }
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 없음: {userId}");
        return null;
#endif
    }
    
    /// <summary>
    /// Firebase에 플레이어 데이터 저장
    /// </summary>
    public async Task<bool> SavePlayerDataAsync(string userId, PlayerData data)
    {
        if (string.IsNullOrEmpty(userId) || data == null)
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID 또는 데이터가 없습니다.");
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
        
        try
        {
            // Dictionary를 직렬화 가능한 List로 변환
            data.UpdateListFromDictionary();
            data.UpdateItemsListFromDictionary();
            
            // StageData의 additionalData 업데이트
            if (data.storyStages != null)
            {
                foreach (var stageData in data.storyStages.Values)
                {
                    stageData.UpdateAdditionalDataListFromDictionary();
                }
            }
            
            // PlayerData 객체를 JSON으로 변환
            string json = JsonUtility.ToJson(data);
            
            // Realtime Database에 저장
            await databaseReference.Child("players").Child(userId).SetRawJsonValueAsync(json);
            
            Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 저장했습니다.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 저장 중 오류 발생: {ex.Message}");
            return false;
        }
#else
        // 테스트용 더미 저장
        await Task.Delay(200); // 네트워크 지연 시뮬레이션
        
        // Dictionary를 직렬화 가능한 List로 변환
        data.UpdateListFromDictionary();
        data.UpdateItemsListFromDictionary();
        
        // StageData의 additionalData 업데이트
        if (data.storyStages != null)
        {
            foreach (var stageData in data.storyStages.Values)
            {
                stageData.UpdateAdditionalDataListFromDictionary();
            }
        }
        
        string json = JsonUtility.ToJson(data);
        mockDatabase[userId] = json;
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 저장: {userId}");
        return true;
#endif
    }
    
    /// <summary>
    /// Firebase에서 플레이어 데이터 삭제
    /// </summary>
    public async Task<bool> DeletePlayerDataAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID가 없습니다.");
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
        
        try
        {
            // Realtime Database에서 데이터 삭제
            await databaseReference.Child("players").Child(userId).RemoveValueAsync();
            
            Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 삭제했습니다.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 삭제 중 오류 발생: {ex.Message}");
            return false;
        }
#else
        // 테스트용 더미 삭제
        await Task.Delay(200); // 네트워크 지연 시뮬레이션
        
        if (mockDatabase.ContainsKey(userId))
        {
            mockDatabase.Remove(userId);
            Debug.Log($"[FirebaseDatabase] 테스트 데이터 삭제: {userId}");
            return true;
        }
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 없음 (삭제 실패): {userId}");
        return false;
#endif
    }
    
    /// <summary>
    /// 플레이어 데이터 일부 업데이트
    /// </summary>
    public async Task<bool> UpdatePlayerDataAsync(string userId, Dictionary<string, object> updates)
    {
        if (string.IsNullOrEmpty(userId) || updates == null || updates.Count == 0)
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID 또는 업데이트 데이터가 없습니다.");
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
        
        try
        {
            var playerRef = databaseReference.Child("players").Child(userId);
            
            // 각 필드 업데이트
            foreach (var entry in updates)
            {
                await playerRef.Child(entry.Key).SetValueAsync(entry.Value);
            }
            
            Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 부분 업데이트했습니다.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 업데이트 중 오류 발생: {ex.Message}");
            return false;
        }
#else
        // 테스트용 더미 업데이트
        await Task.Delay(200); // 네트워크 지연 시뮬레이션
        
        if (mockDatabase.TryGetValue(userId, out string json))
        {
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            
            // 이 부분은 실제로는 JSON을 파싱하여 특정 필드만 업데이트해야 하지만,
            // 테스트용 구현에서는 단순화를 위해 생략합니다.
            
            mockDatabase[userId] = JsonUtility.ToJson(data);
            Debug.Log($"[FirebaseDatabase] 테스트 데이터 부분 업데이트: {userId}");
            return true;
        }
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 없음 (업데이트 실패): {userId}");
        return false;
#endif
    }
    
    /// <summary>
    /// 오프라인 지속성 설정
    /// </summary>
    public void SetPersistenceEnabled(bool enabled)
    {
#if FIREBASE_DATABASE
        if (IsInitialized)
        {
            database.SetPersistenceEnabled(enabled);
            Debug.Log($"[FirebaseDatabase] 오프라인 지속성 설정: {enabled}");
        }
#else
        Debug.Log($"[FirebaseDatabase] 테스트 환경에서는 오프라인 지속성 설정이 무시됩니다: {enabled}");
#endif
    }

    #region 친구 시스템 관련 메서드
    
    /// <summary>
    /// 사용자 기본 정보 조회 (레벨, 마지막 로그인 등)
    /// </summary>
    private async Task<(int level, long lastLogin, bool isOnline)> GetUserBasicInfoAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            return (1, 0, false);
        }
        
#if FIREBASE_DATABASE
        try
        {
            var snapshot = await databaseReference.Child("players").Child(userId).GetValueAsync();
            
            if (snapshot.Exists)
            {
                string playerJson = snapshot.GetRawJsonValue();
                if (!string.IsNullOrEmpty(playerJson))
                {
                    var playerData = JsonUtility.FromJson<PlayerData>(playerJson);
                    
                    // 온라인 상태는 마지막 업데이트가 5분 이내인지로 판단
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    bool isOnline = (currentTime - playerData.lastUpdateTimestamp) < 300000; // 5분 (밀리초)
                    
                    return (playerData.level, playerData.lastUpdateTimestamp, isOnline);
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 사용자 정보 조회 실패: {e.Message}");
        }
#endif
        
        return (1, 0, false);
    }
    
    /// <summary>
    /// 친구 목록 가져오기
    /// </summary>
    public async Task<FriendListResponse> GetFriendsAsync(string userId)
    {
        var response = new FriendListResponse();
        
        if (string.IsNullOrEmpty(userId))
        {
            response.errorMessage = "User ID is empty";
            return response;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            response.errorMessage = "Firebase Database not initialized";
            return response;
        }
        
        try
        {
            var snapshot = await databaseReference.Child("friends").Child(userId).GetValueAsync();
            
            if (snapshot.Exists)
            {
                foreach (var child in snapshot.Children)
                {
                    string friendJson = child.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(friendJson))
                    {
                        var friendData = JsonUtility.FromJson<FriendData>(friendJson);
                        response.friends.Add(friendData);
                    }
                }
            }
            
            response.success = true;
            Debug.Log($"[FirebaseDatabase] 친구 목록 로드 완료: {response.friends.Count}명");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 친구 목록 로드 실패: {e.Message}");
            response.errorMessage = e.Message;
        }
#else
        // 테스트용 더미 데이터
        await Task.Delay(500);
        
        // 테스트 친구 데이터 생성
        response.friends.Add(new FriendData("test_friend_1", "TestFriend1", true, 5));
        response.friends.Add(new FriendData("test_friend_2", "TestFriend2", false, 3));
        response.success = true;
        
        Debug.Log($"[FirebaseDatabase] 테스트 친구 목록 로드: {response.friends.Count}명");
#endif
        
        return response;
    }
    
    /// <summary>
    /// 친구 요청 목록 가져오기 (받은 요청과 보낸 요청 모두)
    /// </summary>
    public async Task<FriendRequestListResponse> GetFriendRequestsAsync(string userId)
    {
        var response = new FriendRequestListResponse();
        
        if (string.IsNullOrEmpty(userId))
        {
            response.errorMessage = "User ID is empty";
            return response;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            response.errorMessage = "Firebase Database not initialized";
            return response;
        }
        
        try
        {
            // 받은 친구 요청 가져오기
            var receivedSnapshot = await databaseReference.Child("friendRequests").Child(userId).GetValueAsync();
            
            if (receivedSnapshot.Exists)
            {
                foreach (var child in receivedSnapshot.Children)
                {
                    string requestJson = child.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(requestJson))
                    {
                        var requestData = JsonUtility.FromJson<FriendRequestData>(requestJson);
                        if (requestData.status == FriendRequestStatus.Pending)
                        {
                            requestData.requestType = FriendRequestType.Received;
                            
                            // 요청 보낸 사람의 추가 정보 조회
                            var (fromLevel, fromLastLogin, fromIsOnline) = await GetUserBasicInfoAsync(requestData.fromUserId);
                            requestData.fromUserLevel = fromLevel;
                            requestData.fromUserLastLogin = fromLastLogin;
                            requestData.fromUserIsOnline = fromIsOnline;
                            
                            response.requests.Add(requestData);
                        }
                    }
                }
            }
            
            // 보낸 친구 요청 가져오기 (모든 사용자의 요청 중에서 내가 보낸 것 찾기)
            var allRequestsSnapshot = await databaseReference.Child("friendRequests").GetValueAsync();
            
            if (allRequestsSnapshot.Exists)
            {
                foreach (var userChild in allRequestsSnapshot.Children)
                {
                    foreach (var requestChild in userChild.Children)
                    {
                        string requestJson = requestChild.GetRawJsonValue();
                        if (!string.IsNullOrEmpty(requestJson))
                        {
                            var requestData = JsonUtility.FromJson<FriendRequestData>(requestJson);
                            if (requestData.fromUserId == userId && requestData.status == FriendRequestStatus.Pending)
                            {
                                requestData.requestType = FriendRequestType.Sent;
                                
                                // 요청 받는 사람의 추가 정보 조회
                                var (toLevel, toLastLogin, toIsOnline) = await GetUserBasicInfoAsync(requestData.toUserId);
                                requestData.toUserLevel = toLevel;
                                requestData.toUserLastLogin = toLastLogin;
                                requestData.toUserIsOnline = toIsOnline;
                                
                                response.requests.Add(requestData);
                            }
                        }
                    }
                }
            }
            
            response.success = true;
            Debug.Log($"[FirebaseDatabase] 친구 요청 목록 로드 완료: {response.requests.Count}개");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 친구 요청 목록 로드 실패: {e.Message}");
            response.errorMessage = e.Message;
        }
#else
        // 테스트용 더미 데이터
        await Task.Delay(500);
        
        // 테스트 받은 친구 요청 데이터 생성
        var receivedRequest = new FriendRequestData("test_user_1", "TestUser1", userId);
        receivedRequest.requestType = FriendRequestType.Received;
        receivedRequest.fromUserLevel = UnityEngine.Random.Range(1, 20);
        receivedRequest.fromUserIsOnline = UnityEngine.Random.value > 0.5f;
        receivedRequest.fromUserLastLogin = DateTimeOffset.UtcNow.AddHours(-UnityEngine.Random.Range(0, 48)).ToUnixTimeMilliseconds();
        response.requests.Add(receivedRequest);
        
        // 테스트 보낸 친구 요청 데이터 생성
        var sentRequest = new FriendRequestData(userId, "MyNickname", "test_user_2", "TestUser2");
        sentRequest.requestType = FriendRequestType.Sent;
        sentRequest.toUserLevel = UnityEngine.Random.Range(1, 20);
        sentRequest.toUserIsOnline = UnityEngine.Random.value > 0.5f;
        sentRequest.toUserLastLogin = DateTimeOffset.UtcNow.AddHours(-UnityEngine.Random.Range(0, 48)).ToUnixTimeMilliseconds();
        response.requests.Add(sentRequest);
        
        response.success = true;
        
        Debug.Log($"[FirebaseDatabase] 테스트 친구 요청 목록 로드: {response.requests.Count}개");
#endif
        
        return response;
    }    
    /// <summary>
    /// 닉네임으로 사용자 검색
    /// </summary>
    public async Task<UserSearchResult> SearchUserByNicknameAsync(string nickname)
    {
        if (string.IsNullOrEmpty(nickname))
        {
            return null;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            return null;
        }
        
        try
        {
            // 닉네임으로 사용자 검색 (대소문자 구분 없이)
            var snapshot = await databaseReference.Child("players")
                .OrderByChild("nickname")
                .EqualTo(nickname)
                .GetValueAsync();
            
            if (snapshot.Exists)
            {
                foreach (var child in snapshot.Children)
                {
                    string playerJson = child.GetRawJsonValue();
                    if (!string.IsNullOrEmpty(playerJson))
                    {
                        var playerData = JsonUtility.FromJson<PlayerData>(playerJson);
                        
                        var result = new UserSearchResult
                        {
                            userId = child.Key,
                            nickname = playerData.nickname,
                            level = playerData.level,
                            isOnline = false // 온라인 상태는 별도 구현 필요
                        };
                        
                        Debug.Log($"[FirebaseDatabase] 사용자 검색 완료: {result.nickname}");
                        return result;
                    }
                }
            }
            
            Debug.Log($"[FirebaseDatabase] 사용자를 찾을 수 없음: {nickname}");
            return null;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 사용자 검색 실패: {e.Message}");
            return null;
        }
#else
        // 테스트용 더미 데이터
        await Task.Delay(500);
        
        // 테스트 사용자 검색 결과
        if (nickname.ToLower().Contains("test"))
        {
            return new UserSearchResult
            {
                userId = "test_search_user",
                nickname = nickname,
                level = UnityEngine.Random.Range(1, 10),
                isOnline = UnityEngine.Random.value > 0.5f
            };
        }
        
        Debug.Log($"[FirebaseDatabase] 테스트 환경에서 사용자를 찾을 수 없음: {nickname}");
        return null;
#endif
    }
    
    /// <summary>
    /// 친구 요청 보내기
    /// </summary>
    public async Task<bool> SendFriendRequestAsync(FriendRequestData requestData)
    {
        if (requestData == null || string.IsNullOrEmpty(requestData.toUserId))
        {
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            return false;
        }
        
        try
        {
            string requestJson = JsonUtility.ToJson(requestData);
            
            // 받는 사람의 친구 요청 목록에 추가
            await databaseReference.Child("friendRequests")
                .Child(requestData.toUserId)
                .Child(requestData.requestId)
                .SetRawJsonValueAsync(requestJson);
            
            Debug.Log($"[FirebaseDatabase] 친구 요청 전송 완료: {requestData.fromNickname} -> {requestData.toUserId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 친구 요청 전송 실패: {e.Message}");
            return false;
        }
#else
        // 테스트용
        await Task.Delay(500);
        Debug.Log($"[FirebaseDatabase] 테스트 친구 요청 전송: {requestData.fromNickname} -> {requestData.toUserId}");
        return true;
#endif
    }
    
    /// <summary>
    /// 친구 요청 취소
    /// </summary>
    public async Task<bool> CancelFriendRequestAsync(string requestId, string toUserId)
    {
        if (string.IsNullOrEmpty(requestId) || string.IsNullOrEmpty(toUserId))
        {
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            return false;
        }
        
        try
        {
            // 받는 사람의 친구 요청 목록에서 삭제
            await databaseReference.Child("friendRequests")
                .Child(toUserId)
                .Child(requestId)
                .RemoveValueAsync();
            
            Debug.Log($"[FirebaseDatabase] 친구 요청 취소 완료: {requestId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 친구 요청 취소 실패: {e.Message}");
            return false;
        }
#else
        // 테스트용
        await Task.Delay(500);
        Debug.Log($"[FirebaseDatabase] 테스트 친구 요청 취소: {requestId}");
        return true;
#endif
    }    
    /// <summary>
    /// 친구 요청에 응답 (수락/거절)
    /// </summary>
    public async Task<bool> RespondToFriendRequestAsync(string requestId, bool accept)
    {
        if (string.IsNullOrEmpty(requestId))
        {
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            return false;
        }
        
        try
        {
            string userId = FirebaseManager.Instance.UserId;
            
            // 요청 데이터 가져오기
            var requestSnapshot = await databaseReference.Child("friendRequests")
                .Child(userId)
                .Child(requestId)
                .GetValueAsync();
            
            if (!requestSnapshot.Exists)
            {
                Debug.LogWarning($"[FirebaseDatabase] 친구 요청을 찾을 수 없음: {requestId}");
                return false;
            }
            
            string requestJson = requestSnapshot.GetRawJsonValue();
            var requestData = JsonUtility.FromJson<FriendRequestData>(requestJson);
            
            if (accept)
            {
                // 수락한 경우 양쪽 친구 목록에 추가
                var myData = new FriendData(requestData.fromUserId, requestData.fromNickname);
                
                // 현재 사용자의 닉네임을 안전하게 가져오기
                string myNickname = "Unknown";
                if (PlayerDataManager.Instance != null && 
                    PlayerDataManager.Instance.IsDataLoaded && 
                    PlayerDataManager.Instance.CurrentPlayerData != null &&
                    !string.IsNullOrEmpty(PlayerDataManager.Instance.CurrentPlayerData.nickname))
                {
                    myNickname = PlayerDataManager.Instance.CurrentPlayerData.nickname;
                }
                
                var friendData = new FriendData(userId, myNickname);
                
                string myDataJson = JsonUtility.ToJson(myData);
                string friendDataJson = JsonUtility.ToJson(friendData);
                
                // 내 친구 목록에 추가
                await databaseReference.Child("friends")
                    .Child(userId)
                    .Child(requestData.fromUserId)
                    .SetRawJsonValueAsync(myDataJson);
                
                // 상대방 친구 목록에 추가
                await databaseReference.Child("friends")
                    .Child(requestData.fromUserId)
                    .Child(userId)
                    .SetRawJsonValueAsync(friendDataJson);
                
                Debug.Log($"[FirebaseDatabase] 친구 요청 수락 완료: {requestData.fromNickname}");
            }
            else
            {
                Debug.Log($"[FirebaseDatabase] 친구 요청 거절: {requestData.fromNickname}");
            }
            
            // 요청 삭제
            await databaseReference.Child("friendRequests")
                .Child(userId)
                .Child(requestId)
                .RemoveValueAsync();
            
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 친구 요청 응답 실패: {e.Message}");
            return false;
        }
#else
        // 테스트용
        await Task.Delay(500);
        string action = accept ? "수락" : "거절";
        Debug.Log($"[FirebaseDatabase] 테스트 친구 요청 {action}: {requestId}");
        return true;
#endif
    }
    
    /// <summary>
    /// 친구 삭제
    /// </summary>
    public async Task<bool> RemoveFriendAsync(string userId, string friendUserId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(friendUserId))
        {
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            return false;
        }
        
        try
        {
            // 양쪽 친구 목록에서 삭제
            await databaseReference.Child("friends")
                .Child(userId)
                .Child(friendUserId)
                .RemoveValueAsync();
            
            await databaseReference.Child("friends")
                .Child(friendUserId)
                .Child(userId)
                .RemoveValueAsync();
            
            Debug.Log($"[FirebaseDatabase] 친구 삭제 완료: {userId} <-> {friendUserId}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 친구 삭제 실패: {e.Message}");
            return false;
        }
#else
        // 테스트용
        await Task.Delay(500);
        Debug.Log($"[FirebaseDatabase] 테스트 친구 삭제: {userId} <-> {friendUserId}");
        return true;
#endif
    }
    
    #endregion
    
    #region 실시간 리스너
    
    // 실시간 리스너 관련 변수
#if FIREBASE_DATABASE
    private Firebase.Database.DatabaseReference friendRequestsListener;
#endif
    private System.Action onFriendRequestsUpdated;
    private string currentListeningUserId;
    
    /// <summary>
    /// 친구 요청 실시간 리스너 시작
    /// </summary>
    public void StartListeningToFriendRequests(string userId, System.Action onUpdated)
    {
        if (string.IsNullOrEmpty(userId) || onUpdated == null)
        {
            Debug.LogWarning("[FirebaseDatabase] 실시간 리스너 시작 실패: 잘못된 매개변수");
            return;
        }
        
        // 기존 리스너가 있다면 중지
        StopListeningToFriendRequests();
        
        currentListeningUserId = userId;
        onFriendRequestsUpdated = onUpdated;
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogWarning("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return;
        }
        
        try
        {
            // 해당 사용자의 친구 요청 경로에 리스너 등록
            friendRequestsListener = databaseReference.Child("friendRequests").Child(userId);
            friendRequestsListener.ValueChanged += OnFriendRequestsValueChanged;
            
            Debug.Log($"[FirebaseDatabase] 친구 요청 실시간 리스너 시작: {userId}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseDatabase] 실시간 리스너 시작 실패: {e.Message}");
        }
#else
        Debug.Log($"[FirebaseDatabase] 테스트 모드 - 실시간 리스너 시작: {userId}");
#endif
    }
    
    /// <summary>
    /// 친구 요청 실시간 리스너 중지
    /// </summary>
    public void StopListeningToFriendRequests()
    {
#if FIREBASE_DATABASE
        if (friendRequestsListener != null)
        {
            try
            {
                friendRequestsListener.ValueChanged -= OnFriendRequestsValueChanged;
                friendRequestsListener = null;
                Debug.Log("[FirebaseDatabase] 친구 요청 실시간 리스너 중지");
            }
            catch (Exception e)
            {
                Debug.LogError($"[FirebaseDatabase] 실시간 리스너 중지 실패: {e.Message}");
            }
        }
#else
        Debug.Log("[FirebaseDatabase] 테스트 모드 - 실시간 리스너 중지");
#endif
        
        currentListeningUserId = null;
        onFriendRequestsUpdated = null;
    }
    
#if FIREBASE_DATABASE
    /// <summary>
    /// Firebase 친구 요청 데이터 변경 이벤트 처리
    /// </summary>
    private void OnFriendRequestsValueChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"[FirebaseDatabase] 실시간 리스너 오류: {args.DatabaseError.Message}");
            return;
        }
        
        Debug.Log("[FirebaseDatabase] 친구 요청 데이터 변경 감지");
        
        // 메인 스레드에서 콜백 실행
        if (UnityMainThreadDispatcher.Instance != null)
        {
            UnityMainThreadDispatcher.Instance.Enqueue(() =>
            {
                onFriendRequestsUpdated?.Invoke();
            });
        }
        else
        {
            // UnityMainThreadDispatcher가 없는 경우 직접 호출 (위험하지만 대안)
            onFriendRequestsUpdated?.Invoke();
        }
    }
#endif
    
    #endregion
    
    #region 게임 설정 관리
    
    /// <summary>
    /// Firebase에서 게임 설정 로드
    /// </summary>
    public async Task<Dictionary<string, object>> LoadGameSettingsAsync()
    {
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return null;
        }
        
        try
        {
            // Realtime Database에서 게임 설정 가져오기
            Firebase.Database.DataSnapshot snapshot = 
                await databaseReference.Child("gameSettings").GetValueAsync();
                
            if (snapshot.Exists)
            {
                var settings = new Dictionary<string, object>();
                
                foreach (var child in snapshot.Children)
                {
                    string key = child.Key;
                    object value = child.Value;
                    
                    // 타입에 따라 적절히 변환
                    if (value is long longValue)
                    {
                        settings[key] = (int)longValue;
                    }
                    else if (value is double doubleValue)
                    {
                        settings[key] = (float)doubleValue;
                    }
                    else
                    {
                        settings[key] = value;
                    }
                }
                
                Debug.Log($"[FirebaseDatabase] 게임 설정 로드 완료: {settings.Count}개 항목");
                return settings;
            }
            else
            {
                Debug.Log("[FirebaseDatabase] 게임 설정이 없습니다. 기본값을 사용합니다.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 게임 설정 로드 중 오류 발생: {ex.Message}");
            return null;
        }
#else
        // 테스트용 더미 설정
        await Task.Delay(200);
        
        var testSettings = new Dictionary<string, object>
        {
            { "heartRecoveryIntervalMinutes", 30 },
            { "defaultMaxHearts", 5 },
            { "absoluteMaxHearts", 99 },
            { "storyModeHeartCost", 1 },
            { "competitiveModeHeartCost", 1 }
        };
        
        Debug.Log("[FirebaseDatabase] 테스트 게임 설정 로드 완료");
        return testSettings;
#endif
    }
    
    /// <summary>
    /// Firebase에 게임 설정 저장 (관리자용)
    /// </summary>
    public async Task<bool> SaveGameSettingsAsync(Dictionary<string, object> settings)
    {
        if (settings == null || settings.Count == 0)
        {
            Debug.LogError("[FirebaseDatabase] 저장할 게임 설정이 없습니다.");
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
        
        try
        {
            // 각 설정값을 개별적으로 저장
            foreach (var setting in settings)
            {
                await databaseReference.Child("gameSettings")
                    .Child(setting.Key)
                    .SetValueAsync(setting.Value);
            }
            
            Debug.Log($"[FirebaseDatabase] 게임 설정 저장 완료: {settings.Count}개 항목");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 게임 설정 저장 중 오류 발생: {ex.Message}");
            return false;
        }
#else
        // 테스트용
        await Task.Delay(200);
        Debug.Log($"[FirebaseDatabase] 테스트 게임 설정 저장: {settings.Count}개 항목");
        return true;
#endif
    }
    
    #endregion
}