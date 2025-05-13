public interface ITimer
{
    float Duration { get; }             // 타이머의 총 지속 시간
    float ElapsedTime { get; }          // 경과 시간
    bool IsRunning { get; }             // 타이머가 실행 중인지 여부
    float RemainingTime { get; }        // 남은 시간 (읽기 전용)
    bool IsCompleted => ElapsedTime >= Duration; // IsCompleted 상태를 추가

    void Start();                       // 타이머 시작
    void Stop();                        // 타이머 중지 및 초기화
    void Pause();                       // 타이머 일시정지
    void Resume();                      // 일시정지에서 다시 시작
    string ToString();                  // 시간 포맷
}
