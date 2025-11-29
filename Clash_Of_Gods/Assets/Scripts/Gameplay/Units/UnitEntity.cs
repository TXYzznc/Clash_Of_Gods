using UnityEngine;
using Mirror;
using ClashOfGods.Core;

namespace ClashOfGods.Gameplay.Units
{
    /// <summary>
    /// 单位实体 - 挂载在所有战斗单位上（支持热重载）
    /// Unit entity - Attached to all combat units (Hot reload compatible)
    /// </summary>
    public class UnitEntity : NetworkBehaviour
    {
        [Header("Unit Data")]
        public UnitConfig Config;
        public UnitStats Stats;
        public UnitInstanceData InstanceData;

        [Header("Components")]
        public UnitAIController AIController;
        public UnitAnimationController AnimationController;
        public UnitBuffManager BuffManager;

        [Header("Runtime State")]
        public bool IsAlive = true;
        public bool IsPlayer = false;
        public Transform CurrentTarget;

        [Header("Debug")]
        public bool EnableDebugLogs = true;

        [Header("Hot Reload Test")]
        public float TestDamageMultiplier = 1.0f;
        public string TestMessage = "Original Message";
        public Color TestColor = Color.red;

        private void Awake()
        {
            InitializeComponents();
        }

        private void Update()
        {
            // 热重载测试：按空格键输出当前参数
            if (Input.GetKeyDown(KeyCode.Space))
            {
                TestHotReload();
            }
        }

        /// <summary>
        /// 热重载测试方法
        /// Hot reload test method
        /// </summary>
        private void TestHotReload()
        {
            Debug.Log("=== [Hot Reload Test] ===");
            Debug.Log($"Unit Name: {Config?.UnitName ?? "No Config"}");
            Debug.Log($"HP: {Stats?.CurrentHP ?? 0}/{Stats?.MaxHP ?? 0}");
            Debug.Log($"Test Damage Multiplier: {TestDamageMultiplier}");
            Debug.Log($"Test Message: {TestMessage}");
            Debug.Log($"Test Color: {TestColor}");
            Debug.Log($"Is Alive: {IsAlive}");
            Debug.Log("========================");
        }

        /// <summary>
        /// 初始化组件
        /// Initialize components
        /// </summary>
        private void InitializeComponents()
        {
            if (AIController == null)
                AIController = GetComponent<UnitAIController>();

            if (AnimationController == null)
                AnimationController = GetComponent<UnitAnimationController>();

            if (BuffManager == null)
                BuffManager = GetComponent<UnitBuffManager>();

            if (Stats == null)
                Stats = new UnitStats();
        }

        /// <summary>
        /// 初始化单位
        /// Initialize unit
        /// </summary>
        public void Initialize(UnitConfig config, UnitInstanceData instanceData = null)
        {
            if (config == null)
            {
                Debug.LogError("[Unit] Cannot initialize with null config!");
                return;
            }

            Config = config;
            InstanceData = instanceData ?? new UnitInstanceData { ConfigID = config.UnitID };

            // 确保Stats已初始化
            if (Stats == null)
            {
                Stats = new UnitStats();
            }

            // 初始化属性
            Stats.Initialize(config);

            // 恢复生命值（生命继承机制）
            if (InstanceData.CurrentHP > 0)
            {
                Stats.CurrentHP = InstanceData.CurrentHP;
            }
            else
            {
                Stats.CurrentHP = Stats.MaxHP;
            }

            IsAlive = true;
            LogDebug($"{config.UnitName} initialized. HP: {Stats.CurrentHP}/{Stats.MaxHP}");
        }

        /// <summary>
        /// 进化单位
        /// Evolve unit
        /// </summary>
        public void Evolve(int branchID)
        {
            if (Config == null || Config.EvolutionBranches == null)
            {
                Debug.LogWarning("[Unit] No evolution branches available");
                return;
            }

            foreach (var branch in Config.EvolutionBranches)
            {
                if (branch.ResultUnitID == branchID)
                {
                    LogDebug($"Evolving to {branch.BranchName}");

                    // 加载新配置
                    UnitConfig newConfig = ConfigManager.Instance?.GetUnitConfig(branch.ResultUnitID);
                    if (newConfig != null)
                    {
                        Config = newConfig;
                        Stats.Initialize(newConfig);
                        InstanceData.Tier++;
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 受到伤害
        /// Take damage
        /// </summary>
        public void TakeDamage(float damage, GameObject source = null)
        {
            if (!IsAlive) return;

            float finalDamage = Stats.CalculateDamageReceived(damage);
            Stats.CurrentHP -= finalDamage;

            LogDebug($"{Config?.UnitName ?? "Unit"} took {finalDamage:F1} damage. HP: {Stats.CurrentHP:F1}/{Stats.MaxHP:F1}");

            // 触发受伤事件
            EventManager.Instance?.TriggerEvent(new UnitDamagedEvent
            {
                Unit = gameObject,
                Damage = finalDamage,
                Source = source
            });

            if (Stats.CurrentHP <= 0)
            {
                OnDeath();
            }

            // 触发受伤动画
            AnimationController?.TriggerAnimation("Hit");
        }

        /// <summary>
        /// 治疗
        /// Apply healing
        /// </summary>
        public void ApplyHealing(float amount)
        {
            if (!IsAlive) return;

            float previousHP = Stats.CurrentHP;
            Stats.CurrentHP = Mathf.Min(Stats.CurrentHP + amount, Stats.MaxHP);
            float actualHealing = Stats.CurrentHP - previousHP;

            LogDebug($"{Config?.UnitName ?? "Unit"} healed {actualHealing:F1}. HP: {Stats.CurrentHP:F1}/{Stats.MaxHP:F1}");
        }

        /// <summary>
        /// 死亡处理
        /// Handle death
        /// </summary>
        public void OnDeath()
        {
            if (!IsAlive) return;

            IsAlive = false;
            Stats.CurrentHP = 0;
            InstanceData.CurrentHP = 0;

            LogDebug($"{Config?.UnitName ?? "Unit"} died!");

            // 触发死亡动画
            AnimationController?.TriggerAnimation("Death");

            // 触发死亡事件
            EventManager.Instance?.TriggerEvent(new UnitDeathEvent
            {
                Unit = gameObject,
                IsPlayer = IsPlayer
            });

            // 掉落装备
            DropEquipment();

            // 延迟销毁
            Destroy(gameObject, 2f);
        }

        /// <summary>
        /// 掉落装备
        /// Drop equipment
        /// </summary>
        private void DropEquipment()
        {
            if (InstanceData?.EquippedItemIDs != null)
            {
                foreach (int itemID in InstanceData.EquippedItemIDs)
                {
                    if (itemID > 0)
                    {
                        LogDebug($"Dropped item: {itemID}");
                    }
                }
            }
        }

        /// <summary>
        /// 攻击目标
        /// Attack target
        /// </summary>
        public void Attack(GameObject target)
        {
            if (!IsAlive || target == null) return;

            UnitEntity targetEntity = target.GetComponent<UnitEntity>();
            if (targetEntity != null)
            {
                float damage = Stats.GetFinalAttack();
                targetEntity.TakeDamage(damage, gameObject);

                // 触发攻击动画
                AnimationController?.TriggerAnimation("Attack");
            }
        }

        /// <summary>
        /// 获取单位状态摘要
        /// Get unit status summary
        /// </summary>
        public string GetStatusSummary()
        {
            return $"{Config?.UnitName ?? "Unknown"}: HP={Stats?.CurrentHP:F0}/{Stats?.MaxHP:F0}, Alive={IsAlive}";
        }

        /// <summary>
        /// 调试日志
        /// Debug log
        /// </summary>
        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[Unit] {message}");
            }
        }

        #region Hot Reload Support

        /// <summary>
        /// 热重载回调
        /// Hot reload callback
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] UnitEntity reloaded. {GetStatusSummary()}");

            // 重新初始化组件引用
            InitializeComponents();
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] UnitEntity static reload");
        }

        #endregion
    }

    /// <summary>
    /// 单位属性计算
    /// Unit stats calculation
    /// </summary>
    [System.Serializable]
    public class UnitStats
    {
        [Header("Current Stats")]
        public float CurrentHP;
        public float MaxHP;
        public float Attack;
        public float Defense;
        public float AttackSpeed;
        public float MoveSpeed;

        [Header("Modifiers")]
        public float AttackMultiplier = 1f;
        public float DefenseMultiplier = 1f;
        public float AttackSpeedMultiplier = 1f;

        /// <summary>
        /// 初始化属性
        /// Initialize stats
        /// </summary>
        public void Initialize(UnitConfig config)
        {
            if (config == null) return;

            MaxHP = config.BaseHP;
            CurrentHP = MaxHP;
            Attack = config.BaseAttack;
            Defense = config.BaseDefense;
            AttackSpeed = config.AttackSpeed;
            MoveSpeed = config.MoveSpeed;

            // 重置修正值
            AttackMultiplier = 1f;
            DefenseMultiplier = 1f;
            AttackSpeedMultiplier = 1f;
        }

        /// <summary>
        /// 获取最终攻击力
        /// Get final attack value
        /// </summary>
        public float GetFinalAttack()
        {
            return Attack * AttackMultiplier;
        }

        /// <summary>
        /// 获取最终防御力
        /// Get final defense value
        /// </summary>
        public float GetFinalDefense()
        {
            return Defense * DefenseMultiplier;
        }

        /// <summary>
        /// 计算受到的伤害
        /// Calculate damage received
        /// </summary>
        public float CalculateDamageReceived(float incomingDamage)
        {
            float finalDefense = GetFinalDefense();
            float damageReduction = finalDefense / (finalDefense + 100f);
            return Mathf.Max(1f, incomingDamage * (1f - damageReduction));
        }

        /// <summary>
        /// 获取生命值百分比
        /// Get HP percentage
        /// </summary>
        public float GetHPPercentage()
        {
            return MaxHP > 0 ? CurrentHP / MaxHP : 0f;
        }
    }
}
