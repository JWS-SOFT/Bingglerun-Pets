using UnityEngine;

public class Stair : MonoBehaviour
{
    public int index;
    private Collider2D col;

    void Awake()
    {
        col = GetComponent<Collider2D>();
    }

    public void SetCollider(bool enabled)
    {
        col.enabled = enabled;
    }
}
