using System;
using System.Collections;
using System.Collections.Generic;
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

    //05.28 HJ 수정
    //강아지(장애물 파괴)
    //애니메이션, 사운드 추가 필요
    private void DestroyObstacles(float range)
    {
        // 전방 범위 내 장애물 제거 처리
        Debug.Log($"장애물 {range} 범위로 파괴!");

        //짖기 이펙트, 사운드 추가
        PlayerManager.Instance.PlayerController.Player_Animator.SetTrigger("Attack");

        Vector2 playerPosition = PlayerManager.Instance.PlayerController.PlayerPosition;

        GameObject[] obstacles = GameObject.FindGameObjectsWithTag("Obstacle");

        GameObject nearestObstacle = null;
        float nearestDistance = float.MaxValue;

        foreach(GameObject obstacle in obstacles)
        {
            if(obstacle == null) continue;

            Vector2 obstaclePosition = obstacle.transform.position;
            float distance = obstaclePosition.x - playerPosition.x;

            if (distance > 0 && distance <= range && distance < nearestDistance)
            { 
                nearestDistance = distance;
                nearestObstacle = obstacle;
            }
        }

        if(nearestObstacle != null)
        {
            Destroy(nearestObstacle);

            //장애물 파괴 이펙트, 사운드 추가
        }
    }

    //05.21 HJ 수정
    //슈퍼점프(고양이)
    //애니메이션 등 추가 필요
    private void ApplyJump(float jumpForce)
    {
        Debug.Log($"점프력 {jumpForce} 만큼 점프!");

        var player = PlayerManager.Instance.PlayerController;

        if(player != null && !player.IsRecovering)
        {
            StartCoroutine(SuperJumpRoutine(player, jumpForce, jumpForce));
        }
    }

    private IEnumerator SuperJumpRoutine(PlayerController player, float jumpForce, float distance)
    {
        float duration = 0.5f;
        float elapsed = 0f;

        TerrainScrollManager terrain = PlayerManager.Instance.TerrainScrollManager;
        float originalSpeed = terrain != null ? terrain.ScrollSpeed : 5f;
        float boostedSpeed = distance / duration;
        if (terrain != null) terrain.ScrollSpeed = boostedSpeed;

        player.enabled = false;
        Vector2 start = player.transform.position;
        //슈퍼점프 애니메이션 활성화
        PlayerManager.Instance.PlayerController.Player_Animator.SetBool("Jump", true);

        while (elapsed < duration)
        {
            float t = elapsed / duration;

            //포물선 점프 (y축만 변경)
            float y = start.y + Mathf.Sin(t * Mathf.PI) * jumpForce;
            player.transform.position = new Vector2(start.x, y);

            //실시간 점수 반영
            float deltaTime = Time.deltaTime;
            float movedDistance = boostedSpeed * deltaTime;
            ScoreManager.Instance?.AddHorizontalDistance(movedDistance);

            elapsed += deltaTime;
            yield return null;
        }

        //슈퍼점프 애니메이션 비활성화
        PlayerManager.Instance.PlayerController.Player_Animator.SetBool("Jump", false);

        //y 위치 원복
        player.transform.position = start;
        player.enabled = true;
        

        if (terrain != null) terrain.ScrollSpeed = originalSpeed;
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
        float elapsed = 0f;

        TerrainScrollManager terrain = PlayerManager.Instance.TerrainScrollManager;

        float originalSpeed = terrain != null ? terrain.ScrollSpeed : 5f;
        float boostedSpeed = originalSpeed * speedMultiplier;
        if (terrain != null) terrain.ScrollSpeed = boostedSpeed;

        player.enabled = false;
        //구르기 애니메이션 활성화
        PlayerManager.Instance.PlayerController.Player_Animator.SetBool("Walk", true);

        while (elapsed < duration)
        {
            float deltaTime = Time.deltaTime;
            float distance = boostedSpeed * deltaTime;

            ScoreManager.Instance?.AddHorizontalDistance(distance);

            elapsed += deltaTime;
            yield return null;
        }

        //구르기 애니메이션 비활성화
        player.enabled = true;
        PlayerManager.Instance.PlayerController.Player_Animator.SetBool("Walk", false);

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