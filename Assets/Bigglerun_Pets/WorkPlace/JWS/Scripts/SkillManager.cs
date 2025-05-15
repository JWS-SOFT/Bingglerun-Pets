using System;
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
                    duration = 2f,
                    cooldown = 7f,
                    power = 15f, // 점프력
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

    private void DestroyObstacles(float range)
    {
        // 전방 범위 내 장애물 제거 처리
        Debug.Log($"장애물 {range} 범위로 파괴!");
    }

    private void ApplyJump(float jumpForce)
    {
        Debug.Log($"점프력 {jumpForce} 만큼 점프!");
    }

    private void StartRolling(float speed)
    {
        Debug.Log($"속도 {speed}로 굴러가는 중!");
    }

    private void SetInvincibility(float seconds)
    {
        Debug.Log($"무적 상태 {seconds}초 동안 유지");
        // 실제 무적 로직 연결
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