using Firebase;
using Firebase.Auth;
using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Firebase 초기화 및 로그인 처리
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public FirebaseUser CurrentUser { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task<bool> InitializeAndLoginAsync()
    {
        Debug.Log("[FirebaseManager] Firebase 초기화 시도 중...");

        var dependencyResult = await FirebaseApp.CheckAndFixDependenciesAsync();
        if (dependencyResult != DependencyStatus.Available)
        {
            Debug.LogError("Firebase 종속성 실패: " + dependencyResult);
            return false;
        }

        Auth = FirebaseAuth.DefaultInstance;

        Debug.Log("[FirebaseManager] Firebase 인증 초기화 완료");

        return true;
    }

    public async Task<bool> SignInWithEmail(string email, string password)
    {
        var task = Auth.SignInWithEmailAndPasswordAsync(email, password);
        await task;

        if (task.Exception != null)
        {
            Debug.LogError("이메일 로그인 실패: " + task.Exception);
            return false;
        }

        var result = task.Result; // 타입: AuthResult
        CurrentUser = result.User;

        Debug.Log("이메일 로그인 성공: " + CurrentUser.Email);
        return true;
    }

    public async Task<bool> SignInAnonymously()
    {
        var task = Auth.SignInAnonymouslyAsync();
        await task;

        if (task.Exception != null)
        {
            Debug.LogError("익명 로그인 실패: " + task.Exception);
            return false;
        }

        var result = task.Result; // 타입: AuthResult
        CurrentUser = result.User;

        Debug.Log("익명 로그인 성공: " + CurrentUser.UserId);
        return true;
    }

}