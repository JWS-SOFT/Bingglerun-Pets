using UnityEngine;
using UnityEngine.UI;
using Firebase.Auth;

public class TitleUIController : MonoBehaviour
{
    [SerializeField] private Button guestLoginButton;
    [SerializeField] private Button emailLoginButton;
    [SerializeField] private Button googleLoginButton;

    private void Start()
    {
        guestLoginButton.onClick.AddListener(OnGuestLogin);
        emailLoginButton.onClick.AddListener(OnEmailLogin);
        googleLoginButton.onClick.AddListener(OnGoogleLogin);
    }

    private async void OnGuestLogin()
    {
        guestLoginButton.interactable = false;

        bool success = await FirebaseManager.Instance.SignInAnonymously();
        if (success)
        {
            GameManager.Instance.SceneFader.LoadScene("Lobby");
            GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
        }
        else
        {
            guestLoginButton.interactable = true;
        }
    }

    private async void OnEmailLogin()
    {
        emailLoginButton.interactable = false;

        // TODO: 이메일/비밀번호 입력창 연결
        string email = "test@example.com";     // 임시 입력값
        string password = "123456";            // 임시 입력값

        bool success = await FirebaseManager.Instance.SignInWithEmail(email, password);
        if (success)
        {
            GameManager.Instance.SceneFader.LoadScene("Lobby");
            GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
        }
        else
        {
            emailLoginButton.interactable = true;
        }
    }

    private void OnGoogleLogin()
    {
        Debug.Log("구글 로그인은 추후 지원 예정입니다.");
    }
}