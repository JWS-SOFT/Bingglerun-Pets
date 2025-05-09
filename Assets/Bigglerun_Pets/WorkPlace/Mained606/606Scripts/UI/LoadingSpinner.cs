using UnityEngine;

/// <summary>
/// 로딩 스피너 회전 애니메이션
/// </summary>
public class LoadingSpinner : MonoBehaviour
{
    [SerializeField] private GameObject spinner;
    [SerializeField] private float rotateSpeed = 120f;

    private void Update()
    {
        if (spinner != null)
        {
            spinner.transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
        }
        else
        {
            // 스피너가 지정되지 않았으면 자기 자신을 회전
            transform.Rotate(0, 0, -rotateSpeed * Time.deltaTime);
        }
    }
} 