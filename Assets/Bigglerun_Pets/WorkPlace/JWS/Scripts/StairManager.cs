using System.Collections.Generic;
using UnityEngine;

public class StairManager : MonoBehaviour
{
    public GameObject player;
    public GameObject stairPrefab;

    public int stairTotalCount = 20;
    // public int stairWidthCount = 5;
    public float stairWidth = 2f;
    public float stairHeight = 2f;
    public int SpawnItemPercent = 20;

    private Vector2 currentPos = Vector2.zero;
    private List<Stair> stairs = new List<Stair>();
    private int currentIndex = -1;

    private void Start()
    {
        GenerateInitialStairs();
        SetPlayerOnFirstStair();
    }

    int xIndex = 0; // -2 ~ 2 사이 유지 (현재 x 위치를 인덱스로 관리)

    private void GenerateInitialStairs()
    {
        for (int i = 0; i < stairTotalCount; i++)
        {
            int direction = Random.Range(0, 2) == 0 ? -1 : 1;

            // 범위 초과 방지
            if (xIndex + direction < -2 || xIndex + direction > 2)
                direction = -direction; // 반대로 고정

            xIndex += direction;
            currentPos.x = xIndex * stairWidth;
            currentPos.y += stairHeight;

            GameObject stair = Instantiate(stairPrefab, currentPos, Quaternion.identity);
            Stair stairScript = stair.GetComponent<Stair>();
            if (stairScript != null)
            {
                stairScript.index = i;
                if (i > 0 && Random.Range (0, 100) < SpawnItemPercent)
                {
                    stairScript.SetItemPrefab("Coin");
                }

                stairs.Add(stairScript);
            }
        }
    }


    private void SetPlayerOnFirstStair()
    {
        if (stairs.Count == 0) return;

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
        UpdateCurrentStairIndex();
        // UpdateStairColliders();
        // RecycleStairs();
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

    private void UpdateStairColliders()
    {
        foreach (var stair in stairs)
        {
            if (stair.index <= currentIndex || stair.index == currentIndex + 1)
                stair.SetCollider(true);
            else
                stair.SetCollider(false);
        }
    }

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
}
