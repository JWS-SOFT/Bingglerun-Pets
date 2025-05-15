using UnityEngine;
using UnityEngine.Events;

namespace OrientationSystem
{
    [System.Serializable]
    public class OrientationChangedEvent : UnityEvent<bool> { } // bool: true = 가로모드, false = 세로모드

    public class OrientationDetector : MonoBehaviour
    {
        [Header("설정")]
        [Tooltip("화면 비율이 이 값보다 크면 가로 모드로 판단")]
        public float aspectRatioThreshold = 1.0f;
        public bool checkOnStart = true;
        public bool checkContinuously = true;
        public float checkInterval = 0.5f; // 연속 체크 시 시간 간격

        [Header("이벤트")]
        public OrientationChangedEvent onOrientationChanged = new OrientationChangedEvent();
        public UnityEvent onLandscapeModeEntered = new UnityEvent();
        public UnityEvent onPortraitModeEntered = new UnityEvent();

        private bool isLandscape = true;
        private float lastCheckTime = 0f;
        private float lastScreenWidth, lastScreenHeight;

        private void Start()
        {
            // 초기 화면 크기 저장
            lastScreenWidth = Screen.width;
            lastScreenHeight = Screen.height;

            if (checkOnStart)
            {
                CheckOrientation(true);
            }
        }

        private void Update()
        {
            if (checkContinuously && Time.time - lastCheckTime >= checkInterval)
            {
                // 화면 크기가 변경되었는지 확인 (불필요한 검사 방지)
                if (Screen.width != lastScreenWidth || Screen.height != lastScreenHeight)
                {
                    lastScreenWidth = Screen.width;
                    lastScreenHeight = Screen.height;
                    CheckOrientation(false);
                }
                lastCheckTime = Time.time;
            }
        }

        /// <summary>
        /// 현재 화면 방향을 확인하고 필요 시 이벤트 발생
        /// </summary>
        /// <param name="forceNotify">변화가 없더라도 강제로 이벤트 발생시킬지 여부</param>
        public void CheckOrientation(bool forceNotify = false)
        {
            // 현재 화면 비율 계산
            float screenRatio = (float)Screen.width / Screen.height;
            
            // 비율이 임계값보다 작으면 세로 모드, 크면 가로 모드
            bool newIsLandscape = screenRatio >= aspectRatioThreshold;

            // 방향이 변경되었거나 강제 알림이 요청된 경우
            if (newIsLandscape != isLandscape || forceNotify)
            {
                isLandscape = newIsLandscape;
                
                // 이벤트 발생
                onOrientationChanged.Invoke(isLandscape);
                
                if (isLandscape)
                {
                    onLandscapeModeEntered.Invoke();
                    Debug.Log("OrientationDetector: 가로 모드 감지됨");
                }
                else
                {
                    onPortraitModeEntered.Invoke();
                    Debug.Log("OrientationDetector: 세로 모드 감지됨");
                }
            }
        }

        /// <summary>
        /// 현재 장치가 가로 모드인지 확인 (외부에서 호출 가능)
        /// </summary>
        public bool IsLandscapeMode()
        {
            return isLandscape;
        }
    }
} 