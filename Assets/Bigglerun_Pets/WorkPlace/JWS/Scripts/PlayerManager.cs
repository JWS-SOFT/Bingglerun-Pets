using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public static Transform Player_Transform;
    private int currentPlayerCoin = 0;
    private int currentPlayerFloor = 0;
    [SerializeField] private TextMeshProUGUI floorText, coinText;

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
    }

    private void Start()
    {
        floorText.text = "Floor\n" + currentPlayerFloor;
        coinText.text = "Coin\n" + currentPlayerCoin;
    }

    private void Update()
    {
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
}
