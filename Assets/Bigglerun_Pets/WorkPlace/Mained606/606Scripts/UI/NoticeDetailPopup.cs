using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class NoticeDetailPopup : MonoBehaviour
{
    private static NoticeDetailPopup instance;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI dateText;
    [SerializeField] private Button closeButton;

    private void Awake()
    {
        instance = this;
        gameObject.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.AddListener(Hide);
        }
    }

    public static void Show(NoticeData notice)
    {
        if (instance == null)
        {
            Debug.LogError("[NoticeDetailPopup] 인스턴스가 없습니다.");
            return;
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