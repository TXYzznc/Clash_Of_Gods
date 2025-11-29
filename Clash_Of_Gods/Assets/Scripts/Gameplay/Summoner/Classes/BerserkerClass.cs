using UnityEngine;

namespace ClashOfGods.Gameplay.Summoner.Classes
{
    /// <summary>
    /// 狂战职业 - 提升攻速/吸血，可下场战斗
    /// Berserker class - Increases attack speed/lifesteal, can fight directly
    /// </summary>
    public class BerserkerClass : BaseSummonerClass
    {
        [Header("Berserker Settings")]
        public float LowHPThreshold = 0.5f;
        public float DamageBoostBelowHalf = 0.15f;
        public float DamageBoostBelowForty = 0.15f;

        private bool isFuryActive = false;

        public override void OnClassInitialized()
        {
            base.OnClassInitialized();
            Debug.Log("[Berserker] 狂怒之心 (Fury Heart) passive loaded");
        }

        public override void OnHPUpdated(float oldValue, float newValue)
        {
            base.OnHPUpdated(oldValue, newValue);
            
            float hpPercent = newValue / summonerController.RuntimeData.MaxHP;
            
            // 【狂怒之心】被动
            if (hpPercent < LowHPThreshold && !isFuryActive)
            {
                ActivateFury();
            }
            else if (hpPercent >= LowHPThreshold && isFuryActive)
            {
                DeactivateFury();
            }
        }

        /// <summary>
        /// 激活狂怒状态
        /// Activate fury state
        /// </summary>
        private void ActivateFury()
        {
            isFuryActive = true;
            
            // TODO: 应用伤害提升Buff到所有友方单位
            Debug.Log("[Berserker] Fury activated! All allies gain damage boost!");
            
            // 如果HP低于40%，额外提升
            float hpPercent = summonerController.RuntimeData.CurrentHP / summonerController.RuntimeData.MaxHP;
            if (hpPercent < 0.4f)
            {
                Debug.Log("[Berserker] Critical fury! Double damage boost!");
            }
        }

        /// <summary>
        /// 取消狂怒状态
        /// Deactivate fury state
        /// </summary>
        private void DeactivateFury()
        {
            isFuryActive = false;
            Debug.Log("[Berserker] Fury deactivated");
        }

        /// <summary>
        /// 【战意激昂】主动技能
        /// War Fervor active skill
        /// </summary>
        public void CastWarFervor()
        {
            // 消耗20点生命值
            summonerController.TakeDamage(20f);
            
            // TODO: 提升场上所有召唤物的攻速和伤害
            Debug.Log("[Berserker] War Fervor cast! All units gain attack speed and damage!");
            
            // 持续10秒
            Invoke(nameof(EndWarFervor), 10f);
        }

        private void EndWarFervor()
        {
            Debug.Log("[Berserker] War Fervor ended");
        }
    }
}
