using UnityEngine;

namespace ClashOfGods.Gameplay.Summoner.Classes
{
    /// <summary>
    /// 德鲁伊职业 - 强化回复，召唤自然生物
    /// Druid class - Enhanced healing, summons nature creatures
    /// </summary>
    public class DruidClass : BaseSummonerClass
    {
        [Header("Druid Settings")]
        public float FruitSpawnInterval = 10f;
        public float FruitSpawnRadius = 500f;
        public bool IsInAnimalForm = false;

        [Header("Form Bonuses")]
        public float HumanFormHealPercent = 0.01f;
        public float AnimalFormArmorBonus = 0.15f;
        public float AnimalFormMoveSpeedBonus = 0.15f;
        public float AnimalFormDamageBonus = 0.15f;
        public float AnimalFormSpiritCostPerSecond = 0.01f;

        private float fruitSpawnTimer = 0f;

        public override void OnClassInitialized()
        {
            base.OnClassInitialized();
            Debug.Log("[Druid] 大地恩泽 (Earth's Blessing) passive loaded");
        }

        protected override void Update()
        {
            base.Update();
            
            // 【大地恩泽】被动：每10秒生成自然果实
            fruitSpawnTimer += Time.deltaTime;
            if (fruitSpawnTimer >= FruitSpawnInterval)
            {
                SpawnNatureFruit();
                fruitSpawnTimer = 0f;
            }

            // 处理形态
            if (IsInAnimalForm)
            {
                // 动物形态每秒消耗1%灵力
                float spiritCost = summonerController.RuntimeData.MaxSpirit * AnimalFormSpiritCostPerSecond * Time.deltaTime;
                summonerController.RuntimeData.CurrentSpirit -= spiritCost;

                if (summonerController.RuntimeData.CurrentSpirit <= 0)
                {
                    SwitchForm(); // 自动切换回人形
                }
            }
            else
            {
                // 人形态每秒恢复生命值
                float healAmount = summonerController.RuntimeData.MaxHP * HumanFormHealPercent * Time.deltaTime;
                summonerController.RuntimeData.CurrentHP = Mathf.Min(
                    summonerController.RuntimeData.CurrentHP + healAmount,
                    summonerController.RuntimeData.MaxHP
                );
            }
        }

        /// <summary>
        /// 生成自然果实
        /// Spawn nature fruit
        /// </summary>
        private void SpawnNatureFruit()
        {
            Vector3 randomPos = transform.position + Random.insideUnitSphere * FruitSpawnRadius;
            
            // 随机选择果实类型
            FruitType type = (FruitType)Random.Range(0, 4);
            
            Debug.Log($"[Druid] Spawned {type} fruit at {randomPos}");
            
            // TODO: 实例化果实GameObject
        }

        /// <summary>
        /// 【自然之子】主动技能 - 切换形态
        /// Nature's Child active skill - Switch form
        /// </summary>
        public void SwitchForm()
        {
            IsInAnimalForm = !IsInAnimalForm;
            
            if (IsInAnimalForm)
            {
                Debug.Log("[Druid] Switched to Animal Form!");
                // TODO: 应用动物形态Buff
            }
            else
            {
                Debug.Log("[Druid] Switched to Human Form!");
                // TODO: 移除动物形态Buff
            }
        }

        /// <summary>
        /// 召唤树精
        /// Summon Treant
        /// </summary>
        public void SummonTreent()
        {
            Debug.Log("[Druid] Summoning Treant!");
            // TODO: 召唤树精逻辑
        }

        public override void OnHPUpdated(float oldValue, float newValue)
        {
            base.OnHPUpdated(oldValue, newValue);
            
            // 生命值或灵力值低于50%时召唤树精
            float hpPercent = newValue / summonerController.RuntimeData.MaxHP;
            if (hpPercent < 0.5f)
            {
                // TODO: 检查冷却时间（150秒）
                SummonTreent();
            }
        }
    }

    /// <summary>
    /// 自然果实类型
    /// Nature fruit type
    /// </summary>
    public enum FruitType
    {
        Fire,   // 火：暴击+爆伤
        Earth,  // 土：防御+回血
        Water,  // 水：技能冷却+灵力回复
        Wind    // 风：移速+攻速
    }
}
