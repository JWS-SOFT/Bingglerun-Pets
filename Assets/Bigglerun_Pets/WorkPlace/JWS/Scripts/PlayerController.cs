using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed = 5f;
    public StairManager stairManager;

    private bool moving = false;
    private int moveDirection = 1; // 1 = 오른쪽, -1 = 왼쪽
    private int currentStairIndex = 0;

    private Vector2 targetPos;

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

            // 🔽 계단 SpriteRenderer에서 정확한 높이 가져오기
            GameObject nextStair = stairManager.GetStairObject(nextIndex); // 따로 구현돼 있어야 함
            float stairTopY = nextStair != null && nextStair.TryGetComponent<SpriteRenderer>(out var stairRenderer)
                ? nextStair.transform.position.y + (stairRenderer.bounds.size.y / 2f)
                : nextStairPos.y + (stairManager.stairHeight / 2f);

            // 🔽 플레이어 SpriteRenderer 기준 바닥 좌표 보정
            float playerHeight = TryGetComponent<SpriteRenderer>(out var playerRenderer)
                ? playerRenderer.bounds.size.y
                : GetComponent<Collider2D>().bounds.size.y;

            float correctedY = stairTopY + (playerHeight / 2f);

            // 🔽 최종 점프 위치 보정
            targetPos = new Vector2(nextStairPos.x, correctedY);
            moving = true;
            currentStairIndex = nextIndex;
        }
        else
        {
            Debug.Log("다음 계단 없음 → 게임 오버");
            TriggerGameOver();
        }
    }

    private void Update()
    {
        if (moving) return;
    }

    private void FixedUpdate()
    {
        if (moving)
        {
            transform.position = Vector2.MoveTowards(transform.position, targetPos, moveSpeed * Time.fixedDeltaTime);

            if ((Vector2)transform.position == targetPos)
            {
                moving = false;
            }
        }
    }

    private void TriggerGameOver()
    {
        // 예시: 움직이지 못하게 막고, 메시지 띄우기
        moving = false;
        enabled = false;

        Debug.Log("Game Over!");
        // UIManager.Instance.ShowGameOverUI(); // 선택 사항
    }

}
