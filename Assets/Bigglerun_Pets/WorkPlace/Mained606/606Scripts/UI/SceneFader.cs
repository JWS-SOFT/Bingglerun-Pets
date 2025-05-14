using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 씬 전환을 부드럽게 처리하는 클래스
/// </summary>
public class SceneFader : MonoBehaviour
{
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    public void LoadScene(string sceneName)
    {
        Debug.Log($"[SceneFader] 씬 전환: {sceneName}");
        SceneManager.LoadScene(sceneName);
        UIManager.Instance.SceneChange();
    }
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"씬 {scene.name} 로드 완료!");

        // ✅ 씬 로드 완료 후 실행할 작업
        UIManager.Instance.SceneChange();
    }
}