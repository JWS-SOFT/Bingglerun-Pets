using System.Collections.Generic;
using UnityEngine;

public class SpawnItem : MonoBehaviour
{
    public List<Sprite> spriteList;
    //public float floatAmplitude = 0.1f; // 위아래 움직임 크기
    //public float floatFrequency = 2f;   // 움직임 속도
    //public float flipFrequency = 1f;    // 회전 속도
    //public float rotateAmplitude = 15f;     // 회전 각도 (좌우 최대 각도)
    //public float rotateFrequency = 2f;      // 회전 속도

    private SpriteRenderer item_Sprite;
    private BoxCollider2D BoxCollider2D;
    //private Vector3 startPos;
    //private float flipTimer;
    //private bool flipped = false;

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

        //startPos = transform.localPosition;
    }

    private void Update()
    {
        //// 1. 위아래로 살짝 움직이기
        //float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        //transform.localPosition = new Vector3(startPos.x, newY, startPos.z);

        //// 2. 회전 느낌 주기 (Z축 기준으로 흔들림)
        //float angleZ = Mathf.Sin(Time.time * rotateFrequency) * rotateAmplitude;
        //transform.localRotation = Quaternion.Euler(0, 0, angleZ);
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
            Stair stair = transform.GetComponentInParent<Stair>();
            PlayerController controller = collision.gameObject.GetComponent<PlayerController>();
            if (controller != null && stair != null && stair.index == controller.currentStairIndex)
            {
                item_Sprite.enabled = false;
                PlayerManager.ChangeCoin();
                Debug.Log("플레이어와 접촉");
            }
        }
    }
}
