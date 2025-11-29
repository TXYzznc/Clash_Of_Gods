using UnityEngine;

namespace ClashOfGods.Core
{
    /// <summary>
    /// 技能配置 - ScriptableObject
    /// Skill configuration - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "SkillConfig", menuName = "ClashOfGods/Configs/Skill Config")]
    public class SkillConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public int SkillID;
        public string SkillName;
        public string Description;
        public Sprite Icon;

        [Header("Skill Type")]
        public SkillType Type;
        public bool IsPassive;

        [Header("Cost & Cooldown")]
        public float ManaCost;
        public float Cooldown;

        [Header("Effect")]
        public float Damage;
        public float HealAmount;
        public float BuffDuration;
        public float Range;
        public float Radius;

        [Header("Visual & Audio")]
        public GameObject EffectPrefab;
        public AudioClip CastSound;
        public string AnimationTrigger;
    }

    /// <summary>
    /// 技能类型
    /// Skill type
    /// </summary>
    public enum SkillType
    {
        Active,     // 主动技能
        Passive     // 被动技能
    }
}
