using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Firebase Realtime Database와 통신하기 위한 매니저 클래스
/// </summary>
public class FirebaseDatabase : MonoBehaviour
{
    #region 싱글톤
    public static FirebaseDatabase Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        InitializeFirebaseDatabase();
    }
    #endregion
    
    // Firebase 초기화 상태
    public bool IsInitialized { get; private set; }
    
    // Firebase SDK 참조 (조건부 컴파일용)
#if FIREBASE_DATABASE
    private Firebase.Database.FirebaseDatabase database;
    private Firebase.Database.DatabaseReference databaseReference;
#endif

    // 더미 데이터 (Firebase SDK가 없을 때 사용)
    private Dictionary<string, string> mockDatabase = new Dictionary<string, string>();
    
    /// <summary>
    /// Firebase Database 초기화
    /// </summary>
    private void InitializeFirebaseDatabase()
    {
#if FIREBASE_DATABASE
        try
        {
            // URL 직접 지정하여 데이터베이스 인스턴스 가져오기
            database = Firebase.Database.FirebaseDatabase.GetInstance("https://bigglerun-pets-default-rtdb.firebaseio.com/");
            databaseReference = database.RootReference;
            
            IsInitialized = true;
            Debug.Log("[FirebaseDatabase] Firebase Database 초기화 완료");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] Firebase Database 초기화 중 오류 발생: {ex.Message}");
            IsInitialized = false;
        }
#else
        // Firebase SDK가 없을 때 테스트용 초기화
        Debug.LogWarning("[FirebaseDatabase] Firebase SDK가 없습니다. 테스트용 DB를 사용합니다.");
        IsInitialized = true;
#endif
    }
    
    /// <summary>
    /// Firebase에서 플레이어 데이터 로드
    /// </summary>
    public async Task<PlayerData> LoadPlayerDataAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID가 없습니다.");
            return null;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return null;
        }
        
        try
        {
            // Realtime Database에서 플레이어 데이터 가져오기
            Firebase.Database.DataSnapshot snapshot = 
                await databaseReference.Child("players").Child(userId).GetValueAsync();
                
            if (snapshot.Exists)
            {
                // JSON 데이터 가져오기
                string json = snapshot.GetRawJsonValue();
                
                // JSON을 PlayerData 객체로 변환
                PlayerData playerData = JsonUtility.FromJson<PlayerData>(json);
                
                Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 로드했습니다.");
                return playerData;
            }
            else
            {
                Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터가 없습니다.");
                return null;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 로드 중 오류 발생: {ex.Message}");
            return null;
        }
#else
        // 테스트용 더미 로드
        await Task.Delay(500); // 네트워크 지연 시뮬레이션
        
        if (mockDatabase.TryGetValue(userId, out string json))
        {
            Debug.Log($"[FirebaseDatabase] 테스트 데이터 로드: {userId}");
            return JsonUtility.FromJson<PlayerData>(json);
        }
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 없음: {userId}");
        return null;
#endif
    }
    
    /// <summary>
    /// Firebase에 플레이어 데이터 저장
    /// </summary>
    public async Task<bool> SavePlayerDataAsync(string userId, PlayerData data)
    {
        if (string.IsNullOrEmpty(userId) || data == null)
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID 또는 데이터가 없습니다.");
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
        
        try
        {
            // PlayerData 객체를 JSON으로 변환
            string json = JsonUtility.ToJson(data);
            
            // Realtime Database에 저장
            await databaseReference.Child("players").Child(userId).SetRawJsonValueAsync(json);
            
            Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 저장했습니다.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 저장 중 오류 발생: {ex.Message}");
            return false;
        }
#else
        // 테스트용 더미 저장
        await Task.Delay(200); // 네트워크 지연 시뮬레이션
        
        string json = JsonUtility.ToJson(data);
        mockDatabase[userId] = json;
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 저장: {userId}");
        return true;
#endif
    }
    
    /// <summary>
    /// Firebase에서 플레이어 데이터 삭제
    /// </summary>
    public async Task<bool> DeletePlayerDataAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID가 없습니다.");
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
        
        try
        {
            // Realtime Database에서 데이터 삭제
            await databaseReference.Child("players").Child(userId).RemoveValueAsync();
            
            Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 삭제했습니다.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 삭제 중 오류 발생: {ex.Message}");
            return false;
        }
#else
        // 테스트용 더미 삭제
        await Task.Delay(200); // 네트워크 지연 시뮬레이션
        
        if (mockDatabase.ContainsKey(userId))
        {
            mockDatabase.Remove(userId);
            Debug.Log($"[FirebaseDatabase] 테스트 데이터 삭제: {userId}");
            return true;
        }
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 없음 (삭제 실패): {userId}");
        return false;
#endif
    }
    
    /// <summary>
    /// 플레이어 데이터 일부 업데이트
    /// </summary>
    public async Task<bool> UpdatePlayerDataAsync(string userId, Dictionary<string, object> updates)
    {
        if (string.IsNullOrEmpty(userId) || updates == null || updates.Count == 0)
        {
            Debug.LogError("[FirebaseDatabase] 유저 ID 또는 업데이트 데이터가 없습니다.");
            return false;
        }
        
#if FIREBASE_DATABASE
        if (!IsInitialized)
        {
            Debug.LogError("[FirebaseDatabase] Firebase Database가 초기화되지 않았습니다.");
            return false;
        }
        
        try
        {
            var playerRef = databaseReference.Child("players").Child(userId);
            
            // 각 필드 업데이트
            foreach (var entry in updates)
            {
                await playerRef.Child(entry.Key).SetValueAsync(entry.Value);
            }
            
            Debug.Log($"[FirebaseDatabase] 유저 {userId}의 데이터를 부분 업데이트했습니다.");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[FirebaseDatabase] 플레이어 데이터 업데이트 중 오류 발생: {ex.Message}");
            return false;
        }
#else
        // 테스트용 더미 업데이트
        await Task.Delay(200); // 네트워크 지연 시뮬레이션
        
        if (mockDatabase.TryGetValue(userId, out string json))
        {
            PlayerData data = JsonUtility.FromJson<PlayerData>(json);
            
            // 이 부분은 실제로는 JSON을 파싱하여 특정 필드만 업데이트해야 하지만,
            // 테스트용 구현에서는 단순화를 위해 생략합니다.
            
            mockDatabase[userId] = JsonUtility.ToJson(data);
            Debug.Log($"[FirebaseDatabase] 테스트 데이터 부분 업데이트: {userId}");
            return true;
        }
        
        Debug.Log($"[FirebaseDatabase] 테스트 데이터 없음 (업데이트 실패): {userId}");
        return false;
#endif
    }
    
    /// <summary>
    /// 오프라인 지속성 설정
    /// </summary>
    public void SetPersistenceEnabled(bool enabled)
    {
#if FIREBASE_DATABASE
        if (IsInitialized)
        {
            database.SetPersistenceEnabled(enabled);
            Debug.Log($"[FirebaseDatabase] 오프라인 지속성 설정: {enabled}");
        }
#else
        Debug.Log($"[FirebaseDatabase] 테스트 환경에서는 오프라인 지속성 설정이 무시됩니다: {enabled}");
#endif
    }
} 