using UnityEngine;
using UnityEngine.SceneManagement;

namespace OrientationSystem
{
    [RequireComponent(typeof(OrientationDetector))]
    public class AutoCanvasOrienter : MonoBehaviour
    {
        [Header("캔버스 설정")]
        [Tooltip("자동으로 방향을 조정할 캔버스들 (비워두면 모든 캔버스를 찾습니다)")]
        public Canvas[] canvasesToAdjust;
        public bool findAllCanvasesIfEmpty = true;

        [Header("가로 모드 설정")]
        public Vector2 landscapeReferenceResolution = new Vector2(1920, 1080);
        public float landscapeMatchWidthOrHeight = 0f; // 0은 너비 기준, 1은 높이 기준

        [Header("세로 모드 설정")]
        public Vector2 portraitReferenceResolution = new Vector2(1080, 1920);
        public float portraitMatchWidthOrHeight = 1f;

        [Header("씬 전환 설정")]
        [Tooltip("씬 전환 시 자동으로 새 캔버스를 찾을지 여부")]
        public bool findCanvasesOnSceneLoad = true;
        [Tooltip("DontDestroyOnLoad 캔버스도 처리할지 여부 (여러 씬에 걸쳐 존재하는 캔버스)")]
        public bool handleDontDestroyOnLoadCanvas = true;

        private OrientationDetector orientationDetector;
        private CanvasOrientationHandler[] canvasHandlers;

        private void Awake()
        {
            orientationDetector = GetComponent<OrientationDetector>();

            // 씬 전환 감지를 위해 이 객체를 유지
            if (findCanvasesOnSceneLoad)
            {
                DontDestroyOnLoad(gameObject);
            }

            FindAndSetupCanvases();
        }

        private void OnEnable()
        {
            // 방향 감지기에 이벤트 등록
            if (orientationDetector != null)
            {
                orientationDetector.onOrientationChanged.AddListener(OnOrientationChanged);
            }

            // 씬 로드 이벤트 구독
            if (findCanvasesOnSceneLoad)
            {
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
        }

        private void OnDisable()
        {
            // 이벤트 등록 해제
            if (orientationDetector != null)
            {
                orientationDetector.onOrientationChanged.RemoveListener(OnOrientationChanged);
            }

            // 씬 로드 이벤트 구독 해제
            if (findCanvasesOnSceneLoad)
            {
                SceneManager.sceneLoaded -= OnSceneLoaded;
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Debug.Log($"씬 '{scene.name}'이(가) 로드됨: 캔버스 찾기 및 설정 중...");
            FindAndSetupCanvases();
            
            // 현재 방향에 맞게 즉시 업데이트
            bool isLandscape = orientationDetector.IsLandscapeMode();
            OnOrientationChanged(isLandscape);
        }

        private void FindAndSetupCanvases()
        {
            if (canvasesToAdjust == null || canvasesToAdjust.Length == 0 || findAllCanvasesIfEmpty)
            {
                canvasesToAdjust = FindObjectsOfType<Canvas>();
                Debug.Log($"AutoCanvasOrienter: {canvasesToAdjust.Length}개의 캔버스를 자동 발견");
            }

            SetupCanvasHandlers();
        }

        private void SetupCanvasHandlers()
        {
            if (canvasesToAdjust == null || canvasesToAdjust.Length == 0)
            {
                Debug.LogWarning("AutoCanvasOrienter: 조정할 캔버스가 없습니다!");
                return;
            }

            // 기존 핸들러 정리
            if (canvasHandlers != null)
            {
                foreach (var handler in canvasHandlers)
                {
                    if (handler != null && handler.gameObject != null)
                    {
                        // DontDestroyOnLoad인 경우 기존 핸들러 유지 여부 결정
                        if (!handleDontDestroyOnLoadCanvas && IsInDontDestroyOnLoadScene(handler.gameObject))
                        {
                            continue;
                        }
                    }
                }
            }

            canvasHandlers = new CanvasOrientationHandler[canvasesToAdjust.Length];

            for (int i = 0; i < canvasesToAdjust.Length; i++)
            {
                Canvas canvas = canvasesToAdjust[i];
                if (canvas == null) continue;

                // DontDestroyOnLoad 설정된 캔버스 처리 여부 확인
                if (!handleDontDestroyOnLoadCanvas && IsInDontDestroyOnLoadScene(canvas.gameObject))
                {
                    Debug.Log($"캔버스 '{canvas.name}'은(는) DontDestroyOnLoad 상태로, 설정에 따라 건너뜁니다.");
                    continue;
                }

                // 이미 CanvasOrientationHandler가 있는지 확인
                CanvasOrientationHandler handler = canvas.GetComponent<CanvasOrientationHandler>();
                
                if (handler == null)
                {
                    // 새로 추가
                    handler = canvas.gameObject.AddComponent<CanvasOrientationHandler>();
                    handler.updateOnStart = false; // 우리가 직접 호출할 것이므로
                    handler.updateContinuously = false; // 이벤트에 의해서만 업데이트
                }

                // 설정 복사
                handler.targetCanvas = canvas;
                handler.landscapeReferenceResolution = landscapeReferenceResolution;
                handler.landscapeMatchWidthOrHeight = landscapeMatchWidthOrHeight;
                handler.portraitReferenceResolution = portraitReferenceResolution;
                handler.portraitMatchWidthOrHeight = portraitMatchWidthOrHeight;
                handler.aspectRatioThreshold = orientationDetector.aspectRatioThreshold;

                canvasHandlers[i] = handler;
            }
        }

        private bool IsInDontDestroyOnLoadScene(GameObject obj)
        {
            // DontDestroyOnLoad 상태의 오브젝트는 Scene 이름이 "DontDestroyOnLoad"임
            return obj.scene.name == "DontDestroyOnLoad";
        }

        private void OnOrientationChanged(bool isLandscape)
        {
            if (canvasHandlers == null) return;

            int updatedCount = 0;
            foreach (CanvasOrientationHandler handler in canvasHandlers)
            {
                if (handler != null)
                {
                    handler.ForceUpdateOrientation();
                    updatedCount++;
                }
            }

            Debug.Log($"AutoCanvasOrienter: {(isLandscape ? "가로" : "세로")} 모드에 맞게 {updatedCount}개의 캔버스 조정 완료");
        }

        /// <summary>
        /// 캔버스 핸들러를 수동으로 다시 설정합니다 (런타임에 새 캔버스가 추가된 경우 호출)
        /// </summary>
        public void RefreshCanvasHandlers()
        {
            FindAndSetupCanvases();
            // 현재 방향에 맞게 즉시 업데이트
            bool isLandscape = orientationDetector.IsLandscapeMode();
            OnOrientationChanged(isLandscape);
        }
    }
} 