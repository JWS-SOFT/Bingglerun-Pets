using System.Collections.Generic;
using UnityEngine;

public class StairManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject stairPrefab;

    [SerializeField] private int stairTotalCount = 0;
    // [SerializeField] private  int stairWidthCount = 5;
    public float stairWidth = 2f;
    public float stairHeight = 2f;
    [SerializeField] private int SpawnItemPercent = 20;
    [SerializeField] private float invincibleItemChance = 0.5f;  //05.20 HJ 추가
    [Header("🔐 난이도 조정")]
    [Range(0f, 1f)]
    [SerializeField] private float changeDirectionChance = 0.3f; // 방향 바꿀 확률 (난이도용)

    private int lastDirection = 1; // 시작 방향
    private Vector2 currentPos = Vector2.zero;
    private List<Stair> stairs = new List<Stair>();
    private int currentIndex = -1;
    private int xIndex = 0; // -2 ~ 2 사이 유지 (현재 x 위치를 인덱스로 관리)

    private void Start()
    {
        stairTotalCount = PlayerManager.GetStageStair;
        GenerateInitialStairs();
        SetPlayerOnFirstStair();
    }

    private void GenerateInitialStairs()
    {
        for (int i = 0; i < stairTotalCount; i++)
        {
            // 일정 확률로 방향 전환
            if (Random.value < changeDirectionChance)
            {
                lastDirection = -lastDirection;
            }

            // 범위 초과 방지
            if (xIndex + lastDirection < -2 || xIndex + lastDirection > 2)
            {
                lastDirection = -lastDirection;
            }

            xIndex += lastDirection;
            currentPos.x = xIndex * stairWidth;
            currentPos.y += stairHeight;

            GameObject stair = Instantiate(stairPrefab, currentPos, Quaternion.identity);
            Stair stairScript = stair.GetComponent<Stair>();
            if (stairScript != null)
            {
                stairScript.index = i;
                if (i > 0 && Random.Range(0, 100) < SpawnItemPercent)
                {
                    //05.14 HJ 추가
                    if (Random.value > invincibleItemChance)
                        stairScript.SetItemPrefab("Coin");
                    else
                        stairScript.SetItemPrefab("Wing");
                }
                stairs.Add(stairScript);
            }
        }
    }

    private void SetPlayerOnFirstStair()
    {
        if (stairs.Count == 0) return;
        if (PlayerManager.IsSettingPlayer())
        {
            PlayerManager.Player_Transform.gameObject.SetActive(true);
        }
        else
        {
            return;
        }

        Vector3 firstStairPos = stairs[0].transform.position;

        // 계단의 SpriteRenderer 높이 계산
        SpriteRenderer stairRenderer = stairs[0].GetComponent<SpriteRenderer>();
        float stairHeight = stairRenderer != null ? stairRenderer.bounds.size.y : this.stairHeight;

        // 플레이어의 SpriteRenderer 높이 계산
        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
        float playerHeight = playerRenderer != null ? playerRenderer.bounds.size.y : 2f;

        // 계단 상단에 플레이어 바닥이 닿도록 위치 설정
        float posY = firstStairPos.y + (stairHeight / 2f) + (playerHeight / 2f);

        Vector3 playerStartPos = new Vector3(firstStairPos.x, posY, 0);
        player.transform.position = playerStartPos;
    }


    private void Update()
    {
        if (!PlayerManager.Instance.isGameStartReady)
        {
            UpdateCurrentStairIndex();

            // UpdateStairColliders();
            // RecycleStairs();
        }
    }

    private void UpdateCurrentStairIndex()
    {
        float playerY = player.transform.position.y;
        float closestDist = float.MaxValue;

        foreach (var stair in stairs)
        {
            float stairY = stair.transform.position.y;
            float diff = playerY - stairY;

            if (diff >= -0.5f && diff < closestDist)
            {
                closestDist = diff;
                currentIndex = stair.index;
            }
        }
    }

    //private void UpdateStairColliders()
    //{
    //    foreach (var stair in stairs)
    //    {
    //        if (stair.index <= currentIndex || stair.index == currentIndex + 1)
    //            stair.SetCollider(true);
    //        else
    //            stair.SetCollider(false);
    //    }
    //}

    private void RecycleStairs()
    {
        // 플레이어가 5칸 이상 올라갔을 때만 실행
        if (currentIndex >= 5)
        {
            Stair firstStair = stairs[0];
            stairs.RemoveAt(0); // 리스트에서 제거

            Vector3 newPosition = stairs[stairs.Count - 1].transform.position;
            newPosition.x += Random.Range(-stairWidth, stairWidth); // 좌/우 랜덤
            newPosition.y += stairHeight; // 위로 쌓기

            firstStair.transform.position = newPosition; // 기존 계단 이동
            firstStair.index = stairTotalCount++; // 인덱스 갱신

            stairs.Add(firstStair); // 리스트 끝에 다시 추가
        }
    }

    public bool TryGetStairPosition(int index, out Vector2 position)
    {
        foreach (var stair in stairs)
        {
            if (stair.index == index)
            {
                position = stair.transform.position;
                return true;
            }
        }

        position = Vector2.zero;
        return false;
    }

    public GameObject GetStairObject(int index)
    {
        if (index >= 0 && index < stairs.Count)
        {
            return stairs[index].gameObject;
        }
        return null;
    }

    public void ResetStiar()
    {
        foreach (Stair st in stairs)
        {
            if ( st != null ) Destroy(st.gameObject);
        }
    }
}
