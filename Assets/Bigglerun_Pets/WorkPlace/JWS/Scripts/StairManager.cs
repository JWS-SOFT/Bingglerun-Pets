using System.Collections.Generic;
using UnityEngine;

public class StairManager : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private GameObject stairPrefab;

    [SerializeField] private int stairTotalCount = 0;
    public float stairWidth = 2f;
    public float stairHeight = 2f;
    [SerializeField] private int SpawnItemPercent = 20;
    [SerializeField] private float invincibleItemChance = 0.5f;

    [Header("🔐 난이도 조정")]
    [Range(0f, 1f)]
    [SerializeField] private float changeDirectionChance = 0.3f;

    private int lastDirection = 1;
    private Vector2 currentPos = Vector2.zero;
    private List<Stair> stairs = new List<Stair>();
    private int currentIndex = -1;
    private int xIndex = 0;

    private bool isStairModeReady = false;

    // 🔁 계단 풀링용 큐
    private Queue<GameObject> stairPool = new Queue<GameObject>();

    public void StartStairs()
    {
        stairTotalCount = PlayerManager.GetStageStair;
        InitStairPool(stairTotalCount);
        GenerateInitialStairs();
        SetPlayerOnFirstStair();
        isStairModeReady = true;
    }

    // 🔁 계단 오브젝트 미리 풀링
    private void InitStairPool(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject stair = Instantiate(stairPrefab);
            stair.SetActive(false);
            stair.transform.SetParent(transform);
            stairPool.Enqueue(stair);
        }
    }

    // 계단 생성 (풀에서 꺼내서 재사용)
    private void GenerateInitialStairs()
    {
        for (int i = 0; i < stairTotalCount; i++)
        {
            if (Random.value < changeDirectionChance)
            {
                lastDirection = -lastDirection;
            }

            if (xIndex + lastDirection < -2 || xIndex + lastDirection > 2)
            {
                lastDirection = -lastDirection;
            }

            xIndex += lastDirection;
            currentPos.x = xIndex * stairWidth;
            currentPos.y += stairHeight;

            GameObject stair = GetStairFromPool(currentPos);
            Stair stairScript = stair.GetComponent<Stair>();

            if (stairScript != null)
            {
                stairScript.index = i;

                if (i > 0 && Random.Range(0, 100) < SpawnItemPercent)
                {
                    if (Random.value > invincibleItemChance)
                        stairScript.SetItemPrefab("Coin");
                    else
                        stairScript.SetItemPrefab("Wing");
                }

                stairs.Add(stairScript);
            }
        }
    }

    // 🔁 풀에서 계단 꺼내기
    private GameObject GetStairFromPool(Vector2 position)
    {
        GameObject stair;
        if (stairPool.Count > 0)
        {
            stair = stairPool.Dequeue();
            stair.transform.position = position;
            stair.transform.rotation = Quaternion.identity;
            stair.transform.SetParent(null);
            stair.SetActive(true);
        }
        else
        {
            stair = Instantiate(stairPrefab, position, Quaternion.identity);
        }

        return stair;
    }

    // 플레이어를 첫 계단에 배치
    private void SetPlayerOnFirstStair()
    {
        if (stairs.Count == 0) return;

        if (PlayerManager.IsSettingPlayer())
        {
            PlayerManager.Player_Transform.gameObject.SetActive(true);
        }
        else return;

        Vector3 firstStairPos = stairs[0].transform.position;

        SpriteRenderer stairRenderer = stairs[0].GetComponent<SpriteRenderer>();
        float stairHeight = stairRenderer != null ? stairRenderer.bounds.size.y : this.stairHeight;

        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
        float playerHeight = playerRenderer != null ? playerRenderer.bounds.size.y : 1f;

        float posY = firstStairPos.y + (stairHeight / 2f) + (playerHeight / 2f);

        Vector3 playerStartPos = new Vector3(firstStairPos.x, posY, 0);
        player.transform.position = playerStartPos;
    }

    private void Update()
    {
        if (isStairModeReady && !PlayerManager.Instance.isGameStartReady)
        {
            UpdateCurrentStairIndex();
        }
    }

    public void UpdateCurrentStairIndex()
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

    // 🔁 전체 계단 초기화 및 풀에 반환
    public void ResetStiar()
    {
        isStairModeReady = false;
        foreach (Stair st in stairs)
        {
            if (st != null) ReturnStair(st);
        }

        stairs.Clear();
    }

    // 🔁 계단 반환
    public void ReturnStair(Stair stair)
    {
        if (stair == null) return;

        stair.gameObject.SetActive(false);
        stair.transform.SetParent(transform);
        stairPool.Enqueue(stair.gameObject);
    }
}
