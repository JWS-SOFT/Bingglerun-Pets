using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TerrainScrollManager : MonoBehaviour
{
    [Header("지형 설정")]
    [SerializeField] private GameObject terrainPrefab;
    [SerializeField] private int poolSize = 50;
    [SerializeField] private float scrollSpeed = 5f;
    public float ScrollSpeed
    {
        get => scrollSpeed;
        set => scrollSpeed = value;
    }

    [Header("장애물 설정")]
    [SerializeField] private List<GameObject> obstaclePrefabs;
    [SerializeField] private float obstacleHeightOffset = 0f;

    [Header("아이템 설정")]
    [SerializeField] private GameObject spawnItem;
    [Range(0f, 1f)][SerializeField] private float spawnPercent = 0.5f;
    [Range(0f, 1f)][SerializeField] private float invincibleItemChance = 0.1f;    //05.20 HJ 추가


    [Header("패턴 설정")]
    [SerializeField] private int maxTilesBetweenEmpty = 1;

    private float terrainWidth;
    private Queue<GameObject> terrainPool = new Queue<GameObject>();
    private int terrainIndex = 0;
    private float terrainDistance = 0;

    private List<int> spawnPattern;

    //05.22 HJ 추가
    private int invincibleCooldownCounter = 0;
    private const int invincibleCooldownLength = 5;

    //05.26 HJ 추가
    private int obstacleCooldownCounter = 0;
    private bool obstacleJustSpawned = false;
    private const int cooldownAfterObstacle = 5;

    public void StartTest(Vector3 lastTerranPosition)
    {
        if (terrainPrefab == null || PlayerManager.Player_Transform.gameObject == null)
        {
            Debug.LogError("지형 프리팹 또는 플레이어가 비어 있습니다!");
            return;
        }
        if (!PlayerManager.Instance.isBattleMode)
        {
            PlayerManager.Instance.stageLevel = GameManager.Instance.currentPlayStage;
        }

        terrainIndex = 0;
        terrainDistance = 0;
        terrainWidth = GetPrefabWidth(terrainPrefab);

        // 패턴 생성
        spawnPattern = GenerateSpawnPattern(poolSize, maxTilesBetweenEmpty);

        for (int i = 0; i < poolSize; i++)
        {
            Vector3 pos = lastTerranPosition + new Vector3(i * terrainWidth, 0, 0);
            GameObject obj;

            if (terrainPool.Count < poolSize)
            {
                // 새로 생성
                obj = Instantiate(terrainPrefab, pos, Quaternion.identity, transform);
            }
            else
            {
                // 풀에서 꺼내 재사용
                obj = terrainPool.Dequeue();
                obj.transform.position = pos;
            }

            terrainPool.Enqueue(obj);

            bool first = i > 5;
            SetTileActive(obj, spawnPattern[i], first);

            if (i == 0)
                SpawnPlayerOnTerrain(obj);

            terrainIndex++;
        }

        PlayerManager.Instance.SetPoisition();

        Debug.Log("설정거리 : " + PlayerManager.GetStageDistance() + "M");
    }

    private void Update()
    {
        if (!PlayerManager.Instance.isGameStartReady) return;
        if (PlayerManager.GetStageDistance() <= terrainDistance) return;
        
        float delta = scrollSpeed * Time.deltaTime;
        terrainDistance += delta;
        PlayerManager.ChangeDistance(terrainDistance);

        bool isTerrainDistanceMax = PlayerManager.GetStageDistance() <= (terrainIndex -1)* terrainWidth;
        foreach (var obj in terrainPool)
        {
            obj.transform.Translate(Vector3.left * scrollSpeed * Time.deltaTime);
        }

        GameObject first = terrainPool.Peek();
       if (!isTerrainDistanceMax && first.transform.position.x < -terrainWidth * 5)
        {
            terrainPool.Dequeue();
            float lastX = GetLastTerrainX();
            first.transform.position = new Vector3(lastX + terrainWidth, first.transform.position.y, 0);

            // 다음 타일 패턴 적용
            if (terrainIndex >= spawnPattern.Count)
                spawnPattern = GenerateSpawnPattern(poolSize, maxTilesBetweenEmpty);

            int patternValue = spawnPattern[terrainIndex % spawnPattern.Count];
            SetTileActive(first, patternValue, true);

            // 장애물 생성 (필요 시 활성화)
            SpawnObstacleOnTerrain(first, terrainIndex);

            terrainIndex++;
            terrainPool.Enqueue(first);
        }
    }

    public void RestTerrain()
    {
        foreach (var terrain in terrainPool)
        {
            terrain.SetActive(false);
            // Destroy(terrain);
        }

        // 거리 초기화도 함께 하고 싶으면 추가
        if (!PlayerManager.Instance.isBattleMode)
        {
            terrainDistance = 0f;
            terrainIndex = 0;
        }
    }

    // 타일 활성/비활성
    private void SetTileActive(GameObject tile, int state, bool first)
    {
        // 기존 장애물 제거
        if (tile.transform.childCount > 1)
        {
            Destroy(tile.transform.GetChild(tile.transform.childCount - 1).gameObject);
        }
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
        GameObject playerInstance = PlayerManager.Player_Transform.gameObject;
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

    //private int consecutiveObstacleCount = 0;
    //private int skipCount = 0;

    private void SpawnObstacleOnTerrain(GameObject terrain, int index)
    {
        if (index == 0 || obstaclePrefabs == null || obstaclePrefabs.Count == 0)
            return;

        if (!terrain.activeSelf)
            return; // 비활성 타일에는 장애물 생성 금지

        //if (skipCount > 0)
        //{
        //    skipCount--;
        //    consecutiveObstacleCount = 0;
        //    return;
        //}

        // 장애물 생성 쿨다운 중이라면 무조건 스킵
        if (obstacleCooldownCounter > 0)
        {
            obstacleCooldownCounter--;
            obstacleJustSpawned = false;
            return;
        }

        // 랜덤으로 장애물 생성 결정
        bool placeObstacle = Random.value < 0.5f;

        if (placeObstacle)
        {
            float x = terrain.transform.position.x;
            float y = terrain.transform.position.y + obstacleHeightOffset;

            GameObject prefab = obstaclePrefabs[Random.Range(0, obstaclePrefabs.Count)];
            GameObject obstacle = Instantiate(prefab, new Vector3(x, y, 0f), Quaternion.identity, terrain.transform);

            FitObstacleToTile(obstacle, terrain, prefab);

            obstacle.tag = "Obstacle";

            // 이전 타일도 장애물이 있었으면 → 연속 2개
            // 이번 타일이 첫 장애물이라도 → 쿨다운 시작
            obstacleCooldownCounter = cooldownAfterObstacle;
            obstacleJustSpawned = true;
        }
        else
        {
            obstacleJustSpawned = false;
        }
    }

    public static void FitObstacleToTile(GameObject obstacle, GameObject tile, GameObject prefab)
    {
        // 1. 타일의 보이는 가로 폭
        SpriteRenderer tileSR = tile.GetComponent<SpriteRenderer>();
        if (tileSR == null)
        {
            Debug.LogWarning("Tile에 SpriteRenderer가 없습니다.");
            return;
        }
        float tileWidth = tileSR.bounds.size.x;

        // 2. 장애물의 SpriteRenderer
        SpriteRenderer obstacleSR = obstacle.GetComponent<SpriteRenderer>();
        if (obstacleSR == null)
        {
            Debug.LogWarning("장애물에 SpriteRenderer가 없습니다.");
            return;
        }

        // 3. 프리팹에서 원본 스프라이트 비율과 로컬 스케일 비율 가져오기
        SpriteRenderer prefabSR = prefab.GetComponent<SpriteRenderer>();
        if (prefabSR == null)
        {
            Debug.LogWarning("프리팹에 SpriteRenderer가 없습니다.");
            return;
        }

        Vector2 spriteSize = prefabSR.sprite.bounds.size;
        Vector3 prefabScale = prefab.transform.localScale;

        // 스프라이트 원본 비율 × 프리팹에서 디자인된 스케일 비율
        float totalAspectRatio = (spriteSize.y / spriteSize.x) * (prefabScale.y / prefabScale.x);

        // 4. 장애물의 현재 보이는 가로 폭
        float obstacleWidth = obstacleSR.bounds.size.x;

        // 5. 타일에 맞추기 위한 X축 스케일
        float scaleX = tileWidth / obstacleWidth;

        // 6. 비율 유지한 Y축 스케일
        float scaleY = scaleX * totalAspectRatio;

        // 7. 부모 스케일 역보정
        Vector3 parentScale = tile.transform.lossyScale;
        Vector3 inverseParent = new Vector3(
            1f / parentScale.x,
            1f / parentScale.y,
            1f / parentScale.z
        );

        // 8. 최종 스케일 적용
        obstacle.transform.localScale = new Vector3(
            scaleX * inverseParent.x,
            scaleY * inverseParent.y,
            1f
        );
    }


    private void SpawnItemOnTerrain(GameObject terrain)
    {
        if (spawnItem == null || Random.value > spawnPercent)
            return;

        float x = terrain.transform.position.x;
        float y;

        if (Random.Range(0,10) > 2)
        {
            // 타일 위에 생성
            y = terrain.transform.position.y + 1f; // 적절한 아이템 높이 조절
        }
        else
        {
            // 빈 공간일 경우 공중에 생성
            y = terrain.transform.position.y + 2f;
        }

        SpawnItem spitem = Instantiate(spawnItem, new Vector3(x, y, 0f), Quaternion.identity, terrain.transform).GetComponent<SpawnItem>();
        spitem.transform.localScale = new Vector3(0.25f, 0.75f, 0);
        spitem.transform.SetSiblingIndex(terrain.transform.childCount - 1);

        //05.22 HJ 수정
        //아이템 스폰 확률 및 쿨다운 반영
        if (Random.value < spawnPercent)
        {
            string itemToSpawn;

            if (invincibleCooldownCounter > 0)
            {
                //쿨다운 중이면 무조건 코인
                itemToSpawn = "Coin";
                invincibleCooldownCounter--;
            }
            else
            {
                //쿨다운이 없으면 무적 확률 적용
                if (Random.value < invincibleItemChance)
                {
                    itemToSpawn = "Wing";
                    invincibleCooldownCounter = invincibleCooldownLength;
                }
                else
                {
                    itemToSpawn = "Coin";
                }
            }

            spitem.SetItemType(itemToSpawn);

            //if (Random.value > invincibleItemChance)
            //    spitem.SetItemType("Coin");
            //else
            //    spitem.SetItemType("Wing");
        }
        else
        {
            //아이템이 안나왔어도 쿨다운 감소
            if (invincibleCooldownCounter > 0)
                invincibleCooldownCounter--;
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
