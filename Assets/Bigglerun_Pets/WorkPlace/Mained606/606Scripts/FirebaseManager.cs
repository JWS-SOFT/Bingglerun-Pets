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
    public string UserEmail { get; private set; }
    
    // 로그인 상태 변경 이벤트
    public event Action<bool> OnLoginStateChanged;
    
    // 로그인 유형
    public enum LoginType
    {
        None,
        Guest,
        Google,
        Email
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
    /// Firebase를 초기화하고 기존 로그인 확인 (자동 게스트 로그인 없음)
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
                
                // 더 이상 자동으로 게스트 로그인을 시도하지 않음
                Debug.Log("[FirebaseManager] 기존 로그인 정보 없음");
                return false;
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
        // 파이어베이스 SDK가 없을 때 테스트용 초기화 (자동 로그인 없음)
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 초기화만 수행합니다.");
        await Task.Delay(500); // 가짜 딜레이 처리
        
        IsInitialized = true;
        // 더 이상 자동으로 인증 상태를 true로 설정하지 않음
        IsAuthenticated = false;
        CurrentLoginType = LoginType.None;
        
        Debug.Log("[FirebaseManager] 테스트 초기화 완료 (자동 로그인 없음)");
        
        return false;
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
            UserEmail = user.Email;
            
            // 로그인 타입 설정 (ProviderData로 확인)
            if (user.ProviderData.Any() && user.ProviderData.FirstOrDefault()?.ProviderId == "google.com")
            {
                CurrentLoginType = LoginType.Google;
            }
            else if (user.ProviderData.Any() && user.ProviderData.FirstOrDefault()?.ProviderId == "password")
            {
                CurrentLoginType = LoginType.Email;
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
            UserEmail = null;
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
        UserEmail = null;
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
            await Task.FromResult(false);
            return false;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseManager] 구글 로그인 실패: {e.Message}");
            return false;
        }
#elif FIREBASE_AUTH
        Debug.LogWarning("[FirebaseManager] 구글 로그인은 Android 플랫폼에서만 지원됩니다.");
        await Task.FromResult(false);
        return false;
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 로그인
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 구글 로그인을 수행합니다.");
        await Task.Delay(500);
        
        IsAuthenticated = true;
        UserId = "google_" + UnityEngine.Random.Range(1000, 9999);
        UserEmail = "test-google@example.com";
        CurrentLoginType = LoginType.Google;
        
        Debug.Log($"[FirebaseManager] 테스트 구글 로그인 완료: {UserId}");
        OnLoginStateChanged?.Invoke(true);
        
        return true;
#endif
    }

    /// <summary>
    /// 이메일/비밀번호 회원가입
    /// </summary>
    public async Task<(bool success, string errorMessage)> CreateUserWithEmailAsync(string email, string password)
    {
#if FIREBASE_AUTH
        try
        {
            Debug.Log($"[FirebaseManager] 이메일 회원가입 시도 중: {email}");
            var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
            if (result != null && result.User != null)
            {
                UpdateUserInfo(result.User);
                Debug.Log("[FirebaseManager] 이메일 회원가입 성공");
                return (true, null);
            }
            return (false, "회원가입 실패");
        }
        catch (Exception e)
        {
            string errorMessage = "회원가입 실패";
            
            // Firebase 에러 코드별 사용자 친화적 메시지
            if (e is Firebase.FirebaseException firebaseEx)
            {
                switch (firebaseEx.ErrorCode)
                {
                    case 6: // ERROR_WEAK_PASSWORD
                        errorMessage = "비밀번호가 너무 약합니다. 최소 6자리 이상 입력해주세요.";
                        break;
                    case 9: // ERROR_EMAIL_ALREADY_IN_USE
                        errorMessage = "이미 사용 중인 이메일입니다. 다른 이메일을 사용해주세요.";
                        break;
                    case 11: // ERROR_INVALID_EMAIL
                        errorMessage = "유효하지 않은 이메일 형식입니다.";
                        break;
                    default:
                        errorMessage = $"회원가입 실패: {e.Message}";
                        break;
                }
            }
            
            Debug.LogError($"[FirebaseManager] 이메일 회원가입 실패: {e.Message}");
            return (false, errorMessage);
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 회원가입
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 이메일 회원가입을 수행합니다.");
        await Task.Delay(500);
        
        // 테스트 검증
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            return (false, "유효하지 않은 이메일 형식입니다.");
        }
        
        if (string.IsNullOrEmpty(password) || password.Length < 6)
        {
            return (false, "비밀번호가 너무 약합니다. 최소 6자리 이상 입력해주세요.");
        }
        
        // 가끔 실패 시뮬레이션
        if (UnityEngine.Random.value < 0.1f)
        {
            return (false, "이미 사용 중인 이메일입니다. 다른 이메일을 사용해주세요.");
        }
        
        IsAuthenticated = true;
        UserId = "email_" + UnityEngine.Random.Range(1000, 9999);
        UserEmail = email;
        CurrentLoginType = LoginType.Email;
        
        Debug.Log($"[FirebaseManager] 테스트 이메일 회원가입 완료: {UserEmail}");
        OnLoginStateChanged?.Invoke(true);
        
        return (true, null);
#endif
    }

    /// <summary>
    /// 이메일/비밀번호 로그인
    /// </summary>
    public async Task<(bool success, string errorMessage)> SignInWithEmailAsync(string email, string password)
    {
#if FIREBASE_AUTH
        try
        {
            Debug.Log($"[FirebaseManager] 이메일 로그인 시도 중: {email}");
            var result = await auth.SignInWithEmailAndPasswordAsync(email, password);
            if (result != null && result.User != null)
            {
                UpdateUserInfo(result.User);
                Debug.Log("[FirebaseManager] 이메일 로그인 성공");
                return (true, null);
            }
            return (false, "로그인 실패");
        }
        catch (Exception e)
        {
            string errorMessage = "로그인 실패";
            
            // Firebase 에러 코드별 사용자 친화적 메시지
            if (e is Firebase.FirebaseException firebaseEx)
            {
                switch (firebaseEx.ErrorCode)
                {
                    case 17: // ERROR_USER_NOT_FOUND
                    case 8: // ERROR_WRONG_PASSWORD
                        errorMessage = "이메일 또는 비밀번호가 일치하지 않습니다.";
                        break;
                    case 11: // ERROR_INVALID_EMAIL
                        errorMessage = "유효하지 않은 이메일 형식입니다.";
                        break;
                    default:
                        errorMessage = $"로그인 실패: {e.Message}";
                        break;
                }
            }
            
            Debug.LogError($"[FirebaseManager] 이메일 로그인 실패: {e.Message}");
            return (false, errorMessage);
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 로그인
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 이메일 로그인을 수행합니다.");
        await Task.Delay(500);
        
        // 테스트 검증
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            return (false, "유효하지 않은 이메일 형식입니다.");
        }
        
        if (string.IsNullOrEmpty(password))
        {
            return (false, "비밀번호를 입력해주세요.");
        }
        
        // 가끔 실패 시뮬레이션
        if (UnityEngine.Random.value < 0.1f)
        {
            return (false, "이메일 또는 비밀번호가 일치하지 않습니다.");
        }
        
        IsAuthenticated = true;
        UserId = "email_" + UnityEngine.Random.Range(1000, 9999);
        UserEmail = email;
        CurrentLoginType = LoginType.Email;
        
        Debug.Log($"[FirebaseManager] 테스트 이메일 로그인 완료: {UserEmail}");
        OnLoginStateChanged?.Invoke(true);
        
        return (true, null);
#endif
    }

    /// <summary>
    /// 비밀번호 재설정 이메일 발송
    /// </summary>
    public async Task<(bool success, string errorMessage)> SendPasswordResetEmailAsync(string email)
    {
#if FIREBASE_AUTH
        try
        {
            Debug.Log($"[FirebaseManager] 비밀번호 재설정 이메일 발송 시도: {email}");
            await auth.SendPasswordResetEmailAsync(email);
            Debug.Log("[FirebaseManager] 비밀번호 재설정 이메일 발송 성공");
            return (true, null);
        }
        catch (Exception e)
        {
            string errorMessage = "비밀번호 재설정 실패";
            
            // Firebase 에러 코드별 사용자 친화적 메시지
            if (e is Firebase.FirebaseException firebaseEx)
            {
                switch (firebaseEx.ErrorCode)
                {
                    case 17: // ERROR_USER_NOT_FOUND
                        errorMessage = "등록되지 않은 이메일입니다.";
                        break;
                    case 11: // ERROR_INVALID_EMAIL
                        errorMessage = "유효하지 않은 이메일 형식입니다.";
                        break;
                    default:
                        errorMessage = $"비밀번호 재설정 실패: {e.Message}";
                        break;
                }
            }
            
            Debug.LogError($"[FirebaseManager] 비밀번호 재설정 이메일 발송 실패: {e.Message}");
            return (false, errorMessage);
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 비밀번호 재설정
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 비밀번호 재설정을 수행합니다.");
        await Task.Delay(500);
        
        // 테스트 검증
        if (string.IsNullOrEmpty(email) || !email.Contains("@"))
        {
            return (false, "유효하지 않은 이메일 형식입니다.");
        }
        
        Debug.Log($"[FirebaseManager] 테스트 비밀번호 재설정 이메일 발송 완료: {email}");
        return (true, null);
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
            Debug.Log("[FirebaseManager] 로그아웃 시도 중...");
            auth.SignOut();
            currentUser = null;
            IsAuthenticated = false;
            UserId = null;
            UserEmail = null;
            CurrentLoginType = LoginType.None;
            
            OnLoginStateChanged?.Invoke(false);
            Debug.Log("[FirebaseManager] 로그아웃 성공");
            await Task.FromResult(true);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseManager] 로그아웃 실패: {e.Message}");
            return false;
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 로그아웃
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 로그아웃을 수행합니다.");
        await Task.Delay(500);
        
        IsAuthenticated = false;
        UserId = null;
        UserEmail = null;
        CurrentLoginType = LoginType.None;
        
        Debug.Log("[FirebaseManager] 테스트 로그아웃 완료");
        OnLoginStateChanged?.Invoke(false);
        
        return true;
#endif
    }

    /// <summary>
    /// 사용자 계정 삭제
    /// </summary>
    public async Task<bool> DeleteUserAccountAsync()
    {
#if FIREBASE_AUTH
        try
        {
            if (auth.CurrentUser == null)
            {
                Debug.LogWarning("[FirebaseManager] 계정 삭제 실패: 로그인되지 않음");
                return false;
            }
            
            Debug.Log("[FirebaseManager] 계정 삭제 시도 중...");
            await auth.CurrentUser.DeleteAsync();
            
            // 삭제 후 정보 초기화
            currentUser = null;
            IsAuthenticated = false;
            UserId = null;
            UserEmail = null;
            CurrentLoginType = LoginType.None;
            
            OnLoginStateChanged?.Invoke(false);
            Debug.Log("[FirebaseManager] 계정 삭제 성공");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[FirebaseManager] 계정 삭제 실패: {e.Message}");
            return false;
        }
#else
        // 파이어베이스 SDK가 없을 때 테스트용 가짜 계정 삭제
        Debug.LogWarning("[FirebaseManager] Firebase SDK가 없습니다. 가짜 계정 삭제를 수행합니다.");
        await Task.Delay(500);
        
        IsAuthenticated = false;
        UserId = null;
        UserEmail = null;
        CurrentLoginType = LoginType.None;
        
        Debug.Log("[FirebaseManager] 테스트 계정 삭제 완료");
        OnLoginStateChanged?.Invoke(false);
        
        return true;
#endif
    }

    /// <summary>
    /// 자동 게스트 계정 생성 없이 인증 상태만 확인
    /// </summary>
    /// <returns>인증된 상태이면 true, 아니면 false</returns>
    public bool CheckAuthenticationWithoutAutoLogin()
    {
        Debug.Log("[FirebaseManager] 기존 로그인 상태 확인(자동 생성 없이)");
        
#if FIREBASE_AUTH
        // Firebase 초기화가 안 되었으면 false 반환
        if (!IsInitialized || auth == null)
        {
            Debug.Log("[FirebaseManager] Firebase가 초기화되지 않았습니다.");
            return false;
        }
        
        // 현재 사용자 정보 확인 (새 계정 생성하지 않음)
        var user = auth.CurrentUser;
        if (user != null)
        {
            // 이미 로그인된 사용자가 있으면 정보 업데이트
            UpdateUserInfo(user);
            return true;
        }
        
        Debug.Log("[FirebaseManager] 기존 로그인된 계정이 없습니다.");
        return false;
#else
        // 테스트 환경에서는 IsAuthenticated 값만 확인
        // 이미 SignInAnonymouslyAsync()가 호출된 경우에만 true
        Debug.Log($"[FirebaseManager] 테스트 환경에서 로그인 상태 확인: {IsAuthenticated}");
        return IsAuthenticated;
#endif
    }
}