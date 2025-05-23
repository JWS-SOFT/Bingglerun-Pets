using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

/// <summary>
/// 직렬화 가능한 스테이지 데이터 키-값 쌍
/// </summary>
[Serializable]
public class SerializableStageData
{
    public string key;
    public StageData value;
}

/// <summary>
/// 직렬화 가능한 추가 데이터 키-값 쌍
/// </summary>
[Serializable]
public class SerializableAdditionalData
{
    public string key;
    public string value; // JSON 직렬화된 값으로 저장
}

/// <summary>
/// 직렬화 가능한 아이템 데이터 키-값 쌍
/// </summary>
[Serializable]
public class SerializableItemData
{
    public string key;   // 아이템 ID
    public int value;    // 아이템 수량
}

/// <summary>
/// 캐릭터별 경쟁모드 점수 데이터
/// </summary>
[Serializable]
public class SerializableCharacterScore
{
    public string characterName;        // 캐릭터 이름 (Dog, Cat, Hamster 등)
    public int bestScore;              // 해당 캐릭터로 달성한 최고 점수
    public long bestScoreTimestamp;    // 최고 점수 달성 시간
    
    public SerializableCharacterScore()
    {
        characterName = "";
        bestScore = 0;
        bestScoreTimestamp = 0;
    }
    
    public SerializableCharacterScore(string character, int score, long timestamp)
    {
        characterName = character;
        bestScore = score;
        bestScoreTimestamp = timestamp;
    }
}

/// <summary>
/// 플레이어 데이터를 저장하는 구조체 (Firebase 직렬화용)
/// </summary>
[Serializable]
public class PlayerData
{
    // 기본 정보
    public string playerId;
    public string nickname;
    public int level;
    public int experience;
    
    // 재화
    public int gold;
    public int diamond;
    public int heart;
    
    // 게임 진행
    [NonSerialized]
    public Dictionary<string, StageData> storyStages; // 메모리에서만 사용됨
    public List<SerializableStageData> storyStagesList = new List<SerializableStageData>(); // 직렬화용
    
    public int highestStage;
    public int totalStars;
    
    // 아이템 및 장식
    [NonSerialized]
    public Dictionary<string, int> items; // 메모리에서만 사용됨
    
    // 아이템 리스트 (직렬화용) - 새 형식으로 변경
    public List<SerializableItemData> itemsList = new List<SerializableItemData>();
    
    // 이전 형식 아이템 리스트 (하위 호환성 유지용)
    [Obsolete("이전 형식의 아이템 리스트, 새 형식으로 마이그레이션 필요")]
    private List<KeyValuePair<string, int>> oldItemsList = new List<KeyValuePair<string, int>>();
    
    public List<string> unlockedDecorations; // 해금된 장식 ID 목록
    
    // 장착 중인 아이템/장식
    public string equippedHat;
    public string equippedBody;
    public string equippedShoes;
    public string selectedPreGameItem; // 선택된 시작 아이템

    // 게임 설정
    public List<float> volumeList = new List<float>();
    public bool soundEnabled;
    public bool vibrationEnabled;
    
    // 캐릭터 관련
    public List<string> unlockedCharacters;
    public string currentCharacter;
    
    // 통계 데이터
    public int totalPlayCount;
    public int totalCoinsCollected;
    public int competitiveBestScore; // 경쟁모드 전용 베스트 스코어 (전체 최고점수)
    
    // 경쟁모드 관련 추가 정보
    public string competitiveBestCharacter; // 최고 점수를 기록한 캐릭터
    public long competitiveBestScoreTimestamp; // 최고 점수 기록 시간
    
    // 캐릭터별 경쟁모드 점수 (새로운 시스템)
    public List<SerializableCharacterScore> characterCompetitiveScores = new List<SerializableCharacterScore>();
    
    // 마지막 업데이트 시간
    public long lastUpdateTimestamp;
    
    /// <summary>
    /// Dictionary와 List 간 변환 메서드 (스테이지 데이터)
    /// </summary>
    public void InitializeStagesFromList()
    {
        storyStages = new Dictionary<string, StageData>();
        Debug.Log($"[PlayerData] 스테이지 데이터 Dictionary 초기화 시작 - 리스트 항목 수: {(storyStagesList != null ? storyStagesList.Count : 0)}");
        
        if (storyStagesList != null && storyStagesList.Count > 0)
        {
            foreach (var item in storyStagesList)
            {
                if (item.key != null && item.value != null)
                {
                    storyStages[item.key] = item.value;
                    Debug.Log($"[PlayerData] 스테이지 {item.key} 데이터 변환 - 잠금 상태: {item.value.isUnlocked}, 별 개수: {item.value.stars}");
                }
                else
                {
                    Debug.LogWarning("[PlayerData] 스테이지 데이터 변환 오류: 키 또는 값이 null입니다.");
                }
            }
        }
        else
        {
            Debug.LogWarning("[PlayerData] 변환할 스테이지 리스트가 비어있거나 null입니다.");
        }
        
        Debug.Log($"[PlayerData] 스테이지 데이터 Dictionary 초기화 완료 - {(storyStages != null ? storyStages.Count : 0)}개 항목");
    }
    
    /// <summary>
    /// Dictionary를 List로 변환 (스테이지 데이터)
    /// </summary>
    public void UpdateListFromDictionary()
    {
        if (storyStagesList == null)
            storyStagesList = new List<SerializableStageData>();
        else
            storyStagesList.Clear();
            
        Debug.Log($"[PlayerData] 스테이지 List 업데이트 시작 - Dictionary 항목 수: {(storyStages != null ? storyStages.Count : 0)}");
            
        if (storyStages != null && storyStages.Count > 0)
        {
            foreach (var pair in storyStages)
            {
                storyStagesList.Add(new SerializableStageData { key = pair.Key, value = pair.Value });
                Debug.Log($"[PlayerData] 스테이지 {pair.Key} 리스트에 추가 - 잠금 상태: {pair.Value.isUnlocked}, 별 개수: {pair.Value.stars}");
            }
        }
        else
        {
            Debug.LogWarning("[PlayerData] 변환할 스테이지 Dictionary가 비어있거나 null입니다.");
        }
        
        Debug.Log($"[PlayerData] 스테이지 List 업데이트 완료 - {storyStagesList.Count}개 항목");
    }
    
    /// <summary>
    /// Dictionary와 List 간 변환 메서드 (아이템 데이터)
    /// </summary>
    public void InitializeItemsFromList()
    {
        items = new Dictionary<string, int>();
        Debug.Log($"[PlayerData] 아이템 데이터 Dictionary 초기화 시작");
        
        // 새 형식 아이템 리스트 처리
        if (itemsList != null && itemsList.Count > 0)
        {
            Debug.Log($"[PlayerData] 새 형식 아이템 리스트에서 불러오기 - 항목 수: {itemsList.Count}");
            foreach (var item in itemsList)
            {
                if (!string.IsNullOrEmpty(item.key))
                {
                    items[item.key] = item.value;
                    Debug.Log($"[PlayerData] 아이템 {item.key} 변환 완료 - 수량: {item.value}");
                }
            }
        }
        // 이전 형식 아이템 리스트 처리 (하위 호환성)
        else if (oldItemsList != null && oldItemsList.Count > 0)
        {
            Debug.Log($"[PlayerData] 이전 형식 아이템 리스트에서 불러오기 - 항목 수: {oldItemsList.Count}");
            foreach (var item in oldItemsList)
            {
                if (!string.IsNullOrEmpty(item.Key))
                {
                    items[item.Key] = item.Value;
                    // 새 형식으로 마이그레이션
                    itemsList.Add(new SerializableItemData { key = item.Key, value = item.Value });
                }
            }
            // 이전 형식은 이제 비움
            oldItemsList.Clear();
            Debug.Log("[PlayerData] 이전 형식에서 새 형식으로 아이템 데이터 마이그레이션 완료");
        }
        else
        {
            Debug.LogWarning("[PlayerData] 변환할 아이템 리스트가 비어있거나 null입니다.");
        }
        
        Debug.Log($"[PlayerData] 아이템 데이터 Dictionary 초기화 완료 - {items.Count}개 항목");
    }
    
    /// <summary>
    /// Dictionary를 List로 변환 (아이템 데이터)
    /// </summary>
    public void UpdateItemsListFromDictionary()
    {
        if (itemsList == null)
            itemsList = new List<SerializableItemData>();
        else
            itemsList.Clear();
            
        Debug.Log($"[PlayerData] 아이템 List 업데이트 시작 - Dictionary 항목 수: {(items != null ? items.Count : 0)}");
            
        if (items != null && items.Count > 0)
        {
            foreach (var pair in items)
            {
                if (!string.IsNullOrEmpty(pair.Key))
                {
                    itemsList.Add(new SerializableItemData { key = pair.Key, value = pair.Value });
                    Debug.Log($"[PlayerData] 아이템 {pair.Key} 리스트에 추가 - 수량: {pair.Value}");
                }
            }
        }
        else
        {
            Debug.LogWarning("[PlayerData] 변환할 아이템 Dictionary가 비어있거나 null입니다.");
        }
        
        Debug.Log($"[PlayerData] 아이템 List 업데이트 완료 - {itemsList.Count}개 항목");
    }
    
    /// <summary>
    /// 기본 플레이어 데이터 생성
    /// </summary>
    public static PlayerData CreateDefault(string userId)
    {
        var playerData = new PlayerData
        {
            playerId = userId,
            nickname = "Player" + UnityEngine.Random.Range(1000, 9999),
            level = 1,
            experience = 0,
            gold = 1000,
            diamond = 50,
            heart = 5,
            storyStages = new Dictionary<string, StageData>
            {
                { "1", new StageData { stageId = "1", stars = 0, highScore = 0, isUnlocked = true } }
            },
            highestStage = 1,
            totalStars = 0,
            items = new Dictionary<string, int>(),
            unlockedDecorations = new List<string>(),
            unlockedCharacters = new List<string> { "dog" },
            currentCharacter = "dog",
            equippedHat = "",
            equippedBody = "",
            equippedShoes = "",
            selectedPreGameItem = "",
            volumeList = { 1f, 1f, 1f },
            soundEnabled = true,
            vibrationEnabled = true,
            totalPlayCount = 0,
            totalCoinsCollected = 0,
            competitiveBestScore = 0,
            competitiveBestCharacter = "",
            competitiveBestScoreTimestamp = 0,
            characterCompetitiveScores = new List<SerializableCharacterScore>(),
            lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
        
        // 리스트 초기화
        playerData.UpdateListFromDictionary();
        playerData.UpdateItemsListFromDictionary();
        
        return playerData;
    }

    /// <summary>
    /// 특정 캐릭터의 최고 점수 반환
    /// </summary>
    public int GetCharacterBestScore(string characterName)
    {
        if (string.IsNullOrEmpty(characterName) || characterCompetitiveScores == null)
            return 0;
            
        var characterScore = characterCompetitiveScores.Find(cs => 
            string.Equals(cs.characterName, characterName, StringComparison.OrdinalIgnoreCase));
        
        return characterScore?.bestScore ?? 0;
    }
    
    /// <summary>
    /// 특정 캐릭터의 점수 업데이트 (더 높은 점수일 때만)
    /// </summary>
    public bool UpdateCharacterScore(string characterName, int newScore)
    {
        if (string.IsNullOrEmpty(characterName) || newScore <= 0)
        {
            Debug.LogWarning($"[PlayerData] 유효하지 않은 캐릭터 점수 업데이트 시도: {characterName}, 점수: {newScore}");
            return false;
        }
        
        if (characterCompetitiveScores == null)
            characterCompetitiveScores = new List<SerializableCharacterScore>();
        
        // 기존 캐릭터 점수 찾기
        var existingScore = characterCompetitiveScores.Find(cs => 
            string.Equals(cs.characterName, characterName, StringComparison.OrdinalIgnoreCase));
        
        bool scoreUpdated = false;
        long currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
        if (existingScore != null)
        {
            // 기존 점수보다 높은 경우에만 업데이트
            if (newScore > existingScore.bestScore)
            {
                existingScore.bestScore = newScore;
                existingScore.bestScoreTimestamp = currentTimestamp;
                scoreUpdated = true;
                Debug.Log($"[PlayerData] {characterName} 캐릭터 점수 업데이트: {newScore}");
            }
        }
        else
        {
            // 새로운 캐릭터 점수 추가
            characterCompetitiveScores.Add(new SerializableCharacterScore(characterName, newScore, currentTimestamp));
            scoreUpdated = true;
            Debug.Log($"[PlayerData] {characterName} 캐릭터 새로운 점수 추가: {newScore}");
        }
        
        // 전체 최고 점수 업데이트
        if (scoreUpdated && newScore > competitiveBestScore)
        {
            competitiveBestScore = newScore;
            competitiveBestCharacter = characterName;
            competitiveBestScoreTimestamp = currentTimestamp;
            Debug.Log($"[PlayerData] 전체 최고 점수 업데이트: {newScore} ({characterName})");
        }
        
        return scoreUpdated;
    }
    
    /// <summary>
    /// 모든 캐릭터별 점수 목록 반환
    /// </summary>
    public List<SerializableCharacterScore> GetAllCharacterScores()
    {
        return characterCompetitiveScores ?? new List<SerializableCharacterScore>();
    }
    
    /// <summary>
    /// 기존 데이터에서 캐릭터별 점수로 마이그레이션
    /// </summary>
    public void MigrateToCharacterScores()
    {
        // 기존 competitiveBestScore가 있고 캐릭터별 점수가 없는 경우 마이그레이션
        if (competitiveBestScore > 0 && 
            (characterCompetitiveScores == null || characterCompetitiveScores.Count == 0))
        {
            if (characterCompetitiveScores == null)
                characterCompetitiveScores = new List<SerializableCharacterScore>();
            
            string character = !string.IsNullOrEmpty(competitiveBestCharacter) ? competitiveBestCharacter : "Dog";
            long timestamp = competitiveBestScoreTimestamp > 0 ? competitiveBestScoreTimestamp : DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            
            characterCompetitiveScores.Add(new SerializableCharacterScore(character, competitiveBestScore, timestamp));
            
            Debug.Log($"[PlayerData] 기존 점수를 캐릭터별 점수로 마이그레이션: {character} - {competitiveBestScore}");
        }
    }
}

/// <summary>
/// 스테이지 데이터를 저장하는 구조체
/// </summary>
[Serializable]
public class StageData
{
    public string stageId;
    public int stars; // 0-3
    public int highScore;
    public bool isUnlocked;
    public long completedTimestamp; // 스테이지를 마지막으로 완료한 시간
    
    // 추가 스테이지 데이터 (필요시 확장)
    [NonSerialized]
    public Dictionary<string, object> additionalData;
    public List<SerializableAdditionalData> additionalDataList = new List<SerializableAdditionalData>();
    
    /// <summary>
    /// Dictionary와 List 간 변환 메서드 (추가 데이터)
    /// </summary>
    public void InitializeAdditionalDataFromList()
    {
        additionalData = new Dictionary<string, object>();
        if (additionalDataList != null)
        {
            foreach (var item in additionalDataList)
            {
                if (item.key != null && item.value != null)
                {
                    try
                    {
                        // 간단한 방식 - 실제로는 타입에 따라 다르게 처리해야 할 수 있음
                        additionalData[item.key] = item.value;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"추가 데이터 변환 오류: {e.Message}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Dictionary를 List로 변환 (추가 데이터)
    /// </summary>
    public void UpdateAdditionalDataListFromDictionary()
    {
        if (additionalDataList == null)
            additionalDataList = new List<SerializableAdditionalData>();
        else
            additionalDataList.Clear();
            
        if (additionalData != null)
        {
            foreach (var pair in additionalData)
            {
                try
                {
                    // 값을 JSON으로 직렬화하여 저장
                    string jsonValue = JsonUtility.ToJson(pair.Value);
                    additionalDataList.Add(new SerializableAdditionalData { key = pair.Key, value = jsonValue });
                }
                catch (Exception e)
                {
                    Debug.LogError($"추가 데이터 직렬화 오류: {e.Message}");
                }
            }
        }
    }
}