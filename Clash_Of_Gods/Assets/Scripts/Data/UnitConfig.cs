using UnityEngine;

namespace ClashOfGods.Core
{
    /// <summary>
    /// 单位配置 - ScriptableObject
    /// Unit configuration - ScriptableObject
    /// </summary>
    [CreateAssetMenu(fileName = "UnitConfig", menuName = "ClashOfGods/Configs/Unit Config")]
    public class UnitConfig : ScriptableObject
    {
        [Header("Basic Info")]
        public int UnitID;
        public string UnitName;
        public string Description;
        public Sprite Icon;
        public GameObject Prefab;

        [Header("Mythology System")]
        public MythologyType MythologyType;
        public UnitRarity Rarity;

        [Header("Base Stats")]
        public float BaseHP = 100f;
        public float BaseAttack = 10f;
        public float BaseDefense = 5f;
        public float AttackSpeed = 1f;
        public float MoveSpeed = 3f;

        [Header("Evolution")]
        public int MaxTier = 4;
        public EvolutionBranch[] EvolutionBranches;

        [Header("Skills")]
        public int[] SkillIDs;

        [Header("Population Cost")]
        public int PopulationCost = 1;
    }

    /// <summary>
    /// 神话体系类型
    /// Mythology type
    /// </summary>
    public enum MythologyType
    {
        Chinese,    // 中华
        Japanese,   // 日本
        Norse,      // 北欧
        Indian,     // 印度
        Greek,      // 希腊
        Egyptian    // 埃及
    }

    /// <summary>
    /// 单位稀有度
    /// Unit rarity
    /// </summary>
    public enum UnitRarity
    {
        Common,     // 普通
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }

    /// <summary>
    /// 进化分支
    /// Evolution branch
    /// </summary>
    [System.Serializable]
    public class EvolutionBranch
    {
        public string BranchName;
        public int RequiredTier;
        public int RequiredItemID;
        public int ResultUnitID;
    }
}
