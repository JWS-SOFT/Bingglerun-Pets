using UnityEngine;
using UnityEngine.UI;
using System.Threading.Tasks;

/// <summary>
/// 이메일 로그인, 회원가입, 비밀번호 재설정 패널 관리 클래스
/// </summary>
public class EmailPanelController : MonoBehaviour
{
    [Header("로그인 패널")]
    [SerializeField] private GameObject loginPanel;
    [SerializeField] private InputField emailInputField;
    [SerializeField] private InputField passwordInputField;
    [SerializeField] private Button loginButton;
    [SerializeField] private Button backButton;
    [SerializeField] private Button registerButton;
    [SerializeField] private Button forgotPasswordButton;
    [SerializeField] private Text errorMessageText;

    [Header("회원가입 패널")]
    [SerializeField] private GameObject registerPanel;
    [SerializeField] private InputField registerEmailInput;
    [SerializeField] private InputField registerPasswordInput;
    [SerializeField] private InputField confirmPasswordInput;
    [SerializeField] private Button createAccountButton;
    [SerializeField] private Button backToLoginButton;
    [SerializeField] private Text registerErrorText;

    [Header("비밀번호 재설정 패널")]
    [SerializeField] private GameObject resetPasswordPanel;
    [SerializeField] private InputField resetEmailInput;
    [SerializeField] private Button sendResetEmailButton;
    [SerializeField] private Button backToLoginFromResetButton;
    [SerializeField] private Text resetErrorText;

    // 이벤트 콜백 정의
    public delegate void LoginCallback(bool success, string message);
    public event LoginCallback OnLoginResult;

    public delegate void RegisterCallback(bool success, string message);
    public event RegisterCallback OnRegisterResult;

    public delegate void ResetPasswordCallback(bool success, string message);
    public event ResetPasswordCallback OnResetPasswordResult;

    public delegate void BackButtonCallback();
    public event BackButtonCallback OnBackButtonPressed;

    private void Awake()
    {
        // 초기화 - 모든 패널을 비활성화
        if (loginPanel) loginPanel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(false);
        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);

        // 오류 메시지 초기화
        ClearErrorMessages();
    }

    private void Start()
    {
        // 버튼 이벤트 설정
        if (loginButton) loginButton.onClick.AddListener(OnLoginButtonClicked);
        if (backButton) backButton.onClick.AddListener(OnBackButtonClicked);
        if (registerButton) registerButton.onClick.AddListener(ShowRegisterPanel);
        if (forgotPasswordButton) forgotPasswordButton.onClick.AddListener(ShowResetPasswordPanel);

        if (createAccountButton) createAccountButton.onClick.AddListener(OnRegisterButtonClicked);
        if (backToLoginButton) backToLoginButton.onClick.AddListener(ShowLoginPanel);

        if (sendResetEmailButton) sendResetEmailButton.onClick.AddListener(OnResetPasswordButtonClicked);
        if (backToLoginFromResetButton) backToLoginFromResetButton.onClick.AddListener(ShowLoginPanel);
    }

    /// <summary>
    /// 로그인 패널 표시
    /// </summary>
    public void ShowLoginPanel()
    {
        if (loginPanel) loginPanel.SetActive(true);
        if (registerPanel) registerPanel.SetActive(false);
        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);

        // 입력 필드 초기화
        if (emailInputField) emailInputField.text = "";
        if (passwordInputField) passwordInputField.text = "";

        ClearErrorMessages();
    }

    /// <summary>
    /// 회원가입 패널 표시
    /// </summary>
    public void ShowRegisterPanel()
    {
        if (loginPanel) loginPanel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(true);
        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);

        // 입력 필드 초기화
        if (registerEmailInput) registerEmailInput.text = "";
        if (registerPasswordInput) registerPasswordInput.text = "";
        if (confirmPasswordInput) confirmPasswordInput.text = "";

        ClearErrorMessages();
    }

    /// <summary>
    /// 비밀번호 재설정 패널 표시
    /// </summary>
    public void ShowResetPasswordPanel()
    {
        if (loginPanel) loginPanel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(false);
        if (resetPasswordPanel) resetPasswordPanel.SetActive(true);

        // 입력 필드 초기화
        if (resetEmailInput) resetEmailInput.text = "";

        ClearErrorMessages();
    }

    /// <summary>
    /// 패널 숨기기
    /// </summary>
    public void HideAllPanels()
    {
        if (loginPanel) loginPanel.SetActive(false);
        if (registerPanel) registerPanel.SetActive(false);
        if (resetPasswordPanel) resetPasswordPanel.SetActive(false);

        ClearErrorMessages();
    }

    /// <summary>
    /// 로그인 버튼 클릭 처리
    /// </summary>
    private async void OnLoginButtonClicked()
    {
        string email = emailInputField ? emailInputField.text : "";
        string password = passwordInputField ? passwordInputField.text : "";

        // 입력 검증
        if (string.IsNullOrEmpty(email))
        {
            ShowLoginError("이메일을 입력해주세요.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowLoginError("비밀번호를 입력해주세요.");
            return;
        }

        // Firebase 로그인 시도
        var result = await FirebaseManager.Instance.SignInWithEmailAsync(email, password);

        if (result.success)
        {
            ClearErrorMessages();
            OnLoginResult?.Invoke(true, "로그인 성공");
            HideAllPanels();
        }
        else
        {
            ShowLoginError(result.errorMessage);
            OnLoginResult?.Invoke(false, result.errorMessage);
        }
    }

    /// <summary>
    /// 회원가입 버튼 클릭 처리
    /// </summary>
    private async void OnRegisterButtonClicked()
    {
        string email = registerEmailInput ? registerEmailInput.text : "";
        string password = registerPasswordInput ? registerPasswordInput.text : "";
        string confirmPassword = confirmPasswordInput ? confirmPasswordInput.text : "";

        // 입력 검증
        if (string.IsNullOrEmpty(email))
        {
            ShowRegisterError("이메일을 입력해주세요.");
            return;
        }

        if (string.IsNullOrEmpty(password))
        {
            ShowRegisterError("비밀번호를 입력해주세요.");
            return;
        }

        if (password.Length < 6)
        {
            ShowRegisterError("비밀번호는 최소 6자리 이상이어야 합니다.");
            return;
        }

        if (password != confirmPassword)
        {
            ShowRegisterError("비밀번호가 일치하지 않습니다.");
            return;
        }

        // Firebase 회원가입 시도
        var result = await FirebaseManager.Instance.CreateUserWithEmailAsync(email, password);

        if (result.success)
        {
            ClearErrorMessages();
            OnRegisterResult?.Invoke(true, "회원가입 성공");
            HideAllPanels();
        }
        else
        {
            ShowRegisterError(result.errorMessage);
            OnRegisterResult?.Invoke(false, result.errorMessage);
        }
    }

    /// <summary>
    /// 비밀번호 재설정 버튼 클릭 처리
    /// </summary>
    private async void OnResetPasswordButtonClicked()
    {
        string email = resetEmailInput ? resetEmailInput.text : "";

        // 입력 검증
        if (string.IsNullOrEmpty(email))
        {
            ShowResetError("이메일을 입력해주세요.");
            return;
        }

        // Firebase 비밀번호 재설정 이메일 발송 시도
        var result = await FirebaseManager.Instance.SendPasswordResetEmailAsync(email);

        if (result.success)
        {
            ClearErrorMessages();
            OnResetPasswordResult?.Invoke(true, "비밀번호 재설정 이메일이 발송되었습니다.");
            
            // 로그인 패널로 다시 전환하면서 메시지 표시
            ShowLoginPanel();
            ShowLoginMessage("비밀번호 재설정 이메일이 발송되었습니다.");
        }
        else
        {
            ShowResetError(result.errorMessage);
            OnResetPasswordResult?.Invoke(false, result.errorMessage);
        }
    }

    /// <summary>
    /// 뒤로 가기 버튼 클릭
    /// </summary>
    private void OnBackButtonClicked()
    {
        HideAllPanels();
        OnBackButtonPressed?.Invoke();
    }

    /// <summary>
    /// 로그인 오류 메시지 표시
    /// </summary>
    private void ShowLoginError(string message)
    {
        if (errorMessageText) errorMessageText.text = message;
    }

    /// <summary>
    /// 로그인 메시지 표시 (오류 형식이 아닌 일반 메시지)
    /// </summary>
    public void ShowLoginMessage(string message)
    {
        if (errorMessageText) errorMessageText.text = message;
    }

    /// <summary>
    /// 회원가입 오류 메시지 표시
    /// </summary>
    private void ShowRegisterError(string message)
    {
        if (registerErrorText) registerErrorText.text = message;
    }

    /// <summary>
    /// 비밀번호 재설정 오류 메시지 표시
    /// </summary>
    private void ShowResetError(string message)
    {
        if (resetErrorText) resetErrorText.text = message;
    }

    /// <summary>
    /// 모든 오류 메시지 초기화
    /// </summary>
    private void ClearErrorMessages()
    {
        if (errorMessageText) errorMessageText.text = "";
        if (registerErrorText) registerErrorText.text = "";
        if (resetErrorText) resetErrorText.text = "";
    }
} 