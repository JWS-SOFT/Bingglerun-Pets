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
        canvas = FindAnyObjectByType<Canvas>().transform;
        if(canvas != null)
        {
            hud = canvas.transform.GetChild(0);
            popup = canvas.transform.GetChild(1);
        }

        fader = FindAnyObjectByType<SceneFader>();
    }

    public void SceneChange()
    {
        canvas = FindAnyObjectByType<Canvas>().transform;
        if (canvas != null)
        {
            hud = canvas.transform.GetChild(0);
            popup = canvas.transform.GetChild(1);
        }
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