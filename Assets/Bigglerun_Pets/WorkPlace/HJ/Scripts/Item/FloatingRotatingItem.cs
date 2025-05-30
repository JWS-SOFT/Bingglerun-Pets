using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class FloatingRotatingItem : MonoBehaviour
{
    [Header("Rotation Settings")]
    [Tooltip("초당 회전 속도 (도 단위)")]
    [SerializeField] private float rotationSpeed = 90f;

    [Header("Floating Settings")]
    [Tooltip("위아래로 이동하는 거리")]
    [SerializeField] private float floatAmplitude = 0.25f;

    [Tooltip("부유 애니메이션의 속도")]
    [SerializeField] private float floatFrequency = 1f;

    [Header("Alpha Pulse Settings")]
    [Tooltip("투명도 변화 여부")]
    [SerializeField] private bool enableAlphaPulse = false;

    [Tooltip("최소 투명도")]
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.5f;

    [Tooltip("최대 투명도")]
    [Range(0f, 1f)]
    [SerializeField] private float maxAlpha = 1f;

    private SpriteRenderer spriteRenderer;
    private Vector3 startPosition;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
    }

    void Update()
    {
        Rotate();
        Float();
        if (enableAlphaPulse)
            PulseAlpha();
    }

    private void Rotate()
    {
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.World);
    }

    private void Float()
    {
        float offsetY = Mathf.Sin(Time.time * floatFrequency * 2 * Mathf.PI) * floatAmplitude;
        transform.position = startPosition + new Vector3(0f, offsetY, 0f);
    }

    private void PulseAlpha()
    {
        float t = Mathf.Sin(Time.time * floatFrequency * 2 * Mathf.PI) * 0.5f + 0.5f;
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, t);

        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }
}
