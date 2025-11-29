using System.Collections.Generic;
using UnityEngine;

namespace ClashOfGods.Gameplay.Units
{
    /// <summary>
    /// 单位Buff管理器 - 管理Buff/Debuff（支持热重载）
    /// Unit buff manager - Manages buffs and debuffs (Hot reload compatible)
    /// </summary>
    public class UnitBuffManager : MonoBehaviour
    {
        [Header("Active Buffs")]
        public List<BuffData> ActiveBuffs = new List<BuffData>();

        private UnitEntity unitEntity;

        private void Awake()
        {
            unitEntity = GetComponent<UnitEntity>();
        }

        private void Update()
        {
            Tick();
        }

        /// <summary>
        /// 添加Buff
        /// Add buff
        /// </summary>
        public void AddBuff(BuffData buff)
        {
            // 检查是否已存在相同Buff
            BuffData existingBuff = ActiveBuffs.Find(b => b.BuffID == buff.BuffID);
            
            if (existingBuff != null)
            {
                // 刷新持续时间或叠加层数
                if (buff.IsStackable)
                {
                    existingBuff.StackCount = Mathf.Min(existingBuff.StackCount + 1, existingBuff.MaxStack);
                }
                existingBuff.RemainingDuration = buff.Duration;
            }
            else
            {
                ActiveBuffs.Add(buff);
                ApplyBuffEffect(buff, true);
            }

            Debug.Log($"[Buff] Added {buff.BuffName} to {unitEntity.Config.UnitName}");
        }

        /// <summary>
        /// 移除Buff
        /// Remove buff
        /// </summary>
        public void RemoveBuff(int buffID)
        {
            BuffData buff = ActiveBuffs.Find(b => b.BuffID == buffID);
            
            if (buff != null)
            {
                ApplyBuffEffect(buff, false);
                ActiveBuffs.Remove(buff);
                Debug.Log($"[Buff] Removed {buff.BuffName} from {unitEntity.Config.UnitName}");
            }
        }

        /// <summary>
        /// 每帧更新Buff
        /// Tick buffs per frame
        /// </summary>
        public void Tick()
        {
            List<BuffData> buffsToRemove = new List<BuffData>();

            foreach (BuffData buff in ActiveBuffs)
            {
                buff.RemainingDuration -= Time.deltaTime;

                // 处理持续性效果（如流血、中毒）
                if (buff.TickInterval > 0)
                {
                    buff.TickTimer += Time.deltaTime;
                    if (buff.TickTimer >= buff.TickInterval)
                    {
                        ApplyTickEffect(buff);
                        buff.TickTimer = 0f;
                    }
                }

                if (buff.RemainingDuration <= 0)
                {
                    buffsToRemove.Add(buff);
                }
            }

            // 移除过期Buff
            foreach (BuffData buff in buffsToRemove)
            {
                RemoveBuff(buff.BuffID);
            }
        }

        /// <summary>
        /// 应用Buff效果
        /// Apply buff effect
        /// </summary>
        private void ApplyBuffEffect(BuffData buff, bool isApplying)
        {
            float multiplier = isApplying ? 1f : -1f;

            switch (buff.Type)
            {
                case BuffType.AttackBoost:
                    unitEntity.Stats.AttackMultiplier += buff.Value * multiplier;
                    break;
                case BuffType.DefenseBoost:
                    unitEntity.Stats.DefenseMultiplier += buff.Value * multiplier;
                    break;
                case BuffType.AttackSpeedBoost:
                    unitEntity.Stats.AttackSpeedMultiplier += buff.Value * multiplier;
                    break;
                case BuffType.Stun:
                    // TODO: 实现眩晕效果
                    break;
                case BuffType.Slow:
                    unitEntity.Stats.MoveSpeed *= isApplying ? (1f - buff.Value) : (1f / (1f - buff.Value));
                    break;
            }
        }

        /// <summary>
        /// 应用持续性效果
        /// Apply tick effect
        /// </summary>
        private void ApplyTickEffect(BuffData buff)
        {
            switch (buff.Type)
            {
                case BuffType.Poison:
                case BuffType.Bleed:
                    unitEntity.TakeDamage(buff.TickDamage);
                    break;
                case BuffType.Regeneration:
                    unitEntity.ApplyHealing(buff.TickDamage);
                    break;
            }
        }

        /// <summary>
        /// 净化负面效果
        /// Cleanse negative effects
        /// </summary>
        public void Cleanse(BuffType type)
        {
            List<BuffData> buffsToRemove = new List<BuffData>();

            foreach (BuffData buff in ActiveBuffs)
            {
                if (!buff.IsPositive && (type == BuffType.All || buff.Type == type))
                {
                    buffsToRemove.Add(buff);
                }
            }

            foreach (BuffData buff in buffsToRemove)
            {
                RemoveBuff(buff.BuffID);
            }

            Debug.Log($"[Buff] Cleansed {buffsToRemove.Count} negative effects");
        }

        #region Hot Reload Support
        
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] UnitBuffManager reloaded. Active buffs: {ActiveBuffs.Count}");
            if (unitEntity == null)
                unitEntity = GetComponent<UnitEntity>();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] UnitBuffManager static reload");
        }

        #endregion
    }

    /// <summary>
    /// Buff数据
    /// Buff data
    /// </summary>
    [System.Serializable]
    public class BuffData
    {
        public int BuffID;
        public string BuffName;
        public BuffType Type;
        public bool IsPositive = true;
        public float Value;
        public float Duration;
        public float RemainingDuration;
        
        [Header("Stacking")]
        public bool IsStackable = false;
        public int StackCount = 1;
        public int MaxStack = 5;

        [Header("Tick Effect")]
        public float TickInterval = 0f;
        public float TickDamage = 0f;
        public float TickTimer = 0f;
    }

    /// <summary>
    /// Buff类型
    /// Buff type
    /// </summary>
    public enum BuffType
    {
        All,                // 全部
        AttackBoost,        // 攻击提升
        DefenseBoost,       // 防御提升
        AttackSpeedBoost,   // 攻速提升
        MoveSpeedBoost,     // 移速提升
        Stun,               // 眩晕
        Slow,               // 减速
        Poison,             // 中毒
        Bleed,              // 流血
        Curse,              // 诅咒
        Regeneration,       // 回复
        Shield              // 护盾
    }
}
