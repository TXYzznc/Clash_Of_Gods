using UnityEngine;

namespace ClashOfGods.Core
{
    /// <summary>
    /// 卡牌配置 - ScriptableObject
    /// Card configuration - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "CardConfig", menuName = "ClashOfGods/Configs/Card Config")]
    public class CardConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public int CardID;
        public string CardName;
        public string Description;
        public Sprite CardIcon;

        [Header("Card Type")]
        public CardType Type;

        [Header("Cost")]
        public int ManaCost;

        [Header("Effects")]
        public CardEffectType EffectType;
        public float EffectValue;
        public float EffectRadius;
        public float EffectDuration;

        [Header("Visual")]
        public GameObject EffectPrefab;
        public AudioClip CastSound;
    }

    /// <summary>
    /// 卡牌类型
    /// Card type
    /// </summary>
    public enum CardType
    {
        Strategy,   // 策略卡
        Summon      // 召唤卡
    }

    /// <summary>
    /// 卡牌效果类型
    /// Card effect type
    /// </summary>
    public enum CardEffectType
    {
        Damage,         // 伤害
        Heal,           // 治疗
        Buff,           // 增益
        Debuff,         // 减益
        Control,        // 控制
        Summon,         // 召唤
        TerrainChange   // 地形改变
    }
}
