using UnityEngine;

public class MenuUI : MonoBehaviour
{

    public void ResetScore()
    {
        ScoreManager.Instance.ResetScore();
    }
}
