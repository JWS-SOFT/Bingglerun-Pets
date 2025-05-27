using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 백그라운드 스레드에서 메인 스레드로 작업을 전달하기 위한 디스패처
/// Firebase 콜백 등에서 UI 업데이트를 안전하게 수행하기 위해 사용
/// </summary>
public class UnityMainThreadDispatcher : MonoBehaviour
{
    public static UnityMainThreadDispatcher Instance { get; private set; }
    
    private readonly Queue<Action> executionQueue = new Queue<Action>();
    private readonly object queueLock = new object();
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
        
        Debug.Log("[UnityMainThreadDispatcher] 초기화 완료");
    }
    
    private void Update()
    {
        // 큐에 있는 모든 작업을 메인 스레드에서 실행
        lock (queueLock)
        {
            while (executionQueue.Count > 0)
            {
                try
                {
                    executionQueue.Dequeue().Invoke();
                }
                catch (Exception e)
                {
                    Debug.LogError($"[UnityMainThreadDispatcher] 작업 실행 중 오류: {e.Message}");
                }
            }
        }
    }
    
    /// <summary>
    /// 메인 스레드에서 실행할 작업을 큐에 추가
    /// </summary>
    /// <param name="action">실행할 작업</param>
    public void Enqueue(Action action)
    {
        if (action == null)
        {
            Debug.LogWarning("[UnityMainThreadDispatcher] null 작업은 큐에 추가할 수 없습니다.");
            return;
        }
        
        lock (queueLock)
        {
            executionQueue.Enqueue(action);
        }
    }
    
    /// <summary>
    /// 코루틴을 메인 스레드에서 시작
    /// </summary>
    /// <param name="coroutine">시작할 코루틴</param>
    public void EnqueueCoroutine(IEnumerator coroutine)
    {
        if (coroutine == null)
        {
            Debug.LogWarning("[UnityMainThreadDispatcher] null 코루틴은 시작할 수 없습니다.");
            return;
        }
        
        Enqueue(() => StartCoroutine(coroutine));
    }
    
    /// <summary>
    /// 현재 스레드가 메인 스레드인지 확인
    /// </summary>
    /// <returns>메인 스레드이면 true</returns>
    public static bool IsMainThread()
    {
        return System.Threading.Thread.CurrentThread.ManagedThreadId == 1;
    }
    
    /// <summary>
    /// 안전하게 메인 스레드에서 작업 실행
    /// 이미 메인 스레드라면 즉시 실행, 아니라면 큐에 추가
    /// </summary>
    /// <param name="action">실행할 작업</param>
    public static void SafeExecute(Action action)
    {
        if (action == null)
        {
            return;
        }
        
        if (IsMainThread())
        {
            // 이미 메인 스레드라면 즉시 실행
            try
            {
                action.Invoke();
            }
            catch (Exception e)
            {
                Debug.LogError($"[UnityMainThreadDispatcher] 즉시 실행 중 오류: {e.Message}");
            }
        }
        else
        {
            // 백그라운드 스레드라면 큐에 추가
            if (Instance != null)
            {
                Instance.Enqueue(action);
            }
            else
            {
                Debug.LogError("[UnityMainThreadDispatcher] Instance가 null입니다. 메인 스레드에서 작업을 실행할 수 없습니다.");
            }
        }
    }
    
    private void OnDestroy()
    {
        Debug.Log("[UnityMainThreadDispatcher] 종료");
    }
}