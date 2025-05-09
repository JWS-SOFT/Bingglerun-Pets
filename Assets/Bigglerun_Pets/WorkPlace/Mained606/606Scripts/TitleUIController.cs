using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TitleUIController : MonoBehaviour
{
    [Header("로그인 UI")]
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private Button googleLoginButton;
    
    [Header("로딩 UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private Text loadingText;
    
    [Header("로그인 성공 UI")]
    [SerializeField] private GameObject loginSuccessPanel;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Text userIdText;

    private void Start()
    {
        // 버튼 이벤트 설정
        if (guestLoginButton != null)
            guestLoginButton.onClick.AddListener(OnClickGuestLogin);
        
        if (googleLoginButton != null)
            googleLoginButton.onClick.AddListener(OnClickGoogleLogin);
            
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnClickStartGame);
            
        // UI 초기화
        ShowLoginButtons(true);
        ShowLoadingUI(false);
        ShowLoginSuccessUI(false);
        
        // 이미 로그인되어 있는지 확인
        CheckExistingLogin();
    }
    
    /// <summary>
    /// 이미 로그인된 세션이 있는지 확인
    /// </summary>
    private async void CheckExistingLogin()
    {
        var firebase = FirebaseManager.Instance;
        
        // 이미 인증된 상태라면 로그인 성공 UI 표시
        if (firebase.IsAuthenticated)
        {
            Debug.Log("[TitleUI] 기존 로그인 세션 발견");
            ShowLoginSuccessUI(true);
            UpdateUserInfo();
        }
    }
    
    /// <summary>
    /// 게스트 로그인 버튼 클릭
    /// </summary>
    private async void OnClickGuestLogin()
    {
        // 로딩 UI 표시
        ShowLoginButtons(false);
        ShowLoadingUI(true, "게스트 로그인 중...");
        
        var firebase = FirebaseManager.Instance;
        bool success = await firebase.SignInAnonymouslyAsync();
        
        if (success)
        {
            ShowLoadingUI(false);
            ShowLoginSuccessUI(true);
            UpdateUserInfo();
        }
        else
        {
            // 로그인 실패 처리
            ShowLoadingUI(false);
            ShowLoginButtons(true);
            Debug.LogWarning("[TitleUI] 게스트 로그인 실패");
        }
    }
    
    /// <summary>
    /// 구글 로그인 버튼 클릭
    /// </summary>
    private async void OnClickGoogleLogin()
    {
        // 로딩 UI 표시
        ShowLoginButtons(false);
        ShowLoadingUI(true, "구글 로그인 중...");
        
        var firebase = FirebaseManager.Instance;
        bool success = await firebase.SignInWithGoogleAsync();
        
        if (success)
        {
            ShowLoadingUI(false);
            ShowLoginSuccessUI(true);
            UpdateUserInfo();
        }
        else
        {
            // 로그인 실패 처리
            ShowLoadingUI(false);
            ShowLoginButtons(true);
            Debug.LogWarning("[TitleUI] 구글 로그인 실패");
        }
    }
    
    /// <summary>
    /// 시작하기 버튼 클릭 (로그인 성공 후)
    /// </summary>
    private void OnClickStartGame()
    {
        // 씬 전환 → 로비로
        GameManager.Instance.SceneFader.LoadScene("LobbyScene");

        // 상태 전환은 씬 로딩 이후 Start 등에서 처리하거나, 여기서 즉시
        GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
    }
    
    /// <summary>
    /// 사용자 정보 업데이트
    /// </summary>
    private void UpdateUserInfo()
    {
        var firebase = FirebaseManager.Instance;
        
        if (userIdText != null)
        {
            string loginTypeStr = "";
            switch (firebase.CurrentLoginType)
            {
                case FirebaseManager.LoginType.Guest:
                    loginTypeStr = "게스트";
                    break;
                case FirebaseManager.LoginType.Google:
                    loginTypeStr = "구글";
                    break;
            }
            
            userIdText.text = $"ID: {firebase.UserId}\n로그인: {loginTypeStr}";
        }
    }
    
    /// <summary>
    /// 로그인 버튼 UI 표시/숨김
    /// </summary>
    private void ShowLoginButtons(bool show)
    {
        if (guestLoginButton != null)
            guestLoginButton.gameObject.SetActive(show);
            
        if (googleLoginButton != null)
            googleLoginButton.gameObject.SetActive(show);
    }
    
    /// <summary>
    /// 로딩 UI 표시/숨김
    /// </summary>
    private void ShowLoadingUI(bool show, string message = "로딩중...")
    {
        if (loadingPanel != null)
            loadingPanel.SetActive(show);
            
        if (loadingText != null)
            loadingText.text = message;
    }
    
    /// <summary>
    /// 로그인 성공 UI 표시/숨김
    /// </summary>
    private void ShowLoginSuccessUI(bool show)
    {
        if (loginSuccessPanel != null)
            loginSuccessPanel.SetActive(show);
    }
}