using UnityEngine;

public class Huddle : MonoBehaviour
{
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerController controller = collision.gameObject.GetComponent<PlayerController>();
            if (controller != null)
            {
                Debug.Log("장애물과 접촉");
                //controller.TriggerGameOver();
                PlayerManager.Instance.TakeDamage();
                Debug.Log("장애물 충돌");
            }
        }
    }
}
