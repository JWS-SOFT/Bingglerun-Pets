using UnityEngine;

public class Stair : MonoBehaviour
{
    public int index;

    private Collider2D col;
    private SpawnItem spawnItem;

    private void Awake()
    {
        col = GetComponent<Collider2D>();
        spawnItem = GetComponentInChildren<SpawnItem>();
    }

    private void Start()
    {
        
    }

    public void SetCollider(bool enabled)
    {
        col.enabled = enabled;
    }

    public void SetItemPrefab(string itemName)
    {
        spawnItem.SetItemType(itemName);
    }
}
