using System.Collections.Generic;
using UnityEngine;
using ClashOfGods.Core;

namespace ClashOfGods.Gameplay.Summoner
{
    /// <summary>
    /// 召唤师技能系统 - 管理技能树、进阶和技能效果
    /// Summoner skill system - Manages skill tree, advancement, and skill effects
    /// </summary>
    public class SummonerSkillSystem : MonoBehaviour
    {
        [Header("Skill Data")]
        public List<int> UnlockedSkills = new List<int>();
        public Dictionary<int, float> SkillCooldowns = new Dictionary<int, float>();

        [Header("Advancement")]
        public int CurrentPhase = 1; // 当前阶级 (1-5)
        public int MaxPhase = 5;

        private SummonerController summonerController;

        private void Awake()
        {
            summonerController = GetComponent<SummonerController>();
        }

        private void Update()
        {
            UpdateCooldowns();
        }

        /// <summary>
        /// 更新技能冷却
        /// Update skill cooldowns
        /// </summary>
        private void UpdateCooldowns()
        {
            List<int> keysToUpdate = new List<int>(SkillCooldowns.Keys);
            
            foreach (int skillID in keysToUpdate)
            {
                if (SkillCooldowns[skillID] > 0)
                {
                    SkillCooldowns[skillID] -= Time.deltaTime;
                }
            }
        }

        /// <summary>
        /// 解锁职业进阶
        /// Unlock class phase advancement
        /// </summary>
        public void UnlockClassPhase(int phase)
        {
            if (phase > MaxPhase)
            {
                Debug.LogWarning($"[SkillSystem] Cannot advance beyond phase {MaxPhase}");
                return;
            }

            if (phase <= CurrentPhase)
            {
                Debug.LogWarning($"[SkillSystem] Already at phase {CurrentPhase}");
                return;
            }

            CurrentPhase = phase;
            UnlockPhaseSkills(phase);
            
            Debug.Log($"[SkillSystem] Advanced to phase {phase}");
        }

        /// <summary>
        /// 解锁阶段技能
        /// Unlock phase skills
        /// </summary>
        private void UnlockPhaseSkills(int phase)
        {
            // TODO: 根据职业和阶段解锁对应技能
            Debug.Log($"[SkillSystem] Unlocking skills for phase {phase}");
        }

        /// <summary>
        /// 检查被动触发
        /// Check passive triggers
        /// </summary>
        public void CheckPassiveTriggers(TriggerType type)
        {
            // TODO: 实现被动技能触发检测
            // 例如：狂战的【狂怒之心】在HP低于50%时触发
            
            switch (summonerController.RuntimeData.ClassType)
            {
                case SummonerClassType.Berserker:
                    CheckBerserkerPassives(type);
                    break;
                case SummonerClassType.Warlock:
                    CheckWarlockPassives(type);
                    break;
                case SummonerClassType.Chaos:
                    CheckChaosPassives(type);
                    break;
                case SummonerClassType.Druid:
                    CheckDruidPassives(type);
                    break;
            }
        }

        #region Class-Specific Passive Checks

        private void CheckBerserkerPassives(TriggerType type)
        {
            // 狂战被动：【狂怒之心】
            if (type == TriggerType.OnHPChange)
            {
                float hpPercent = summonerController.RuntimeData.CurrentHP / summonerController.RuntimeData.MaxHP;
                if (hpPercent < 0.5f)
                {
                    // TODO: 应用伤害提升Buff
                    Debug.Log("[Berserker] Fury Heart activated!");
                }
            }
        }

        private void CheckWarlockPassives(TriggerType type)
        {
            // 术士被动：【暗影咒体】
            if (type == TriggerType.OnTakeDamage || type == TriggerType.OnDealDamage)
            {
                // TODO: 应用诅咒效果
                Debug.Log("[Warlock] Shadow Curse triggered!");
            }
        }

        private void CheckChaosPassives(TriggerType type)
        {
            // 混沌被动：【混沌回响】
            if (type == TriggerType.OnSkillCast)
            {
                float random = Random.value;
                // TODO: 实现混沌效果
                Debug.Log($"[Chaos] Chaos Echo: {random}");
            }
        }

        private void CheckDruidPassives(TriggerType type)
        {
            // 德鲁伊被动：【大地恩泽】
            if (type == TriggerType.OnTimerTick)
            {
                // TODO: 生成自然果实
                Debug.Log("[Druid] Earth's Blessing spawned fruit!");
            }
        }

        #endregion

        /// <summary>
        /// 使用技能
        /// Use skill
        /// </summary>
        public bool UseSkill(int skillID, Vector3 target)
        {
            if (!CanUseSkill(skillID))
            {
                return false;
            }

            SkillConfig config = ConfigManager.Instance.GetSkillConfig(skillID);
            if (config == null)
            {
                return false;
            }

            // 消耗灵力
            summonerController.RuntimeData.CurrentSpirit -= config.ManaCost;

            // 设置冷却
            SkillCooldowns[skillID] = config.Cooldown;

            // 执行技能效果
            ExecuteSkillEffect(config, target);

            return true;
        }

        /// <summary>
        /// 检查是否可以使用技能
        /// Check if can use skill
        /// </summary>
        private bool CanUseSkill(int skillID)
        {
            if (!UnlockedSkills.Contains(skillID))
            {
                return false;
            }

            if (SkillCooldowns.ContainsKey(skillID) && SkillCooldowns[skillID] > 0)
            {
                return false;
            }

            SkillConfig config = ConfigManager.Instance.GetSkillConfig(skillID);
            if (config != null && summonerController.RuntimeData.CurrentSpirit < config.ManaCost)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 执行技能效果
        /// Execute skill effect
        /// </summary>
        private void ExecuteSkillEffect(SkillConfig config, Vector3 target)
        {
            // TODO: 实现具体技能效果
            Debug.Log($"[SkillSystem] Executing skill: {config.SkillName}");
        }
    }

    /// <summary>
    /// 触发类型
    /// Trigger type
    /// </summary>
    public enum TriggerType
    {
        OnHPChange,
        OnTakeDamage,
        OnDealDamage,
        OnSkillCast,
        OnUnitDeath,
        OnTimerTick
    }
}
