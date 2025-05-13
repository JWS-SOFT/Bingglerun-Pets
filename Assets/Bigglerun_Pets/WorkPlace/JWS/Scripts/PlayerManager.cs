using System.Xml.Serialization;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public static Transform Player_Transform;
    public float timeAction = 2f;

    private int currentPlayerCoin = 0;
    private int currentPlayerFloor = 0;
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
    }

    private void Start()
    {
        floorText.text = "Floor\n" + currentPlayerFloor;
        coinText.text = "Coin\n" + currentPlayerCoin;
        timerSlider.maxValue = timeAction;
        timerSlider.value = actionTimer.RemainingTime;
        timerText.text = actionTimer.RemainingTime.ToString("N1");
    }

    private void Update()
    {
        if (actionTimer.IsRunning)
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
}
