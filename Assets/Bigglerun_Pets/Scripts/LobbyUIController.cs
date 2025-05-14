using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 로비 UI 컨트롤러
/// </summary>
public class LobbyUIController : MonoBehaviour
{
    [Header("플레이어 정보 UI")]
    [SerializeField] private Text nicknameText;
    [SerializeField] private Text levelText;
    [SerializeField] private Text goldText;
    [SerializeField] private Text diamondText;
    [SerializeField] private Text starsText;
    
    [Header("메뉴 버튼")]
    [SerializeField] private Button storyModeButton;
    [SerializeField] private Button competitiveModeButton;
    [SerializeField] private Button shopButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private Button inventoryButton;
    [SerializeField] private Button profileButton;
    
    private void Start()
    {
        // UI 초기화
        InitializeUI();
        
        // PlayerDataManager가 데이터를 로드했는지 확인
        if (PlayerDataManager.Instance != null)
        {
            if (PlayerDataManager.Instance.IsDataLoaded)
            {
                UpdateUI();
            }
            else
            {
                // 데이터가 아직 로드되지 않았으면 이벤트 구독
                PlayerDataManager.Instance.OnDataLoaded += UpdateUI;
            }
        }
        else
        {
            Debug.LogError("[LobbyUIController] PlayerDataManager가 없습니다.");
        }
    }
    
    /// <summary>
    /// UI 이벤트 초기화
    /// </summary>
    private void InitializeUI()
    {
        if (storyModeButton != null)
            storyModeButton.onClick.AddListener(OnClickStoryMode);
            
        if (competitiveModeButton != null)
            competitiveModeButton.onClick.AddListener(OnClickCompetitiveMode);
            
        if (shopButton != null)
            shopButton.onClick.AddListener(OnClickShop);
            
        if (settingsButton != null)
            settingsButton.onClick.AddListener(OnClickSettings);
            
        if (inventoryButton != null)
            inventoryButton.onClick.AddListener(OnClickInventory);
            
        if (profileButton != null)
            profileButton.onClick.AddListener(OnClickProfile);
    }
    
    /// <summary>
    /// 플레이어 데이터를 UI에 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (PlayerDataManager.Instance == null || !PlayerDataManager.Instance.IsDataLoaded)
            return;
            
        PlayerData data = PlayerDataManager.Instance.CurrentPlayerData;
        
        if (nicknameText != null)
            nicknameText.text = data.nickname;
            
        if (levelText != null)
            levelText.text = $"Lv. {data.level}";
            
        if (goldText != null)
            goldText.text = data.gold.ToString("N0");
            
        if (diamondText != null)
            diamondText.text = data.diamond.ToString("N0");
            
        if (starsText != null)
            starsText.text = data.totalStars.ToString();
            
        // 이벤트 구독
        PlayerDataManager.Instance.OnGoldChanged += OnGoldChanged;
        PlayerDataManager.Instance.OnDiamondChanged += OnDiamondChanged;
        PlayerDataManager.Instance.OnLevelChanged += OnLevelChanged;
        PlayerDataManager.Instance.OnTotalStarsChanged += OnStarsChanged;
        PlayerDataManager.Instance.OnNicknameChanged += OnNicknameChanged;
    }
    
    /// <summary>
    /// UI 업데이트 이벤트 핸들러
    /// </summary>
    private void OnGoldChanged(int gold)
    {
        if (goldText != null)
            goldText.text = gold.ToString("N0");
    }
    
    private void OnDiamondChanged(int diamond)
    {
        if (diamondText != null)
            diamondText.text = diamond.ToString("N0");
    }
    
    private void OnLevelChanged(int level)
    {
        if (levelText != null)
            levelText.text = $"Lv. {level}";
    }
    
    private void OnStarsChanged(int stars)
    {
        if (starsText != null)
            starsText.text = stars.ToString();
    }
    
    private void OnNicknameChanged(string nickname)
    {
        if (nicknameText != null)
            nicknameText.text = nickname;
    }
    
    /// <summary>
    /// 버튼 클릭 이벤트 핸들러
    /// </summary>
    private void OnClickStoryMode()
    {
        GameManager.Instance.StateMachine.ChangeState(GameState.StoryStageSelect);
    }
    
    private void OnClickCompetitiveMode()
    {
        GameManager.Instance.StateMachine.ChangeState(GameState.CompetitiveSetup);
    }
    
    private void OnClickShop()
    {
        UIManager.Instance.TogglePopupUI("ShopUI");
    }
    
    private void OnClickSettings()
    {
        UIManager.Instance.TogglePopupUI("SettingsUI");
    }
    
    private void OnClickInventory()
    {
        UIManager.Instance.TogglePopupUI("InventoryUI");
    }
    
    private void OnClickProfile()
    {
        UIManager.Instance.TogglePopupUI("ProfileUI");
    }
    
    private void OnDestroy()
    {
        // 이벤트 해제
        if (PlayerDataManager.Instance != null)
        {
            PlayerDataManager.Instance.OnDataLoaded -= UpdateUI;
            PlayerDataManager.Instance.OnGoldChanged -= OnGoldChanged;
            PlayerDataManager.Instance.OnDiamondChanged -= OnDiamondChanged;
            PlayerDataManager.Instance.OnLevelChanged -= OnLevelChanged;
            PlayerDataManager.Instance.OnTotalStarsChanged -= OnStarsChanged;
            PlayerDataManager.Instance.OnNicknameChanged -= OnNicknameChanged;
        }
    }
} 