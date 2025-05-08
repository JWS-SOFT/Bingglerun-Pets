using UnityEngine;
using UnityEngine.UI;

public class TitleUIController : MonoBehaviour
{
    [SerializeField] private Button loginButton;

    private void Start()
    {
        loginButton.onClick.AddListener(OnClickLogin);
    }

    private async void OnClickLogin()
    {
        loginButton.interactable = false;

        bool success = await FirebaseManager.Instance.InitializeAndLoginAsync();

        if (success)
        {
            // 씬 전환 → 로비로
            GameManager.Instance.SceneFader.LoadScene("LobbyScene");

            // 상태 전환은 씬 로딩 이후 Start 등에서 처리하거나, 여기서 즉시
            GameManager.Instance.StateMachine.ChangeState(GameState.Lobby);
        }
        else
        {
            Debug.LogWarning("로그인 실패");
            loginButton.interactable = true;
        }
    }
}