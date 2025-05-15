using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private StairManager stairManager;
    [SerializeField] private Transform footPoint;

    [SerializeField] private float jumpTimer = 0f;       // ⬅ 점프 시간 진행 추적
    [SerializeField] private float jumpDuration = 0.25f; // ⬅ 총 이동에 걸릴 시간
    [SerializeField] private float jumpHeight = 2.5f;    // ⬅ 점프 높이
    private bool moving = false;
    private int moveDirection = 1; // 1 = 오른쪽, -1 = 왼쪽
    public int currentStairIndex = 0;

    private Vector2 targetPos;
    private Vector2 startJumpPos;       // ⬅ 점프 시작 위치 저장
    private bool isGameOver = false;
    private bool isGamemode = false;  // false 계단, true 횡런게임.
    private Rigidbody2D Rigidbody2D;

    private void Start()
    {
        PlayerManager.Player_Transform = transform;
        //gameObject.SetActive(false);
        isGamemode = PlayerManager.PlayMode;
        jumpDuration = !isGamemode ? 0.25f : 0.5f; // ⬅ 총 이동에 걸릴 시간
        jumpHeight = !isGamemode ? 2f : 3f;    // ⬅ 점프 높이
        Rigidbody2D = GetComponent<Rigidbody2D>();
        if (!isGamemode) Rigidbody2D.Sleep();
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
        if (!moving)
        {
            Debug.Log(PlayerManager.Instance.isGameStartReady);
            // 🟥 횡스크롤 모드에서 아래 타일 유무 체크
            if (isGamemode && PlayerManager.Instance.isGameStartReady)
            {
                Vector2 checkPos = footPoint.position; // 발밑 바로 아래
                Collider2D hit = Physics2D.OverlapCircle(checkPos, 0.1f, LayerMask.GetMask("Ground")); // 'Ground' 레이어로 타일 설정했다고 가정
                if (hit == null)
                {
                    // 아래에 타일이 없고, 점프 중이 아님 → 게임오버
                    TriggerGameOver();
                    return;
                }
            }

            return;
        }

        jumpTimer += Time.fixedDeltaTime;
        float t = Mathf.Clamp01(jumpTimer / jumpDuration);
        float heightOffset = Mathf.Sin(t * Mathf.PI) * jumpHeight;

        if (isGamemode)
        {
            // 🟦 횡스크롤 모드: 제자리에서 위아래로 점프
            Vector2 jumpPos = startJumpPos;
            jumpPos.y += heightOffset;
            transform.position = jumpPos;

            if (t >= 1f)
            {
                transform.position = startJumpPos; // 정확히 제자리 복귀
                moving = false;
            }
        }
        else
        {
            // 🟩 계단 점프: 위치를 이동하며 점프
            Vector2 flatPos = Vector2.Lerp(startJumpPos, targetPos, t);
            flatPos.y += heightOffset;
            transform.position = flatPos;

            if (t >= 1f)
            {
                transform.position = targetPos;
                moving = false;
            }
        }
    }

    public void TurnButtonClick()
    {
        if (isGameOver) return;

        // ✅ 게임모드일 때는 방향 전환 비활성화
        if (isGamemode) return;

        if (currentStairIndex > 0 && !PlayerManager.ActionTImerCheck())
        {
            TriggerGameOver();
            return;
        }

        moveDirection *= -1;

        // 스프라이트 방향 반전
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * moveDirection;
        transform.localScale = scale;
    }

    public void JumpButtonClick()
    {
        if (moving || isGameOver) return;

        // ✅ 횡스크롤 모드일 경우: 제자리 점프
        if (isGamemode)
        {
            moving = true;
            jumpTimer = 0f;
            startJumpPos = transform.position;
            targetPos = startJumpPos; // 제자리 점프
            return;
        }

        // ✅ 계단 모드일 경우
        if (currentStairIndex > 0 && !PlayerManager.ActionTImerCheck())
        {
            TriggerGameOver();
            return;
        }

        moving = true;
        jumpTimer = 0f;
        startJumpPos = transform.position;

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

    public void TriggerGameOver()
    {
        moving = false;
        enabled = false;
        isGameOver = true;
        if(!isGamemode) PlayerManager.ActionTImeStop();
        if (UIManager.Instance != null) UIManager.Instance.TogglePopupUI("GameOverUI");
        Debug.Log("Game Over!");
        Time.timeScale = 0f;
        // UIManager.Instance.ShowGameOverUI(); // 선택 사항
    }
}
