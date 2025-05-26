using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using System.Linq;

/// <summary>
/// 친구 시스템을 관리하는 Firebase 매니저
/// </summary>
public class FriendSystemManager : MonoBehaviour
{
    #region 싱글톤
    public static FriendSystemManager Instance { get; private set; }

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

    // 이벤트들
    public event Action<FriendInfo> OnFriendAdded;
    public event Action<string> OnFriendRemoved;
    public event Action<FriendRequest> OnFriendRequestReceived;
    public event Action<FriendRequest> OnFriendRequestSent;
    public event Action<string> OnFriendRequestAccepted;
    public event Action<string> OnFriendRequestRejected;
    public event Action<List<FriendInfo>> OnFriendsListUpdated;
    public event Action<List<UserPublicProfile>> OnUserSearchCompleted;

    // 현재 사용자의 친구 데이터
    private UserFriendData currentUserFriendData;
    private string currentUserId;

    // Firebase 참조들
#if UNITY_EDITOR || UNITY_ANDROID || UNITY_IOS
    #if FIREBASE_DATABASE
    private Firebase.Database.DatabaseReference friendsRef;
    private Firebase.Database.DatabaseReference requestsRef;
    private Firebase.Database.DatabaseReference profilesRef;
    #endif
#endif

    /// <summary>
    /// 친구 시스템 초기화
    /// </summary>
    public async Task<bool> InitializeFriendSystemAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[FriendSystemManager] 유저 ID가 없습니다.");
            return false;
        }

        currentUserId = userId;
        currentUserFriendData = new UserFriendData();

#if FIREBASE_DATABASE
        try
        {
            var database = Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/");
            friendsRef = database.RootReference.Child("friends");
            requestsRef = database.RootReference.Child("friend_requests");
            profilesRef = database.RootReference.Child("public_profiles");

            // 실시간 리스너를 먼저 설정 (기존 데이터도 감지하기 위해)
            SetupRealtimeListeners();

            // 사용자의 친구 데이터 로드
            await LoadUserFriendDataAsync();

            Debug.Log("[FriendSystemManager] 친구 시스템 초기화 완료");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 초기화 실패: {ex.Message}");
            return false;
        }
#else
        // 테스트용 초기화
        Debug.LogWarning("[FriendSystemManager] Firebase SDK 없음. 테스트 모드로 초기화");
        await Task.Delay(500);
        return true;
#endif
    }

    /// <summary>
    /// 사용자의 친구 데이터 로드
    /// </summary>
    private async Task LoadUserFriendDataAsync()
    {
#if FIREBASE_DATABASE
        try
        {
            Debug.Log($"[FriendSystemManager] 유저 {currentUserId}의 데이터 로드 시작");

            // 1. 친구 목록 로드
            var friendsSnapshot = await friendsRef.Child(currentUserId).GetValueAsync();
            if (friendsSnapshot.Exists)
            {
                string json = friendsSnapshot.GetRawJsonValue();
                var tempData = JsonUtility.FromJson<UserFriendData>(json);
                if (tempData?.Friends != null)
                {
                    currentUserFriendData.SetFriendsList(tempData.Friends);
                    Debug.Log($"[FriendSystemManager] 친구 목록 로드 완료: {currentUserFriendData.Friends.Count}개");
                }
            }

            // 2. 보낸 친구 요청 로드
            var sentRequestsSnapshot = await requestsRef.Child(currentUserId).Child("sent").GetValueAsync();
            if (sentRequestsSnapshot.Exists)
            {
                var sentRequestsList = new List<FriendRequest>();
                foreach (var child in sentRequestsSnapshot.Children)
                {
                    string requestJson = child.GetRawJsonValue();
                    var request = SafeParseJsonToFriendRequest(requestJson);
                    if (request != null)
                    {
                        sentRequestsList.Add(request);
                    }
                }
                currentUserFriendData.SetSentRequestsList(sentRequestsList);
                Debug.Log($"[FriendSystemManager] 보낸 친구 요청 로드 완료: {currentUserFriendData.SentRequests.Count}개");
            }

            // 3. 받은 친구 요청 로드
            var receivedRequestsSnapshot = await requestsRef.Child(currentUserId).Child("received").GetValueAsync();
            if (receivedRequestsSnapshot.Exists)
            {
                var receivedRequestsList = new List<FriendRequest>();
                foreach (var child in receivedRequestsSnapshot.Children)
                {
                    string requestJson = child.GetRawJsonValue();
                    var request = SafeParseJsonToFriendRequest(requestJson);
                    if (request != null)
                    {
                        receivedRequestsList.Add(request);
                    }
                }
                currentUserFriendData.SetReceivedRequestsList(receivedRequestsList);
                Debug.Log($"[FriendSystemManager] 받은 친구 요청 로드 완료: {currentUserFriendData.ReceivedRequests.Count}개");
            }

            Debug.Log($"[FriendSystemManager] 친구 데이터 로드 완료. 친구 수: {currentUserFriendData.Friends.Count}, 보낸 요청: {currentUserFriendData.SentRequests.Count}, 받은 요청: {currentUserFriendData.ReceivedRequests.Count}");
            
            // 로드된 데이터 상세 로그
            if (currentUserFriendData.SentRequests.Count > 0)
            {
                Debug.Log("[FriendSystemManager] 로드된 보낸 요청들:");
                foreach (var request in currentUserFriendData.SentRequests)
                {
                    Debug.Log($"  - {request.requestId}: To {request.toUserId}, Status: {request.status}");
                }
            }
            
            if (currentUserFriendData.ReceivedRequests.Count > 0)
            {
                Debug.Log("[FriendSystemManager] 로드된 받은 요청들:");
                foreach (var request in currentUserFriendData.ReceivedRequests)
                {
                    Debug.Log($"  - {request.requestId}: From {request.fromDisplayName} ({request.fromUserId}), Status: {request.status}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 친구 데이터 로드 실패: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// 실시간 리스너 설정
    /// </summary>
    private void SetupRealtimeListeners()
    {
#if FIREBASE_DATABASE
        Debug.Log("[FriendSystemManager] 실시간 리스너 설정 중...");
        
        // 받은 친구 요청 리스너 (기존 데이터와 새 데이터 모두 감지)
        requestsRef.Child(currentUserId).Child("received").ValueChanged += OnReceivedRequestsValueChanged;
        
        // 보낸 친구 요청 리스너 (상태 변경 감지)
        requestsRef.Child(currentUserId).Child("sent").ValueChanged += OnSentRequestsValueChanged;
        
        // 친구 목록 변경 리스너
        friendsRef.Child(currentUserId).ValueChanged += OnFriendsValueChanged;
        
        Debug.Log("[FriendSystemManager] 실시간 리스너 설정 완료");
#endif
    }

#if FIREBASE_DATABASE
    private void OnReceivedRequestsValueChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"[FriendSystemManager] 받은 요청 리스너 오류: {args.DatabaseError.Message}");
            return;
        }

        try
        {
            Debug.Log("[FriendSystemManager] 받은 친구 요청 데이터 변경 감지");
            
            if (!args.Snapshot.Exists)
            {
                // 받은 요청이 없는 경우
                currentUserFriendData.ClearReceivedRequests();
                return;
            }

            var receivedRequestsList = new List<FriendRequest>();
            
            foreach (var child in args.Snapshot.Children)
            {
                string json = child.GetRawJsonValue();
                FriendRequest request = SafeParseJsonToFriendRequest(json);
                
                if (request != null)
                {
                    receivedRequestsList.Add(request);
                    
                    // 새로운 대기 중인 요청인 경우 이벤트 발생
                    if (request.status == FriendRequestStatus.Pending)
                    {
                        Debug.Log($"[FriendSystemManager] 새 친구 요청 감지: {request.fromDisplayName}");
                        OnFriendRequestReceived?.Invoke(request);
                    }
                }
            }
            
            currentUserFriendData.SetReceivedRequestsList(receivedRequestsList);
            Debug.Log($"[FriendSystemManager] 받은 요청 목록 업데이트 완료: {currentUserFriendData.ReceivedRequests.Count}개");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 받은 요청 처리 오류: {ex.Message}");
        }
    }

    private void OnSentRequestsValueChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"[FriendSystemManager] 보낸 요청 리스너 오류: {args.DatabaseError.Message}");
            return;
        }

        try
        {
            Debug.Log("[FriendSystemManager] 보낸 친구 요청 데이터 변경 감지");
            
            if (!args.Snapshot.Exists)
            {
                currentUserFriendData.ClearSentRequests();
                return;
            }

            var sentRequestsList = new List<FriendRequest>();
            
            foreach (var child in args.Snapshot.Children)
            {
                string json = child.GetRawJsonValue();
                FriendRequest request = SafeParseJsonToFriendRequest(json);
                
                if (request != null)
                {
                    sentRequestsList.Add(request);
                    
                    // 요청 상태가 변경된 경우 이벤트 발생
                    if (request.status == FriendRequestStatus.Accepted)
                    {
                        OnFriendRequestAccepted?.Invoke(request.requestId);
                    }
                    else if (request.status == FriendRequestStatus.Rejected)
                    {
                        OnFriendRequestRejected?.Invoke(request.requestId);
                    }
                }
            }
            
            currentUserFriendData.SetSentRequestsList(sentRequestsList);
            Debug.Log($"[FriendSystemManager] 보낸 요청 목록 업데이트 완료: {currentUserFriendData.SentRequests.Count}개");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 보낸 요청 처리 오류: {ex.Message}");
        }
    }

    private void OnFriendsValueChanged(object sender, Firebase.Database.ValueChangedEventArgs args)
    {
        if (args.DatabaseError != null)
        {
            Debug.LogError($"[FriendSystemManager] 친구 목록 리스너 오류: {args.DatabaseError.Message}");
            return;
        }

        try
        {
            Debug.Log("[FriendSystemManager] 친구 목록 데이터 변경 감지");
            
            if (!args.Snapshot.Exists)
            {
                currentUserFriendData.ClearFriends();
                return;
            }

            // 친구 목록 업데이트 로직은 기존과 동일하게 유지
            string json = args.Snapshot.GetRawJsonValue();
            var tempData = SafeParseJsonToUserFriendData(json);
            if (tempData?.Friends != null)
            {
                currentUserFriendData.SetFriendsList(tempData.Friends);
                OnFriendsListUpdated?.Invoke(currentUserFriendData.Friends);
                Debug.Log($"[FriendSystemManager] 친구 목록 업데이트 완료: {currentUserFriendData.Friends.Count}개");
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 친구 목록 처리 오류: {ex.Message}");
        }
    }

    // 기존 리스너 메서드들은 제거하거나 주석 처리
    /*
    private void OnFriendRequestReceivedListener(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        // 이 메서드는 더 이상 사용하지 않음 - OnReceivedRequestsValueChanged로 대체
    }

    private void OnFriendRequestStatusChangedListener(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        // 이 메서드는 더 이상 사용하지 않음 - OnReceivedRequestsValueChanged로 대체
    }

    private void OnSentRequestStatusChangedListener(object sender, Firebase.Database.ChildChangedEventArgs args)
    {
        // 이 메서드는 더 이상 사용하지 않음 - OnSentRequestsValueChanged로 대체
    }
    */
#endif

    /// <summary>
    /// 사용자 검색
    /// </summary>
    public async Task<List<UserPublicProfile>> SearchUsersAsync(string searchTerm, int maxResults = 20)
    {
        var results = new List<UserPublicProfile>();

        if (string.IsNullOrEmpty(searchTerm))
        {
            Debug.LogWarning("[FriendSystemManager] 검색어가 비어있습니다.");
            OnUserSearchCompleted?.Invoke(results);
            return results;
        }

        // FriendSystemManager 초기화 상태 확인
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogError("[FriendSystemManager] 사용자 ID가 설정되지 않았습니다. InitializeFriendSystemAsync를 먼저 호출해주세요.");
            OnUserSearchCompleted?.Invoke(results);
            return results;
        }

        if (currentUserFriendData == null)
        {
            Debug.LogError("[FriendSystemManager] 친구 데이터가 초기화되지 않았습니다. InitializeFriendSystemAsync를 먼저 호출해주세요.");
            OnUserSearchCompleted?.Invoke(results);
            return results;
        }

#if FIREBASE_DATABASE
        // Firebase 참조 null 체크
        if (profilesRef == null)
        {
            Debug.LogError("[FriendSystemManager] Firebase Database 참조가 초기화되지 않았습니다. InitializeFriendSystemAsync를 먼저 호출해주세요.");
            OnUserSearchCompleted?.Invoke(results);
            return results;
        }

        try
        {
            Debug.Log($"[FriendSystemManager] 사용자 검색 시작: '{searchTerm}'");
            
            // 디버그: 전체 public_profiles 확인
            var allProfilesSnapshot = await profilesRef.GetValueAsync();
            Debug.Log($"[FriendSystemManager] 전체 public_profiles 개수: {allProfilesSnapshot.ChildrenCount}");
            
            if (allProfilesSnapshot.ChildrenCount > 0)
            {
                Debug.Log("[FriendSystemManager] 저장된 프로필들:");
                foreach (var child in allProfilesSnapshot.Children)
                {
                    try
                    {
                        string json = child.GetRawJsonValue();
                        UserPublicProfile profile = JsonUtility.FromJson<UserPublicProfile>(json);
                        Debug.Log($"  - {profile.displayName} (ID: {profile.userId})");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"  - 프로필 파싱 오류: {ex.Message}");
                    }
                }
            }
            
            // 표시 이름으로 검색 (Firebase 쿼리)
            var query = profilesRef.OrderByChild("displayName")
                                  .StartAt(searchTerm)
                                  .EndAt(searchTerm + "\uf8ff")
                                  .LimitToFirst(maxResults);

            var snapshot = await query.GetValueAsync();
            
            if (snapshot == null)
            {
                Debug.LogWarning("[FriendSystemManager] Firebase 검색 결과가 null입니다.");
                OnUserSearchCompleted?.Invoke(results);
                return results;
            }

            Debug.Log($"[FriendSystemManager] Firebase 검색 완료. 쿼리 결과 수: {snapshot.ChildrenCount}");
            
            // Firebase 쿼리 결과 처리
            foreach (var child in snapshot.Children)
            {
                try
                {
                    if (child == null) continue;
                    
                    string json = child.GetRawJsonValue();
                    if (string.IsNullOrEmpty(json)) continue;
                    
                    UserPublicProfile profile = JsonUtility.FromJson<UserPublicProfile>(json);
                    if (profile == null) continue;
                    
                    // 자신은 제외하고, 검색 가능한 사용자만 포함
                    if (profile.userId != currentUserId && profile.isSearchable)
                    {
                        results.Add(profile);
                        Debug.Log($"[FriendSystemManager] 검색 결과 추가: {profile.displayName} (ID: {profile.userId})");
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[FriendSystemManager] 검색 결과 파싱 오류: {ex.Message}");
                }
            }
            
            // Firebase 쿼리로 결과가 없으면 전체 데이터에서 부분 매칭 검색
            if (results.Count == 0)
            {
                Debug.Log("[FriendSystemManager] Firebase 쿼리 결과가 없어서 전체 데이터에서 부분 매칭 검색을 시도합니다.");
                
                foreach (var child in allProfilesSnapshot.Children)
                {
                    try
                    {
                        if (child == null) continue;
                        
                        string json = child.GetRawJsonValue();
                        if (string.IsNullOrEmpty(json)) continue;
                        
                        UserPublicProfile profile = JsonUtility.FromJson<UserPublicProfile>(json);
                        if (profile == null) continue;
                        
                        // 자신은 제외하고, 검색 가능한 사용자만 포함
                        if (profile.userId != currentUserId && profile.isSearchable)
                        {
                            // 대소문자 무시하고 부분 매칭 확인
                            if (profile.displayName.ToLower().Contains(searchTerm.ToLower()))
                            {
                                results.Add(profile);
                                Debug.Log($"[FriendSystemManager] 부분 매칭 결과 추가: {profile.displayName} (ID: {profile.userId})");
                                
                                if (results.Count >= maxResults) break;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[FriendSystemManager] 부분 매칭 검색 파싱 오류: {ex.Message}");
                    }
                }
            }

            Debug.Log($"[FriendSystemManager] 검색 완료. 최종 결과: {results.Count}개");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 사용자 검색 실패: {ex.Message}");
            Debug.LogError($"[FriendSystemManager] 스택 트레이스: {ex.StackTrace}");
        }
#else
        // 테스트용 더미 검색
        Debug.Log("[FriendSystemManager] Firebase 없음. 테스트 데이터로 검색 중...");
        await Task.Delay(1000);
        
        if (searchTerm.ToLower().Contains("test") || searchTerm.ToLower().Contains("테스트"))
        {
            results.Add(new UserPublicProfile 
            { 
                userId = "test_user_1", 
                displayName = "테스트유저1", 
                bestScore = 1000,
                isSearchable = true
            });
            results.Add(new UserPublicProfile 
            { 
                userId = "test_user_2", 
                displayName = "테스트유저2", 
                bestScore = 1500,
                isSearchable = true
            });
        }
        
        Debug.Log($"[FriendSystemManager] 테스트 검색 완료. 결과: {results.Count}개");
#endif

        OnUserSearchCompleted?.Invoke(results);
        return results;
    }

    /// <summary>
    /// 친구 요청 보내기
    /// </summary>
    public async Task<bool> SendFriendRequestAsync(string targetUserId, string targetDisplayName)
    {
        if (string.IsNullOrEmpty(targetUserId) || targetUserId == currentUserId)
        {
            Debug.LogError("[FriendSystemManager] 잘못된 대상 사용자 ID");
            return false;
        }

        // 이미 친구인지 확인
        if (currentUserFriendData.IsFriend(targetUserId))
        {
            Debug.LogWarning("[FriendSystemManager] 이미 친구입니다.");
            return false;
        }

        // 이미 요청을 보냈는지 확인
        if (currentUserFriendData.HasSentRequestTo(targetUserId))
        {
            Debug.LogWarning("[FriendSystemManager] 이미 요청을 보냈습니다.");
            return false;
        }

        try
        {
            // 현재 사용자의 정보 가져오기 (표시 이름)
            string myDisplayName = await GetMyDisplayNameAsync();
            
            var request = new FriendRequest(currentUserId, myDisplayName, targetUserId);
            
            Debug.Log($"[FriendSystemManager] 친구 요청 생성: {request.requestId}, From: {myDisplayName} ({currentUserId}), To: {targetDisplayName} ({targetUserId})");

#if FIREBASE_DATABASE
            // Firebase에 요청 저장
            var updates = new Dictionary<string, object>
            {
                [$"friend_requests/{currentUserId}/sent/{request.requestId}"] = JsonUtility.ToJson(request),
                [$"friend_requests/{targetUserId}/received/{request.requestId}"] = JsonUtility.ToJson(request)
            };

            Debug.Log($"[FriendSystemManager] Firebase 업데이트 시작...");
            await Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/").RootReference.UpdateChildrenAsync(updates);
            Debug.Log($"[FriendSystemManager] Firebase 업데이트 완료");
#endif

            // 로컬 데이터 업데이트
            currentUserFriendData.AddSentRequest(request);
            
            // 이벤트 발생
            OnFriendRequestSent?.Invoke(request);

            Debug.Log($"[FriendSystemManager] 친구 요청 전송 완료: {targetDisplayName}");
            Debug.Log($"[FriendSystemManager] 현재 보낸 요청 수: {currentUserFriendData.SentRequests.Count}");
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 친구 요청 전송 실패: {ex.Message}\nStackTrace: {ex.StackTrace}");
            return false;
        }
    }

    /// <summary>
    /// 친구 요청 수락
    /// </summary>
    public async Task<bool> AcceptFriendRequestAsync(string requestId)
    {
        if (!currentUserFriendData.ReceivedRequestsDict.ContainsKey(requestId))
        {
            Debug.LogError("[FriendSystemManager] 요청을 찾을 수 없습니다.");
            return false;
        }

        var request = currentUserFriendData.ReceivedRequestsDict[requestId];
        
        try
        {
            // 요청 상태를 수락으로 변경
            request.status = FriendRequestStatus.Accepted;

            // 상대방 정보 가져오기
            var friendProfile = await GetUserPublicProfileAsync(request.fromUserId);
            if (friendProfile == null)
            {
                Debug.LogError("[FriendSystemManager] 친구 프로필을 찾을 수 없습니다.");
                return false;
            }

            var newFriend = new FriendInfo(request.fromUserId, request.fromDisplayName, friendProfile.bestScore);
            
            // 나의 정보도 상대방에게 친구로 추가
            var myProfile = await GetMyPublicProfileAsync();
            var myFriendInfo = new FriendInfo(currentUserId, myProfile.displayName, myProfile.bestScore);

#if FIREBASE_DATABASE
            var updates = new Dictionary<string, object>
            {
                // 요청 상태 업데이트
                [$"friend_requests/{currentUserId}/received/{requestId}"] = JsonUtility.ToJson(request),
                [$"friend_requests/{request.fromUserId}/sent/{requestId}"] = JsonUtility.ToJson(request),
                
                // 양쪽에 친구 추가
                [$"friends/{currentUserId}/friendsList/{request.fromUserId}"] = JsonUtility.ToJson(newFriend),
                [$"friends/{request.fromUserId}/friendsList/{currentUserId}"] = JsonUtility.ToJson(myFriendInfo)
            };

            await Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/").RootReference.UpdateChildrenAsync(updates);
#endif

            // 로컬 데이터 업데이트
            currentUserFriendData.AddFriend(newFriend);
            currentUserFriendData.ReceivedRequestsDict[requestId] = request;

            OnFriendAdded?.Invoke(newFriend);
            OnFriendRequestAccepted?.Invoke(requestId);

            Debug.Log($"[FriendSystemManager] 친구 요청 수락 완료: {request.fromDisplayName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 친구 요청 수락 실패: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 친구 요청 거절
    /// </summary>
    public async Task<bool> RejectFriendRequestAsync(string requestId)
    {
        if (!currentUserFriendData.ReceivedRequestsDict.ContainsKey(requestId))
        {
            Debug.LogError("[FriendSystemManager] 요청을 찾을 수 없습니다.");
            return false;
        }

        var request = currentUserFriendData.ReceivedRequestsDict[requestId];
        
        try
        {
            request.status = FriendRequestStatus.Rejected;

#if FIREBASE_DATABASE
            var updates = new Dictionary<string, object>
            {
                [$"friend_requests/{currentUserId}/received/{requestId}"] = JsonUtility.ToJson(request),
                [$"friend_requests/{request.fromUserId}/sent/{requestId}"] = JsonUtility.ToJson(request)
            };

            await Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/").RootReference.UpdateChildrenAsync(updates);
#endif

            currentUserFriendData.ReceivedRequestsDict[requestId] = request;
            OnFriendRequestRejected?.Invoke(requestId);

            Debug.Log($"[FriendSystemManager] 친구 요청 거절 완료: {request.fromDisplayName}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 친구 요청 거절 실패: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 친구 삭제
    /// </summary>
    public async Task<bool> RemoveFriendAsync(string friendUserId)
    {
        if (!currentUserFriendData.IsFriend(friendUserId))
        {
            Debug.LogError("[FriendSystemManager] 친구가 아닙니다.");
            return false;
        }

        try
        {
#if FIREBASE_DATABASE
            var updates = new Dictionary<string, object>
            {
                [$"friends/{currentUserId}/friendsList/{friendUserId}"] = null,
                [$"friends/{friendUserId}/friendsList/{currentUserId}"] = null
            };

            await Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/").RootReference.UpdateChildrenAsync(updates);
#endif

            currentUserFriendData.RemoveFriend(friendUserId);
            OnFriendRemoved?.Invoke(friendUserId);

            Debug.Log($"[FriendSystemManager] 친구 삭제 완료: {friendUserId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 친구 삭제 실패: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 사용자의 공개 프로필 업데이트
    /// </summary>
    public async Task<bool> UpdateMyPublicProfileAsync(string displayName, int bestScore, string characterId, bool isSearchable = true)
    {
        // 초기화 상태 확인
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogWarning("[FriendSystemManager] 공개 프로필 업데이트 실패: currentUserId가 설정되지 않았습니다.");
            return false;
        }

#if FIREBASE_DATABASE
        if (profilesRef == null)
        {
            Debug.LogWarning("[FriendSystemManager] 공개 프로필 업데이트 실패: Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
#endif

        try
        {
            var profile = new UserPublicProfile
            {
                userId = currentUserId,
                displayName = displayName,
                bestScore = bestScore,
                characterId = characterId,
                lastLoginTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                isSearchable = isSearchable
            };

#if FIREBASE_DATABASE
            string json = JsonUtility.ToJson(profile);
            await profilesRef.Child(currentUserId).SetRawJsonValueAsync(json);
#endif

            Debug.Log($"[FriendSystemManager] 공개 프로필 업데이트 완료");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 공개 프로필 업데이트 실패: {ex.Message}");
            return false;
        }
    }

    // 헬퍼 메소드들
    private async Task<string> GetMyDisplayNameAsync()
    {
        var profile = await GetMyPublicProfileAsync();
        return profile?.displayName ?? "Unknown";
    }

    private async Task<UserPublicProfile> GetMyPublicProfileAsync()
    {
#if FIREBASE_DATABASE
        try
        {
            var snapshot = await profilesRef.Child(currentUserId).GetValueAsync();
            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                return JsonUtility.FromJson<UserPublicProfile>(json);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 내 프로필 가져오기 실패: {ex.Message}");
        }
#endif
        return null;
    }

    private async Task<UserPublicProfile> GetUserPublicProfileAsync(string userId)
    {
#if FIREBASE_DATABASE
        try
        {
            var snapshot = await profilesRef.Child(userId).GetValueAsync();
            if (snapshot.Exists)
            {
                string json = snapshot.GetRawJsonValue();
                return JsonUtility.FromJson<UserPublicProfile>(json);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 사용자 프로필 가져오기 실패: {ex.Message}");
        }
#endif
        return null;
    }

    // 공개 프로퍼티들
    public List<FriendInfo> GetFriendsList()
    {
        return currentUserFriendData?.Friends ?? new List<FriendInfo>();
    }

    public List<FriendRequest> GetReceivedRequests()
    {
        return currentUserFriendData?.ReceivedRequests?.Where(r => r.status == FriendRequestStatus.Pending).ToList() 
               ?? new List<FriendRequest>();
    }

    public List<FriendRequest> GetSentRequests()
    {
        return currentUserFriendData?.SentRequests?.Where(r => r.status == FriendRequestStatus.Pending).ToList() 
               ?? new List<FriendRequest>();
    }

    public bool IsFriend(string userId)
    {
        return currentUserFriendData?.IsFriend(userId) ?? false;
    }

    public bool HasSentRequestTo(string userId)
    {
        return currentUserFriendData?.HasSentRequestTo(userId) ?? false;
    }

    public bool HasReceivedRequestFrom(string userId)
    {
        return currentUserFriendData?.HasReceivedRequestFrom(userId) ?? false;
    }

    /// <summary>
    /// 친구 시스템이 초기화되었는지 확인
    /// </summary>
    public bool IsInitialized()
    {
        bool hasUserId = !string.IsNullOrEmpty(currentUserId);
        bool hasData = currentUserFriendData != null;
        bool hasFirebase = IsFirebaseConnected();
        
        Debug.Log($"[FriendSystemManager] 초기화 상태 확인 - UserID: {hasUserId}, Data: {hasData}, Firebase: {hasFirebase}");
        
        return hasUserId && hasData && hasFirebase;
    }

    /// <summary>
    /// Firebase 연결 상태 확인
    /// </summary>
    public bool IsFirebaseConnected()
    {
#if FIREBASE_DATABASE
        bool connected = profilesRef != null && friendsRef != null && requestsRef != null;
        Debug.Log($"[FriendSystemManager] Firebase 연결 상태: {connected} (profiles: {profilesRef != null}, friends: {friendsRef != null}, requests: {requestsRef != null})");
        return connected;
#else
        Debug.Log("[FriendSystemManager] Firebase SDK 없음");
        return false;
#endif
    }

    /// <summary>
    /// 현재 사용자 ID 반환
    /// </summary>
    public string GetCurrentUserId()
    {
        return currentUserId;
    }

    /// <summary>
    /// 실시간 리스너 재설정 (문제 해결용)
    /// </summary>
    public void ResetRealtimeListeners()
    {
        if (string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogError("[FriendSystemManager] 사용자 ID가 설정되지 않아 리스너를 재설정할 수 없습니다.");
            return;
        }

#if FIREBASE_DATABASE
        try
        {
            Debug.Log("[FriendSystemManager] 실시간 리스너 재설정 시작...");
            
            // 기존 리스너 제거
            if (requestsRef != null)
            {
                requestsRef.Child(currentUserId).Child("received").ValueChanged -= OnReceivedRequestsValueChanged;
                requestsRef.Child(currentUserId).Child("sent").ValueChanged -= OnSentRequestsValueChanged;
            }
            
            if (friendsRef != null)
            {
                friendsRef.Child(currentUserId).ValueChanged -= OnFriendsValueChanged;
            }
            
            // 새 리스너 설정
            SetupRealtimeListeners();
            
            Debug.Log("[FriendSystemManager] 실시간 리스너 재설정 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 실시간 리스너 재설정 실패: {ex.Message}");
        }
#endif
    }

    /// <summary>
    /// 친구 데이터 강제 새로고침
    /// </summary>
    public async Task<bool> RefreshFriendDataAsync()
    {
        if (!IsFirebaseConnected() || string.IsNullOrEmpty(currentUserId))
        {
            Debug.LogError("[FriendSystemManager] Firebase가 연결되지 않았거나 사용자 ID가 없습니다.");
            return false;
        }

        try
        {
            Debug.Log("[FriendSystemManager] 친구 데이터 강제 새로고침 시작...");
            await LoadUserFriendDataAsync();
            Debug.Log("[FriendSystemManager] 친구 데이터 강제 새로고침 완료");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] 친구 데이터 새로고침 실패: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// 안전한 JSON 파싱 메서드 - FriendRequest용
    /// </summary>
    private FriendRequest SafeParseJsonToFriendRequest(string json)
    {
        try
        {
            // null 또는 빈 문자열 체크
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[FriendSystemManager] JSON이 null 또는 비어있습니다.");
                return null;
            }

            // 원시 값 체크 (문자열, 숫자, boolean 등)
            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
            {
                Debug.LogWarning($"[FriendSystemManager] JSON이 객체 형식이 아닙니다: {json}");
                return null;
            }

            // JSON 파싱 시도
            FriendRequest request = JsonUtility.FromJson<FriendRequest>(json);
            
            // 파싱된 객체의 필수 필드 검증
            if (request != null && string.IsNullOrEmpty(request.requestId))
            {
                Debug.LogWarning("[FriendSystemManager] 파싱된 FriendRequest의 필수 필드가 누락되었습니다.");
                return null;
            }

            return request;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] JSON 파싱 오류: {ex.Message}, JSON: {json}");
            return null;
        }
    }

    /// <summary>
    /// 안전한 JSON 파싱 메서드 - UserFriendData용
    /// </summary>
    private UserFriendData SafeParseJsonToUserFriendData(string json)
    {
        try
        {
            // null 또는 빈 문자열 체크
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning("[FriendSystemManager] JSON이 null 또는 비어있습니다.");
                return null;
            }

            // 원시 값 체크
            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
            {
                Debug.LogWarning($"[FriendSystemManager] JSON이 객체 형식이 아닙니다: {json}");
                return null;
            }

            // JSON 파싱 시도
            UserFriendData data = JsonUtility.FromJson<UserFriendData>(json);
            return data;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FriendSystemManager] JSON 파싱 오류: {ex.Message}, JSON: {json}");
            return null;
        }
    }
} 