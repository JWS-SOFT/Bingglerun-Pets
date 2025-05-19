using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;

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
    public Dictionary<string, StageData> storyStages; // 스테이지 ID를 키로 사용
    public int highestStage;
    public int totalStars;
    
    // 아이템 및 장식
    public Dictionary<string, int> items; // 아이템 ID, 수량
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
    public int bestScore;
    public int totalCoinsCollected;
    public int competitiveBestScore;
    
    // 마지막 업데이트 시간
    public long lastUpdateTimestamp;
    
    /// <summary>
    /// 기본 플레이어 데이터 생성
    /// </summary>
    public static PlayerData CreateDefault(string userId)
    {
        return new PlayerData
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
            bestScore = 0,
            totalCoinsCollected = 0,
            lastUpdateTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        };
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
    public Dictionary<string, object> additionalData;
}