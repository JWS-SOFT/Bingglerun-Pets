using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;
    public float followSpeed = 2f;

    private void LateUpdate()
    {
        if (player != null)
        {
            Vector3 targetPos = new Vector3(player.position.x, player.position.y + 3f, -10f);
            transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
        }
    }
}