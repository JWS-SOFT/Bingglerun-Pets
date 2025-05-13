using System.Collections.Generic;
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
    [SerializeField] private List<Transform> popupGroup = new List<Transform>();
    [SerializeField] private string currentOpenedPopup = "";

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

    public void ShowTitleUI()
    {
        Debug.Log("타이틀 UI 표시");
        // 로그인 버튼 → GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
    }

    public void TogglePopupUI(string uiName)
    {
        Transform target = null;
        target = FindDirectChildByName(uiName);
        

        if (target != null)
        {
            target.gameObject.SetActive(!target.gameObject.activeSelf);
        }
    }

    private Transform FindDirectChildByName(string uiName)
    {
        foreach (Transform target in popupGroup)
        {
            if (target.name == uiName)
                return target;
        }
        Debug.Log($"{uiName} 라는 이름을 가진 UI가 존재하지 않습니다.");
        return null;
    }

    public void ExitPopup()
    {

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