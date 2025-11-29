using UnityEngine;

namespace ClashOfGods.Gameplay.Summoner.Classes
{
    /// <summary>
    /// 术士职业 - 擅长Debuff和诅咒
    /// Warlock class - Specializes in debuffs and curses
    /// </summary>
    public class WarlockClass : BaseSummonerClass
    {
        [Header("Warlock Settings")]
        public float CurseChanceOnTakeDamage = 0.8f;
        public float CurseChanceOnDealDamage = 0.3f;
        public float DamageBoostToCursed = 0.1f;

        public override void OnClassInitialized()
        {
            base.OnClassInitialized();
            Debug.Log("[Warlock] 暗影咒体 (Shadow Curse Body) passive loaded");
        }

        public override void OnTakeDamage(GameObject source, float damage)
        {
            base.OnTakeDamage(source, damage);
            
            // 【暗影咒体】被动：受到伤害时80%概率诅咒伤害来源
            if (Random.value < CurseChanceOnTakeDamage)
            {
                ApplyCurse(source);
            }
        }

        public override void OnDealDamage(GameObject target, float damage)
        {
            base.OnDealDamage(target, damage);
            
            // 【暗影咒体】被动：造成伤害时30%概率诅咒目标
            if (Random.value < CurseChanceOnDealDamage)
            {
                ApplyCurse(target);
            }
        }

        /// <summary>
        /// 应用诅咒效果
        /// Apply curse effect
        /// </summary>
        private void ApplyCurse(GameObject target)
        {
            if (target == null) return;
            
            // TODO: 实现诅咒Buff系统
            Debug.Log($"[Warlock] Cursed {target.name}!");
        }

        /// <summary>
        /// 【生命虹吸】主动技能
        /// Life Siphon active skill
        /// </summary>
        public void CastLifeSiphon(GameObject target)
        {
            if (target == null) return;
            
            Debug.Log($"[Warlock] Life Siphon cast on {target.name}!");
            
            // TODO: 实现持续吸血和治疗逻辑
            // 持续时间内从目标吸取生命值
            // 治疗自己和生命值最低的友方
            // 目标进入虚弱状态
        }
    }
}
