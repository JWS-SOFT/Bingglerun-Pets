using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 친구 UI 컨트롤러
/// 친구 목록, 친구 검색, 친구 요청 관리를 담당
/// </summary>
public class FriendUIController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject friendListPanel;
    [SerializeField] private GameObject friendSearchPanel;
    [SerializeField] private GameObject friendRequestPanel;
    [SerializeField] private Button closeButton;
    
    [Header("Friend List")]
    [SerializeField] private Transform friendListContent;
    [SerializeField] private GameObject friendListItemPrefab;
    [SerializeField] private TextMeshProUGUI friendCountText;
    
    [Header("Friend Search")]
    [SerializeField] private TMP_InputField searchInputField;
    [SerializeField] private Button searchButton;
    [SerializeField] private GameObject searchResultPanel;
    [SerializeField] private TextMeshProUGUI searchResultNickname;
    [SerializeField] private TextMeshProUGUI searchResultLevel;
    [SerializeField] private Button sendRequestButton;
    [SerializeField] private TextMeshProUGUI searchStatusText;
    
    [Header("Friend Requests")]
    [SerializeField] private Transform friendRequestContent;
    [SerializeField] private GameObject friendRequestItemPrefab;
    [SerializeField] private TextMeshProUGUI requestCountText;
    
    [Header("Tab Buttons")]
    [SerializeField] private Button friendListTabButton;
    [SerializeField] private Button friendSearchTabButton;
    [SerializeField] private Button friendRequestTabButton;
    
    [Header("Status")]
    [SerializeField] private TextMeshProUGUI statusText;
    
    // 현재 검색 결과
    private UserSearchResult currentSearchResult;
    
    // 친구 목록 아이템들
    private List<FriendListItem> friendListItems = new List<FriendListItem>();
    private List<FriendRequestItem> friendRequestItems = new List<FriendRequestItem>();
    
    private void Start()
    {
        InitializeUI();
        SetupEventListeners();
        RegisterWithUIManager();
        AdjustUILayout();
        
        // 기본적으로 친구 목록 탭 활성화
        ShowFriendListTab();
    }
    

    
    /// <summary>
    /// UI 레이아웃 조정
    /// </summary>
    private void AdjustUILayout()
    {
        // UI가 화면을 벗어나지 않도록 조정
        RectTransform rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            // 화면 크기 가져오기
            Canvas canvas = GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                if (canvasRect != null)
                {
                    Vector2 canvasSize = canvasRect.sizeDelta;
                    Vector2 currentSize = rectTransform.sizeDelta;
                    
                    // UI가 화면보다 크면 조정
                    if (currentSize.x > canvasSize.x * 0.9f)
                    {
                        currentSize.x = canvasSize.x * 0.9f;
                    }
                    if (currentSize.y > canvasSize.y * 0.9f)
                    {
                        currentSize.y = canvasSize.y * 0.9f;
                    }
                    
                    rectTransform.sizeDelta = currentSize;
                    
                    // 중앙에 위치시키기
                    rectTransform.anchoredPosition = Vector2.zero;
                    
                    Debug.Log($"[FriendUIController] UI 레이아웃 조정 완료 - 크기: {currentSize}");
                }
            }
        }
        
        // ScrollRect 설정 확인 및 조정
        AdjustScrollRects();
    }
    

    
    /// <summary>
    /// ScrollRect 설정 조정
    /// </summary>
    private void AdjustScrollRects()
    {
        // 친구 목록 ScrollRect 조정
        if (friendListContent != null)
        {
            var scrollRect = friendListContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
                scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
                
                // Content Size Fitter 설정
                var contentSizeFitter = friendListContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (contentSizeFitter == null)
                {
                    contentSizeFitter = friendListContent.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                }
                contentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                
                // Vertical Layout Group 설정
                var layoutGroup = friendListContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = friendListContent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                }
                layoutGroup.childControlHeight = false;
                layoutGroup.childControlWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = true;
            }
        }
        
        // 친구 요청 목록 ScrollRect 조정
        if (friendRequestContent != null)
        {
            var scrollRect = friendRequestContent.GetComponentInParent<UnityEngine.UI.ScrollRect>();
            if (scrollRect != null)
            {
                scrollRect.vertical = true;
                scrollRect.horizontal = false;
                scrollRect.movementType = UnityEngine.UI.ScrollRect.MovementType.Clamped;
                
                // Content Size Fitter 설정
                var contentSizeFitter = friendRequestContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
                if (contentSizeFitter == null)
                {
                    contentSizeFitter = friendRequestContent.gameObject.AddComponent<UnityEngine.UI.ContentSizeFitter>();
                }
                contentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                
                // Vertical Layout Group 설정
                var layoutGroup = friendRequestContent.GetComponent<UnityEngine.UI.VerticalLayoutGroup>();
                if (layoutGroup == null)
                {
                    layoutGroup = friendRequestContent.gameObject.AddComponent<UnityEngine.UI.VerticalLayoutGroup>();
                }
                layoutGroup.childControlHeight = false;
                layoutGroup.childControlWidth = true;
                layoutGroup.childForceExpandHeight = false;
                layoutGroup.childForceExpandWidth = true;
            }
        }
    }
    
    /// <summary>
    /// UIManager에 이 UI를 등록
    /// </summary>
    private void RegisterWithUIManager()
    {
        if (UIManager.Instance != null)
        {
            // UIManager의 popupGroup에 이 UI가 등록되어 있는지 확인하고 등록
            Debug.Log("[FriendUIController] UIManager에 친구 UI 등록 시도");
        }
    }
    
    /// <summary>
    /// UI 초기화
    /// </summary>
    private void InitializeUI()
    {
        // 검색 결과 패널 숨기기
        if (searchResultPanel != null)
            searchResultPanel.SetActive(false);
        
        // 상태 텍스트 초기화
        if (statusText != null)
            statusText.text = "";
        
        // 카운트 텍스트 초기화
        if (friendCountText != null)
            friendCountText.text = "Friends: 0";
        
        if (requestCountText != null)
            requestCountText.text = "Requests: 0";
            
        // 닫기 버튼 자동 찾기
        if (closeButton == null)
        {
            FindCloseButton();
        }
        
        // 중요한 UI 요소들이 null인 경우 자동으로 찾기
        FindMissingUIElements();
    }
    
    /// <summary>
    /// 누락된 UI 요소들 자동 찾기
    /// </summary>
    private void FindMissingUIElements()
    {
        // 친구 요청 버튼 찾기
        if (sendRequestButton == null)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);
            foreach (Button btn in buttons)
            {
                if (btn.name.ToLower().Contains("send") || 
                    btn.name.ToLower().Contains("request") ||
                    btn.name.ToLower().Contains("add"))
                {
                    sendRequestButton = btn;
                    Debug.Log($"[FriendUIController] 친구 요청 버튼 자동 찾기 성공: {btn.name}");
                    break;
                }
            }
        }
        
        // 검색 결과 패널 찾기
        if (searchResultPanel == null)
        {
            Transform[] transforms = GetComponentsInChildren<Transform>(true);
            foreach (Transform t in transforms)
            {
                if (t.name.ToLower().Contains("result") || 
                    t.name.ToLower().Contains("search"))
                {
                    searchResultPanel = t.gameObject;
                    Debug.Log($"[FriendUIController] 검색 결과 패널 자동 찾기 성공: {t.name}");
                    break;
                }
            }
        }
        
        Debug.Log($"[FriendUIController] UI 요소 상태 - 친구요청버튼: {sendRequestButton != null}, 검색결과패널: {searchResultPanel != null}");
    }
    
    /// <summary>
    /// 닫기 버튼 자동 찾기
    /// </summary>
    private void FindCloseButton()
    {
        // X 버튼이나 Close 버튼을 찾아서 설정
        Button[] buttons = GetComponentsInChildren<Button>(true);
        foreach (Button btn in buttons)
        {
            if (btn.name.ToLower().Contains("close") || 
                btn.name.ToLower().Contains("exit") || 
                btn.name == "X" ||
                btn.name.ToLower().Contains("x"))
            {
                closeButton = btn;
                Debug.Log($"[FriendUIController] 닫기 버튼 자동 찾기 성공: {btn.name}");
                break;
            }
        }
        
        if (closeButton == null)
        {
            Debug.LogWarning("[FriendUIController] 닫기 버튼을 찾을 수 없습니다. Inspector에서 수동으로 설정해주세요.");
        }
    }
    
    /// <summary>
    /// 이벤트 리스너 설정
    /// </summary>
    private void SetupEventListeners()
    {
        // 닫기 버튼 이벤트
        if (closeButton != null)
            closeButton.onClick.AddListener(CloseFriendUI);
        
        // 탭 버튼 이벤트
        if (friendListTabButton != null)
            friendListTabButton.onClick.AddListener(ShowFriendListTab);
        
        if (friendSearchTabButton != null)
            friendSearchTabButton.onClick.AddListener(ShowFriendSearchTab);
        
        if (friendRequestTabButton != null)
            friendRequestTabButton.onClick.AddListener(ShowFriendRequestTab);
        
        // 검색 버튼 이벤트
        if (searchButton != null)
            searchButton.onClick.AddListener(OnSearchButtonClicked);
        
        // 친구 요청 보내기 버튼 이벤트
        if (sendRequestButton != null)
            sendRequestButton.onClick.AddListener(OnSendRequestButtonClicked);
        
        // 검색 입력 필드 엔터 키 이벤트
        if (searchInputField != null)
            searchInputField.onSubmit.AddListener(OnSearchSubmit);
        
        // FriendManager 이벤트 구독
        if (FriendManager.Instance != null)
        {
            FriendManager.Instance.OnFriendListUpdated += OnFriendListUpdated;
            FriendManager.Instance.OnFriendRequestsUpdated += OnFriendRequestsUpdated;
            FriendManager.Instance.OnFriendRequestSent += OnFriendRequestSent;
            FriendManager.Instance.OnFriendRequestResponded += OnFriendRequestResponded;
            FriendManager.Instance.OnFriendRemoved += OnFriendRemoved;
            FriendManager.Instance.OnError += OnFriendSystemError;
        }
    }
    
    /// <summary>
    /// 친구 목록 탭 표시
    /// </summary>
    public void ShowFriendListTab()
    {
        SetActivePanel(friendListPanel);
        RefreshFriendList();
    }
    
    /// <summary>
    /// 친구 검색 탭 표시
    /// </summary>
    public void ShowFriendSearchTab()
    {
        SetActivePanel(friendSearchPanel);
        ClearSearchResult();
    }
    
    /// <summary>
    /// 친구 요청 탭 표시
    /// </summary>
    public void ShowFriendRequestTab()
    {
        SetActivePanel(friendRequestPanel);
        RefreshFriendRequests();
    }
    
    /// <summary>
    /// 활성 패널 설정
    /// </summary>
    private void SetActivePanel(GameObject activePanel)
    {
        if (friendListPanel != null)
            friendListPanel.SetActive(friendListPanel == activePanel);
        
        if (friendSearchPanel != null)
            friendSearchPanel.SetActive(friendSearchPanel == activePanel);
        
        if (friendRequestPanel != null)
            friendRequestPanel.SetActive(friendRequestPanel == activePanel);
    }
    
    /// <summary>
    /// 친구 UI 닫기
    /// </summary>
    public void CloseFriendUI()
    {
        Debug.Log("[FriendUIController] 친구 UI 닫기 요청");
        
        // 여러 방법으로 UI 닫기 시도
        bool closed = false;
        
        // 1. UIManager를 통해 팝업 닫기 시도
        if (UIManager.Instance != null)
        {
            try
            {
                UIManager.Instance.ExitPopup();
                closed = true;
                Debug.Log("[FriendUIController] UIManager.ExitPopup() 호출 성공");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[FriendUIController] UIManager.ExitPopup() 실패: {e.Message}");
            }
        }
        
        // 2. UIManager 방법이 실패했거나 없는 경우, 직접 비활성화
        if (!closed)
        {
            // 부모 오브젝트 찾기 (보통 팝업 UI는 부모 오브젝트를 비활성화해야 함)
            Transform parentToClose = transform;
            
            // "FriendUI", "Friend", "Popup" 등의 이름을 가진 부모 찾기
            while (parentToClose.parent != null)
            {
                if (parentToClose.name.ToLower().Contains("friend") || 
                    parentToClose.name.ToLower().Contains("popup"))
                {
                    break;
                }
                parentToClose = parentToClose.parent;
            }
            
            parentToClose.gameObject.SetActive(false);
            Debug.Log($"[FriendUIController] 직접 UI 닫기: {parentToClose.name}");
        }
        
        // 3. 검색 결과 초기화
        ClearSearchResult();
    }    
    /// <summary>
    /// 친구 목록 새로고침
    /// </summary>
    private void RefreshFriendList()
    {
        if (FriendManager.Instance != null)
        {
            var friends = FriendManager.Instance.GetCachedFriends();
            OnFriendListUpdated(friends);
        }
    }
    
    /// <summary>
    /// 친구 요청 목록 새로고침
    /// </summary>
    private void RefreshFriendRequests()
    {
        if (FriendManager.Instance != null)
        {
            var requests = FriendManager.Instance.GetCachedFriendRequests();
            OnFriendRequestsUpdated(requests);
        }
    }
    
    /// <summary>
    /// 검색 버튼 클릭 처리
    /// </summary>
    private async void OnSearchButtonClicked()
    {
        await PerformSearch();
    }
    
    /// <summary>
    /// 검색 입력 필드 엔터 키 처리
    /// </summary>
    private async void OnSearchSubmit(string value)
    {
        await PerformSearch();
    }
    
    /// <summary>
    /// 사용자 검색 수행
    /// </summary>
    private async System.Threading.Tasks.Task PerformSearch()
    {
        if (searchInputField == null || FriendManager.Instance == null)
        {
            ShowSearchStatus("Friend system not available");
            return;
        }
        
        string nickname = searchInputField.text.Trim();
        
        if (string.IsNullOrEmpty(nickname))
        {
            ShowSearchStatus("Please enter a nickname to search");
            return;
        }
        
        // 최소 길이 확인
        if (nickname.Length < 2)
        {
            ShowSearchStatus("Nickname must be at least 2 characters");
            return;
        }
        
        ShowSearchStatus("Searching...");
        ClearSearchResult();
        
        try
        {
            currentSearchResult = await FriendManager.Instance.SearchUserByNicknameAsync(nickname);
            
            if (currentSearchResult != null && !string.IsNullOrEmpty(currentSearchResult.userId))
            {
                ShowSearchResult(currentSearchResult);
                ShowSearchStatus("");
            }
            else
            {
                ShowSearchStatus($"User '{nickname}' not found");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendUIController] 검색 중 오류 발생: {e.Message}");
            ShowSearchStatus("Search failed. Please try again.");
        }
    }
    
    /// <summary>
    /// 검색 결과 표시
    /// </summary>
    private void ShowSearchResult(UserSearchResult result)
    {
        Debug.Log($"[FriendUIController] 검색 결과 표시: {result.nickname}");
        
        if (searchResultPanel != null)
        {
            searchResultPanel.SetActive(true);
            Debug.Log("[FriendUIController] 검색 결과 패널 활성화");
        }
        else
        {
            Debug.LogWarning("[FriendUIController] searchResultPanel이 null입니다!");
        }
        
        if (searchResultNickname != null)
            searchResultNickname.text = result.nickname;
        
        if (searchResultLevel != null)
            searchResultLevel.text = $"Level {result.level}";
        
        // 친구 요청 버튼 상태 설정
        if (sendRequestButton != null)
        {
            bool canSendRequest = !result.isFriend && !result.hasPendingRequest;
            sendRequestButton.interactable = canSendRequest;
            sendRequestButton.gameObject.SetActive(true); // 버튼 명시적으로 활성화
            
            // 버튼 텍스트 컴포넌트 안전하게 가져오기
            var buttonTextTMP = sendRequestButton.GetComponentInChildren<TextMeshProUGUI>();
            var buttonTextUI = sendRequestButton.GetComponentInChildren<Text>();
            
            string buttonTextToSet = "";
            if (result.isFriend)
            {
                buttonTextToSet = "Already Friend";
            }
            else if (result.hasPendingRequest)
            {
                buttonTextToSet = "Request Sent";
            }
            else
            {
                buttonTextToSet = "Send Request";
            }
            
            // TextMeshPro 우선 사용
            if (buttonTextTMP != null)
            {
                buttonTextTMP.text = buttonTextToSet;
            }
            else if (buttonTextUI != null)
            {
                buttonTextUI.text = buttonTextToSet;
            }
            else
            {
                Debug.LogWarning("[FriendUIController] 친구 요청 버튼의 텍스트 컴포넌트를 찾을 수 없습니다!");
            }
            
            Debug.Log($"[FriendUIController] 버튼 상태 설정 - 활성화: {canSendRequest}, 친구: {result.isFriend}, 요청대기: {result.hasPendingRequest}");
        }
        else
        {
            Debug.LogWarning("[FriendUIController] sendRequestButton이 null입니다!");
        }
        
        // UI 레이아웃 강제 업데이트
        if (searchResultPanel != null)
        {
            Canvas.ForceUpdateCanvases();
        }
    }    
    /// <summary>
    /// 검색 결과 초기화
    /// </summary>
    private void ClearSearchResult()
    {
        if (searchResultPanel != null)
            searchResultPanel.SetActive(false);
        
        currentSearchResult = null;
    }
    
    /// <summary>
    /// 검색 상태 텍스트 표시
    /// </summary>
    private void ShowSearchStatus(string message)
    {
        if (searchStatusText != null)
            searchStatusText.text = message;
    }
    
    /// <summary>
    /// 친구 요청 보내기 버튼 클릭 처리
    /// </summary>
    private async void OnSendRequestButtonClicked()
    {
        if (currentSearchResult == null || FriendManager.Instance == null)
        {
            Debug.LogWarning("[FriendUIController] 검색 결과가 없거나 FriendManager가 없습니다.");
            return;
        }
        
        // 버튼 비활성화 (중복 클릭 방지)
        if (sendRequestButton != null)
            sendRequestButton.interactable = false;
        
        ShowSearchStatus("Sending friend request...");
        
        try
        {
            bool success = await FriendManager.Instance.SendFriendRequestAsync(
                currentSearchResult.userId, 
                currentSearchResult.nickname);
            
            if (success)
            {
                // 검색 결과 업데이트
                currentSearchResult.hasPendingRequest = true;
                ShowSearchResult(currentSearchResult);
                ShowSearchStatus("Friend request sent successfully!");
            }
            else
            {
                // 실패 시 버튼 다시 활성화
                if (sendRequestButton != null)
                    sendRequestButton.interactable = true;
                ShowSearchStatus("Failed to send friend request");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FriendUIController] 친구 요청 전송 중 오류: {e.Message}");
            // 실패 시 버튼 다시 활성화
            if (sendRequestButton != null)
                sendRequestButton.interactable = true;
            ShowSearchStatus("Failed to send friend request");
        }
    }
    
    /// <summary>
    /// 친구 목록 업데이트 이벤트 처리
    /// </summary>
    private void OnFriendListUpdated(List<FriendData> friends)
    {
        Debug.Log($"[FriendUIController] 친구 목록 업데이트 - 친구 수: {friends.Count}");
        
        // 기존 아이템들 제거
        foreach (var item in friendListItems)
        {
            if (item != null && item.gameObject != null)
                Destroy(item.gameObject);
        }
        friendListItems.Clear();
        
        // 새 아이템들 생성
        if (friendListContent != null && friendListItemPrefab != null)
        {
            foreach (var friend in friends)
            {
                Debug.Log($"[FriendUIController] 친구 아이템 생성 - 친구: {friend.nickname}");
                
                GameObject itemObj = Instantiate(friendListItemPrefab, friendListContent);
                FriendListItem item = itemObj.GetComponent<FriendListItem>();
                
                if (item != null)
                {
                    item.Setup(friend, OnRemoveFriendClicked);
                    friendListItems.Add(item);
                    Debug.Log($"[FriendUIController] 친구 아이템 설정 완료 - 친구: {friend.nickname}");
                }
                else
                {
                    Debug.LogError("[FriendUIController] FriendListItem 컴포넌트를 찾을 수 없습니다!");
                }
            }
        }
        else
        {
            if (friendListContent == null)
                Debug.LogError("[FriendUIController] friendListContent가 null입니다!");
            if (friendListItemPrefab == null)
                Debug.LogError("[FriendUIController] friendListItemPrefab이 null입니다!");
        }
        
        // 친구 수 업데이트
        if (friendCountText != null)
            friendCountText.text = $"Friends: {friends.Count}";
            
        // UI 레이아웃 강제 업데이트
        if (friendListContent != null)
        {
            Canvas.ForceUpdateCanvases();
            
            // Content Size Fitter가 있다면 강제로 레이아웃 재계산
            var contentSizeFitter = friendListContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.SetLayoutVertical();
            }
            
            // Layout Group이 있다면 강제로 레이아웃 재계산
            var layoutGroup = friendListContent.GetComponent<UnityEngine.UI.LayoutGroup>();
            if (layoutGroup != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(friendListContent as RectTransform);
            }
        }
    }
    
    /// <summary>
    /// 친구 요청 목록 업데이트 이벤트 처리
    /// </summary>
    private void OnFriendRequestsUpdated(List<FriendRequestData> requests)
    {
        Debug.Log($"[FriendUIController] 친구 요청 목록 업데이트 - 요청 수: {requests.Count}");
        

        
        // 기존 아이템들 제거
        foreach (var item in friendRequestItems)
        {
            if (item != null && item.gameObject != null)
                Destroy(item.gameObject);
        }
        friendRequestItems.Clear();
        
        // 새 아이템들 생성
        if (friendRequestContent != null && friendRequestItemPrefab != null)
        {
            foreach (var request in requests)
            {
                Debug.Log($"[FriendUIController] 친구 요청 아이템 생성 - 요청자: {request.fromNickname}");
                
                GameObject itemObj = Instantiate(friendRequestItemPrefab, friendRequestContent);
                FriendRequestItem item = itemObj.GetComponent<FriendRequestItem>();
                
                if (item != null)
                {
                    item.Setup(request, OnAcceptRequestClicked, OnRejectRequestClicked);
                    friendRequestItems.Add(item);
                    Debug.Log($"[FriendUIController] 친구 요청 아이템 설정 완료 - 요청자: {request.fromNickname}");
                }
                else
                {
                    Debug.LogError("[FriendUIController] FriendRequestItem 컴포넌트를 찾을 수 없습니다!");
                }
            }
        }
        else
        {
            if (friendRequestContent == null)
                Debug.LogError("[FriendUIController] friendRequestContent가 null입니다!");
            if (friendRequestItemPrefab == null)
                Debug.LogError("[FriendUIController] friendRequestItemPrefab이 null입니다!");
        }
        
        // 요청 수 업데이트
        if (requestCountText != null)
            requestCountText.text = $"Requests: {requests.Count}";
            
        // UI 레이아웃 강제 업데이트
        if (friendRequestContent != null)
        {
            Canvas.ForceUpdateCanvases();
            
            // Content Size Fitter가 있다면 강제로 레이아웃 재계산
            var contentSizeFitter = friendRequestContent.GetComponent<UnityEngine.UI.ContentSizeFitter>();
            if (contentSizeFitter != null)
            {
                contentSizeFitter.SetLayoutVertical();
            }
            
            // Layout Group이 있다면 강제로 레이아웃 재계산
            var layoutGroup = friendRequestContent.GetComponent<UnityEngine.UI.LayoutGroup>();
            if (layoutGroup != null)
            {
                UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(friendRequestContent as RectTransform);
            }
        }
    }    
    /// <summary>
    /// 친구 삭제 버튼 클릭 처리
    /// </summary>
    private async void OnRemoveFriendClicked(FriendData friend)
    {
        if (FriendManager.Instance != null)
        {
            await FriendManager.Instance.RemoveFriendAsync(friend.userId, friend.nickname);
        }
    }
    
    /// <summary>
    /// 친구 요청 수락 버튼 클릭 처리
    /// </summary>
    private async void OnAcceptRequestClicked(FriendRequestData request)
    {
        if (FriendManager.Instance != null)
        {
            await FriendManager.Instance.RespondToFriendRequestAsync(request.requestId, true);
        }
    }
    
    /// <summary>
    /// 친구 요청 거절 버튼 클릭 처리
    /// </summary>
    private async void OnRejectRequestClicked(FriendRequestData request)
    {
        if (FriendManager.Instance != null)
        {
            await FriendManager.Instance.RespondToFriendRequestAsync(request.requestId, false);
        }
    }
    
    /// <summary>
    /// 친구 요청 전송 완료 이벤트 처리
    /// </summary>
    private void OnFriendRequestSent(string message)
    {
        ShowStatus(message);
    }
    
    /// <summary>
    /// 친구 요청 응답 완료 이벤트 처리
    /// </summary>
    private void OnFriendRequestResponded(string message, bool accepted)
    {
        ShowStatus(message);
    }
    
    /// <summary>
    /// 친구 삭제 완료 이벤트 처리
    /// </summary>
    private void OnFriendRemoved(string message)
    {
        ShowStatus(message);
    }
    
    /// <summary>
    /// 에러 이벤트 처리
    /// </summary>
    private void OnFriendSystemError(string errorMessage)
    {
        ShowStatus($"Error: {errorMessage}");
    }
    
    /// <summary>
    /// 상태 메시지 표시
    /// </summary>
    private void ShowStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
            
            // 3초 후 메시지 자동 삭제
            Invoke(nameof(ClearStatus), 3f);
        }
    }
    
    /// <summary>
    /// 상태 메시지 초기화
    /// </summary>
    private void ClearStatus()
    {
        if (statusText != null)
            statusText.text = "";
    }
    
    private void OnDestroy()
    {
        // 이벤트 구독 해제
        if (FriendManager.Instance != null)
        {
            FriendManager.Instance.OnFriendListUpdated -= OnFriendListUpdated;
            FriendManager.Instance.OnFriendRequestsUpdated -= OnFriendRequestsUpdated;
            FriendManager.Instance.OnFriendRequestSent -= OnFriendRequestSent;
            FriendManager.Instance.OnFriendRequestResponded -= OnFriendRequestResponded;
            FriendManager.Instance.OnFriendRemoved -= OnFriendRemoved;
            FriendManager.Instance.OnError -= OnFriendSystemError;
        }
    }
}