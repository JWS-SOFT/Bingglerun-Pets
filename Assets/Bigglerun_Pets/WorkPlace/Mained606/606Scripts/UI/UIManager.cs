using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;
using System.Linq; // LINQ 기능 사용을 위해 추가
using System.Collections;

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

        // 씬 로드 이벤트 구독
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 페이더 참조 설정
        if (fader == null)
            fader = FindObjectOfType<SceneFader>();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬 로드 이벤트 핸들러
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[UIManager] 씬 전환 감지: {scene.name}");
        
        // UI 초기화 호출
        Invoke("UIInitialize", 0.2f);
        
        // 스테이지 선택 씬 + 게임 상태가 스테이지 셀렉트일 때만 특별 처리
        if ((scene.name.Contains("StageSelect") || scene.name.Contains("StoryStage")) && IsStageSelectState())
        {
            StartCoroutine(DelayedStageSelectUISearch());
        }
    }

    private void Start()
    {
        UIInitialize();
    }

    private void UIInitialize()
    {
        Debug.Log("[UIManager] UI 요소 초기화 시작");
        
        // 캔버스 찾기
        canvas = null; // 씬 전환 시 이전 참조 제거
        if (canvas == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>();
            foreach (Canvas c in canvases)
            {
                if (c.gameObject.name.Contains("Canvas") && c.transform.root == c.transform)
                {
                    canvas = c.transform;
                    Debug.Log("[UIManager] 캔버스 자동 찾기 성공: " + canvas.name);
                    break;
                }
            }
            
            // 아직도 못 찾았으면 모든 캔버스 검사
            if (canvas == null && canvases.Length > 0)
            {
                canvas = canvases[0].transform;
                Debug.Log("[UIManager] 첫 번째 캔버스로 설정: " + canvas.name);
            }
        }

        // HUD 찾기 (이전 참조 제거)
        hud = null;
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

        // popup 없을 경우 자동으로 찾기 (이전 참조 제거)
        popup = null;
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

        // UIController 찾기 (이전 참조 제거)
        uiController = null;
        if (uiController == null)
        {
            uiController = FindObjectOfType<UIController>();
            if (uiController != null)
            {
                Debug.Log("[UIManager] UIController 자동 찾기 성공");
            }
            else
            {
                // 설정된 UI 요소들을 확인
                if (canvas != null)
                {
                    // Canvas의 자식에서 UIController를 찾음
                    UIController[] controllers = canvas.GetComponentsInChildren<UIController>(true);
                    if (controllers.Length > 0)
                    {
                        uiController = controllers[0];
                        Debug.Log("[UIManager] Canvas의 자식에서 UIController 찾음: " + uiController.name);
                    }
                    else
                    {
                        // LobbyUI나 UIController가 포함된 이름의 오브젝트 찾기
                        Transform[] allTransforms = FindObjectsOfType<Transform>();
                        foreach (Transform t in allTransforms)
                        {
                            if (t.name.Contains("LobbyUI") || t.name.Contains("UIController"))
                            {
                                UIController controller = t.GetComponent<UIController>();
                                if (controller != null)
                                {
                                    uiController = controller;
                                    Debug.Log("[UIManager] 이름으로 UIController 찾음: " + uiController.name);
                                    break;
                                }
                            }
                        }
                    }
                }
                
                if (uiController == null)
                {
                    Debug.LogWarning("[UIManager] UIController를 찾을 수 없습니다.");
                }
            }
        }

        // StageSelectUI 찾기 - 게임 스테이트가 스테이지 셀렉트일 때만 검색
        if (IsStageSelectState())
        {
            stageSelectUI = null;
            
            // 스테이지 선택 UI 검색
            if (GameObject.Find("StageSelectUI") != null)
            {
                stageSelectUI = GameObject.Find("StageSelectUI");
                Debug.Log("[UIManager] StageSelectUI 자동 찾기 성공: " + stageSelectUI.name);
                
                // StageSelectUIController 찾기
                stageSelectUIController = stageSelectUI.GetComponent<StageSelectUIController>();
                if (stageSelectUIController != null)
                {
                    Debug.Log("[UIManager] StageSelectUIController 자동 찾기 성공");
                }
            }
            else
            {
                Debug.LogWarning("[UIManager] StageSelectUI를 찾지 못했습니다. 필요할 때 다시 시도합니다.");
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
        
        // 이미 찾은 참조가 유효한 경우 바로 표시
        if (stageSelectUI != null && stageSelectUI.scene.isLoaded)
        {
            stageSelectUI.SetActive(true);
            if (stageSelectUIController != null)
            {
                stageSelectUIController.RefreshAllStageButtons();
            }
        }
        else
        {
            // 지연 실행으로 UI 찾기
            StartCoroutine(DelayedStageSelectUISearch());
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

    // 간결한 지연 검색 코루틴
    private IEnumerator DelayedStageSelectUISearch()
    {
        // 씬 전환이 완료될 때까지 잠시 대기
        yield return new WaitForSeconds(0.3f);
        
        if (FindStageSelectUI())
        {
            Debug.Log($"[UIManager] StageSelectUI 찾음: {stageSelectUI.name}");
            
            // StageSelectUIController 찾기
            stageSelectUIController = stageSelectUI.GetComponent<StageSelectUIController>();
            if (stageSelectUIController == null)
            {
                stageSelectUIController = stageSelectUI.GetComponentInChildren<StageSelectUIController>(true);
            }
            
            // UI 표시
            stageSelectUI.SetActive(true);
            
            // 데이터 갱신
            if (stageSelectUIController != null)
            {
                stageSelectUIController.RefreshAllStageButtons();
            }
            else
            {
                Debug.LogWarning("[UIManager] StageSelectUIController를 찾을 수 없습니다.");
            }
        }
        else
        {
            // 디버깅 정보 제공
            GameObject[] roots = SceneManager.GetActiveScene().GetRootGameObjects();
            string rootNames = string.Join(", ", roots.Select(r => r.name));
            Debug.LogError($"[UIManager] StageSelectUI를 찾을 수 없습니다. 씬({SceneManager.GetActiveScene().name})에 있는 루트 오브젝트: {rootNames}");
        }
    }
    
    // 효율적인 StageSelectUI 검색
    private bool FindStageSelectUI()
    {
        // 기존 참조 제거
        stageSelectUI = null;
        
        // 1. 직접 이름으로 찾기
        stageSelectUI = GameObject.Find("StageSelectUI");
        if (stageSelectUI != null) return true;
        
        // 2. 비활성화된 오브젝트도 검색 (Resources.FindObjectsOfTypeAll)
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        foreach (GameObject obj in allObjects)
        {
            // 씬에 속한 오브젝트인지 확인
            if (obj.scene.isLoaded && (obj.name == "StageSelectUI" || obj.name.Contains("StageSelect")))
            {
                stageSelectUI = obj;
                return true;
            }
        }
        
        // 3. 컨트롤러로 찾기
        StageSelectUIController[] controllers = FindObjectsOfType<StageSelectUIController>(true);
        if (controllers.Length > 0)
        {
            stageSelectUI = controllers[0].gameObject;
            stageSelectUIController = controllers[0];
            return true;
        }
        
        return false;
    }

    // 게임 스테이트가 스테이지 셀렉트 상태인지 확인
    private bool IsStageSelectState()
    {
        if (GameManager.Instance != null && GameManager.Instance.StateMachine != null)
        {
            // GameState가 StoryStageSelect인지 확인
            return GameManager.Instance.StateMachine.CurrentState == GameState.StoryStageSelect;
        }
        return false;
    }
}