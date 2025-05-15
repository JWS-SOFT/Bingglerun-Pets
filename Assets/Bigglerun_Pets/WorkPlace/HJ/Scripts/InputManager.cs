using UnityEngine;

public class InputManager : MonoBehaviour
{
    public enum ScreenMode { Portrait, Landscape }   //세로, 가로
    public ScreenMode CurrentScreenMode { get; private set; }
    
    //영역 감지(누르고 있는 동안 입력 이펙트 등에 적용)
    public bool InputZone1 { get; private set; }    //세로: 하단 / 가로: 왼쪽
    public bool InputZone2 { get; private set; }    //세로: 상단 / 가로: 오른쪽

    //입력(입력 딜레이 반영)
    public bool IsZone1Pressed { get; private set; }
    public bool IsZone2Pressed { get; private set; }

    [SerializeField] private float zone1Cooldown = 0.2f;
    [SerializeField] private float zone2Cooldown = 0.2f;
    private float zone1Timer = 0f;
    private float zone2Timer = 0f;


    #region Singleton
    public static InputManager Instance;

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
    #endregion

    private void Update()
    {
        // 초기화
        InputZone1 = false;
        InputZone2 = false;
        IsZone1Pressed = false;
        IsZone2Pressed = false;

        zone1Timer -= Time.deltaTime;
        zone2Timer -= Time.deltaTime;

        // 게임 상태가 InGame이 아니면 입력 처리를 하지 않음
        if (GameManager.Instance == null || 
            GameManager.Instance.StateMachine == null || 
            GameManager.Instance.StateMachine.CurrentState != GameState.InGame)
        {
            return;
        }

        bool touchedZone1 = false;
        bool touchedZone2 = false;

#if UNITY_EDITOR
        //마우스 테스트
        if (Input.GetMouseButton(0))
        {
            Vector2 mousePos = Input.mousePosition;
            CurrentScreenMode = Screen.width > Screen.height ? ScreenMode.Landscape : ScreenMode.Portrait;

            if (CurrentScreenMode == ScreenMode.Landscape)
            {
                touchedZone1 = mousePos.x <= Screen.width / 2f;
                touchedZone2 = mousePos.x > Screen.width / 2f;
            }
            else
            {
                touchedZone1 = mousePos.y <= Screen.height / 3f;
                touchedZone2 = mousePos.y > Screen.height / 3f;
            }
        }
#else
        if (Input.touchCount > 0)
        {
            Vector2 touchPos = Input.GetTouch(0).position;
            CurrentScreenMode = Screen.width > Screen.height ? ScreenMode.Landscape : ScreenMode.Portrait;

            if (CurrentScreenMode == ScreenMode.Landscape)
            {
                touchedZone1 = touchPos.x <= Screen.width / 2f;
                touchedZone2 = touchPos.x > Screen.width / 2f;
            }
            else // Portrait
            {
                touchedZone1 = touchPos.y <= Screen.height / 3f;
                touchedZone2 = touchPos.y > Screen.height / 3f;
            }
        }
#endif

        if (touchedZone1)
        {
            InputZone1 = true;
            if (zone1Timer <= 0f)
            {
                IsZone1Pressed = true;
                zone1Timer = zone1Cooldown;
                Debug.Log("Zone1 눌림");
            }
            else
            {
                Debug.Log("Zone1 쿨다운 중");
            }
        }

        if (touchedZone2)
        {
            InputZone2 = true;
            if (zone2Timer <= 0f)
            {
                IsZone2Pressed = true;
                zone2Timer = zone2Cooldown;
                Debug.Log("Zone2 눌림");
            }
            else
            {
                Debug.Log("Zone2 쿨다운 중");
            }
        }
    }
}
