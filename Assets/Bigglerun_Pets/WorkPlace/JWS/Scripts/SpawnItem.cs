using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SpawnItem : MonoBehaviour
{
    public List<Sprite> spriteList;
    private SpriteRenderer item_Sprite;
    private BoxCollider2D BoxCollider2D;
    private enum spriteNameType
    {
        Coin,
        Gem,
        Heart,
        Wing
    }

    private void OnEnable()
    {
        item_Sprite = GetComponent<SpriteRenderer>();
        BoxCollider2D = GetComponent<BoxCollider2D>();
        BoxCollider2D.enabled = false;
    }

    public void SetItemType(string type)
    {
        if (System.Enum.TryParse(type, out spriteNameType itemType))
        {
            item_Sprite.sprite = spriteList[(int)itemType];

            // BoxCollider2D 활성화
            BoxCollider2D.enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            item_Sprite.enabled = false;
            Debug.Log("플레이어와 접촉");
        }
    }
}
