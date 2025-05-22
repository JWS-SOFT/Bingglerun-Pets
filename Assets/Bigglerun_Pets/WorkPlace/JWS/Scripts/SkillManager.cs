using System;
using System.Collections;
using UnityEngine;

public class SkillManager : MonoBehaviour
{
    public PlayerSkillData currentSkill;

    public void ActivateSkill(string characterId)
    {
        // 스킬을 불러오거나 캐릭터 타입에 따라 설정
        switch (characterId)
        {
            case "dog":
                currentSkill = new PlayerSkillData
                {
                    characterId = "dog",
                    skillType = PlayerSkillType.DogBarkDestroy,
                    skillName = "짖기",
                    description = "크게 짖어서 전방 장애물을 파괴합니다.",
                    duration = 0f,
                    cooldown = 5f,
                    power = 10f, // 파괴 범위 등
                    isInvincibleDuringSkill = false
                };
                break;

            case "cat":
                currentSkill = new PlayerSkillData
                {
                    characterId = "cat",
                    skillType = PlayerSkillType.CatSuperJumpInvincible,
                    skillName = "수퍼점프",
                    description = "높게 점프하고 무적 상태가 됩니다.",
                    duration = 5f,
                    cooldown = 7f,
                    power = /*15f*/5f, // 점프력
                    isInvincibleDuringSkill = true
                };
                break;

            case "hamster":
                currentSkill = new PlayerSkillData
                {
                    characterId = "hamster",
                    skillType = PlayerSkillType.HamsterRollInvincible,
                    skillName = "굴러가기",
                    description = "굴러서 이동하며 무적이 됩니다.",
                    duration = 3f,
                    cooldown = 8f,
                    power = 5f, // 속도나 거리
                    isInvincibleDuringSkill = true
                };
                break;
        }

        Debug.Log($"[스킬 발동] {currentSkill.characterId} - {currentSkill.skillName}");

        // 실제 실행 로직 연결
        StartSkillEffect();
    }

    private void StartSkillEffect()
    {
        // 여기서 실제 효과 (파괴, 점프, 굴림) 처리
        switch (currentSkill.skillType)
        {
            case PlayerSkillType.DogBarkDestroy:
                DestroyObstacles(currentSkill.power);
                break;
            case PlayerSkillType.CatSuperJumpInvincible:
                ApplyJump(currentSkill.power);
                SetInvincibility(currentSkill.duration);
                break;
            case PlayerSkillType.HamsterRollInvincible:
                StartRolling(currentSkill.power);
                SetInvincibility(currentSkill.duration);
                break;
        }
    }

    //05.21 HJ 수정
    //강아지(장애물 파괴)
    private void DestroyObstacles(float range)
    {
        // 전방 범위 내 장애물 제거 처리
        Debug.Log($"장애물 {range} 범위로 파괴!");
    }

    //슈퍼점프(고양이)
    //애니메이션 등 추가 필요
    private void ApplyJump(float jumpForce)
    {
        Debug.Log($"점프력 {jumpForce} 만큼 점프!");

        var player = PlayerManager.Instance.PlayerController;

        if(player != null && !player.IsRecovering)
        {
            StartCoroutine(SuperJumpRoutine(player, jumpForce, 3f));
        }
    }

    private IEnumerator SuperJumpRoutine(PlayerController player, float jumpForce, float distance)
    {
        float duration = 0.5f;  //점프시간
        float elapsed = 0f;

        //위치, 방향
        Vector2 start = player.transform.position;
        Vector2 direction = Vector2.right;
        Vector2 end = start + direction * distance;

        //카메라 오프셋
        Vector3 cameraInitialOffset = Camera.main.transform.position - player.transform.position;

        //카메라 y,z 축 고정 (현재 카메라 높이 유지)
        float fixedCameraY = Camera.main.transform.position.y;
        float fixedCameraZ = Camera.main.transform.position.z;

        player.enabled = false; //점프 시간동안 입력방지

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            float x = Mathf.Lerp(start.x, end.x, t);    //수펑 이동 보간
            float y = start.y + Mathf.Sin(t * Mathf.PI) * jumpForce;    //포물선 점프

            Vector2 pos = new Vector2(x, y);
            player.transform.position = pos;

            //카메라 x축 이동, y,z축 고정
            float cameraX = player.transform.position.x + cameraInitialOffset.x;
            Camera.main.transform.position = new Vector3(cameraX, fixedCameraY, fixedCameraZ);

            elapsed += Time.deltaTime;
            yield return null;
        }

        player.transform.position = end;
        player.enabled = true;
    }

    //구르기(햄스터)
    //애니메이션 추가 필요
    private void StartRolling(float speed)
    {
        Debug.Log($"속도 {speed}로 굴러가는 중!");

        var player = PlayerManager.Instance.PlayerController;

        if (player != null && !player.IsRecovering)
        {
            float duration = currentSkill.duration;
            StartCoroutine(StartRollingRoutine(player, speed, duration));
        }
    }

    private IEnumerator StartRollingRoutine(PlayerController player, float speedMultiplier, float duration)
    {
        //float elapsed = 0f;
        //Vector3 direction = Vector3.right;

        ////카메라 오프셋
        //Vector3 cameraInitialOffset = Camera.main.transform.position - player.transform.position;

        ////카메라 y,z 축 고정 (현재 카메라 높이 유지)
        //float fixedCameraY = Camera.main.transform.position.y;
        //float fixedCameraZ = Camera.main.transform.position.z;

        ////플레이어 입력 방지
        //player.enabled = false;
        ////구르기 애니메이션 처리

        //while (elapsed < duration)
        //{
        //    player.transform.Translate(direction * speed * Time.deltaTime);

        //    //카메라 X축만 이동
        //    float cameraX = player.transform.position.x + cameraInitialOffset.x;
        //    Camera.main.transform.position = new Vector3(cameraX, fixedCameraY, fixedCameraZ);


        //    elapsed += Time.deltaTime;
        //    yield return null;
        //}

        //player.enabled = true;
        ////구르기 애니메이션 처리
        ///

        float elapsed = 0f;

        TerrainScrollManager terrain = FindFirstObjectByType<TerrainScrollManager>();
        float originalSpeed = terrain != null ? terrain.ScrollSpeed : 5f;
        float boostedSpeed = originalSpeed * speedMultiplier;
        if (terrain != null) terrain.ScrollSpeed = boostedSpeed;

        player.enabled = false;

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            float distance = boostedSpeed * deltaTime;

            // ✅ 실시간 점수 반영
            ScoreManager.Instance?.AddHorizontalDistance(distance);

            elapsed += deltaTime;
            yield return null;
        }

        player.enabled = true;

        if (terrain != null) terrain.ScrollSpeed = originalSpeed;
    }

    private void SetInvincibility(float seconds)
    {
        Debug.Log($"무적 상태 {seconds}초 동안 유지");

        PlayerManager.Instance.SetInvincible(seconds);
    }
}

 [Serializable]
public class PlayerSkillData
{
    public string characterId;                 // "dog", "cat", "hamster"
    public PlayerSkillType skillType;
    public string skillName;
    public string description;

    public float duration;                     // 지속시간 (무적 등)
    public float cooldown;                     // 쿨타임
    public float power;                        // 영향력 (예: 파괴 범위, 점프력 등)

    // 기타 옵션 확장용
    public bool isInvincibleDuringSkill;
}

public enum PlayerSkillType
{
    None,
    DogBarkDestroy,
    CatSuperJumpInvincible,
    HamsterRollInvincible
}