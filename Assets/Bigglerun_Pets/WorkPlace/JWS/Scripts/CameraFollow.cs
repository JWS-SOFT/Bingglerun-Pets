using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 3f;
    public float fixedY = 3.5f; // 횡모드에서 고정할 Y값

    private void LateUpdate()
    {
        if (player == null) return;

        Vector3 targetPos;

        if (PlayerManager.PlayMode)
        {
            // ➤ 횡스크롤 모드 (y는 고정, x만 따라감)
            followSpeed *= 0;
            targetPos = new Vector3(player.position.x + 1.5f , player.position.y + 10f, -10f);
        }
        else
        {
            // ➤ 계단 모드 (y도 따라감, 살짝 위를 보게)
            targetPos = new Vector3(player.position.x, player.position.y + 1f, -10f);
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
    }
}
