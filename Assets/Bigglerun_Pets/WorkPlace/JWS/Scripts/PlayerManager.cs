using System.Xml.Serialization;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public static Transform Player_Transform;
    public float timeAction = 10f;  // 리미티드 시간설정 
    public float timeBonus = 0.25f;  // 보너스 시간설정 
    public static bool PlayMode
    {
        get { return Instance.play_Mode; }
        set { Instance.play_Mode = value; }
    }
    [SerializeField] private bool play_Mode = true;
    [SerializeField] private StairManager StairManager;
    [SerializeField] private TerrainScrollManager terrainScrollManager;
    private int currentPlayerCoin = 0;
    private int currentPlayerFloor = 0;
    private float currentPlayerDistance = 0;
    private BasicTimer actionTimer;

    [SerializeField] private TextMeshProUGUI floorText, coinText, timerText;
    [SerializeField] private Slider timerSlider;

    public static bool IsSettingPlayer()
    {
        return Player_Transform != null;
    }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        actionTimer = new BasicTimer(timeAction);

        if (play_Mode)
        {
            StairManager.enabled = false;
            terrainScrollManager.enabled = true;
        }
        else
        {
            StairManager.enabled = true;
            terrainScrollManager.enabled = false;
        }
    }

    private void Start()
    {
        if (!play_Mode)
        {
            floorText.text = "Floor\n" + currentPlayerFloor;
            coinText.text = "Coin\n" + currentPlayerCoin;
            timerSlider.maxValue = timeAction;
            timerSlider.value = actionTimer.RemainingTime;
            timerText.text = actionTimer.RemainingTime.ToString("N1");
        }
        else
        {
            floorText.text = "";
            coinText.text = "Coin\n" + currentPlayerCoin;
            timerText.text = "";
        }
    }

    private void Update()
    {
        if (!play_Mode && actionTimer.IsRunning)
        {
            timerSlider.value = actionTimer.RemainingTime;
            timerText.text = actionTimer.RemainingTime.ToString("N1");
        }
    }

    public static void ChangeFloor(int floor)
    {
        Instance.currentPlayerFloor = floor;
        Instance.floorText.text = "Floor\n" + Instance.currentPlayerFloor;
    }

    public static void ChangeDistance(float distance)
    {
        Instance.currentPlayerDistance = distance;
        Instance.floorText.text = "Distance\n" + Instance.currentPlayerDistance.ToString("N1") + "m";
        Instance.timerText.text = Instance.currentPlayerDistance.ToString("N1") + "m";
    }

    public static void ChangeCoin()
    {
        Instance.currentPlayerCoin++;
        Instance.coinText.text = "Coin\n" + Instance.currentPlayerCoin;
    }

    public static bool ActionTImerCheck()
    {
        return Instance.actionTimer.IsRunning;
    }

    public static void ActionTImeStart()
    {
        TimerManager.Instance.StopTimer(Instance.actionTimer);
        TimerManager.Instance.StartTimer(Instance.actionTimer);
    }

    public static void ActionTImeSuccess()
    {
        Instance.actionTimer.AddTime(-Instance.timeBonus);
    }

    public static void ActionTImeStop()
    {
        TimerManager.Instance.StopTimer(Instance.actionTimer);
    }
}
