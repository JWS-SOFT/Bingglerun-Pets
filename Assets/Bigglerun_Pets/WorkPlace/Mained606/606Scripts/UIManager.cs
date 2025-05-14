using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// UI 제어를 담당하는 매니저 (로비, 타이틀 등 상태별로 처리)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Transform canvas;
    [SerializeField] public Transform hud;
    [SerializeField] public Transform popup;
    [SerializeField] private List<Transform> popupGroup = new List<Transform>();
    [SerializeField] private Stack<Transform> openedPopups = new Stack<Transform>();
    [SerializeField] private string lastOpenedPopup = "";

    [SerializeField] private SceneFader fader;
    
    // 로딩 화면 관련
    [SerializeField] private GameObject loadingScreen;
    [SerializeField] private Text loadingText;

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

        UIInitialize();
    }

    private void UIInitialize()
    {
        canvas = FindAnyObjectByType<Canvas>().transform;
        if (canvas != null)
        {
            hud = canvas.transform.GetChild(0);
            popup = FindAnyObjectByType<CanvasGroup>().transform;
            
            // 로딩 화면 찾기
            loadingScreen = GameObject.Find("LoadingScreen");
            if (loadingScreen != null)
            {
                loadingText = loadingScreen.GetComponentInChildren<Text>();
                loadingScreen.SetActive(false);
            }
        }


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
    }
    
    /// <summary>
    /// 로딩 화면 표시/숨김
    /// </summary>
    public void ShowLoadingScreen(bool show, string message = "로딩 중...")
    {
        if (loadingScreen == null)
        {
            Debug.LogWarning("[UIManager] 로딩 화면이 설정되지 않았습니다.");
            return;
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
}