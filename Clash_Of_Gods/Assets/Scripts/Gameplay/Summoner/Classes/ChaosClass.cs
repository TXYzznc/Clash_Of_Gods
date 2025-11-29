using UnityEngine;

namespace ClashOfGods.Gameplay.Summoner.Classes
{
    /// <summary>
    /// 混沌职业 - 随机性强，高风险高回报
    /// Chaos class - High randomness, high risk high reward
    /// </summary>
    public class ChaosClass : BaseSummonerClass
    {
        [Header("Chaos Settings")]
        public float DoubleDamageChance = 0.15f;
        public float DoubleHealChance = 0.1f;
        public float HalfDamageChance = 0.1f;
        public float BackfireChance = 0.08f;

        public override void OnClassInitialized()
        {
            base.OnClassInitialized();
            Debug.Log("[Chaos] 混沌回响 (Chaos Echo) passive loaded");
        }

        /// <summary>
        /// 【混沌回响】被动 - 技能有概率产生混沌效果
        /// Chaos Echo passive - Skills have chance to produce chaos effects
        /// </summary>
        public float ApplyChaosEffect(float baseValue, bool isHealing = false)
        {
            float random = Random.value;
            
            if (isHealing)
            {
                if (random < DoubleHealChance)
                {
                    Debug.Log("[Chaos] Chaos Echo: Double healing!");
                    return baseValue * 2f;
                }
            }
            else
            {
                if (random < DoubleDamageChance)
                {
                    Debug.Log("[Chaos] Chaos Echo: Double damage!");
                    return baseValue * 2f;
                }
                else if (random < DoubleDamageChance + HalfDamageChance)
                {
                    Debug.Log("[Chaos] Chaos Echo: Half damage!");
                    return baseValue * 0.5f;
                }
                else if (random < DoubleDamageChance + HalfDamageChance + BackfireChance)
                {
                    Debug.Log("[Chaos] Chaos Echo: Backfire!");
                    // TODO: 对自身造成轻微反噬
                    summonerController.TakeDamage(baseValue * 0.2f);
                    return 0f;
                }
            }
            
            return baseValue;
        }

        /// <summary>
        /// 【混沌飞弹】主动技能
        /// Chaos Missile active skill
        /// </summary>
        public void CastChaosMissile()
        {
            int missileCount = Random.Range(3, 6);
            
            Debug.Log($"[Chaos] Chaos Missile: Firing {missileCount} missiles!");
            
            for (int i = 0; i < missileCount; i++)
            {
                // TODO: 发射混沌能量弹
                Vector3 randomDirection = Random.insideUnitSphere;
                Debug.Log($"[Chaos] Missile {i + 1} fired in direction: {randomDirection}");
            }
        }

        /// <summary>
        /// 【命运骰子】特化技能（第三阶）
        /// Fate Dice specialized skill (Phase 3)
        /// </summary>
        public void CastFateDice()
        {
            int diceRoll = Random.Range(1, 7);
            
            Debug.Log($"[Chaos] Fate Dice rolled: {diceRoll}");
            
            switch (diceRoll)
            {
                case 1:
                    Debug.Log("[Chaos] Rolled 1: Silenced for 3 seconds!");
                    // TODO: 沉默自己3秒
                    break;
                case 2:
                case 3:
                    Debug.Log("[Chaos] Rolled 2-3: Small buff gained!");
                    // TODO: 获得小额增益
                    break;
                case 4:
                case 5:
                    Debug.Log("[Chaos] Rolled 4-5: Medium buff gained!");
                    // TODO: 获得中额增益
                    break;
                case 6:
                    Debug.Log("[Chaos] Rolled 6: EPIC BUFF!");
                    // TODO: 获得史诗增益
                    break;
            }
        }
    }
}
