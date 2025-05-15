using System.Collections.Generic;
using UnityEngine;

public class TerrainScrollManager : MonoBehaviour
{
    [Header("지형 설정")]
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private float scrollSpeed = 5f;

    [Header("플레이어 설정")]
    [SerializeField] private GameObject playerInstance;

    [Header("장애물 설정")]
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [Range(0.1f, 1f)]
    [SerializeField] private float obstacleHeightOffset = 1.45f;

    [Header("아이템 설정")]
    [SerializeField] private GameObject spawnItem;
    [Range(0f, 1f)][SerializeField] private float spawnPercent = 0.5f;

    [Header("패턴 설정")]
    [SerializeField] private int maxTilesBetweenEmpty = 1;

    private float terrainWidth;
    private Queue<GameObject> terrainPool = new Queue<GameObject>();
    private int terrainIndex = 0;
    private float terrainDistance = 0;

    private List<int> spawnPattern;

    private void Start()
    {
        if (terrainPrefab == null || playerInstance == null)
        {
            Debug.LogError("지형 프리팹 또는 플레이어가 비어 있습니다!");
            return;
        }

        terrainWidth = GetPrefabWidth(terrainPrefab);

        // 패턴 생성
        spawnPattern = GenerateSpawnPattern(poolSize, maxTilesBetweenEmpty);

        for (int i = 0; i < poolSize; i++)
        {
            Vector3 pos = new Vector3(i * terrainWidth, 0, 0);
            GameObject obj = Instantiate(terrainPrefab, pos, Quaternion.identity, transform);
            terrainPool.Enqueue(obj);

            bool first = i > 5;
            // 타일/빈공간 설정
            SetTileActive(obj, spawnPattern[i], first);

            // 장애물 생성 (주석 처리된 상태면 유지)
            // SpawnObstacleOnTerrain(obj, terrainIndex);

            if (i == 0)
                SpawnPlayerOnTerrain(obj);

            terrainIndex++;
        }
        PlayerManager.Instance.isGameStartReady = true;
    }

    private void Update()
    {
        float delta = scrollSpeed * Time.deltaTime;
        terrainDistance += delta;
        PlayerManager.ChangeDistance(terrainDistance);

        foreach (var obj in terrainPool)
            obj.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);

        GameObject first = terrainPool.Peek();
        if (first.transform.position.x < -terrainWidth * 5)
        {
            terrainPool.Dequeue();
            float lastX = GetLastTerrainX();
            first.transform.position = new Vector3(lastX + terrainWidth, 0, 0);

            // 기존 장애물 제거
            foreach (Transform child in first.transform)
            {
                if (child.CompareTag("Obstacle"))
                    Destroy(child.gameObject);
            }

            // 다음 타일 패턴 적용
            if (terrainIndex >= spawnPattern.Count)
                spawnPattern = GenerateSpawnPattern(poolSize, maxTilesBetweenEmpty);

            int patternValue = spawnPattern[terrainIndex % spawnPattern.Count];
            SetTileActive(first, patternValue, true);

            // 장애물 생성 (필요 시 활성화)
            // SpawnObstacleOnTerrain(first, terrainIndex);

            terrainIndex++;
            terrainPool.Enqueue(first);
        }
    }

    // 타일 활성/비활성
    private void SetTileActive(GameObject tile, int state, bool first)
    {
        tile.SetActive(state == 1);
        if (state == 1 && first) SpawnItemOnTerrain(tile);
    }

    // 랜덤 패턴 생성 (빈공간 좌우 타일 보장 + 빈공간 간 최대 타일 수 설정)
    private List<int> GenerateSpawnPattern(int length, int maxBetweenEmpty)
    {
        List<int> pattern = new List<int>();
        int tilesSinceLastEmpty = 0;

        pattern.Add(1); // 첫 타일은 항상 활성화
        for (int i = 1; i < length - 1; i++)
        {
            if (tilesSinceLastEmpty >= maxBetweenEmpty && Random.value < 0.3f)
            {
                pattern.Add(0); // 빈공간 삽입
                tilesSinceLastEmpty = 0;
            }
            else
            {
                pattern.Add(1);
                tilesSinceLastEmpty++;
            }
        }
        pattern.Add(1); // 마지막 타일도 항상 활성화

        // 빈공간 양옆 검사: 강제 보정
        for (int i = 1; i < pattern.Count - 1; i++)
        {
            if (pattern[i] == 0)
            {
                pattern[i - 1] = 1;
                pattern[i + 1] = 1;
            }
        }

        return pattern;
    }

    private float GetPrefabWidth(GameObject prefab)
    {
        if (prefab.TryGetComponent<SpriteRenderer>(out var sr))
            return sr.bounds.size.x;
        if (prefab.TryGetComponent<Collider2D>(out var col))
            return col.bounds.size.x;
        return 1f;
    }

    private void SpawnPlayerOnTerrain(GameObject terrain)
    {
        float terrainTopY = 0f;
        if (terrain.TryGetComponent<SpriteRenderer>(out var terrainSR))
            terrainTopY = terrain.transform.position.y + terrainSR.bounds.extents.y;
        else if (terrain.TryGetComponent<Collider2D>(out var terrainCol))
            terrainTopY = terrain.transform.position.y + terrainCol.bounds.extents.y;

        float playerHeight = 1f;
        if (playerInstance.TryGetComponent<SpriteRenderer>(out var playerSR))
            playerHeight = playerSR.bounds.size.y;
        else if (playerInstance.TryGetComponent<Collider2D>(out var playerCol))
            playerHeight = playerCol.bounds.size.y;

        Vector3 spawnPos = new Vector3(
            terrain.transform.position.x,
            terrainTopY + playerHeight / 2f,
            0f
        );

        playerInstance.transform.position = spawnPos;
    }

    private int consecutiveObstacleCount = 0;
    private int skipCount = 0;

    private void SpawnObstacleOnTerrain(GameObject terrain, int index)
    {
        if (index == 0 || obstaclePrefabs == null || obstaclePrefabs.Count == 0)
            return;

        if (!terrain.activeSelf)
            return; // 비활성 타일에는 장애물 생성 금지

        if (skipCount > 0)
        {
            skipCount--;
            consecutiveObstacleCount = 0;
            return;
        }

        bool placeObstacle = Random.value < 0.5f;

        if (placeObstacle)
        {
            float x = terrain.transform.position.x;
            float y = terrain.transform.position.y + obstacleHeightOffset;

            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            GameObject obstacle = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, terrain.transform);
            obstacle.tag = "Obstacle";

            consecutiveObstacleCount++;

            if (consecutiveObstacleCount >= 2)
            {
                skipCount = 2;
                consecutiveObstacleCount = 0;
            }
        }
        else
        {
            consecutiveObstacleCount = 0;
        }
    }

    private void SpawnItemOnTerrain(GameObject terrain)
    {
        if (spawnItem == null || Random.value > spawnPercent)
            return;

        float x = terrain.transform.position.x;
        float y;

        if (Random.Range(0,4) > 1)
        {
            // 타일 위에 생성
            y = terrain.transform.position.y + 1.75f; // 적절한 아이템 높이 조절
        }
        else
        {
            // 빈 공간일 경우 공중에 생성
            y = terrain.transform.position.y + 3.25f;
        }

        SpawnItem spitem = Instantiate(spawnItem, new Vector3(x, y, 0f), Quaternion.identity, terrain.transform).GetComponent<SpawnItem>();
        spitem.transform.localScale = new Vector3(0.15f, 0.15f, 0);
        spitem.SetItemType("Coin");
    }


    private float GetLastTerrainX()
    {
        float maxX = float.MinValue;
        foreach (var obj in terrainPool)
        {
            if (obj.transform.position.x > maxX)
                maxX = obj.transform.position.x;
        }
        return maxX;
    }
}
