using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Threading.Tasks;

/// <summary>
/// 친구 시스템 UI 컨트롤러
/// </summary>
public class FriendSystemUI : MonoBehaviour
{
    [Header("메인 패널들")]
    [SerializeField] private GameObject friendsMainPanel;
    [SerializeField] private GameObject friendsListPanel;
    [SerializeField] private GameObject addFriendPanel;
    [SerializeField] private GameObject friendRequestsPanel;
    [SerializeField] private GameObject searchResultsPanel;

    [Header("친구 목록 UI")]
    [SerializeField] private Transform friendsListContent;
    [SerializeField] private GameObject friendItemPrefab;
    [SerializeField] private TextMeshProUGUI friendsCountText;
    [SerializeField] private Button addFriendButton;
    [SerializeField] private Button friendRequestsButton;

    [Header("친구 추가 UI")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button searchButton;
    [SerializeField] private Button backFromAddFriendButton;
    [SerializeField] private Transform searchResultsContent;
    [SerializeField] private GameObject searchResultItemPrefab;
    [SerializeField] private TextMeshProUGUI searchStatusText;

    [Header("친구 요청 UI")]
    [SerializeField] private Transform receivedRequestsContent;
    [SerializeField] private Transform sentRequestsContent;
    [SerializeField] private GameObject friendRequestItemPrefab;
    [SerializeField] private Button backFromRequestsButton;
    [SerializeField] private TextMeshProUGUI receivedRequestsCountText;
    [SerializeField] private TextMeshProUGUI sentRequestsCountText;

    [Header("알림 UI")]
    [SerializeField] private GameObject notificationPanel;
    [SerializeField] private TextMeshProUGUI notificationText;
    [SerializeField] private Button notificationOkButton;

    // 현재 표시된 아이템들 추적
    private List<GameObject> currentFriendItems = new List<GameObject>();
    private List<GameObject> currentSearchResultItems = new List<GameObject>();
    private List<GameObject> currentRequestItems = new List<GameObject>();

    private void Start()
    {
        InitializeUI();
        SubscribeToEvents();
        
        // FriendSystemManager 초기화 상태 확인
        if (FriendSystemManager.Instance == null)
        {
            Debug.LogError("[FriendSystemUI] FriendSystemManager 인스턴스가 없습니다.");
            ShowNotification("Friend system is unavailable. Please restart the game.");
            return;
        }

        if (!FriendSystemManager.Instance.IsInitialized())
        {
            Debug.LogWarning("[FriendSystemUI] FriendSystemManager가 아직 초기화되지 않았습니다.");
            ShowNotification("Friend system is initializing. Please wait.");
            
            // 5초 후에 다시 확인
            StartCoroutine(CheckInitializationAfterDelay());
        }
        
        LoadFriendsList();
    }

    private void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        // 버튼 참조 상태 확인
        Debug.Log($"[FriendSystemUI] 버튼 참조 상태 확인:");
        Debug.Log($"  - addFriendButton: {(addFriendButton != null ? "연결됨" : "NULL")}");
        Debug.Log($"  - friendRequestsButton: {(friendRequestsButton != null ? "연결됨" : "NULL")}");
        Debug.Log($"  - searchButton: {(searchButton != null ? "연결됨" : "NULL")}");
        
        // 버튼 이벤트 연결
        if (addFriendButton != null)
        {
            addFriendButton.onClick.AddListener(ShowAddFriendPanel);
            Debug.Log($"[FriendSystemUI] addFriendButton 이벤트 연결됨. Interactable: {addFriendButton.interactable}");
        }
        else
        {
            Debug.LogError("[FriendSystemUI] addFriendButton이 null입니다! Inspector에서 연결하세요.");
        }
        
        friendRequestsButton?.onClick.AddListener(ShowFriendRequestsPanel);
        backFromAddFriendButton?.onClick.AddListener(ShowFriendsListPanel);
        backFromRequestsButton?.onClick.AddListener(ShowFriendsListPanel);
        searchButton?.onClick.AddListener(SearchUsers);
        notificationOkButton?.onClick.AddListener(HideNotification);

        // 검색 입력 필드 이벤트
        if (searchInputField != null)
        {
            searchInputField.onSubmit.AddListener(delegate { SearchUsers(); });
        }

        // 초기 패널 상태 설정
        ShowFriendsListPanel();
        HideNotification();
    }

    /// <summary>
    /// 이벤트 구독
    /// </summary>
    private void SubscribeToEvents()
    {
        if (FriendSystemManager.Instance != null)
        {
            FriendSystemManager.Instance.OnFriendAdded += OnFriendAdded;
            FriendSystemManager.Instance.OnFriendRemoved += OnFriendRemoved;
            FriendSystemManager.Instance.OnFriendRequestReceived += OnFriendRequestReceived;
            FriendSystemManager.Instance.OnFriendRequestSent += OnFriendRequestSent;
            FriendSystemManager.Instance.OnFriendRequestAccepted += OnFriendRequestAccepted;
            FriendSystemManager.Instance.OnFriendRequestRejected += OnFriendRequestRejected;
            FriendSystemManager.Instance.OnUserSearchCompleted += OnUserSearchCompleted;
        }
    }

    /// <summary>
    /// 이벤트 구독 해제
    /// </summary>
    private void UnsubscribeFromEvents()
    {
        if (FriendSystemManager.Instance != null)
        {
            FriendSystemManager.Instance.OnFriendAdded -= OnFriendAdded;
            FriendSystemManager.Instance.OnFriendRemoved -= OnFriendRemoved;
            FriendSystemManager.Instance.OnFriendRequestReceived -= OnFriendRequestReceived;
            FriendSystemManager.Instance.OnFriendRequestSent -= OnFriendRequestSent;
            FriendSystemManager.Instance.OnFriendRequestAccepted -= OnFriendRequestAccepted;
            FriendSystemManager.Instance.OnFriendRequestRejected -= OnFriendRequestRejected;
            FriendSystemManager.Instance.OnUserSearchCompleted -= OnUserSearchCompleted;
        }
    }

    #region 패널 관리

    /// <summary>
    /// 친구 목록 패널 표시
    /// </summary>
    public void ShowFriendsListPanel()
    {
        SetActivePanel(friendsListPanel);
        RefreshFriendsList();
        UpdateFriendRequestsButtonBadge();
    }

    /// <summary>
    /// 친구 추가 패널 표시
    /// </summary>
    public void ShowAddFriendPanel()
    {
        Debug.Log("[FriendSystemUI] ShowAddFriendPanel 호출됨!");
        
        SetActivePanel(addFriendPanel);
        ClearSearchResults();
        if (searchInputField != null) searchInputField.text = "";
        UpdateSearchStatus("Enter a nickname to search for a friend");
        
        // 공개 프로필이 없으면 강제 업데이트
        EnsurePublicProfileExists();
    }
    
    /// <summary>
    /// 공개 프로필 존재 확인 및 생성
    /// </summary>
    private async void EnsurePublicProfileExists()
    {
        if (FriendSystemManager.Instance == null || !FriendSystemManager.Instance.IsInitialized())
        {
            return;
        }
        
        if (PlayerDataManager.Instance?.CurrentPlayerData == null)
        {
            return;
        }
        
        var playerData = PlayerDataManager.Instance.CurrentPlayerData;
        string displayName = !string.IsNullOrEmpty(playerData.nickname) ? playerData.nickname : "Player";
        int bestScore = playerData.competitiveBestScore;
        string characterId = playerData.currentCharacter ?? "default";
        
        bool success = await FriendSystemManager.Instance.UpdateMyPublicProfileAsync(
            displayName, bestScore, characterId, true);
        
        if (success)
        {
            Debug.Log($"[FriendSystemUI] 공개 프로필 업데이트 완료: {displayName}");
        }
    }

    /// <summary>
    /// 친구 요청 패널 표시
    /// </summary>
    public void ShowFriendRequestsPanel()
    {
        Debug.Log("[FriendSystemUI] 친구 요청 패널 표시");
        SetActivePanel(friendRequestsPanel);
        RefreshFriendRequestsList();
    }

    /// <summary>
    /// 활성 패널 설정
    /// </summary>
    private void SetActivePanel(GameObject activePanel)
    {
        friendsListPanel?.SetActive(false);
        addFriendPanel?.SetActive(false);
        friendRequestsPanel?.SetActive(false);
        searchResultsPanel?.SetActive(false);

        activePanel?.SetActive(true);
        friendsMainPanel?.SetActive(true);
    }

    #endregion

    #region 친구 목록 관리

    /// <summary>
    /// 친구 목록 로드
    /// </summary>
    private void LoadFriendsList()
    {
        if (FriendSystemManager.Instance != null)
        {
            RefreshFriendsList();
        }
    }

    /// <summary>
    /// 친구 목록 새로고침
    /// </summary>
    private void RefreshFriendsList()
    {
        ClearFriendsList();

        if (FriendSystemManager.Instance == null) return;

        var friends = FriendSystemManager.Instance.GetFriendsList();
        
        foreach (var friend in friends)
        {
            CreateFriendListItem(friend);
        }

        UpdateFriendsCountText(friends.Count);
    }

    /// <summary>
    /// 친구 목록 아이템 생성
    /// </summary>
    private void CreateFriendListItem(FriendInfo friend)
    {
        if (friendItemPrefab == null || friendsListContent == null) return;

        GameObject item = Instantiate(friendItemPrefab, friendsListContent);
        FriendListItem friendListItem = item.GetComponent<FriendListItem>();
        
        if (friendListItem != null)
        {
            friendListItem.Setup(friend, OnRemoveFriendClicked, OnCompareFriendClicked);
        }

        currentFriendItems.Add(item);
    }

    /// <summary>
    /// 친구 목록 클리어
    /// </summary>
    private void ClearFriendsList()
    {
        foreach (var item in currentFriendItems)
        {
            if (item != null) Destroy(item);
        }
        currentFriendItems.Clear();
    }

    /// <summary>
    /// 친구 수 텍스트 업데이트
    /// </summary>
    private void UpdateFriendsCountText(int count)
    {
        if (friendsCountText != null)
        {
            friendsCountText.text = $"Friends ({count})";
        }
    }

    #endregion

    #region 사용자 검색

    /// <summary>
    /// 사용자 검색
    /// </summary>
    private async void SearchUsers()
    {
        if (searchInputField == null)
        {
            Debug.LogError("[FriendSystemUI] 검색 입력 필드가 null입니다.");
            UpdateSearchStatus("Search input field error");
            return;
        }

        if (FriendSystemManager.Instance == null)
        {
            Debug.LogError("[FriendSystemUI] FriendSystemManager 인스턴스가 null입니다.");
            UpdateSearchStatus("Friend system not initialized");
            return;
        }

        // FriendSystemManager 초기화 상태 확인
        if (!FriendSystemManager.Instance.IsInitialized())
        {
            Debug.LogError("[FriendSystemUI] FriendSystemManager가 초기화되지 않았습니다.");
            UpdateSearchStatus("Friend system not initialized. Please log in again.");
            return;
        }

        string searchTerm = searchInputField.text.Trim();
        if (string.IsNullOrEmpty(searchTerm))
        {
            UpdateSearchStatus("Please enter a nickname to search");
            return;
        }

        Debug.Log($"[FriendSystemUI] 검색 시작: '{searchTerm}'");
        UpdateSearchStatus("Searching...");
        ClearSearchResults();

        try
        {
            await FriendSystemManager.Instance.SearchUsersAsync(searchTerm);
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[FriendSystemUI] 검색 오류: {ex.Message}");
            Debug.LogError($"[FriendSystemUI] 스택 트레이스: {ex.StackTrace}");
            UpdateSearchStatus("An error occurred while searching");
        }
    }

    /// <summary>
    /// 검색 결과 표시
    /// </summary>
    private void OnUserSearchCompleted(List<UserPublicProfile> results)
    {
        ClearSearchResults();

        if (results == null)
        {
            Debug.LogError("[FriendSystemUI] 검색 결과가 null입니다.");
            UpdateSearchStatus("Unable to receive search results");
            return;
        }

        if (results.Count == 0)
        {
            UpdateSearchStatus("No search results found");
            Debug.Log("[FriendSystemUI] 검색 결과 없음");
            return;
        }

        UpdateSearchStatus($"Found {results.Count} users");
        Debug.Log($"[FriendSystemUI] 검색 결과 표시: {results.Count}개");

        foreach (var profile in results)
        {
            if (profile != null)
            {
                CreateSearchResultItem(profile);
            }
        }
    }

    /// <summary>
    /// 검색 결과 아이템 생성
    /// </summary>
    private void CreateSearchResultItem(UserPublicProfile profile)
    {
        if (searchResultItemPrefab == null || searchResultsContent == null) return;

        GameObject item = Instantiate(searchResultItemPrefab, searchResultsContent);
        SearchResultItem searchResultItem = item.GetComponent<SearchResultItem>();
        
        if (searchResultItem != null)
        {
            bool isFriend = FriendSystemManager.Instance.IsFriend(profile.userId);
            bool hasSentRequest = FriendSystemManager.Instance.HasSentRequestTo(profile.userId);
            bool hasReceivedRequest = FriendSystemManager.Instance.HasReceivedRequestFrom(profile.userId);

            searchResultItem.Setup(profile, isFriend, hasSentRequest, hasReceivedRequest, OnSendFriendRequestClicked);
        }

        currentSearchResultItems.Add(item);
    }

    /// <summary>
    /// 검색 결과 클리어
    /// </summary>
    private void ClearSearchResults()
    {
        foreach (var item in currentSearchResultItems)
        {
            if (item != null) Destroy(item);
        }
        currentSearchResultItems.Clear();
    }

    /// <summary>
    /// 검색 상태 텍스트 업데이트
    /// </summary>
    private void UpdateSearchStatus(string message)
    {
        if (searchStatusText != null)
        {
            searchStatusText.text = message;
        }
    }

    #endregion

    #region 친구 요청 관리

    /// <summary>
    /// 친구 요청 목록 새로고침
    /// </summary>
    private void RefreshFriendRequestsList()
    {
        Debug.Log("[FriendSystemUI] 친구 요청 목록 새로고침 시작");
        ClearFriendRequestsList();

        if (FriendSystemManager.Instance == null) 
        {
            Debug.LogWarning("[FriendSystemUI] FriendSystemManager 인스턴스가 null입니다");
            return;
        }

        // Manager 초기화 상태 확인
        if (!FriendSystemManager.Instance.IsInitialized())
        {
            Debug.LogWarning("[FriendSystemUI] FriendSystemManager가 초기화되지 않았습니다. 재시도를 시작합니다.");
            StartCoroutine(WaitForManagerInitialization());
            return;
        }

        // Firebase 연결 상태 확인
        if (!FriendSystemManager.Instance.IsFirebaseConnected())
        {
            Debug.LogWarning("[FriendSystemUI] Firebase 연결이 끊어져 있습니다.");
        }

        // 현재 사용자 ID 확인
        string currentUserId = FriendSystemManager.Instance.GetCurrentUserId();
        Debug.Log($"[FriendSystemUI] 현재 사용자 ID: {currentUserId}");

        // 받은 요청들
        var receivedRequests = FriendSystemManager.Instance.GetReceivedRequests();
        Debug.Log($"[FriendSystemUI] 받은 요청 수: {receivedRequests.Count}");
        foreach (var request in receivedRequests)
        {
            Debug.Log($"[FriendSystemUI] 받은 요청 아이템 생성: {request.fromDisplayName} ({request.requestId})");
            CreateReceivedRequestItem(request);
        }

        // 보낸 요청들
        var sentRequests = FriendSystemManager.Instance.GetSentRequests();
        Debug.Log($"[FriendSystemUI] 보낸 요청 수: {sentRequests.Count}");
        foreach (var request in sentRequests)
        {
            Debug.Log($"[FriendSystemUI] 보낸 요청 아이템 생성: {request.toUserId} ({request.requestId})");
            CreateSentRequestItem(request);
        }

        UpdateRequestsCountText(receivedRequests.Count, sentRequests.Count);
        Debug.Log("[FriendSystemUI] 친구 요청 목록 새로고침 완료");
    }

    /// <summary>
    /// 받은 요청 아이템 생성
    /// </summary>
    private void CreateReceivedRequestItem(FriendRequest request)
    {
        if (friendRequestItemPrefab == null || receivedRequestsContent == null) return;

        GameObject item = Instantiate(friendRequestItemPrefab, receivedRequestsContent);
        FriendRequestItem requestItem = item.GetComponent<FriendRequestItem>();
        
        if (requestItem != null)
        {
            requestItem.SetupReceivedRequest(request, OnAcceptRequestClicked, OnRejectRequestClicked);
        }

        currentRequestItems.Add(item);
    }

    /// <summary>
    /// 보낸 요청 아이템 생성
    /// </summary>
    private void CreateSentRequestItem(FriendRequest request)
    {
        if (friendRequestItemPrefab == null || sentRequestsContent == null) return;

        GameObject item = Instantiate(friendRequestItemPrefab, sentRequestsContent);
        FriendRequestItem requestItem = item.GetComponent<FriendRequestItem>();
        
        if (requestItem != null)
        {
            requestItem.SetupSentRequest(request, OnCancelRequestClicked);
        }

        currentRequestItems.Add(item);
    }

    /// <summary>
    /// 친구 요청 목록 클리어
    /// </summary>
    private void ClearFriendRequestsList()
    {
        foreach (var item in currentRequestItems)
        {
            if (item != null) Destroy(item);
        }
        currentRequestItems.Clear();
    }

    /// <summary>
    /// 요청 수 텍스트 업데이트
    /// </summary>
    private void UpdateRequestsCountText(int receivedCount, int sentCount)
    {
        if (receivedRequestsCountText != null)
        {
            receivedRequestsCountText.text = $"Received Requests ({receivedCount})";
        }

        if (sentRequestsCountText != null)
        {
            sentRequestsCountText.text = $"Sent Requests ({sentCount})";
        }
    }

    /// <summary>
    /// 친구 요청 버튼 배지 업데이트
    /// </summary>
    private void UpdateFriendRequestsButtonBadge()
    {
        if (FriendSystemManager.Instance == null) return;

        int pendingRequestsCount = FriendSystemManager.Instance.GetReceivedRequests().Count;
        
        // 배지 표시 로직 (구현에 따라 다름)
        // 예: 친구 요청 버튼에 빨간 점이나 숫자 표시
    }

    #endregion

    #region 이벤트 핸들러

    /// <summary>
    /// 친구 추가됨 이벤트
    /// </summary>
    private void OnFriendAdded(FriendInfo friend)
    {
        ShowNotification($"{friend.displayName} is now your friend!");
        
        if (friendsListPanel.activeInHierarchy)
        {
            RefreshFriendsList();
        }
        
        RefreshSearchResultsRelationship();
    }

    /// <summary>
    /// 친구 제거됨 이벤트
    /// </summary>
    private void OnFriendRemoved(string userId)
    {
        ShowNotification("Your friend has been removed");
        
        if (friendsListPanel.activeInHierarchy)
        {
            RefreshFriendsList();
        }
    }

    /// <summary>
    /// 친구 요청 받음 이벤트
    /// </summary>
    private void OnFriendRequestReceived(FriendRequest request)
    {
        Debug.Log($"[FriendSystemUI] 친구 요청 받음 이벤트: {request.fromDisplayName}");
        ShowNotification($"{request.fromDisplayName} has sent you a friend request");
        UpdateFriendRequestsButtonBadge();
        
        if (friendRequestsPanel.activeInHierarchy)
        {
            RefreshFriendRequestsList();
        }
    }

    /// <summary>
    /// 친구 요청 보냄 이벤트
    /// </summary>
    private void OnFriendRequestSent(FriendRequest request)
    {
        Debug.Log($"[FriendSystemUI] 친구 요청 보냄 이벤트: {request.toUserId}");
        ShowNotification("You have sent a friend request");
        RefreshSearchResultsRelationship();
        
        // 보낸 요청 패널이 열려있으면 새로고침
        if (friendRequestsPanel.activeInHierarchy)
        {
            RefreshFriendRequestsList();
        }
    }

    /// <summary>
    /// 친구 요청 수락됨 이벤트
    /// </summary>
    private void OnFriendRequestAccepted(string requestId)
    {
        Debug.Log($"[FriendSystemUI] 친구 요청 수락됨 이벤트: {requestId}");
        if (friendRequestsPanel.activeInHierarchy)
        {
            RefreshFriendRequestsList();
        }
    }

    /// <summary>
    /// 친구 요청 거절됨 이벤트
    /// </summary>
    private void OnFriendRequestRejected(string requestId)
    {
        Debug.Log($"[FriendSystemUI] 친구 요청 거절됨 이벤트: {requestId}");
        if (friendRequestsPanel.activeInHierarchy)
        {
            RefreshFriendRequestsList();
        }
    }

    #endregion

    #region 버튼 클릭 핸들러

    /// <summary>
    /// 친구 요청 보내기 버튼 클릭
    /// </summary>
    private async void OnSendFriendRequestClicked(string userId, string displayName)
    {
        if (FriendSystemManager.Instance != null)
        {
            bool success = await FriendSystemManager.Instance.SendFriendRequestAsync(userId, displayName);
            if (!success)
            {
                ShowNotification("You cannot send a friend request");
            }
        }
    }

    /// <summary>
    /// 친구 요청 수락 버튼 클릭
    /// </summary>
    private async void OnAcceptRequestClicked(string requestId)
    {
        if (FriendSystemManager.Instance != null)
        {
            bool success = await FriendSystemManager.Instance.AcceptFriendRequestAsync(requestId);
            if (!success)
            {
                ShowNotification("You cannot accept this friend request");
            }
        }
    }

    /// <summary>
    /// 친구 요청 거절 버튼 클릭
    /// </summary>
    private async void OnRejectRequestClicked(string requestId)
    {
        if (FriendSystemManager.Instance != null)
        {
            bool success = await FriendSystemManager.Instance.RejectFriendRequestAsync(requestId);
            if (!success)
            {
                ShowNotification("You cannot reject this friend request");
            }
        }
    }

    /// <summary>
    /// 친구 요청 취소 버튼 클릭
    /// </summary>
    private void OnCancelRequestClicked(string requestId)
    {
        // 취소 기능 구현 (필요시)
        ShowNotification("The request has been canceled");
    }

    /// <summary>
    /// 친구 삭제 버튼 클릭
    /// </summary>
    private async void OnRemoveFriendClicked(string userId)
    {
        if (FriendSystemManager.Instance != null)
        {
            bool success = await FriendSystemManager.Instance.RemoveFriendAsync(userId);
            if (!success)
            {
                ShowNotification("You cannot remove this friend");
            }
        }
    }

    /// <summary>
    /// 친구 점수 비교 버튼 클릭
    /// </summary>
    private void OnCompareFriendClicked(string userId)
    {
        // 점수 비교 UI 표시 (구현 필요)
        ShowNotification("Score comparison feature is not implemented yet");
    }

    #endregion

    #region 알림 시스템

    /// <summary>
    /// 알림 표시
    /// </summary>
    private void ShowNotification(string message)
    {
        if (notificationPanel != null && notificationText != null)
        {
            notificationText.text = message;
            notificationPanel.SetActive(true);
        }
    }

    /// <summary>
    /// 알림 숨기기
    /// </summary>
    private void HideNotification()
    {
        notificationPanel?.SetActive(false);
    }

    #endregion

    #region 초기화 관련

    /// <summary>
    /// 일정 시간 후 초기화 상태 재확인
    /// </summary>
    private System.Collections.IEnumerator CheckInitializationAfterDelay()
    {
        yield return new WaitForSeconds(5.0f);
        
        if (FriendSystemManager.Instance != null && FriendSystemManager.Instance.IsInitialized())
        {
            Debug.Log("[FriendSystemUI] FriendSystemManager 초기화 완료됨");
            HideNotification();
            RefreshFriendsList();
        }
        else
        {
            Debug.LogWarning("[FriendSystemUI] FriendSystemManager 초기화가 여전히 완료되지 않음");
            ShowNotification("Friend system initialization failed. Please restart the game.");
        }
    }

    /// <summary>
    /// Manager 초기화를 기다리는 코루틴
    /// </summary>
    private System.Collections.IEnumerator WaitForManagerInitialization()
    {
        int maxRetries = 10;
        int retryCount = 0;
        
        while (retryCount < maxRetries)
        {
            yield return new WaitForSeconds(1f);
            retryCount++;
            
            if (FriendSystemManager.Instance != null && FriendSystemManager.Instance.IsInitialized())
            {
                Debug.Log($"[FriendSystemUI] Manager 초기화 완료 (재시도 {retryCount}회)");
                RefreshFriendRequestsList();
                yield break;
            }
            
            Debug.Log($"[FriendSystemUI] Manager 초기화 대기 중... ({retryCount}/{maxRetries})");
        }
        
        Debug.LogError("[FriendSystemUI] Manager 초기화 시간 초과");
        
        // 마지막 시도: 강제 새로고침
        if (FriendSystemManager.Instance != null)
        {
            Debug.Log("[FriendSystemUI] 강제 새로고침 시도...");
            FriendSystemManager.Instance.ResetRealtimeListeners();
            StartCoroutine(ForceRefreshAfterDelay());
        }
    }

    /// <summary>
    /// 강제 새로고침 (지연 후)
    /// </summary>
    private System.Collections.IEnumerator ForceRefreshAfterDelay()
    {
        yield return new WaitForSeconds(2f);
        
        if (FriendSystemManager.Instance != null)
        {
            Debug.Log("[FriendSystemUI] 강제 데이터 새로고침 실행...");
            var refreshTask = FriendSystemManager.Instance.RefreshFriendDataAsync();
            
            // Task 완료 대기
            while (!refreshTask.IsCompleted)
            {
                yield return null;
            }
            
            if (refreshTask.Result)
            {
                Debug.Log("[FriendSystemUI] 강제 새로고침 성공");
                RefreshFriendRequestsList();
            }
            else
            {
                Debug.LogError("[FriendSystemUI] 강제 새로고침 실패");
            }
        }
    }

    #endregion

    #region 헬퍼 메소드

    /// <summary>
    /// 검색 결과의 관계 상태 새로고침
    /// </summary>
    private void RefreshSearchResultsRelationship()
    {
        if (!searchResultsPanel.activeInHierarchy) return;

        // 현재 검색 결과들의 관계 상태를 업데이트
        foreach (var item in currentSearchResultItems)
        {
            SearchResultItem searchResultItem = item.GetComponent<SearchResultItem>();
            if (searchResultItem != null)
            {
                searchResultItem.RefreshRelationshipStatus();
            }
        }
    }

    #endregion
} 