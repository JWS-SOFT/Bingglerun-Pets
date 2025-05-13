using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    public static TimerManager Instance;
    private Dictionary<ITimer, Coroutine> runningTimers = new Dictionary<ITimer, Coroutine>();
        
    // 게임이 일시정지 상태인지 확인
    public bool IsGamePaused { get; private set; } = false;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        Time.timeScale = 1f;
    }


    public void StartTimer(ITimer timer, bool autoStart = true)
    {
        if (runningTimers.ContainsKey(timer))
        {
            StopTimer(timer);
        }

        if (autoStart)
        {
            Coroutine timerCoroutine = StartCoroutine(TimerRoutine(timer));
            runningTimers[timer] = timerCoroutine;
        }
    }

    private IEnumerator TimerRoutine(ITimer timer)
    {
        BasicTimer basicTimer = timer as BasicTimer;
        basicTimer.Start();

        while (!timer.IsCompleted)
        {
            if (basicTimer.IsRunning && !basicTimer.IsPaused)
            {
                basicTimer.AddTime(Time.deltaTime);
            }
            yield return null;
        }
        runningTimers.Remove(timer);
    }

    public void StopTimer(ITimer timer)
    {
        if (runningTimers.TryGetValue(timer, out Coroutine timerCoroutine))
        {
            StopCoroutine(timerCoroutine);
            runningTimers.Remove(timer);
            timer.Stop();
        }
    }
    
    // 게임 일시정지 메서드
    public void PauseGame()
    {
        if (!IsGamePaused)
        {
            // 현재 타임스케일을 저장하고 0으로 설정
            Time.timeScale = 0f;
            IsGamePaused = true;
            
            //Debug.Log("게임 일시정지: 타임스케일 0으로 설정");
        }
    }

    // 게임 재개 메서드
    public void ResumeGame()
    {
        if (IsGamePaused)
        {
            // 이전 타임스케일로 복원
            Time.timeScale = 1f;
            IsGamePaused = false;
            
            //Debug.Log("게임 재개");
        }
    }
}
