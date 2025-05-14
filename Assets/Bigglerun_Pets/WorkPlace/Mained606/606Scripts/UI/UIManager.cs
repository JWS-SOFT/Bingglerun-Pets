using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI 제어를 담당하는 매니저 (로비, 타이틀 등 상태별로 처리)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Transform canvas;
    [SerializeField] public Transform hud;
    [SerializeField] public Transform popup;
    public UIController uiController;
    [SerializeField] private List<Transform> popupGroup = new List<Transform>();
    [SerializeField] private Stack<Transform> openedPopups = new Stack<Transform>();
    [SerializeField] private string lastOpenedPopup = "";

    [SerializeField] private SceneFader fader;
    
    // 로딩 화면 관련
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private TextMeshProUGUI loadingText;

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

    private void Start()
    {
        fader = FindAnyObjectByType<SceneFader>();
        
        // UI 초기화 호출
        UIInitialize();
        
        // 로딩 스크린이 없으면 자동으로 생성
        CreateLoadingScreenIfNeeded();
    }

    private void UIInitialize()
    {
        var canvasObj = FindAnyObjectByType<Canvas>();
        if (canvasObj != null)
        {
            canvas = canvasObj.transform;
            
            if (canvas.childCount > 0)
            {
                hud = canvas.GetChild(0);
                uiController = canvas.GetComponent<UIController>();
            }
            
            // CanvasGroup 컴포넌트 찾기 (null 체크 추가)
            var canvasGroup = FindAnyObjectByType<CanvasGroup>();
            if (canvasGroup != null)
            {
                popup = canvasGroup.transform;
            }
            
            // 로딩 화면 찾기
            GameObject loadingObj = GameObject.Find("LoadingScreen");
            if (loadingObj != null)
            {
                loadingScreen = loadingObj;
                loadingText = loadingObj.GetComponentInChildren<TextMeshProUGUI>();
                loadingScreen.SetActive(false);
            }
        }

        // popup이 null이 아닐 때만 초기화
        if (popup != null)
        {
            PopupGroupInit();
        }
    }

    private void PopupGroupInit()
    {
        popupGroup.Clear();

        for(int i = 0; i < popup.childCount; i++)
        {
            popupGroup.Add(popup.GetChild(i));
        }
    }

    public void SceneChange()
    {
        UIInitialize();
        
        // 씬 변경 시에도 로딩 스크린 확인
        CreateLoadingScreenIfNeeded();
    }
    
    /// <summary>
    /// 로딩 화면 표시/숨김
    /// </summary>
    public void ShowLoadingScreen(bool show, string message = "Loading...")
    {
        // 로딩 스크린이 없으면 생성
        if (loadingScreen == null)
        {
            CreateLoadingScreenIfNeeded();
            
            if (loadingScreen == null)
            {
                Debug.LogWarning("[UIManager] 로딩 화면을 생성할 수 없습니다.");
                return;
            }
        }
        
        loadingScreen.SetActive(show);
        
        if (show && loadingText != null)
        {
            loadingText.text = message;
        }
    }

    public void ShowTitleUI()
    {
        Debug.Log("타이틀 UI 표시");
        // 로그인 버튼 → GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
    }

    public void TogglePopupUI(string uiName)
    {
        Transform target = null;
        target = FindDirectChildByName(uiName);
        
        if(target == this.transform)
        {
            Debug.Log($"{uiName}에 해당하는 UI가 없습니다.");
            return;
        }

        if (target != null)
        {
            if (!target.gameObject.activeSelf)
            {
                openedPopups.Push(target);
                lastOpenedPopup = target.name;
                Debug.Log($"{target.name} 오픈");
            }
            else
            {
                if(openedPopups.Count> 0)
                    lastOpenedPopup = openedPopups.Pop().name;
            }
            target.gameObject.SetActive(!target.gameObject.activeSelf);
        }
        else    // ExitButton
        {
            if(openedPopups.Count > 0)
            {
                lastOpenedPopup = openedPopups.Pop().name;
            }

            target = FindDirectChildByName(lastOpenedPopup);

            if (target != null)
            {
                target.gameObject.SetActive(false);
                lastOpenedPopup = null;
            }
        }
    }

    private Transform FindDirectChildByName(string uiName)
    {
        if (uiName == "")
        {
            return null;
        }

        foreach (Transform target in popupGroup)
        {
            if (target.name == uiName)
                return target;
        }

        return this.transform;
    }

    public void ExitPopup()
    {
        Transform target;
        target = FindDirectChildByName(lastOpenedPopup);

        if(target != null)
        {
            target.gameObject.SetActive(false);
            lastOpenedPopup = null;
        }
    }

    public void ShowLobbyUI()
    {
        Debug.Log("로비 UI 표시");
    }

    public void ShowModeSelectUI()
    {
        Debug.Log("모드 선택 UI 표시");
    }

    public void ShowStoryStageSelectUI()
    {
        Debug.Log("스토리 스테이지 선택 UI 표시");
    }

    public void ShowCompetitiveSetupUI()
    {
        Debug.Log("경쟁모드 캐릭터 선택/아이템 선택 UI 표시");
    }

    public void ShowResultUI()
    {
        Debug.Log("결과 화면 표시");
    }

    public void ShowPauseMenu()
    {
        Debug.Log("일시정지 메뉴 표시");
    }

    public void HideAll()
    {
        Debug.Log("모든 UI 숨김");
    }

    // 로딩 스크린이 없으면 자동으로 생성하는 메서드
    private void CreateLoadingScreenIfNeeded()
    {
        if (loadingScreen == null && canvas != null)
        {
            Debug.Log("[UIManager] 로딩 스크린이 없어서 자동으로 생성합니다.");
            
            // LoadingScreen 생성
            GameObject loadingObj = new GameObject("LoadingScreen");
            RectTransform rect = loadingObj.AddComponent<RectTransform>();
            loadingObj.AddComponent<CanvasGroup>();
            
            // 캔버스의 자식으로 설정
            loadingObj.transform.SetParent(canvas, false);
            
            // 화면 전체를 채우도록 설정
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            // 배경 패널 생성
            GameObject bgPanel = new GameObject("Background");
            RectTransform bgRect = bgPanel.AddComponent<RectTransform>();
            Image bgImage = bgPanel.AddComponent<Image>();
            bgPanel.transform.SetParent(loadingObj.transform, false);
            
            // 배경 패널 설정
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            bgImage.color = new Color(0, 0, 0, 0.8f);
            
            // 로딩 텍스트 생성 (TextMeshPro 사용)
            GameObject textObj = new GameObject("LoadingText");
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            textObj.transform.SetParent(loadingObj.transform, false);
            
            // 텍스트 설정
            textRect.anchorMin = new Vector2(0.5f, 0.5f);
            textRect.anchorMax = new Vector2(0.5f, 0.5f);
            textRect.sizeDelta = new Vector2(300, 50);
            text.text = "Loading...";
            text.fontSize = 24;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            // 참조 설정
            loadingScreen = loadingObj;
            loadingText = text;
            
            // 기본적으로 비활성화
            loadingObj.SetActive(false);
        }
    }
}