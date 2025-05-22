using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Transform player;
    public float followSpeed = 3f;
    private float currentSpeed;

    public bool mode = false;
    private Vector3 targetPos;

    private void Update()
    {
        player = PlayerManager.Player_Transform;
        if (player == null) return;

        if (PlayerManager.PlayMode)
        {
            // ➤ 횡스크롤 모드 (y는 고정, x만 따라감)
            if (PlayerManager.Instance.isGameStartReady) currentSpeed = 0;
            targetPos = new Vector3(player.position.x + 1.5f, player.position.y + 2.5f, -10f);
        }
        else
        {
            // ➤ 계단 모드 (y도 따라감, 살짝 위를 보게)
            currentSpeed = followSpeed;
            targetPos = new Vector3(player.position.x, player.position.y + 1f, -10f);
        }
    }

    private void LateUpdate()
    {
        transform.position = Vector3.Lerp(transform.position, targetPos, currentSpeed * Time.deltaTime);
    }
}
