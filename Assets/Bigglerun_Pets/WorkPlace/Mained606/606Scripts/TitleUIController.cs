using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

public class TitleUIController : MonoBehaviour
{
    [Header("로그인 UI")]
    public Button guestLoginButton;
    public Button googleLoginButton;
    public Button emailLoginButton; // 이메일 로그인 버튼
    
    [Header("로딩 UI")]
    public GameObject loadingPanel;
    public Text loadingText;
    
    [Header("로그인 성공 UI")]
    public GameObject loginSuccessPanel;
    public Button startGameButton;
    public Text userIdText;
    public Button deleteAccountButton;
    
    [Header("이메일 로그인 UI")]
    public GameObject emailLoginPanel;
    public InputField emailInputField;
    public InputField passwordInputField;
    public Button loginButton;
    public Button backToMainButton;
    public Button goToRegisterButton;
    public Button forgotPasswordButton;
    public Text errorMessageText;
    
    [Header("이메일 회원가입 UI")]
    public GameObject emailRegisterPanel;
    public InputField registerEmailInputField;
    public InputField registerPasswordInputField;
    public InputField confirmPasswordInputField;
    public Button registerButton;
    public Button backToLoginButton;
    public Text registerErrorMessageText;
    
    [Header("비밀번호 재설정 UI")]
    public GameObject resetPasswordPanel;
    public InputField resetEmailInputField;
    public Button sendResetEmailButton;
    public Button backToLoginFromResetButton;
    public Text resetErrorMessageText;

    private void Start()
    {
        // 버튼 이벤트 설정
        if (guestLoginButton != null)
            guestLoginButton.onClick.AddListener(OnClickGuestLogin);
        
        if (googleLoginButton != null)
            googleLoginButton.onClick.AddListener(OnClickGoogleLogin);
            
        if (emailLoginButton != null)
            emailLoginButton.onClick.AddListener(OnClickOpenEmailLogin);
            
        if (startGameButton != null)
            startGameButton.onClick.AddListener(OnClickStartGame);
            
        // 계정 삭제 버튼 이벤트 설정
        if (deleteAccountButton != null)
            deleteAccountButton.onClick.AddListener(OnClickDeleteAccount);
            
        // 이메일 로그인 패널 버튼 이벤트
        if (loginButton != null)
            loginButton.onClick.AddListener(OnClickEmailLogin);
            
        if (backToMainButton != null)
            backToMainButton.onClick.AddListener(OnClickBackToMain);
            
        if (goToRegisterButton != null)
            goToRegisterButton.onClick.AddListener(OnClickGoToRegister);
            
        if (forgotPasswordButton != null)
            forgotPasswordButton.onClick.AddListener(OnClickForgotPassword);
            
        // 이메일 회원가입 패널 버튼 이벤트
        if (registerButton != null)
            registerButton.onClick.AddListener(OnClickRegister);
            
        if (backToLoginButton != null)
            backToLoginButton.onClick.AddListener(OnClickBackToLogin);
            
        // 비밀번호 재설정 패널 버튼 이벤트
        if (sendResetEmailButton != null)
            sendResetEmailButton.onClick.AddListener(OnClickSendResetEmail);
            
        if (backToLoginFromResetButton != null)
            backToLoginFromResetButton.onClick.AddListener(OnClickBackToLoginFromReset);
            
        // UI 초기화
        ShowLoginButtons(true);
        ShowLoadingUI(false);
        ShowLoginSuccessUI(false);
        ShowEmailLoginUI(false);
        ShowEmailRegisterUI(false);
        ShowResetPasswordUI(false);
        
        // 오류 메시지 초기화
        if (errorMessageText != null)
            errorMessageText.text = "";
            
        if (registerErrorMessageText != null)
            registerErrorMessageText.text = "";
            
        if (resetErrorMessageText != null)
            resetErrorMessageText.text = "";
        
        // 이미 로그인되어 있는지 확인
        CheckExistingLoginWithoutAutoCreate();
    }
    
    /// <summary>
    /// 이미 로그인된 세션이 있는지 확인 (게스트 계정 자동 생성 없이)
    /// </summary>
    private void CheckExistingLoginWithoutAutoCreate()
    {
        var firebase = FirebaseManager.Instance;
        
        // Firebase Manager에 추가된 메서드 호출
        // 게스트 계정 자동 생성 없이 기존 로그인 상태만 확인
        bool isAuthenticated = firebase.CheckAuthenticationWithoutAutoLogin();
        
        if (isAuthenticated)
        {
            Debug.Log("[TitleUI] 기존 로그인 세션 발견");
            ShowLoginSuccessUI(true);
            UpdateUserInfo();
        }
        else
        {
            // 기존 계정이 없으면 로그인 선택 화면 표시
            ShowLoginButtons(true);
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
    /// 이메일 로그인 패널 열기
    /// </summary>
    private void OnClickOpenEmailLogin()
    {
        ShowLoginButtons(false);
        ShowEmailLoginUI(true);
        
        // 입력필드 초기화
        if (emailInputField != null)
            emailInputField.text = "";
            
        if (passwordInputField != null)
            passwordInputField.text = "";
            
        if (errorMessageText != null)
            errorMessageText.text = "";
    }
    
    /// <summary>
    /// 이메일 로그인 버튼 클릭
    /// </summary>
    private async void OnClickEmailLogin()
    {
        if (emailInputField == null || passwordInputField == null)
            return;
            
        string email = emailInputField.text;
        string password = passwordInputField.text;
        
        // 입력 검증
        if (string.IsNullOrEmpty(email))
        {
            if (errorMessageText != null)
                errorMessageText.text = "이메일을 입력해주세요.";
            return;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            if (errorMessageText != null)
                errorMessageText.text = "비밀번호를 입력해주세요.";
            return;
        }
        
        // 로딩 UI 표시
        ShowEmailLoginUI(false);
        ShowLoadingUI(true, "이메일 로그인 중...");
        
        var firebase = FirebaseManager.Instance;
        var result = await firebase.SignInWithEmailAsync(email, password);
        
        if (result.success)
        {
            ShowLoadingUI(false);
            ShowLoginSuccessUI(true);
            UpdateUserInfo();
        }
        else
        {
            // 로그인 실패 처리
            ShowLoadingUI(false);
            ShowEmailLoginUI(true);
            
            if (errorMessageText != null)
                errorMessageText.text = result.errorMessage;
                
            Debug.LogWarning("[TitleUI] 이메일 로그인 실패: " + result.errorMessage);
        }
    }
    
    /// <summary>
    /// 메인 화면으로 돌아가기
    /// </summary>
    private void OnClickBackToMain()
    {
        ShowEmailLoginUI(false);
        ShowLoginButtons(true);
    }
    
    /// <summary>
    /// 회원가입 화면으로 이동
    /// </summary>
    private void OnClickGoToRegister()
    {
        ShowEmailLoginUI(false);
        ShowEmailRegisterUI(true);
        
        // 입력필드 초기화
        if (registerEmailInputField != null)
            registerEmailInputField.text = "";
            
        if (registerPasswordInputField != null)
            registerPasswordInputField.text = "";
            
        if (confirmPasswordInputField != null)
            confirmPasswordInputField.text = "";
            
        if (registerErrorMessageText != null)
            registerErrorMessageText.text = "";
    }
    
    /// <summary>
    /// 비밀번호 찾기 화면으로 이동
    /// </summary>
    private void OnClickForgotPassword()
    {
        ShowEmailLoginUI(false);
        ShowResetPasswordUI(true);
        
        // 입력필드 초기화
        if (resetEmailInputField != null)
            resetEmailInputField.text = "";
            
        if (resetErrorMessageText != null)
            resetErrorMessageText.text = "";
    }
    
    /// <summary>
    /// 회원가입 버튼 클릭
    /// </summary>
    private async void OnClickRegister()
    {
        if (registerEmailInputField == null || registerPasswordInputField == null || confirmPasswordInputField == null)
            return;
            
        string email = registerEmailInputField.text;
        string password = registerPasswordInputField.text;
        string confirmPassword = confirmPasswordInputField.text;
        
        // 입력 검증
        if (string.IsNullOrEmpty(email))
        {
            if (registerErrorMessageText != null)
                registerErrorMessageText.text = "이메일을 입력해주세요.";
            return;
        }
        
        if (string.IsNullOrEmpty(password))
        {
            if (registerErrorMessageText != null)
                registerErrorMessageText.text = "비밀번호를 입력해주세요.";
            return;
        }
        
        if (password.Length < 6)
        {
            if (registerErrorMessageText != null)
                registerErrorMessageText.text = "비밀번호는 최소 6자리 이상이어야 합니다.";
            return;
        }
        
        if (password != confirmPassword)
        {
            if (registerErrorMessageText != null)
                registerErrorMessageText.text = "비밀번호가 일치하지 않습니다.";
            return;
        }
        
        // 로딩 UI 표시
        ShowEmailRegisterUI(false);
        ShowLoadingUI(true, "회원가입 중...");
        
        var firebase = FirebaseManager.Instance;
        var result = await firebase.CreateUserWithEmailAsync(email, password);
        
        if (result.success)
        {
            ShowLoadingUI(false);
            ShowLoginSuccessUI(true);
            UpdateUserInfo();
        }
        else
        {
            // 회원가입 실패 처리
            ShowLoadingUI(false);
            ShowEmailRegisterUI(true);
            
            if (registerErrorMessageText != null)
                registerErrorMessageText.text = result.errorMessage;
                
            Debug.LogWarning("[TitleUI] 회원가입 실패: " + result.errorMessage);
        }
    }
    
    /// <summary>
    /// 로그인 화면으로 돌아가기
    /// </summary>
    private void OnClickBackToLogin()
    {
        ShowEmailRegisterUI(false);
        ShowEmailLoginUI(true);
        
        // 입력필드 초기화
        if (emailInputField != null)
            emailInputField.text = "";
            
        if (passwordInputField != null)
            passwordInputField.text = "";
            
        if (errorMessageText != null)
            errorMessageText.text = "";
    }
    
    /// <summary>
    /// 비밀번호 재설정 이메일 전송
    /// </summary>
    private async void OnClickSendResetEmail()
    {
        if (resetEmailInputField == null)
            return;
            
        string email = resetEmailInputField.text;
        
        // 입력 검증
        if (string.IsNullOrEmpty(email))
        {
            if (resetErrorMessageText != null)
                resetErrorMessageText.text = "이메일을 입력해주세요.";
            return;
        }
        
        // 로딩 UI 표시
        ShowResetPasswordUI(false);
        ShowLoadingUI(true, "이메일 전송 중...");
        
        var firebase = FirebaseManager.Instance;
        var result = await firebase.SendPasswordResetEmailAsync(email);
        
        if (result.success)
        {
            ShowLoadingUI(false);
            ShowEmailLoginUI(true);
            
            if (errorMessageText != null)
                errorMessageText.text = "비밀번호 재설정 이메일이 전송되었습니다.";
        }
        else
        {
            // 이메일 전송 실패 처리
            ShowLoadingUI(false);
            ShowResetPasswordUI(true);
            
            if (resetErrorMessageText != null)
                resetErrorMessageText.text = result.errorMessage;
                
            Debug.LogWarning("[TitleUI] 비밀번호 재설정 이메일 전송 실패: " + result.errorMessage);
        }
    }
    
    /// <summary>
    /// 로그인 화면으로 돌아가기 (비밀번호 재설정에서)
    /// </summary>
    private void OnClickBackToLoginFromReset()
    {
        ShowResetPasswordUI(false);
        ShowEmailLoginUI(true);
        
        // 입력필드 초기화
        if (emailInputField != null)
            emailInputField.text = "";
            
        if (passwordInputField != null)
            passwordInputField.text = "";
            
        if (errorMessageText != null)
            errorMessageText.text = "";
    }
    
    /// <summary>
    /// 시작하기 버튼 클릭 (로그인 성공 후)
    /// </summary>
    private void OnClickStartGame()
    {
        // 게임 시작 처리
        Debug.Log("[TitleUI] 게임 시작");
        
        // PlayerDataManager를 통해 데이터 로드 후 로비 씬으로 전환
        GameManager.Instance.LoadPlayerDataAndGoToLobby();
    }
    
    /// <summary>
    /// 계정 삭제 버튼 클릭
    /// </summary>
    private async void OnClickDeleteAccount()
    {
        // 확인 메시지 표시 (실제 UI는 구현 필요)
        if (!Confirm("정말 계정을 삭제하시겠습니까?"))
            return;
        
        // 로딩 UI 표시
        ShowLoginSuccessUI(false);
        ShowLoadingUI(true, "계정 삭제 중...");
        
        var firebase = FirebaseManager.Instance;
        bool success = await firebase.DeleteUserAccountAsync();
        
        if (success)
        {
            Debug.Log("[TitleUI] 계정 삭제 성공");
            ShowLoadingUI(false);
            ShowLoginButtons(true);
        }
        else
        {
            // 삭제 실패 처리
            Debug.LogWarning("[TitleUI] 계정 삭제 실패");
            ShowLoadingUI(false);
            ShowLoginSuccessUI(true);
        }
    }
    
    /// <summary>
    /// 확인 대화상자 표시 (임시 구현)
    /// </summary>
    private bool Confirm(string message)
    {
        // 실제로는 UI 대화상자를 표시해야 하지만,
        // 임시로 에디터용 대화상자 사용
#if UNITY_EDITOR
        return UnityEditor.EditorUtility.DisplayDialog("확인", message, "예", "아니오");
#else
        // 실제 게임에서는 항상 true 반환 (UI는 별도 구현 필요)
        Debug.Log($"[TitleUI] 확인 대화상자: {message}");
        return true;
#endif
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
                case FirebaseManager.LoginType.Email:
                    loginTypeStr = "이메일";
                    break;
            }
            
            string displayInfo = $"ID: {firebase.UserId}\n로그인: {loginTypeStr}";
            
            // 이메일 로그인인 경우 이메일도 표시
            if (firebase.CurrentLoginType == FirebaseManager.LoginType.Email && !string.IsNullOrEmpty(firebase.UserEmail))
            {
                displayInfo += $"\n이메일: {firebase.UserEmail}";
            }
            
            userIdText.text = displayInfo;
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
            
        if (emailLoginButton != null)
            emailLoginButton.gameObject.SetActive(show);
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
    
    /// <summary>
    /// 이메일 로그인 UI 표시/숨김
    /// </summary>
    private void ShowEmailLoginUI(bool show)
    {
        if (emailLoginPanel != null)
            emailLoginPanel.SetActive(show);
    }
    
    /// <summary>
    /// 이메일 회원가입 UI 표시/숨김
    /// </summary>
    private void ShowEmailRegisterUI(bool show)
    {
        if (emailRegisterPanel != null)
            emailRegisterPanel.SetActive(show);
    }
    
    /// <summary>
    /// 비밀번호 재설정 UI 표시/숨김
    /// </summary>
    private void ShowResetPasswordUI(bool show)
    {
        if (resetPasswordPanel != null)
            resetPasswordPanel.SetActive(show);
    }
}