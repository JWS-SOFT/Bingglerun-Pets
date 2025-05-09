using UnityEngine;

/// <summary>
/// 게임 매니저가 없을 경우 자동으로 생성하는 스크립트
/// 타이틀 씬에서 사용되며, GameManager가 존재하는지 확인하고 없으면 생성합니다.
/// </summary>
public class GameManagerInitializer : MonoBehaviour
{
    [Header("게임 매니저 프리팹")]
    [SerializeField] private GameObject gameManagerPrefab;
    
    [Header("초기화 확인")]
    [SerializeField] private bool destroyAfterInit = true;
    
    private void Awake()
    {
        // 이미 게임 매니저가 있는지 확인
        if (GameManager.Instance == null)
        {
            Debug.Log("[GameManagerInitializer] GameManager를 생성합니다.");
            
            if (gameManagerPrefab != null)
            {
                // 프리팹에서 생성
                Instantiate(gameManagerPrefab);
            }
            else
            {
                // 빈 게임 오브젝트에 컴포넌트 추가
                GameObject gameManagerObj = new GameObject("GameManager");
                gameManagerObj.AddComponent<GameManager>();
            }
        }
        else
        {
            Debug.Log("[GameManagerInitializer] GameManager가 이미 존재합니다.");
        }
        
        // 초기화 후 자신을 제거할지 여부
        if (destroyAfterInit)
        {
            Destroy(gameObject);
        }
    }
} 