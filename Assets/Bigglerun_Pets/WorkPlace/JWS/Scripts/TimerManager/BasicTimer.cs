using UnityEngine;

public class BasicTimer : ITimer
{
    public float Duration { get; private set; }
    public float ElapsedTime { get; private set; }
    public bool IsRunning { get; private set; }
    public bool IsPaused { get; private set; }

    public float RemainingTime => Mathf.Max(0, Duration - ElapsedTime);
    public float RemainingPercent => Mathf.Max(0, RemainingTime / Duration);
    public bool IsCompleted => ElapsedTime >= Duration; // IsCompleted 상태를 추가

    public BasicTimer(float duration)
    {
        Duration = duration;
        ElapsedTime = 0f;
        IsRunning = false;
        IsPaused = false;
    }

    public void Start()
    {
        IsRunning = true;
        IsPaused = false;
        ElapsedTime = 0f;
    }

    public void Stop()
    {
        IsRunning = false;
        ElapsedTime = 0f;
    }

    public void Pause()
    {
        IsPaused = true;
    }

    public void Resume()
    {
        if (IsRunning)
        {
            IsPaused = false;
        }
    }

    public override string ToString()
    {
        return $"{RemainingTime:F2} / {Duration:F2}";
    }

    public void AddTime(float deltaTime)
    {
        if (IsRunning && !IsPaused && !IsCompleted)
        {
            ElapsedTime += deltaTime;

            // 음수 방지
            if (ElapsedTime < 0f) ElapsedTime = 0f;

            // 완료 처리
            if (ElapsedTime >= Duration)
            {
                IsRunning = false;
            }
        }
    }

}
