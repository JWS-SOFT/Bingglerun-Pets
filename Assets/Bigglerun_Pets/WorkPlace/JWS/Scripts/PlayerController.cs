using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public StairManager stairManager;

    private bool moving = false;
    private int moveDirection = 1; // 1 = 오른쪽, -1 = 왼쪽
    public int currentStairIndex = 0;

    private Vector2 targetPos;
    private Vector2 startJumpPos;       // ⬅ 점프 시작 위치 저장
    private float jumpTimer = 0f;       // ⬅ 점프 시간 진행 추적
    private float jumpDuration = 0.25f; // ⬅ 총 이동에 걸릴 시간
    private float jumpHeight = 1.5f;    // ⬅ 점프 높이

    private void Start()
    {
        PlayerManager.Player_Transform = transform;
        gameObject.SetActive(false);
    }

    private void Update()
    {
        if (moving) return;
        else
        {
            if (currentStairIndex > 0 && !PlayerManager.ActionTImerCheck())
            {
                TriggerGameOver();
                return;
            }
        }
    }

    private void FixedUpdate()
    {
        if (moving)
        {
            jumpTimer += Time.fixedDeltaTime;
            float t = Mathf.Clamp01(jumpTimer / jumpDuration);

            // 이동 위치 보간
            Vector2 flatPos = Vector2.Lerp(startJumpPos, targetPos, t);

            // 위로 튀는 점프 궤적 추가
            float heightOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;
            flatPos.y += heightOffset;

            transform.position = flatPos;

            // 도착 처리
            if (t >= 1f)
            {
                transform.position = targetPos;
                moving = false;
            }
        }
    }

    public void TurnButtonClick()
    {
        moveDirection *= -1;

        // 스프라이트 방향 반전
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDirection;
        transform.localScale = scale;
    }

    public void JumpButtonClick()
    {
        if (moving) return; // 중복 방지
        if (currentStairIndex > 0 && !PlayerManager.ActionTImerCheck())
        {
            TriggerGameOver();
            return;
        }

        moving = true;              // 🔐 입력 즉시 잠금
        jumpTimer = 0f;             // 점프 시간 초기화
        startJumpPos = transform.position; // 시작 위치 저장

        int nextIndex = currentStairIndex + 1;

        if (stairManager.TryGetStairPosition(nextIndex, out Vector2 nextStairPos))
        {
            Vector2 delta = nextStairPos - (Vector2)transform.position;
            bool isValidDirection = Mathf.Sign(delta.x) == moveDirection;

            if (!isValidDirection)
            {
                Debug.Log("틀린 방향! 허공으로 떨어짐 → 게임 오버");
                TriggerGameOver();
                return;
            }

            GameObject nextStair = stairManager.GetStairObject(nextIndex);
            float stairTopY = nextStair != null && nextStair.TryGetComponent<SpriteRenderer>(out var stairRenderer)
                ? nextStair.transform.position.y + (stairRenderer.bounds.size.y / 2f)
                : nextStairPos.y + (stairManager.stairHeight / 2f);

            float playerHeight = TryGetComponent<SpriteRenderer>(out var playerRenderer)
                ? playerRenderer.bounds.size.y
                : GetComponent<Collider2D>().bounds.size.y;

            float correctedY = stairTopY + (playerHeight / 2f);

            targetPos = new Vector2(nextStairPos.x, correctedY);
            currentStairIndex = nextIndex;
            PlayerManager.ChangeFloor(currentStairIndex);
            if (currentStairIndex > 1) PlayerManager.ActionTImeSuccess();
            else PlayerManager.ActionTImeStart();
        }
        else
        {
            Debug.Log("다음 계단 없음 → 게임 오버");
            TriggerGameOver();
        }
    }

    private void TriggerGameOver()
    {
        moving = false;
        enabled = false;
        PlayerManager.ActionTImeStop();
        Debug.Log("Game Over!");
        // UIManager.Instance.ShowGameOverUI(); // 선택 사항
    }
}
