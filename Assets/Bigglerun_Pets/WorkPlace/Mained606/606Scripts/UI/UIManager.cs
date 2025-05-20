using System;
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

    // StageSelectUI 참조
    [SerializeField] private GameObject stageSelectUI;
    [SerializeField] private StageSelectUIController stageSelectUIController;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 페이더 참조 설정
        if (fader == null)
            fader = FindObjectOfType<SceneFader>();
    }

    private void Start()
    {
        UIInitialize();
    }

    private void UIInitialize()
    {
        // 캔버스 찾기
        if (canvas == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.name.Contains("Canvas"))
                {
                    canvas = c.transform;
                    Debug.Log("[UIManager] 캔버스 자동 찾기 성공: " + canvas.name);
                    break;
                }
            }
        }

        // HUD 찾기
        if (hud == null)
        {
            hud = GameObject.Find("HUD")?.transform;
            if (hud == null)
            {
                // StageSelectHUD 또는 다른 HUD 변형을 찾음
                hud = GameObject.Find("StageSelectHUD")?.transform;
                
                if (hud == null)
                {
                    // 이름에 "HUD"가 포함된 오브젝트를 찾음
                    Transform[] allTransforms = FindObjectsOfType<Transform>();
                    foreach (Transform t in allTransforms)
                    {
                        if (t.name.Contains("HUD") || t.name.Contains("Hud"))
                        {
                            hud = t;
                            Debug.Log("[UIManager] HUD 자동 찾기 성공: " + hud.name);
                            break;
                        }
                    }
                }
                else
                {
                    Debug.Log("[UIManager] HUD 자동 찾기 성공: " + hud.name);
                }
            }
            else
            {
                Debug.Log("[UIManager] HUD 자동 찾기 성공: " + hud.name);
            }
        }

        // popup 없을 경우 자동으로 찾기
        if (popup == null)
        {
            popup = GameObject.Find("PopupUI")?.transform;
            if (popup == null)
            {
                Transform[] allTransforms = FindObjectsOfType<Transform>();
                foreach (Transform t in allTransforms)
                {
                    if (t.name.Contains("PopupUI") || t.name.Contains("Popup"))
                    {
                        popup = t;
                        Debug.Log("[UIManager] 팝업 UI 자동 찾기 성공: " + popup.name);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("[UIManager] 팝업 UI 자동 찾기 성공: " + popup.name);
            }
        }

        // UIController 찾기
        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController>();
            if (uiController != null)
            {
                Debug.Log("[UIManager] UIController 자동 찾기 성공");
            }
        }

        // StageSelectUI 찾기
        if (stageSelectUI == null)
        {
            stageSelectUI = GameObject.Find("StageSelectUI");
            if (stageSelectUI == null)
            {
                // 이름에 "StageSelect"가 포함된 오브젝트 찾기
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("StageSelect"))
                    {
                        stageSelectUI = obj;
                        Debug.Log("[UIManager] StageSelectUI 자동 찾기 성공: " + stageSelectUI.name);
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("[UIManager] StageSelectUI 자동 찾기 성공: " + stageSelectUI.name);
            }
        }

        // StageSelectUIController 찾기
        if (stageSelectUIController == null && stageSelectUI != null)
        {
            stageSelectUIController = stageSelectUI.GetComponent<StageSelectUIController>();
            if (stageSelectUIController != null)
            {
                Debug.Log("[UIManager] StageSelectUIController 자동 찾기 성공");
            }
        }

        PopupGroupInit();

        // 로딩 스크린 찾기 및 초기화
        if (loadingScreen == null)
        {
            // 이름에 "LoadingScreen"이 포함된 오브젝트 찾기
            loadingScreen = GameObject.Find("LoadingScreen");
            if (loadingScreen == null)
            {
                GameObject[] allObjects = FindObjectsOfType<GameObject>();
                foreach (GameObject obj in allObjects)
                {
                    if (obj.name.Contains("Loading") && obj.name.Contains("Screen"))
                    {
                        loadingScreen = obj;
                        Debug.Log("[UIManager] LoadingScreen 자동 찾기 성공: " + loadingScreen.name);
                        
                        // LoadingText 찾기
                        if (loadingText == null)
                        {
                            Transform textTransform = loadingScreen.transform.Find("LoadingText");
                            if (textTransform != null)
                            {
                                loadingText = textTransform.GetComponent<TextMeshProUGUI>();
                                if (loadingText != null)
                                {
                                    Debug.Log("[UIManager] LoadingText 자동 찾기 성공");
                                }
                            }
                        }
                        
                        break;
                    }
                }
            }
            else
            {
                Debug.Log("[UIManager] LoadingScreen 자동 찾기 성공: " + loadingScreen.name);
                
                // LoadingText 찾기
                if (loadingText == null)
                {
                    Transform textTransform = loadingScreen.transform.Find("LoadingText");
                    if (textTransform != null)
                    {
                        loadingText = textTransform.GetComponent<TextMeshProUGUI>();
                        if (loadingText != null)
                        {
                            Debug.Log("[UIManager] LoadingText 자동 찾기 성공");
                        }
                    }
                }
            }
            
            // 로딩 스크린을 못 찾은 경우 생성
            if (loadingScreen == null)
            {
                CreateLoadingScreenIfNeeded();
            }
            else
            {
                loadingScreen.SetActive(false);
            }
        }
        else
        {
            loadingScreen.SetActive(false);
        }

        // 팝업 초기화 (팝업 그룹이 비어있지 않은 경우에만 실행)
        if (popupGroup != null && popupGroup.Count > 0)
        {
            foreach (Transform popupItem in popupGroup)
            {
                if (popupItem != null)
                    popupItem.gameObject.SetActive(false);
            }
        }

        // 페이더 참조 설정
        if (fader == null)
        {
            fader = FindObjectOfType<SceneFader>();
            if (fader != null)
            {
                Debug.Log("[UIManager] SceneFader 자동 찾기 성공");
            }
        }
    }

    private void PopupGroupInit()
    {
        popupGroup.Clear();

        if (popup != null)
        {
            foreach (Transform child in popup)
            {
                popupGroup.Add(child);
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] Popup 그룹이 없습니다.");
        }
    }

    public void SceneChange()
    {
        // 씬 전환 시마다 UI 요소들을 다시 찾음
        UIInitialize();
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
                //Debug.Log($"{target.name} 오픈");
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

    public Transform FindDirectChildByName(string uiName)
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
        
        // 다른 UI 숨기기
        HideAll();
        
        // 스테이지 선택 UI 표시
        if (stageSelectUI != null)
        {
            stageSelectUI.SetActive(true);
            
            // 데이터 갱신
            if (stageSelectUIController != null)
            {
                stageSelectUIController.RefreshAllStageButtons();
            }
            else
            {
                stageSelectUIController = stageSelectUI.GetComponent<StageSelectUIController>();
                if (stageSelectUIController != null)
                {
                    stageSelectUIController.RefreshAllStageButtons();
                }
                else
                {
                    Debug.LogWarning("[UIManager] StageSelectUIController를 찾을 수 없습니다.");
                }
            }
        }
        else
        {
            Debug.LogError("[UIManager] StageSelectUI가 설정되지 않았습니다.");
        }
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