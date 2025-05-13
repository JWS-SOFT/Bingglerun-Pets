using UnityEngine;

/// <summary>
/// UI 제어를 담당하는 매니저 (로비, 타이틀 등 상태별로 처리)
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    [SerializeField] private Transform canvas;
    [SerializeField] public Transform hud;
    [SerializeField] public Transform popup;

    [SerializeField] private SceneFader fader;

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
        FindAndBindCanvas();
        fader = FindAnyObjectByType<SceneFader>();
    }

    /// <summary>
    /// 게임 상태 변경 시 호출되는 메서드
    /// </summary>
    public void HandleGameStateChange(GameState state)
    {
        // 캔버스와 UI 요소들을 찾아서 바인딩
        FindAndBindCanvas();
        
        // 상태에 따른 UI 표시
        switch (state)
        {
            case GameState.Title:
                ShowTitleUI();
                break;
                
            case GameState.Lobby:
                ShowLobbyUI();
                break;
                
            case GameState.ModeSelect:
                ShowModeSelectUI();
                break;
                
            case GameState.StoryStageSelect:
                ShowStoryStageSelectUI();
                break;
                
            case GameState.CompetitiveSetup:
                ShowCompetitiveSetupUI();
                break;
                
            case GameState.Result:
                ShowResultUI();
                break;
                
            case GameState.Pause:
                ShowPauseMenu();
                break;
                
            case GameState.InGame:
                HideAll();
                break;
        }
    }

    // 캔버스와 UI 요소들을 찾아서 바인딩
    private void FindAndBindCanvas()
    {
        canvas = FindAnyObjectByType<Canvas>()?.transform;
        if (canvas != null)
        {
            Debug.Log($"[UIManager] 캔버스 찾음: {canvas.name}");
            
            // 캔버스 하위에 HUD와 Popup이 있는지 확인
            if (canvas.childCount >= 2)
            {
                hud = canvas.GetChild(0);
                popup = canvas.GetChild(1);
                Debug.Log($"[UIManager] UI 바인딩 완료 - HUD: {hud.name}, Popup: {popup.name}");
            }
            else
            {
                Debug.LogWarning("[UIManager] 캔버스에 필요한 하위 UI 요소가 없습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[UIManager] 씬에 캔버스가 없습니다.");
        }
    }

    public void SceneChange()
    {
        // 캔버스 바인딩만 실행
        FindAndBindCanvas();
    }

    public void ShowTitleUI()
    {
        Debug.Log("타이틀 UI 표시");
        // 로그인 버튼 → GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
    }

    public void TogglePopupUI(string uiName)
    {
        Transform ui = FindDirectChildByName(uiName);

        if(ui != null)
        {
            ui.gameObject.SetActive(!ui.gameObject.activeSelf);
        }
    }

    private Transform FindDirectChildByName(string uiName)
    {
        foreach (Transform child in popup)
        {
            if (child.name == uiName)
                return child;
        }
        Debug.Log($"{uiName} 라는 이름을 가진 UI가 존재하지 않습니다.");
        return null;
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