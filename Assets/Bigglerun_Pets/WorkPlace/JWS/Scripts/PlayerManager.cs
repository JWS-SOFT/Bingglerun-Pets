using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager Instance;
    public static Transform Player_Transform;
    public float timeAction = 1f;  // 리미티드 시간설정 
    public float timeBonus = 0.25f;  // 보너스 시간설정 
    public float prepareTimeInterval = 3f; // 횡게임 대기시간.
    public static bool PlayMode
    {
        get { return Instance.play_Mode; }
        set { Instance.play_Mode = value; }
    }
    [SerializeField] private bool play_Mode = false;
    [SerializeField] private SkillManager skillManager;
    [SerializeField] private StairManager stairManager;
    [SerializeField] private int stageLevel = 1;
    [SerializeField] private int stairBaseCount = 20;
    [SerializeField] private int baseDistance = 50;
    public static int GetStageStair;
    public static int GetStageDistance;
    [SerializeField] private TerrainScrollManager terrainScrollManager;
    [SerializeField] private Button[] actionButton = new Button[3];
    private PlayerData playerData;
    private int currentPlayerCoin = 0;
    public int currentPlayerFloor = 0;
    public float currentPlayerDistance = 0;
    private BasicTimer actionTimer;

    [SerializeField] private TextMeshProUGUI floorText, coinText, timerText;
    [SerializeField] private Slider timerSlider;

    public bool isGameStartReady = false;

    public static bool IsSettingPlayer()
    {
        return Player_Transform != null;
    }

    //05.15 HJ 추가
    private PlayerController playerController;

    [SerializeField] private int maxLife = 3;
    private int currentLife = 0;

    [SerializeField] private int maxSkillCount = 3;
    private int currentSkillCount = 0;

    private bool isInvincible = false;


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
        GetStageStair = ((stageLevel * 10) + stairBaseCount);
        GetStageDistance = ((stageLevel * 10) + baseDistance);
    }

    private void Start()
    {
        SetPlayMode(false);
        //playerData = PlayerDataManager.Instance.CurrentPlayerData;
        playerData = PlayerData.CreateDefault("cat");
        if (!PlayMode)
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
        if (Player_Transform != null && playerController == null) playerController = Player_Transform.GetComponent<PlayerController>();
        if (/*!PlayMode &&*/ actionTimer.IsRunning)
        {
            timerSlider.value = actionTimer.RemainingTime;
            timerText.text = actionTimer.RemainingTime.ToString("N1");
        }
        if (PlayMode && !actionTimer.IsRunning)
        {
            isGameStartReady = true;
        }
    }

    public void SetPlayMode(bool mode)
    {
        if (mode)
        {
            // stairManager.enabled = false;
            // terrainScrollManager.enabled = true;
            actionButton[1].gameObject.SetActive(false);
            actionButton[2].gameObject.SetActive(true);
            ActionTImeStop();
            actionTimer = new BasicTimer(prepareTimeInterval);
            ActionTImeStart();
            stairManager.ResetStiar();
            SetTerrain(stairManager.GetStairObject(currentPlayerFloor).transform.position);
            stairManager.gameObject.SetActive(false);
        }
        else
        {
            // stairManager.enabled = true;
            // terrainScrollManager.enabled = false;
            actionButton[1].gameObject.SetActive(true);
            actionButton[2].gameObject.SetActive(false);
            isGameStartReady = false;
        }
        PlayMode = mode;
    }

    public void SetTerrain(Vector3 position)
    {
        terrainScrollManager.StartTest(position);
    }



    public static void ChangeFloor(int floor)
    {
        if (GetStageStair != ((Instance.stageLevel * 10) + Instance.stairBaseCount)) GetStageStair = ((Instance.stageLevel * 10) + Instance.stairBaseCount);
        Instance.currentPlayerFloor = floor;
        Instance.floorText.text = GetStageStair == Instance.currentPlayerFloor + 1 ?
            "Floor\nMax Floor; : " : "Floor\n" + Instance.currentPlayerFloor;
        ActionTImeStart();
        ScoreManager.Instance.AddStep(); // 추가
    }

    public static void ChangeDistance(float distance)
    {
        if (GetStageDistance != ((Instance.stageLevel * 50) + Instance.baseDistance)) GetStageDistance = ((Instance.stageLevel * 50) + Instance.baseDistance);
        Instance.currentPlayerDistance = distance;
        Instance.floorText.text = "Distance\n" + Instance.currentPlayerDistance.ToString("N1") + "m";
        Instance.timerText.text = Instance.currentPlayerDistance.ToString("N1") + "m";
        float movedDistance = 5f * Time.deltaTime; // 추가
        // ScoreManager.Instance.AddHorizontalDistance(movedDistance); // 추가
        if (GetStageDistance <= Instance.currentPlayerDistance)
        {
              Instance.playerController.GameStageClear();
        }
    }

    public static void ChangeCoin()
    {
        Instance.currentPlayerCoin++;
        Instance.coinText.text = "Coin\n" + Instance.currentPlayerCoin;
        ScoreManager.Instance.AddCoin(); // 추가
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

    public static void ActiveSkill()
    {
        Instance.skillManager.ActivateSkill(Instance.playerData.playerId);
    }




    //05.17 HJ 추가 부분
    //목숨 초기화(게임 스타트시 호출)
    public void InitializeLife()
    {
        currentLife = maxLife;
    }

    //스킬 사용횟수 초기화(게임 스타트시 호출)
    public void InitializeSkillCount()
    {
        currentSkillCount = maxSkillCount;
    }

    //데미지
    public void TakeDamage()
    {
        if (isInvincible)   //무적 상태
        {
            if (playerController.IsRecovering) return;

            if (PlayerManager.PlayMode)                     //런모드
                playerController.RecoverToForwardGround();  //앞에 있는 안전한 땅으로 복귀
            else                                            //계단모드
                playerController.RecoverToLastStair();      //이전 계단으로 복귀

            return;
        }

        currentLife--;
        Debug.Log($"현재 생명력: {currentLife}");

        if(currentLife <= 0)
        {
            playerController.TriggerGameOver(); //게임 오버
        }
        else
        {
            if (PlayerManager.PlayMode)                     //런모드
                playerController.RecoverToForwardGround();  //앞에 있는 안전한 땅으로 복귀
            else                                            //계단모드
                playerController.RecoverToLastStair();      //이전 계단으로 복귀

            SetInvincible(0.5f);    //0.5초 동안 무적
        }
    }

    //무적
    public void SetInvincible(float duration)
    {
        //if (isInvincible) return;
        StartCoroutine(InvincibleCoroutine(duration));
    }

    private IEnumerator InvincibleCoroutine(float duration)
    {
        isInvincible = true;

        Debug.Log($"무적 시작 {duration}초");
        yield return new WaitForSeconds(duration);

        isInvincible = false;
        Debug.Log($"무적 종료");
    }

    //부스터
    public void StartBooster(float duration)
    {
        StartCoroutine(BoosterCoroutine(duration));
    }

    private IEnumerator BoosterCoroutine(float duration)
    {
        float elapsed = 0f;

        Debug.Log($"부스터 자동 점프 시작 {duration}초");

        while (elapsed < duration)
        {
            playerController.AlignDirectionToNextStair();   //점프 방향 자동 보정
            playerController.JumpButtonClick();             //점프

            yield return new WaitForSeconds(0.05f);         //템포 조정
            elapsed += 0.05f;
        }

        Debug.Log("부스터 자동 점프 종료");
    }

    //목숨 추가
    public void AddLife(int amount = 1)
    {
        currentLife += amount;
        Debug.Log($"스타트 목숨: {currentLife}");
    }

    //스킬 횟수 추가
    public void AddSkillCount(int amount = 1)
    {
        currentSkillCount += amount;
        Debug.Log($"스타트 스킬 횟수: {currentSkillCount}");
    }
}
