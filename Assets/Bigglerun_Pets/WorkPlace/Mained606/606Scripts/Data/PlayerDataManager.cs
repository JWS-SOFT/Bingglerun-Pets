using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;
using static UnityEngine.Rendering.DebugUI;

/// <summary>
/// 플레이어 데이터를 관리하는 매니저 클래스
/// Firebase와 연동하여 데이터 저장 및 로드를 담당
/// </summary>
public class PlayerDataManager : MonoBehaviour
{
    #region 싱글톤
    public static PlayerDataManager Instance { get; private set; }

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
    #endregion

    // 플레이어 데이터
    public PlayerData CurrentPlayerData { get; private set; }
    public bool IsDataLoaded { get; private set; }
    
    // 데이터 변경 이벤트
    public event Action OnDataLoaded;
    public event Action<int> OnGoldChanged;
    public event Action<int> OnDiamondChanged;
    public event Action<int> OnHeartChanged;
    public event Action<int> OnLevelChanged;
    public event Action<int> OnExperienceChanged;
    public event Action<string> OnNicknameChanged;
    public event Action<int> OnTotalStarsChanged;
    public event Action<string, bool> OnDecorationUnlocked;
    public event Action<string, int> OnItemQuantityChanged;
    public event Action<int> OnCompetitiveBestScoreChanged;

    // 로딩 상태
    public bool IsLoading { get; private set; }
    
    /// <summary>
    /// Firebase에서 플레이어 데이터 로드
    /// </summary>
    public async Task<bool> LoadPlayerDataAsync(string userId)
    {
        if (string.IsNullOrEmpty(userId))
        {
            Debug.LogError("[PlayerDataManager] 플레이어 데이터 로드 실패: 유저 ID가 없습니다.");
            return false;
        }
        
        IsLoading = true;
        
        try
        {
            // Firebase Database에서 데이터 로드
            CurrentPlayerData = await FirebaseDatabase.Instance.LoadPlayerDataAsync(userId);
            
            if (CurrentPlayerData == null)
            {
                // 데이터가 없으면 기본 데이터 생성
                Debug.Log($"[PlayerDataManager] 유저 {userId}의 데이터가 없습니다. 기본 데이터를 생성합니다.");
                CurrentPlayerData = PlayerData.CreateDefault(userId);
                
                // 생성한 기본 데이터 저장
                await FirebaseDatabase.Instance.SavePlayerDataAsync(userId, CurrentPlayerData);
            }
            
            IsDataLoaded = true;
            OnDataLoaded?.Invoke();
            
            Debug.Log($"[PlayerDataManager] 플레이어 데이터 로드 성공: {userId}");
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PlayerDataManager] 플레이어 데이터 로드 중 오류 발생: {ex.Message}");
            return false;
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    /// <summary>
    /// Firebase에 플레이어 데이터 저장
    /// </summary>
    public async Task<bool> SavePlayerDataAsync()
    {
        if (!IsDataLoaded || CurrentPlayerData == null)
        {
            Debug.LogWarning("[PlayerDataManager] 저장할 데이터가 없습니다.");
            return false;
        }
        
        // 타임스탬프 업데이트
        CurrentPlayerData.lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        try
        {
            bool result = await FirebaseDatabase.Instance.SavePlayerDataAsync(
                CurrentPlayerData.playerId, CurrentPlayerData);
                
            if (result)
            {
                Debug.Log($"[PlayerDataManager] 플레이어 데이터 저장 성공: {CurrentPlayerData.playerId}");
            }
            else
            {
                Debug.LogWarning($"[PlayerDataManager] 플레이어 데이터 저장 실패: {CurrentPlayerData.playerId}");
            }
            
            return result;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[PlayerDataManager] 플레이어 데이터 저장 중 오류 발생: {ex.Message}");
            return false;
        }
    }
    
    #region 재화 관련 메서드
    /// <summary>
    /// 골드 추가
    /// </summary>
    public void AddGold(int amount)
    {
        if (!IsDataLoaded || amount <= 0) return;
        
        CurrentPlayerData.gold += amount;
        // 총 수집 코인 업데이트
        CurrentPlayerData.totalCoinsCollected += amount;
        Debug.Log($"골드 추가: {amount}, 현재 총액: {CurrentPlayerData.gold}, 누적 수집량: {CurrentPlayerData.totalCoinsCollected}");
        
        OnGoldChanged?.Invoke(CurrentPlayerData.gold);
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 골드 소비 시도
    /// </summary>
    public bool TrySpendGold(int amount)
    {
        if (!IsDataLoaded || amount <= 0) return false;
        
        if (CurrentPlayerData.gold >= amount)
        {
            CurrentPlayerData.gold -= amount;
            OnGoldChanged?.Invoke(CurrentPlayerData.gold);
            _ = SavePlayerDataAsync();
            return true;
        }
        return false;
    }
    
    /// <summary>
    /// 다이아 추가
    /// </summary>
    public void AddDiamond(int amount)
    {
        if (!IsDataLoaded || amount <= 0) return;
        
        CurrentPlayerData.diamond += amount;
        OnDiamondChanged?.Invoke(CurrentPlayerData.diamond);
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 다이아 소비 시도
    /// </summary>
    public bool TrySpendDiamond(int amount)
    {
        if (!IsDataLoaded || amount <= 0) return false;
        
        if (CurrentPlayerData.diamond >= amount)
        {
            CurrentPlayerData.diamond -= amount;
            OnDiamondChanged?.Invoke(CurrentPlayerData.diamond);
            _ = SavePlayerDataAsync();
            return true;
        }
        return false;
    }

    /// <summary>
    /// 하트 충전
    /// </summary>
    public void RefillHeart(int amount = 1)
    {
        if (!IsDataLoaded || amount <= 0) return;

        CurrentPlayerData.heart += amount;
        OnHeartChanged?.Invoke(CurrentPlayerData.heart);
        UIManager.Instance.uiController.heart.text = CurrentPlayerData.heart.ToString();
        _ = SavePlayerDataAsync();
    }

    /// <summary>
    /// 하트 소비 시도
    /// </summary>
    public bool TrySpendHeart()
    {
        if (!IsDataLoaded) return false;

        if(CurrentPlayerData.heart > 0)
        {
            CurrentPlayerData.heart--;
            OnHeartChanged?.Invoke(CurrentPlayerData.diamond);
            UIManager.Instance.uiController.heart.text = CurrentPlayerData.heart.ToString();
            _ = SavePlayerDataAsync();
            return true;
        }
        return false;
    }
    #endregion
    
    #region 경험치 및 레벨 관련
    /// <summary>
    /// 경험치 추가
    /// </summary>
    public void AddExperience(int amount)
    {
        if (!IsDataLoaded || amount <= 0) return;
        
        CurrentPlayerData.experience += amount;
        OnExperienceChanged?.Invoke(CurrentPlayerData.experience);
        
        // 레벨업 체크 (임시 로직: 100xp당 1레벨)
        CheckLevelUp();
        
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 레벨업 체크
    /// </summary>
    private void CheckLevelUp()
    {
        int experiencePerLevel = 100; // 임시 값, 나중에 레벨별 필요 경험치 테이블로 교체 가능
        int newLevel = 1 + (CurrentPlayerData.experience / experiencePerLevel);
        
        if (newLevel > CurrentPlayerData.level)
        {
            CurrentPlayerData.level = newLevel;
            OnLevelChanged?.Invoke(CurrentPlayerData.level);
            
            // 레벨업 보상 지급 등의 로직 추가 가능
        }
    }
    #endregion
    
    #region 아이템 관련 메서드
    /// <summary>
    /// 아이템 추가
    /// </summary>
    public void AddItem(string itemId, int amount = 1)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(itemId) || amount <= 0) return;
        
        if (CurrentPlayerData.items == null)
            CurrentPlayerData.items = new Dictionary<string, int>();
            
        if (CurrentPlayerData.items.ContainsKey(itemId))
            CurrentPlayerData.items[itemId] += amount;
        else
            CurrentPlayerData.items[itemId] = amount;
            
        OnItemQuantityChanged?.Invoke(itemId, CurrentPlayerData.items[itemId]);
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 아이템 사용/소비
    /// </summary>
    public bool TryUseItem(string itemId, int amount = 1)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(itemId) || amount <= 0) return false;
        
        if (CurrentPlayerData.items != null && 
            CurrentPlayerData.items.TryGetValue(itemId, out int currentAmount) && 
            currentAmount >= amount)
        {
            CurrentPlayerData.items[itemId] -= amount;
            
            // 수량이 0이 되면 아이템 제거
            if (CurrentPlayerData.items[itemId] <= 0)
                CurrentPlayerData.items.Remove(itemId);
                
            OnItemQuantityChanged?.Invoke(itemId, CurrentPlayerData.items.ContainsKey(itemId) ? CurrentPlayerData.items[itemId] : 0);
            _ = SavePlayerDataAsync();
            return true;
        }
        
        return false;
    }
    
    /// <summary>
    /// 아이템 수량 확인
    /// </summary>
    public int GetItemQuantity(string itemId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(itemId) || CurrentPlayerData.items == null)
            return 0;
            
        return CurrentPlayerData.items.TryGetValue(itemId, out int amount) ? amount : 0;
    }
    
    /// <summary>
    /// 시작 아이템 선택
    /// </summary>
    public void SelectPreGameItem(string itemId)
    {
        if (!IsDataLoaded) return;
        
        // 선택 해제 또는 새 아이템 선택
        CurrentPlayerData.selectedPreGameItem = 
            (CurrentPlayerData.selectedPreGameItem == itemId) ? "" : itemId;
            
        _ = SavePlayerDataAsync();
    }
    #endregion
    
    #region 장식 관련 메서드
    /// <summary>
    /// 장식 해금
    /// </summary>
    public void UnlockDecoration(string decorationId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(decorationId)) return;
        
        if (CurrentPlayerData.unlockedDecorations == null)
            CurrentPlayerData.unlockedDecorations = new List<string>();
            
        if (!CurrentPlayerData.unlockedDecorations.Contains(decorationId))
        {
            CurrentPlayerData.unlockedDecorations.Add(decorationId);
            OnDecorationUnlocked?.Invoke(decorationId, true);
            _ = SavePlayerDataAsync();
        }
    }
    
    /// <summary>
    /// 장식 해금 여부 확인
    /// </summary>
    public bool IsDecorationUnlocked(string decorationId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(decorationId) || CurrentPlayerData.unlockedDecorations == null)
            return false;
            
        return CurrentPlayerData.unlockedDecorations.Contains(decorationId);
    }
    
    /// <summary>
    /// 모자 장착
    /// </summary>
    public void EquipHat(string hatId)
    {
        if (!IsDataLoaded) return;
        
        CurrentPlayerData.equippedHat = hatId;
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 바디 장착
    /// </summary>
    public void EquipBody(string bodyId)
    {
        if (!IsDataLoaded) return;
        
        CurrentPlayerData.equippedBody = bodyId;
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 신발 장착
    /// </summary>
    public void EquipShoes(string shoesId)
    {
        if (!IsDataLoaded) return;
        
        CurrentPlayerData.equippedShoes = shoesId;
        _ = SavePlayerDataAsync();
    }
    #endregion
    
    #region 스테이지 관련 메서드
    /// <summary>
    /// 스테이지 해금
    /// </summary>
    public void UnlockStage(string stageId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(stageId)) return;
        
        if (CurrentPlayerData.storyStages == null)
            CurrentPlayerData.storyStages = new Dictionary<string, StageData>();
            
        if (!CurrentPlayerData.storyStages.ContainsKey(stageId))
        {
            CurrentPlayerData.storyStages[stageId] = new StageData
            {
                stageId = stageId,
                stars = 0,
                highScore = 0,
                isUnlocked = true
            };
        }
        else
        {
            CurrentPlayerData.storyStages[stageId].isUnlocked = true;
        }
        
        // 최고 스테이지 업데이트
        int stageNumber;
        if (int.TryParse(stageId, out stageNumber) && stageNumber > CurrentPlayerData.highestStage)
            CurrentPlayerData.highestStage = stageNumber;
            
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 스테이지 결과 업데이트
    /// </summary>
    public void UpdateStageResult(string stageId, int score, int stars)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(stageId)) return;
        
        Debug.Log($"UpdateStageResult 호출: 스테이지 {stageId}, 점수 {score}, 별 {stars}개");
        
        if (CurrentPlayerData.storyStages == null)
            CurrentPlayerData.storyStages = new Dictionary<string, StageData>();
            
        if (!CurrentPlayerData.storyStages.ContainsKey(stageId))
        {
            Debug.Log($"새 스테이지 데이터 생성: {stageId}");
            CurrentPlayerData.storyStages[stageId] = new StageData
            {
                stageId = stageId,
                stars = stars,
                highScore = score,
                isUnlocked = true,
                completedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
            };
            
            // 새 스테이지의 별은 무조건 총 별 개수에 추가
            CurrentPlayerData.totalStars += stars;
            Debug.Log($"새 스테이지의 별 {stars}개 추가, 총 별 개수: {CurrentPlayerData.totalStars}");
            OnTotalStarsChanged?.Invoke(CurrentPlayerData.totalStars);
        }
        else
        {
            StageData stageData = CurrentPlayerData.storyStages[stageId];
            Debug.Log($"기존 스테이지 데이터: 별 {stageData.stars}개, 최고점수 {stageData.highScore}");
            
            // 더 높은 점수만 저장
            if (score > stageData.highScore)
            {
                Debug.Log($"최고점수 갱신: {stageData.highScore} -> {score}");
                stageData.highScore = score;
            }
                
            // 별 개수 업데이트 로직
            if (stars > stageData.stars)
            {
                // 새로 획득한 별 수 계산
                int newStars = stars - stageData.stars;
                
                Debug.Log($"별 개수 갱신: {stageData.stars} -> {stars} (신규 {newStars}개)");
                
                // 별 업데이트
                stageData.stars = stars;
                
                // 총 별 개수 업데이트
                CurrentPlayerData.totalStars += newStars;
                Debug.Log($"총 별 개수 업데이트: {CurrentPlayerData.totalStars}");
                OnTotalStarsChanged?.Invoke(CurrentPlayerData.totalStars);
            }
            else
            {
                Debug.Log($"별 개수 유지: {stageData.stars}개 (획득 {stars}개)");
            }
            
            stageData.completedTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        }
        
        // 전체 통계 업데이트
        if (score > CurrentPlayerData.bestScore)
        {
            Debug.Log($"전체 최고점수 갱신: {CurrentPlayerData.bestScore} -> {score}");
            CurrentPlayerData.bestScore = score;
        }
            
        CurrentPlayerData.totalPlayCount++;
        
        // 저장
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 스테이지 데이터 가져오기
    /// </summary>
    public StageData GetStageData(string stageId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(stageId) || CurrentPlayerData.storyStages == null)
            return null;
            
        return CurrentPlayerData.storyStages.TryGetValue(stageId, out StageData stageData) ? stageData : null;
    }
    
    /// <summary>
    /// 스테이지 해금 여부 확인
    /// </summary>
    public bool IsStageUnlocked(string stageId)
    {
        StageData stageData = GetStageData(stageId);
        return stageData != null && stageData.isUnlocked;
    }

    /// <summary>
    /// 경쟁모드 최고점수 업데이트
    /// </summary>
    /// <param name="score"></param>
    public void UpdateCompetitiveBestScore(int score)
    {
        Debug.Log("UpdateScore");
        if (CurrentPlayerData.competitiveBestScore == 0)
        {
            CurrentPlayerData.competitiveBestScore = score;
        }
        if (score > CurrentPlayerData.competitiveBestScore)
        {
            CurrentPlayerData.competitiveBestScore = score;
        }
        _ = SavePlayerDataAsync();
    }
    #endregion

    #region 닉네임 및 설정 관련
    /// <summary>
    /// 닉네임 변경
    /// </summary>
    public void SetNickname(string nickname)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(nickname)) return;
        
        CurrentPlayerData.nickname = nickname;
        OnNicknameChanged?.Invoke(nickname);
        _ = SavePlayerDataAsync();
    }

    ///<summary>
    /// 볼륨 조절
    /// </summary>
    public void SetVolume(List<float> volumes)
    {
        if (!IsDataLoaded) return;
        
        Debug.Log($"SetVolume 호출됨 - 입력 volumes: {string.Join(", ", volumes)}, 현재 volumeList: {string.Join(", ", CurrentPlayerData.volumeList)}");

        // volumeList가 null인 경우 초기화
        if (CurrentPlayerData.volumeList == null)
        {
            CurrentPlayerData.volumeList = new List<float>();
        }
        
        // volumeList 크기 조정 (최소 3개 요소 보장)
        while (CurrentPlayerData.volumeList.Count < 3)
        {
            CurrentPlayerData.volumeList.Add(1f);
        }

        // volumes가 비어있는 경우 보호 로직
        if (volumes == null || volumes.Count == 0)
        {
            Debug.LogWarning("SetVolume: 입력 volumes가 null이거나 비어있습니다!");
            return;
        }

        // 볼륨 값 업데이트
        int count = Mathf.Min(volumes.Count, CurrentPlayerData.volumeList.Count);
        for (int i = 0; i < count; i++)
        {
            CurrentPlayerData.volumeList[i] = volumes[i];
        }
        
        Debug.Log($"볼륨 저장됨: {string.Join(", ", CurrentPlayerData.volumeList)}");
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 사운드 뮤트 여부 변경
    /// </summary>
    public void SetSoundEnabled(bool enabled)
    {
        if (!IsDataLoaded) return;
        
        CurrentPlayerData.soundEnabled = enabled;
        _ = SavePlayerDataAsync();
    }
    
    /// <summary>
    /// 진동 설정 변경
    /// </summary>
    public void SetVibrationEnabled(bool enabled)
    {
        if (!IsDataLoaded) return;
        
        CurrentPlayerData.vibrationEnabled = enabled;
        _ = SavePlayerDataAsync();
    }
    #endregion
    
    #region 캐릭터 관련
    /// <summary>
    /// 캐릭터 해금
    /// </summary>
    public void UnlockCharacter(string characterId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(characterId)) return;
        
        if (CurrentPlayerData.unlockedCharacters == null)
            CurrentPlayerData.unlockedCharacters = new List<string>();
            
        if (!CurrentPlayerData.unlockedCharacters.Contains(characterId))
        {
            CurrentPlayerData.unlockedCharacters.Add(characterId);
            _ = SavePlayerDataAsync();
        }
    }
    
    /// <summary>
    /// 캐릭터 선택
    /// </summary>
    public void SelectCharacter(string characterId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(characterId)) return;
        
        if (CurrentPlayerData.unlockedCharacters != null && 
            CurrentPlayerData.unlockedCharacters.Contains(characterId))
        {
            CurrentPlayerData.currentCharacter = characterId;
            _ = SavePlayerDataAsync();
        }
    }
    
    /// <summary>
    /// 캐릭터 해금 여부 확인
    /// </summary>
    public bool IsCharacterUnlocked(string characterId)
    {
        if (!IsDataLoaded || string.IsNullOrEmpty(characterId) || CurrentPlayerData.unlockedCharacters == null)
            return characterId == "dog"; // 기본 캐릭터는 항상 해금됨
            
        return CurrentPlayerData.unlockedCharacters.Contains(characterId);
    }
    #endregion
}