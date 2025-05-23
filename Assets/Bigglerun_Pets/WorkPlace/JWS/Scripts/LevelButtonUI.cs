using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButtonUI : MonoBehaviour
{
    private int currentLevelIndex = 0;
    [SerializeField] private Material grayTint, currentButton;
    [SerializeField] private Image startButton;
    [SerializeField] private Image[] starTint;
    [SerializeField] private GameObject startStagePrefab;
    [SerializeField] private TextMeshProUGUI currentStageLeveltext;

    public void SetStageLevelSetting(int level, bool current = false)
    {
        currentLevelIndex = level + 1;
        currentStageLeveltext.text = currentLevelIndex.ToString();
        if (current) startButton.material = currentButton;
    }

    public void ClickStageLevel()
    {
        Instantiate(startStagePrefab, transform.parent);
    }
}
