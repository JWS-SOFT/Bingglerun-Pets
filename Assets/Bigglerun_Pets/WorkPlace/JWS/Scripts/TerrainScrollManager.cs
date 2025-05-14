using System.Collections.Generic;
using UnityEngine;

public class TerrainScrollManager : MonoBehaviour
{
    [Header("지형 설정")]
    public GameObject terrainPrefab;
    public int poolSize = 5;
    public float scrollSpeed = 5f;

    [Header("플레이어 설정")]
    public GameObject playerInstance;

    [Header("장애물 설정")]
    public List<GameObject> obstaclePrefabs;
    [Range(0.1f, 1f)]
    public float obstacleSpawnWidthRatio = 0.6f; // 가로 길이 비율로 스폰
    public float obstacleHeightOffset = 1.45f; // 지면 위로 얼마나 띄울지
    public float obstacleSpace = 4f; // 장애물 간격 얼마나 띄울지

    private float terrainWidth;
    private Queue<GameObject> terrainPool = new Queue<GameObject>();
    private int terrainIndex = 0;

    private void Start()
    {
        if (terrainPrefab == null || playerInstance == null)
        {
            Debug.LogError("지형 프리팹 또는 플레이어가 비어 있습니다!");
            return;
        }

        terrainWidth = GetPrefabWidth(terrainPrefab);

        for (int i = 0; i < poolSize; i++)
        {
            Vector3 pos = new Vector3(i * terrainWidth, 0, 0);
            GameObject obj = Instantiate(terrainPrefab, pos, Quaternion.identity, transform);
            terrainPool.Enqueue(obj);

            if (i == 0)
            {
                SpawnPlayerOnTerrain(obj); // 첫 번째 Terrain 위에 플레이어 배치
            }

            SpawnObstacleOnTerrain(obj, terrainIndex);
            terrainIndex++;
        }
    }

    private void Update()
    {
        foreach (var obj in terrainPool)
        {
            obj.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
        }

        GameObject first = terrainPool.Peek();
        if (first.transform.position.x < -terrainWidth)
        {
            terrainPool.Dequeue();
            float lastX = GetLastTerrainX();
            first.transform.position = new Vector3(lastX + terrainWidth, 0, 0);

            // 기존 장애물 삭제
            foreach (Transform child in first.transform)
            {
                if (child.CompareTag("Obstacle"))
                {
                    Destroy(child.gameObject);
                }
            }

            SpawnObstacleOnTerrain(first, terrainIndex);
            terrainIndex++;

            terrainPool.Enqueue(first);
        }
    }

    private float GetPrefabWidth(GameObject prefab)
    {
        if (prefab.TryGetComponent<SpriteRenderer>(out var sr))
            return sr.bounds.size.x;
        if (prefab.TryGetComponent<Collider2D>(out var col))
            return col.bounds.size.x;

        Debug.LogWarning("프리팹에 SpriteRenderer 또는 Collider2D가 없습니다.");
        return 1f;
    }

    private void SpawnPlayerOnTerrain(GameObject terrain)
    {
        float terrainTopY = 0f;
        if (terrain.TryGetComponent<SpriteRenderer>(out var terrainSR))
        {
            terrainTopY = terrain.transform.position.y + terrainSR.bounds.extents.y;
        }
        else if (terrain.TryGetComponent<Collider2D>(out var terrainCol))
        {
            terrainTopY = terrain.transform.position.y + terrainCol.bounds.extents.y;
        }

        float playerHeight = 1f;
        if (playerInstance.TryGetComponent<SpriteRenderer>(out var playerSR))
        {
            playerHeight = playerSR.bounds.size.y;
        }
        else if (playerInstance.TryGetComponent<Collider2D>(out var playerCol))
        {
            playerHeight = playerCol.bounds.size.y;
        }

        Vector3 spawnPos = new Vector3(
            terrain.transform.position.x,
            terrainTopY + playerHeight / 2f,
            0f
        );

        playerInstance.transform.position = spawnPos;
    }

    private void SpawnObstacleOnTerrain(GameObject terrain, int index)
    {
        if (index == 0 || obstaclePrefabs == null || obstaclePrefabs.Count == 0)
            return;

        float usableWidth = terrainWidth * obstacleSpawnWidthRatio;
        float margin = (terrainWidth - usableWidth) / 2f;

        float left = terrain.transform.position.x - terrainWidth / 2f + margin;
        float right = terrain.transform.position.x + terrainWidth / 2f - margin;

        // 장애물 사이 간격 설정
        float obstacleSpacing = obstacleSpace;
        int obstacleCount = Mathf.FloorToInt(usableWidth / obstacleSpacing);

        for (int i = 0; i < obstacleCount; i++)
        {
            float offset = obstacleSpacing * i + Random.Range(-1f, 1f); // 약간의 무작위 위치
            float x = Mathf.Clamp(left + offset, left, right);
            float groundY = terrain.transform.position.y;
            float y = groundY + obstacleHeightOffset;

            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            GameObject obstacle = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, terrain.transform);
            obstacle.tag = "Obstacle";
        }
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
