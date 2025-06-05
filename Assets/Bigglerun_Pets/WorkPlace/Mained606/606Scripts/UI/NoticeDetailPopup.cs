using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoticeDetailPopup : MonoBehaviour
{
    private static NoticeDetailPopup instance;
    private static GameObject prefab;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
        
        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }

        // 처음 생성시 비활성화
        gameObject.SetActive(false);
    }

    public static void Show(NoticeData notice)
    {
        // 인스턴스가 없으면 생성
        if (instance == null)
        {
            // 프리팹이 로드되지 않았다면 로드
            if (prefab == null)
            {
                prefab = Resources.Load<GameObject>("Prefabs/NoticeDetailPopup");
                if (prefab == null)
                {
                    Debug.LogError("[NoticeDetailPopup] Resources/Prefabs/NoticeDetailPopup 프리팹을 찾을 수 없습니다.");
                    return;
                }
            }

            // 프리팹 인스턴스화
            GameObject popupObject = Instantiate(prefab);
            
            // 캔버스 찾기 또는 생성
            Canvas canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("Canvas");
                canvas = canvasObject.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvasObject.AddComponent<CanvasScaler>();
                canvasObject.AddComponent<GraphicRaycaster>();
            }
            
            // 팝업을 캔버스의 자식으로 설정
            popupObject.transform.SetParent(canvas.transform, false);
            
            // RectTransform 설정
            RectTransform rectTransform = popupObject.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                rectTransform.anchorMin = Vector2.zero;
                rectTransform.anchorMax = Vector2.one;
                rectTransform.sizeDelta = Vector2.zero;
                rectTransform.anchoredPosition = Vector2.zero;
            }

            instance = popupObject.GetComponent<NoticeDetailPopup>();
            if (instance == null)
            {
                Debug.LogError("[NoticeDetailPopup] 생성된 프리팹에서 NoticeDetailPopup 컴포넌트를 찾을 수 없습니다.");
                Destroy(popupObject);
                return;
            }
        }

        instance.ShowPopup(notice);
    }

    private void ShowPopup(NoticeData notice)
    {
        if (titleText != null)
        {
            titleText.text = notice.title;
        }

        if (contentText != null)
        {
            contentText.text = notice.content;
        }

        if (dateText != null)
        {
            dateText.text = notice.timestamp;
        }

        gameObject.SetActive(true);
    }

    private void Hide()
    {
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Hide);
        }

        if (instance == this)
        {
            instance = null;
        }
    }
}