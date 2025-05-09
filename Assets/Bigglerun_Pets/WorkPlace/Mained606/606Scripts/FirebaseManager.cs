using UnityEngine;
using System;
using System.Linq;
using System.Threading.Tasks;

/// <summary>
/// Firebase 초기화 및 로그인 처리
/// </summary>
public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }

    public bool IsInitialized { get; private set; }
    public bool IsAuthenticated { get; private set; }
    public string UserId { get; private set; }
    
    // 로그인 상태 변경 이벤트
    public event Action<bool> OnLoginStateChanged;
    
    // 로그인 유형
    public enum LoginType
    {
        None,
        Guest,
        Google
    }
    
    public LoginType CurrentLoginType { get; private set; } = LoginType.None;

    // 파이어베이스 실제 참조 (조건부 컴파일용)
#if FIREBASE_AUTH
    private Firebase.Auth.FirebaseAuth auth;
    private Firebase.Auth.FirebaseUser currentUser;
#endif

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

    /// <summary>
    /// Firebase를 초기화하고 게스트 로그인 시도
    /// </summary>
    public async Task<bool> InitializeAndLoginAsync()
    {
        Debug.Log("[FirebaseManager] Firebase 초기화 시도 중...");

#if FIREBASE_AUTH
        try
        {
            // Firebase 초기화
            var dependencyTask = Firebase.FirebaseApp.CheckAndFixDependenciesAsync();
            await dependencyTask;

            var dependencyStatus = dependencyTask.Result;
            if (dependencyStatus == Firebase.DependencyStatus.Available)
            {
                // Firebase 인증 인스턴스 가져오기
                auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
                IsInitialized = true;
                Debug.Log("[FirebaseManager] Firebase 초기화 성공");
                
                // 이미 로그인한 사용자가 있는지 확인
                if (auth.CurrentUser != null)
                {
                    UpdateUserInfo(auth.CurrentUser);
                    Debug.Log($"[FirebaseManager] 기존 로그인 정보 발견: {UserId}");
                    return true;
                }
                
                // 기본적으로 게스트 로그인 시도
                return await SignInAnonymouslyAsync();
            }
            else
            {
                Debug.LogError($"[FirebaseManager] Firebase 초기화 실패: {dependencyStatus}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseManager] Firebase 초기화 예외: {e.Message}");
            return false;
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 로그인
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 로그인을 수행합니다.");
        await Task.Delay(1000); // 가짜 딜레이 처리
        
        IsInitialized = true;
        IsAuthenticated = true;
        UserId = "test_user_" + UnityEngine.Random.Range(1000, 9999);
        CurrentLoginType = LoginType.Guest;
        
        Debug.Log($"[FirebaseManager] 테스트 로그인 완료: {UserId}");
        OnLoginStateChanged?.Invoke(true);
        
        return true;
#endif
    }

#if FIREBASE_AUTH
    /// <summary>
    /// 사용자 정보 업데이트
    /// </summary>
    private void UpdateUserInfo(Firebase.Auth.FirebaseUser user)
    {
        currentUser = user;
        IsAuthenticated = user != null;
        
        if (IsAuthenticated)
        {
            UserId = user.UserId;
            
            // 로그인 타입 설정 (ProviderData로 확인)
            if (user.ProviderData.Any() && user.ProviderData.FirstOrDefault()?.ProviderId == "google.com")
            {
                CurrentLoginType = LoginType.Google;
            }
            else if (user.IsAnonymous)
            {
                CurrentLoginType = LoginType.Guest;
            }
            
            OnLoginStateChanged?.Invoke(true);
        }
        else
        {
            UserId = null;
            CurrentLoginType = LoginType.None;
            OnLoginStateChanged?.Invoke(false);
        }
    }
#endif

    /// <summary>
    /// 게스트(익명) 로그인 시도
    /// </summary>
    public async Task<bool> SignInAnonymouslyAsync()
    {
#if FIREBASE_AUTH
        try
        {
            Debug.Log("[FirebaseManager] 게스트 로그인 시도 중...");
            var result = await auth.SignInAnonymouslyAsync();
            if (result != null && result.User != null)
            {
                UpdateUserInfo(result.User);
                Debug.Log("[FirebaseManager] 게스트 로그인 성공");
                return true;
            }
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseManager] 게스트 로그인 실패: {e.Message}");
            return false;
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 로그인
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 게스트 로그인을 수행합니다.");
        await Task.Delay(500);
        
        IsAuthenticated = true;
        UserId = "guest_" + UnityEngine.Random.Range(1000, 9999);
        CurrentLoginType = LoginType.Guest;
        
        Debug.Log($"[FirebaseManager] 테스트 게스트 로그인 완료: {UserId}");
        OnLoginStateChanged?.Invoke(true);
        
        return true;
#endif
    }

    /// <summary>
    /// 구글 로그인 시도
    /// </summary>
    public async Task<bool> SignInWithGoogleAsync()
    {
#if FIREBASE_AUTH && UNITY_ANDROID
        try
        {
            Debug.Log("[FirebaseManager] 구글 로그인 시도 중...");
            
            // 구글 로그인은 플랫폼별 SDK가 필요함
            // Google Play Games 또는 Firebase UI 필요
            // 실제 구현은 아래 주석 코드처럼 작성 필요
            
            /*
            // 1. Google SignIn 초기화 (Google Play Games SDK 필요)
            GoogleSignIn.Configuration = new GoogleSignInConfiguration
            {
                RequestIdToken = true,
                RequestEmail = true,
                WebClientId = "YOUR-WEBCLIENT-ID" // Firebase Console에서 설정한 웹 클라이언트 ID
            };
            
            // 2. 구글 로그인 UI 표시
            var signInTask = GoogleSignIn.DefaultInstance.SignIn();
            var googleUser = await signInTask;
            
            // 3. Firebase 인증 정보로 변환
            string idToken = googleUser.IdToken;
            string accessToken = null;
            
            // 4. Firebase로 로그인
            Firebase.Auth.Credential credential = Firebase.Auth.GoogleAuthProvider.GetCredential(idToken, accessToken);
            var loginTask = auth.SignInWithCredentialAsync(credential);
            var result = await loginTask;
            
            UpdateUserInfo(result.User);
            Debug.Log("[FirebaseManager] 구글 로그인 성공");
            return true;
            */
            
            // 임시 구현
            Debug.LogWarning("[FirebaseManager] 구글 로그인은 Google Play Games SDK가 필요합니다.");
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseManager] 구글 로그인 실패: {e.Message}");
            return false;
        }
#elif FIREBASE_AUTH
        Debug.LogWarning("[FirebaseManager] 구글 로그인은 Android 플랫폼에서만 지원됩니다.");
        return false;
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 로그인
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 구글 로그인을 수행합니다.");
        await Task.Delay(500);
        
        IsAuthenticated = true;
        UserId = "google_" + UnityEngine.Random.Range(1000, 9999);
        CurrentLoginType = LoginType.Google;
        
        Debug.Log($"[FirebaseManager] 테스트 구글 로그인 완료: {UserId}");
        OnLoginStateChanged?.Invoke(true);
        
        return true;
#endif
    }

    /// <summary>
    /// 로그아웃
    /// </summary>
    public async Task<bool> SignOutAsync()
    {
#if FIREBASE_AUTH
        try
        {
            auth.SignOut();
            UpdateUserInfo(null);
            Debug.Log("[FirebaseManager] 로그아웃 성공");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseManager] 로그아웃 실패: {e.Message}");
            return false;
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 로그아웃
        Debug.Log("[FirebaseManager] 테스트 로그아웃 완료");
        
        IsAuthenticated = false;
        UserId = null;
        CurrentLoginType = LoginType.None;
        
        OnLoginStateChanged?.Invoke(false);
        
        return true;
#endif
    }
}