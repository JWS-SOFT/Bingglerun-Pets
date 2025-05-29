using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class NoticeItem : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI contentText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private Button itemButton;
    [SerializeField] private Image backgroundImage;
    
    private NoticeData noticeData;
    
    private void Awake()
    {
        if (itemButton != null)
        {
            itemButton.onClick.AddListener(OnItemClicked);
        }
    }
    
    public void SetData(NoticeData data)
    {
        noticeData = data;
        UpdateUI();
    }
    
    void UpdateUI()
    {
        if (noticeData == null) return;
        
        // 제목 설정
        if (titleText != null)
            titleText.text = noticeData.title;
        
        // 내용 설정 (미리보기용으로 제한)
        if (contentText != null)
        {
            string previewContent = noticeData.content;
            if (previewContent.Length > 100)
            {
                previewContent = previewContent.Substring(0, 100) + "...";
            }
            contentText.text = previewContent;
        }
        
        // 시간 설정
        if (timestampText != null)
        {
            if (DateTime.TryParse(noticeData.timestamp, out DateTime dateTime))
            {
                timestampText.text = dateTime.ToString("yyyy.MM.dd");
            }
            else
            {
                timestampText.text = noticeData.timestamp;
            }
        }
        
        // 타입별 배경색 설정
        if (backgroundImage != null)
        {
            switch (noticeData.type)
            {
                case "notice":
                    backgroundImage.color = new Color(0.9f, 0.9f, 0.9f, 1f);
                    break;
                case "update":
                    backgroundImage.color = new Color(0.8f, 1f, 0.8f, 1f);
                    break;
                case "event":
                    backgroundImage.color = new Color(1f, 0.9f, 0.8f, 1f);
                    break;
            }
        }
    }
    
    private void OnItemClicked()
    {
        if (noticeData != null)
        {
            NoticeDetailPopup.Show(noticeData);
        }
    }

    private void OnDestroy()
    {
        if (itemButton != null)
        {
            itemButton.onClick.RemoveListener(OnItemClicked);
        }
    }
}