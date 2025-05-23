using UnityEngine;
using System;
using System.Runtime.CompilerServices;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    private int score;
    private int stepCount;           // 계단 수, 중복 계산 방지용
    private float horizontalDistance; // 이동 거리 누적값
    private int coin;
    
    [Header("Star System")]
    public int maxStars = 3;         // 최대 별 개수
    private int currentStars;        // 현재 별 개수

    [Header("Score Settings")]
    public int scorePerStep = 5;     // 계단 한 층당 점수
    public int scorePerMeter = 1;    // 1미터당 점수

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        // DontDestroyOnLoad(gameObject);
        
        // 게임 시작시 별 초기화
        ResetStars();
    }

    public void ResetScore()
    {
        score = 0;
        stepCount = 0;
        horizontalDistance = 0f;
        coin = 0;
        ResetStars();
    }
    
    // 별 개수 초기화
    public void ResetStars()
    {
        currentStars = maxStars;
    }
    
    // 별 감소 (장애물 충돌, 추락 등 이벤트 발생시 호출)
    public void DecreaseStar()
    {
        if (currentStars > 0)
        {
            currentStars--;
            Debug.Log($"별 감소: 남은 별 {currentStars}개");
        }
    }
    
    // 별 개수 조회
    public int GetStars()
    {
        return currentStars;
    }

    public void SetStars(int count)
    {
        currentStars = count;
    }

    // 계단 한 층 오를 때마다 호출
    public void AddStep()
    {
        stepCount++;
        score += scorePerStep;
    }

    // 횡모드에서 일정 거리 이동 시 호출 (누적)
    public void AddHorizontalDistance(float distance)
    {
        horizontalDistance += distance;

        int earnedScore = Mathf.FloorToInt(horizontalDistance * scorePerMeter);
        score += earnedScore;
        horizontalDistance -= earnedScore / (float)scorePerMeter; // 이미 계산된 부분 제거
    }
    
    public void AddCoin()
    {
        coin++;
    }

    public int GetScore()
    {
        return score;
    }

    public int GetStepCount()
    {
        return stepCount;
    }

    public float GetHorizontalDistance()
    {
        return horizontalDistance;
    }

    public int GetCoin()
    {
        return coin;
    }
}