using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    public int stageLevel = 1;
    [SerializeField] private int stairBaseCount = 20;
    [SerializeField] private int baseDistance = 50;
    public static int GetStageStair()
    {
        return ((Instance.stageLevel * 10) + Instance.stairBaseCount);
    }
    public static int GetStageDistance()
    {
        return ((Instance.stageLevel * 50) + Instance.baseDistance);
    }
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
    public bool isBattleMode = false;

    public static bool IsSettingPlayer()
    {
        return Player_Transform != null;
    }

    [Header ("구름설정")]
    [SerializeField] private GameObject[] cloudPrefabs;
    [SerializeField] private GameObject cloudSpawnPosition;
    [SerializeField] private float cloudSpeed = 2f;
    [SerializeField] private float spawnDistance = 5f;
    [SerializeField] private float cloudSpacingY = 3f;
    [SerializeField] private int maxCloudsOnScreen = 10;

    private float highestCloudY = 0f;
    private List<GameObject> activeClouds = new List<GameObject>();
    private Queue<GameObject> cloudPool = new Queue<GameObject>();

    [Header ("구름설정2")]
    //05.15 HJ 추가
    private PlayerController playerController;
    public PlayerController PlayerController => playerController;
    [SerializeField] private int maxLife = 3;
    private int currentLife = 0;

    [SerializeField] private int maxSkillCount = 3;
    private int currentSkillCount = 0;
    public int CurrentSkillCount
    {
        get => currentSkillCount;
        private set => currentSkillCount = value;
    }

    private bool isInvincible = false;
    public TerrainScrollManager TerrainScrollManager => terrainScrollManager;

    public event Action OnTakeDamage;

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
        playerData = PlayerDataManager.Instance.CurrentPlayerData;
        SetPlayMode(false);
        //playerData = PlayerData.CreateDefault("hamster");
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

        //05.20 HJ 추가
        //playerController = Player_Transform?.GetComponent<PlayerController>();
        playerController = FindFirstObjectByType<PlayerController>();

        // InitializeCloud();
        InitializeLife();
        InitializeSkillCount();
        if (GameManager.Instance.StateMachine.CurrentState == GameState.CompetitionInGame)
        {
            ItemManager.Instance.UseSelectedPreGameItem();
        }

        ApplySelectedCharacterToPlayer();
    }

    private void Update()
    {
        //if (Player_Transform != null && playerController == null) playerController = Player_Transform.GetComponent<PlayerController>();
        if (/*!PlayMode &&*/ actionTimer.IsRunning)
        {
            timerSlider.maxValue = timeAction;
            timerSlider.value = actionTimer.RemainingTime;
            timerText.text = actionTimer.RemainingTime.ToString("N1");
        }
        if (PlayMode && !actionTimer.IsRunning)
        {
            isGameStartReady = true;
        }

        //MoveClouds();
        //CheckAndSpawnNewClouds();
        //RecycleClouds();
    }

    public void SetPlayMode(bool mode)
    {
        if (mode)
        {
            actionButton[1].gameObject.SetActive(false);
            actionButton[2].gameObject.SetActive(true);
            ActionTImeStop();
            actionTimer = isBattleMode ? new BasicTimer(1f) : new BasicTimer(prepareTimeInterval);
            ActionTImeStart();
            SetTerrain(stairManager.GetStairObject(currentPlayerFloor).transform.position);
            stairManager.ResetStiar();
            currentPlayerFloor = 0;
        }
        else
        {
            actionButton[1].gameObject.SetActive(true);
            actionButton[2].gameObject.SetActive(false);
            terrainScrollManager.RestTerrain();
            isGameStartReady = false;
            actionTimer = new BasicTimer(timeAction);
            stairManager.StartStairs();
        }
        PlayMode = mode;
    }

    public void SetTerrain(Vector3 position)
    {
        terrainScrollManager.StartTest(position);
    }

    private void InitializeCloud()
    {
        highestCloudY = cloudSpawnPosition.transform.position.y;

        // 최초 구름 생성
        for (int i = 0; i < maxCloudsOnScreen; i++)
        {
            SpawnCloud(highestCloudY + i * cloudSpacingY);
        }
    }

    public static void ChangeFloor(int floor)
    {
        Instance.currentPlayerFloor = floor;
        Instance.floorText.text = GetStageStair() == Instance.currentPlayerFloor + 1 ?
            "Floor\nMax Floor; : " : "Floor\n" + Instance.currentPlayerFloor;
        ActionTImeStart();
        ScoreManager.Instance.AddStep(); // 추가
    }

    public static void ChangeDistance(float distance)
    {
        Instance.currentPlayerDistance = distance;
        Instance.floorText.text = "Distance\n" + Instance.currentPlayerDistance.ToString("N1") + "m";
        Instance.timerText.text = Instance.currentPlayerDistance.ToString("N0") + "m";
        Instance.timerSlider.maxValue = GetStageDistance();
        Instance.timerSlider.value = Instance.currentPlayerDistance;

        float movedDistance = 5f * Time.deltaTime; // 추가
        if (ScoreManager.Instance!=null) ScoreManager.Instance.AddHorizontalDistance(movedDistance); // 추가
        if (GetStageDistance() <= Instance.currentPlayerDistance)
        {
            if (!Instance.isBattleMode)
            {
                Instance.playerController.GameStageClear();
            }
            else
            {
                Instance.stageLevel++;
                Instance.SetPlayMode(false);
            }
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
        if (Instance.CurrentSkillCount <= 0) return;

        Instance.skillManager.ActivateSkill(Instance.playerData.currentCharacter);

        Instance.CurrentSkillCount--;
        Debug.Log($"{Instance.playerData.currentCharacter} 현재 스킬 횟수: {Instance.CurrentSkillCount}");
    }

   private void MoveClouds()
    {
        foreach (GameObject cloud in activeClouds)
        {
            if (cloud.activeInHierarchy)
            {
                cloud.transform.Translate(Vector3.right * cloudSpeed * Time.deltaTime);

                // 반복 이동 (왼쪽 -> 오른쪽 무한 루프)
                if (cloud.transform.position.x > 20f) // 임의의 우측 경계
                {
                    cloud.transform.position = new Vector3(-20f, cloud.transform.position.y, cloud.transform.position.z);
                }
            }
        }
    }

    private void CheckAndSpawnNewClouds()
    {
        float playerY = Player_Transform.position.y;

        if (playerY + spawnDistance > highestCloudY)
        {
            highestCloudY += cloudSpacingY;
            SpawnCloud(highestCloudY);
        }
    }

    private void SpawnCloud(float y)
    {
        GameObject prefab = cloudPrefabs[UnityEngine.Random.Range(0, cloudPrefabs.Length)];

        GameObject cloud;
        if (cloudPool.Count > 0)
        {
            cloud = cloudPool.Dequeue();
            cloud.SetActive(true);
        }
        else
        {
            cloud = Instantiate(prefab);
        }

        float x = UnityEngine.Random.Range(-10f, 10f);
        cloud.transform.position = new Vector3(x, y, 0f);
        activeClouds.Add(cloud);
    }

    private void RecycleClouds()
    {
        for (int i = activeClouds.Count - 1; i >= 0; i--)
        {
            if (activeClouds[i].transform.position.y < Player_Transform.position.y - 10f)
            {
                GameObject cloud = activeClouds[i];
                cloud.SetActive(false);
                cloudPool.Enqueue(cloud);
                activeClouds.RemoveAt(i);
            }
        }
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
        if (playerController.IsRecovering) return;

        if (isInvincible)   //무적 상태
        {
            if (playerController.IsRecovering) return;

            if (PlayerManager.PlayMode)                     //런모드
                playerController.RecoverToForwardGround();  //앞에 있는 안전한 땅으로 복귀
            else                                            //계단모드
                playerController.RecoverToLastStair();      //이전 계단으로 복귀

            return;
        }

        // 이벤트 호출
        OnTakeDamage?.Invoke();
        currentLife--;
        
        PlayerController.Player_Animator.SetTrigger("Damaged");

        // 별 감소도 함께 처리
        if (ScoreManager.Instance!=null ) ScoreManager.Instance.DecreaseStar();
        Debug.Log($"현재 생명력: {currentLife}");

        if(currentLife <= 0)
        {
            playerController.TriggerGameOver(); //게임 오버
            //
        }
        else
        {
            if (PlayerManager.PlayMode)                     //런모드
            {
                playerController.RecoverToForwardGround();  //앞에 있는 안전한 땅으로 복귀
                SetInvincible(1f);
            }
            else                                            //계단모드
            {
                playerController.RecoverToLastStair();      //이전 계단으로 복귀
                SetInvincible(0.5f);
            }

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

    //최대 목숨
    public int GetMaxLife()
    {
        return maxLife;
    }
    
    //현재 목숨
    public int GetCurrentLife()
    {
        return currentLife;
    }

    //스킬 횟수 추가
    public void AddSkillCount(int amount = 1)
    {
        currentSkillCount += amount;
        Debug.Log($"스타트 스킬 횟수: {currentSkillCount}");
    }

    public void SetPoisition()
    {
        playerController.PlayerPosition = playerController.transform.position;
    }

    //06.05 HJ 추가
    public CharacterData GetSelectedCharacterData()
    {
        string currentCharacterId = PlayerDataManager.Instance.CurrentPlayerData.currentCharacter;
        return ItemManager.AllCharacterItems.FirstOrDefault(c => c.characterId == currentCharacterId);
    }

    public void ApplySelectedCharacterToPlayer()
    {
        Debug.Log("!!!!!!!!!!!!!!!!!!!!! " + Player_Transform.name + " !!!!!!!!!!!!!!!!!!!!!");

        CharacterData selectedCharacter = GetSelectedCharacterData();

        if (selectedCharacter == null) return;

        foreach(Transform child in Player_Transform)
        {
            if(child.gameObject.name == selectedCharacter.characterName)
            {
                child.gameObject.SetActive(true);
                playerController.Player_Animator = child.gameObject.GetComponent<Animator>();
                Debug.Log("!!!!!!!!!!!!!!!!!!!!! " + child.gameObject.name + " !!!!!!!!!!!!!!!!!!!!!");
            }
        }
    }
}
