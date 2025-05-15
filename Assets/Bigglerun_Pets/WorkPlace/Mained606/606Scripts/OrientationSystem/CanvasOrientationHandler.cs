using UnityEngine;
using UnityEngine.UI;

namespace OrientationSystem
{
    public class CanvasOrientationHandler : MonoBehaviour
    {
        [Header("캔버스 설정")]
        public Canvas targetCanvas;
        public CanvasScaler canvasScaler;

        [Header("가로 모드 설정")]
        public Vector2 landscapeReferenceResolution = new Vector2(1920, 1080);
        public float landscapeMatchWidthOrHeight = 0f; // 0은 너비 기준, 1은 높이 기준, 중간값은 혼합

        [Header("세로 모드 설정")]
        public Vector2 portraitReferenceResolution = new Vector2(1080, 1920);
        public float portraitMatchWidthOrHeight = 1f;

        [Header("설정")]
        [Tooltip("화면 비율이 이 값보다 크면 가로 모드로 판단")]
        public float aspectRatioThreshold = 1.0f;
        public bool updateOnStart = true;
        public bool updateContinuously = true;

        private bool isLandscape = true;
        private float currentScreenRatio;

        private void Awake()
        {
            // 참조값이 없으면 자동으로 찾기
            if (targetCanvas == null)
            {
                targetCanvas = GetComponent<Canvas>();
            }

            if (canvasScaler == null && targetCanvas != null)
            {
                canvasScaler = targetCanvas.GetComponent<CanvasScaler>();
            }

            // 자체적으로 가지고 있지 않으면 씬에서 찾기
            if (canvasScaler == null)
            {
                Debug.LogWarning("CanvasOrientationHandler: CanvasScaler를 찾을 수 없습니다. Canvas가 UI 컴포넌트인지 확인하세요.");
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    CanvasScaler cs = canvas.GetComponent<CanvasScaler>();
                    if (cs != null)
                    {
                        targetCanvas = canvas;
                        canvasScaler = cs;
                        break;
                    }
                }
            }

            if (canvasScaler == null)
            {
                Debug.LogError("CanvasOrientationHandler: 씬에서 CanvasScaler 컴포넌트를 찾을 수 없습니다!");
            }
        }

        private void Start()
        {
            if (updateOnStart)
            {
                UpdateCanvasOrientation();
            }
        }

        private void Update()
        {
            if (updateContinuously)
            {
                UpdateCanvasOrientation();
            }
        }

        /// <summary>
        /// 현재 화면 비율에 따라 캔버스 스케일러 설정을 업데이트
        /// </summary>
        public void UpdateCanvasOrientation()
        {
            if (canvasScaler == null) return;

            // 현재 화면 비율 계산
            float screenRatio = (float)Screen.width / Screen.height;

            // 비율이 임계값보다 작으면 세로 모드, 크면 가로 모드
            bool newIsLandscape = screenRatio >= aspectRatioThreshold;

            // 모드가 변경되었거나 비율이 변경되었을 때만 캔버스 설정 업데이트
            if (newIsLandscape != isLandscape || screenRatio != currentScreenRatio)
            {
                isLandscape = newIsLandscape;
                currentScreenRatio = screenRatio;
                ApplyCanvasSettings();
            }
        }

        /// <summary>
        /// 현재 방향에 맞게 캔버스 설정 적용
        /// </summary>
        private void ApplyCanvasSettings()
        {
            if (canvasScaler == null) return;

            if (isLandscape)
            {
                // 가로 모드 설정 적용
                canvasScaler.referenceResolution = landscapeReferenceResolution;
                canvasScaler.matchWidthOrHeight = landscapeMatchWidthOrHeight;
                Debug.Log("화면 방향: 가로 모드 적용됨");
            }
            else
            {
                // 세로 모드 설정 적용
                canvasScaler.referenceResolution = portraitReferenceResolution;
                canvasScaler.matchWidthOrHeight = portraitMatchWidthOrHeight;
                Debug.Log("화면 방향: 세로 모드 적용됨");
            }
        }

        /// <summary>
        /// 강제로 방향 업데이트 요청 (외부에서 호출 가능)
        /// </summary>
        public void ForceUpdateOrientation()
        {
            UpdateCanvasOrientation();
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