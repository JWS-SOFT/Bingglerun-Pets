using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StageStart : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI stageLeveltext;
    [SerializeField] private Button[] buttons;

    public void ClickCloseWindow()
    {
        Destroy(this.gameObject, 0.2f);
    }

    public void StageGameStart()
    {
        Destroy(this.gameObject, 0.5f);
    }
}
