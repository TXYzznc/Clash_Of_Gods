using UnityEngine;
using ClashOfGods.Core;

namespace ClashOfGods.Gameplay.Units
{
    /// <summary>
    /// 单位实体 - 挂载在所有战斗单位上（单机核心逻辑）
    /// Unit entity - Attached to all combat units (Offline core logic)
    /// 网络同步通过 UnitNetworkSync 组件实现，默认单机模式
    /// </summary>
    public class UnitEntity : MonoBehaviour
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

        private void Awake()
        {
            InitializeComponents();
        }

        /// <summary>
        /// 初始化组件
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

            if (Stats == null)
                Stats = new UnitStats();

            Stats.Initialize(config);

            // 恢复生命值（生命继承机制）
            if (InstanceData.CurrentHP > 0)
                Stats.CurrentHP = InstanceData.CurrentHP;
            else
                Stats.CurrentHP = Stats.MaxHP;

            IsAlive = true;
            LogDebug($"{config.UnitName} initialized. HP: {Stats.CurrentHP}/{Stats.MaxHP}");
        }

        /// <summary>
        /// 进化单位
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
        /// </summary>
        public void TakeDamage(float damage, GameObject source = null)
        {
            if (!IsAlive) return;

            float finalDamage = Stats.CalculateDamageReceived(damage);
            Stats.CurrentHP -= finalDamage;

            LogDebug($"Took {finalDamage:F1} damage. HP: {Stats.CurrentHP:F1}/{Stats.MaxHP:F1}");

            // 触发受伤事件（网络同步组件会监听此事件）
            EventManager.Instance?.TriggerEvent(new UnitDamagedEvent
            {
                Unit = gameObject,
                Damage = finalDamage,
                Source = source
            });

            if (Stats.CurrentHP <= 0)
                OnDeath();

            AnimationController?.TriggerAnimation("Hit");
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void ApplyHealing(float amount)
        {
            if (!IsAlive) return;

            float previousHP = Stats.CurrentHP;
            Stats.CurrentHP = Mathf.Min(Stats.CurrentHP + amount, Stats.MaxHP);
            float actualHealing = Stats.CurrentHP - previousHP;

            LogDebug($"Healed {actualHealing:F1}. HP: {Stats.CurrentHP:F1}/{Stats.MaxHP:F1}");
        }

        /// <summary>
        /// 死亡处理
        /// </summary>
        public void OnDeath()
        {
            if (!IsAlive) return;

            IsAlive = false;
            Stats.CurrentHP = 0;
            InstanceData.CurrentHP = 0;

            LogDebug("Died!");

            AnimationController?.TriggerAnimation("Death");

            // 触发死亡事件（网络同步组件会监听此事件）
            EventManager.Instance?.TriggerEvent(new UnitDeathEvent
            {
                Unit = gameObject,
                IsPlayer = IsPlayer
            });

            DropEquipment();
            Destroy(gameObject, 2f);
        }

        /// <summary>
        /// 掉落装备
        /// </summary>
        private void DropEquipment()
        {
            if (InstanceData?.EquippedItemIDs != null)
            {
                foreach (int itemID in InstanceData.EquippedItemIDs)
                {
                    if (itemID > 0)
                        LogDebug($"Dropped item: {itemID}");
                }
            }
        }

        /// <summary>
        /// 攻击目标
        /// </summary>
        public void Attack(GameObject target)
        {
            if (!IsAlive || target == null) return;

            UnitEntity targetEntity = target.GetComponent<UnitEntity>();
            if (targetEntity != null)
            {
                float damage = Stats.GetFinalAttack();
                targetEntity.TakeDamage(damage, gameObject);
                AnimationController?.TriggerAnimation("Attack");
            }
        }

        /// <summary>
        /// 获取单位状态摘要
        /// </summary>
        public string GetStatusSummary()
        {
            return $"{Config?.UnitName ?? "Unknown"}: HP={Stats?.CurrentHP:F0}/{Stats?.MaxHP:F0}, Alive={IsAlive}";
        }

        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[Unit:{Config?.UnitName ?? "Unknown"}] {message}");
        }

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] UnitEntity reloaded. {GetStatusSummary()}");
            InitializeComponents();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] UnitEntity static reload");
        }

        #endregion
    }

    /// <summary>
    /// 单位属性计算
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

        public void Initialize(UnitConfig config)
        {
            if (config == null) return;

            MaxHP = config.BaseHP;
            CurrentHP = MaxHP;
            Attack = config.BaseAttack;
            Defense = config.BaseDefense;
            AttackSpeed = config.AttackSpeed;
            MoveSpeed = config.MoveSpeed;

            AttackMultiplier = 1f;
            DefenseMultiplier = 1f;
            AttackSpeedMultiplier = 1f;
        }

        public float GetFinalAttack() => Attack * AttackMultiplier;
        public float GetFinalDefense() => Defense * DefenseMultiplier;

        public float CalculateDamageReceived(float incomingDamage)
        {
            float finalDefense = GetFinalDefense();
            float damageReduction = finalDefense / (finalDefense + 100f);
            return Mathf.Max(1f, incomingDamage * (1f - damageReduction));
        }

        public float GetHPPercentage() => MaxHP > 0 ? CurrentHP / MaxHP : 0f;
    }
}
