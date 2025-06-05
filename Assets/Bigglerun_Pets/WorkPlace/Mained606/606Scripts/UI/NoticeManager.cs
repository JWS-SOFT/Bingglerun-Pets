using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;

#if FIREBASE_ENABLED
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
#endif

[System.Serializable]
public class NoticeData
{
    public string id;
    public string type; // "notice", "update", "event"
    public string title;
    public string content;
    public string timestamp;
    public bool isActive;
    public int priority;
}

public class NoticeManager : MonoBehaviour
{
    public static NoticeManager Instance { get; private set; }

    [Header("UI References")]
    private Button noticeButton;
    private Button updateButton;
    private Button eventButton;
    private Button exitButton;
    private Transform noticeContent;
    private ScrollRect scrollRect;
    private GameObject noticeItemPrefab;
    private GameObject noticeUIObject;
    
    [Header("Current Tab")]
    private string currentTab = "notice";
    
#if FIREBASE_ENABLED
    private DatabaseReference databaseRef;
#endif
    private List<NoticeData> allNotices = new List<NoticeData>();
    private List<GameObject> spawnedItems = new List<GameObject>();

    private bool isUIBound = false;
    private bool isWaitingForLobbyScene = false;
    
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
        
        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnEnable()
    {
        GameStateMachine.OnStateChanged += HandleGameStateChanged;
    }

    void OnDisable()
    {
        GameStateMachine.OnStateChanged -= HandleGameStateChanged;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (isWaitingForLobbyScene)
        {
            StartCoroutine(FindNoticeUIAfterDelay());
        }
    }

    private void HandleGameStateChanged(GameState newState)
    {
        if (newState == GameState.Lobby)
        {
            // 로비 상태로 변경되면 플래그 설정
            isWaitingForLobbyScene = true;
            StartCoroutine(FindNoticeUIAfterDelay());
        }
        else
        {
            isWaitingForLobbyScene = false;
            // 다른 씬으로 전환시 UI 숨기기
            HideNoticeUI();
        }
    }

    private IEnumerator FindNoticeUIAfterDelay()
    {
        // 씬이 완전히 로드될 때까지 기다림
        yield return new WaitForSeconds(0.1f);

        // LobbyUI 찾기 시도
        var lobbyUI = GameObject.Find("LobbyUI");
        
        // 못 찾았다면 한 프레임 더 기다렸다가 다시 시도
        if (lobbyUI == null)
        {
            yield return null;
            lobbyUI = GameObject.Find("LobbyUI");
        }

        if (lobbyUI != null)
        {
            // LobbyPopupUI 하위의 NoticeUI 찾기
            var lobbyPopupUI = lobbyUI.transform.Find("LobbyPopupUI");
            if (lobbyPopupUI != null)
            {
                noticeUIObject = lobbyPopupUI.Find("NoticeUI")?.gameObject;
                if (noticeUIObject != null)
                {
                    // UI 컴포넌트 참조 설정
                    SetupUIReferences();
                    
                    // NoticeItem 프리팹 로드
                    noticeItemPrefab = Resources.Load<GameObject>("Prefabs/NoticeItem");
                    if (noticeItemPrefab == null)
                    {
                        Debug.LogError("[NoticeManager] NoticeItem 프리팹을 Resources 폴더에서 로드할 수 없습니다.");
                        yield break;
                    }

                    // UI 이벤트 설정 및 공지사항 로드
                    SetupButtonEvents();
                    LoadNotices();

                    isUIBound = true;
                    noticeUIObject.SetActive(true);
                }
                else
                {
                    Debug.LogWarning("[NoticeManager] LobbyPopupUI 하위에서 NoticeUI를 찾을 수 없습니다.");
                }
            }
            else
            {
                Debug.LogWarning("[NoticeManager] LobbyUI 하위에서 LobbyPopupUI를 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[NoticeManager] LobbyUI를 찾을 수 없습니다.");
        }

        isWaitingForLobbyScene = false;
    }

    private void SetupUIReferences()
    {
        if (noticeUIObject == null) return;

        Debug.Log($"[NoticeManager] NoticeUI 찾음: {noticeUIObject.name}");

        // MenuTab 하위의 버튼들 찾기
        Transform menuTab = noticeUIObject.transform.Find("MenuTab");
        if (menuTab != null)
        {
            noticeButton = menuTab.Find("NoticeButton")?.GetComponent<Button>();
            updateButton = menuTab.Find("UpdateButton")?.GetComponent<Button>();
            eventButton = menuTab.Find("EventButton")?.GetComponent<Button>();
            
            Debug.Log($"[NoticeManager] MenuTab 버튼 상태 - Notice: {noticeButton != null}, Update: {updateButton != null}, Event: {eventButton != null}");
        }
        else
        {
            Debug.LogError("[NoticeManager] MenuTab을 찾을 수 없습니다.");
        }

        // ExitButton 찾기
        exitButton = noticeUIObject.transform.Find("ExitButton")?.GetComponent<Button>();
        if (exitButton == null)
        {
            Debug.LogError("[NoticeManager] ExitButton을 찾을 수 없습니다.");
        }
        
        // OptionArea/NoticeTab 찾기
        Transform optionArea = noticeUIObject.transform.Find("OptionArea");
        if (optionArea != null)
        {
            Debug.Log($"[NoticeManager] OptionArea 찾음: {optionArea.name}");
            
            // NoticeTab을 Content로 사용
            noticeContent = optionArea.Find("NoticeTab");
            if (noticeContent != null)
            {
                Debug.Log($"[NoticeManager] NoticeTab(Content) 찾음: {noticeContent.name}");
                
                // NoticeTab에 ScrollRect 추가 (없다면)
                scrollRect = noticeContent.GetComponent<ScrollRect>();
                if (scrollRect == null)
                {
                    Debug.Log("[NoticeManager] NoticeTab에 ScrollRect 컴포넌트 추가");
                    scrollRect = noticeContent.gameObject.AddComponent<ScrollRect>();
                    
                    // ScrollRect 설정
                    scrollRect.vertical = true;
                    scrollRect.horizontal = false;
                    scrollRect.content = noticeContent.GetComponent<RectTransform>();
                    
                    // Viewport 설정 (NoticeTab 자체를 viewport로 사용)
                    scrollRect.viewport = noticeContent.GetComponent<RectTransform>();
                }
            }
            else
            {
                Debug.LogError("[NoticeManager] OptionArea 아래에서 NoticeTab을 찾을 수 없습니다.");
            }
        }
        else
        {
            Debug.LogError("[NoticeManager] OptionArea를 찾을 수 없습니다.");
        }

        // 컴포넌트 null 체크 및 로그
        if (noticeButton == null) Debug.LogWarning("[NoticeManager] NoticeButton을 찾을 수 없습니다.");
        if (updateButton == null) Debug.LogWarning("[NoticeManager] UpdateButton을 찾을 수 없습니다.");
        if (eventButton == null) Debug.LogWarning("[NoticeManager] EventButton을 찾을 수 없습니다.");
        if (exitButton == null) Debug.LogWarning("[NoticeManager] ExitButton을 찾을 수 없습니다.");
        if (noticeContent == null) Debug.LogWarning("[NoticeManager] NoticeTab(Content)를 찾을 수 없습니다.");
        if (scrollRect == null) Debug.LogWarning("[NoticeManager] ScrollRect를 설정할 수 없습니다.");

        // 모든 필수 컴포넌트가 있는지 확인
        isUIBound = noticeContent != null && scrollRect != null;
        Debug.Log($"[NoticeManager] UI 바인딩 상태: {isUIBound}");
    }

    private void HideNoticeUI()
    {
        if (!isUIBound) return;

        // 생성된 아이템들 정리
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item);
        }
        spawnedItems.Clear();

        // NoticeUI 숨기기
        if (noticeUIObject != null)
        {
            noticeUIObject.SetActive(false);
        }

        // 참조 정리
        noticeButton = null;
        updateButton = null;
        eventButton = null;
        exitButton = null;
        noticeContent = null;
        scrollRect = null;
        
        isUIBound = false;
    }
    
    void InitializeFirebase()
    {
#if FIREBASE_ENABLED
        try
        {
            // Firebase 초기화 확인
            FirebaseApp app = FirebaseApp.DefaultInstance;
            if (app == null)
            {
                Debug.LogError("[NoticeManager] Firebase 앱이 초기화되지 않았습니다.");
                return;
            }

            // Database 인스턴스 가져오기
            databaseRef = Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/").RootReference;
            if (databaseRef != null)
            {
                Debug.Log("[NoticeManager] Firebase Realtime Database 초기화 완료");
            }
            else
            {
                Debug.LogError("[NoticeManager] Firebase Realtime Database 초기화 실패: RootReference가 null입니다.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NoticeManager] Firebase 초기화 중 오류 발생: {e.Message}");
        }
#else
        Debug.LogWarning("[NoticeManager] Firebase가 설치되지 않았습니다. 테스트 데이터를 사용합니다.");
        LoadTestData();
#endif
    }    

    void SetupButtonEvents()
    {
        if (!isUIBound) return;

        if (noticeButton != null) noticeButton.onClick.AddListener(() => SwitchTab("notice"));
        if (updateButton != null) updateButton.onClick.AddListener(() => SwitchTab("update"));
        if (eventButton != null) eventButton.onClick.AddListener(() => SwitchTab("event"));
        if (exitButton != null) exitButton.onClick.AddListener(CloseNoticeUI);
    }
    
    public void LoadNotices()
    {
        if (!isUIBound)
        {
            Debug.LogWarning("[NoticeManager] UI가 바인딩되지 않았습니다.");
            return;
        }

#if FIREBASE_ENABLED
        Debug.Log("[NoticeManager] Firebase에서 공지사항 데이터 로드 시도");
        if (databaseRef != null)
        {
            databaseRef.Child("notices").GetValueAsync().ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError($"[NoticeManager] Firebase 데이터 로드 실패: {task.Exception}");
                    return;
                }
                
                if (task.IsCompletedSuccessfully)
                {
                    DataSnapshot snapshot = task.Result;
                    Debug.Log($"[NoticeManager] Firebase 데이터 로드 성공. 데이터 수: {snapshot.ChildrenCount}");
                    allNotices.Clear();
                    
                    foreach (DataSnapshot childSnapshot in snapshot.Children)
                    {
                        try
                        {
                            var noticeDict = childSnapshot.Value as Dictionary<string, object>;
                            if (noticeDict != null)
                            {
                                var notice = new NoticeData
                                {
                                    id = childSnapshot.Key,
                                    type = noticeDict.ContainsKey("type") ? noticeDict["type"].ToString() : "",
                                    title = noticeDict.ContainsKey("title") ? noticeDict["title"].ToString() : "",
                                    content = noticeDict.ContainsKey("content") ? noticeDict["content"].ToString() : "",
                                    timestamp = noticeDict.ContainsKey("timestamp") ? noticeDict["timestamp"].ToString() : "",
                                    isActive = noticeDict.ContainsKey("isActive") && (bool)noticeDict["isActive"],
                                    priority = noticeDict.ContainsKey("priority") ? System.Convert.ToInt32(noticeDict["priority"]) : 0
                                };
                                
                                if (notice.isActive)
                                {
                                    allNotices.Add(notice);
                                    Debug.Log($"[NoticeManager] 공지사항 추가됨 - ID: {notice.id}, 타입: {notice.type}, 제목: {notice.title}");
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"[NoticeManager] Notice 데이터 파싱 중 오류: {e.Message}");
                        }
                    }
                    
                    // 우선순위로 정렬
                    allNotices.Sort((a, b) => b.priority.CompareTo(a.priority));
                    
                    // UI 업데이트
                    UpdateNoticeList();
                    Debug.Log($"[NoticeManager] 총 {allNotices.Count}개의 공지사항이 로드되었습니다.");
                }
                else
                {
                    Debug.LogError("[NoticeManager] Firebase에서 notices 데이터를 가져오는데 실패했습니다.");
                }
            });
        }
#else
        LoadTestData();
#endif
    }

    private void UpdateNoticeList()
    {
        if (!isUIBound || noticeContent == null)
        {
            Debug.LogError("[NoticeManager] UI가 바인딩되지 않았거나 noticeContent가 null입니다.");
            return;
        }

        Debug.Log($"[NoticeManager] 공지사항 업데이트 시작 - 현재 탭: {currentTab}");

        // 기존 아이템 제거
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item);
        }
        spawnedItems.Clear();

        // 현재 탭에 해당하는 공지사항만 필터링
        var filteredNotices = allNotices.FindAll(notice => notice.type == currentTab);
        Debug.Log($"[NoticeManager] 필터링된 공지사항 수: {filteredNotices.Count}");

        // 새 아이템 생성
        foreach (var notice in filteredNotices)
        {
            try
            {
                GameObject item = Instantiate(noticeItemPrefab, noticeContent);
                if (item != null)
                {
                    // NoticeItem 컴포넌트 가져오기
                    NoticeItem noticeItem = item.GetComponent<NoticeItem>();
                    if (noticeItem != null)
                    {
                        Debug.Log($"[NoticeManager] 공지사항 생성 - ID: {notice.id}, 제목: {notice.title}");
                        noticeItem.SetData(notice);
                        spawnedItems.Add(item);

                        // RectTransform 설정
                        RectTransform rectTransform = item.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchorMin = new Vector2(0, 1);
                            rectTransform.anchorMax = new Vector2(1, 1);
                            rectTransform.pivot = new Vector2(0.5f, 1);
                            rectTransform.anchoredPosition = new Vector2(0, -spawnedItems.Count * rectTransform.rect.height);
                        }
                    }
                    else
                    {
                        Debug.LogError($"[NoticeManager] NoticeItem 컴포넌트를 찾을 수 없습니다.");
                        Destroy(item);
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[NoticeManager] 공지사항 생성 중 오류 발생: {e.Message}");
            }
        }

        // Content 크기 조정
        if (noticeContent != null)
        {
            RectTransform contentRect = noticeContent.GetComponent<RectTransform>();
            if (contentRect != null && spawnedItems.Count > 0)
            {
                float totalHeight = 0;
                foreach (var item in spawnedItems)
                {
                    if (item != null)
                    {
                        RectTransform itemRect = item.GetComponent<RectTransform>();
                        if (itemRect != null)
                        {
                            totalHeight += itemRect.rect.height;
                        }
                    }
                }
                contentRect.sizeDelta = new Vector2(contentRect.sizeDelta.x, totalHeight);
            }
        }

        // 스크롤을 최상단으로
        if (scrollRect != null)
        {
            scrollRect.normalizedPosition = new Vector2(0, 1);
            Debug.Log("[NoticeManager] 스크롤을 최상단으로 이동");
        }
    }

    private void SwitchTab(string tabName)
    {
        currentTab = tabName;
        UpdateNoticeList();
    }

    private void CloseNoticeUI()
    {
        if (noticeUIObject != null)
        {
            noticeUIObject.SetActive(false);
        }
    }

    private void LoadTestData()
    {
        allNotices = new List<NoticeData>
        {
            new NoticeData
            {
                id = "notice1",
                type = "notice",
                title = "중요 공지사항",
                content = "안녕하세요. 빙글런 펫츠를 이용해 주셔서 감사합니다.\n\n현재 게임 서비스가 정상적으로 운영되고 있으며, 더 나은 서비스를 위해 계속 노력하겠습니다.",
                timestamp = System.DateTime.Now.ToString("yyyy-MM-dd"),
                isActive = true,
                priority = 3
            },
            new NoticeData
            {
                id = "update1",
                type = "update",
                title = "1.0.1 업데이트 안내",
                content = "1. 게임 성능 최적화\n2. UI 개선\n3. 버그 수정",
                timestamp = System.DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd"),
                isActive = true,
                priority = 2
            },
            new NoticeData
            {
                id = "event1",
                type = "event",
                title = "출시 기념 이벤트",
                content = "빙글런 펫츠 출시를 기념하여 특별 이벤트를 진행합니다!\n\n참여 기간: 2024.01.01 ~ 2024.01.31",
                timestamp = System.DateTime.Now.AddDays(-2).ToString("yyyy-MM-dd"),
                isActive = true,
                priority = 1
            }
        };

        Debug.Log($"[NoticeManager] {allNotices.Count}개의 테스트 공지사항이 로드되었습니다.");
        UpdateNoticeList();
    }
}