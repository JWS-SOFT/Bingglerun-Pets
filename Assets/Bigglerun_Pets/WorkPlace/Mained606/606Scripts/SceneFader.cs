using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 부드럽게 처리하는 클래스
/// </summary>
public class SceneFader : MonoBehaviour
{
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneFader] 씬 전환: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }
}