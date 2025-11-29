using System.Collections.Generic;
using UnityEngine;
using ClashOfGods.Core;
using ClashOfGods.Gameplay.Units;

namespace ClashOfGods.Gameplay.Combat
{
    /// <summary>
    /// 战斗管理器 - 管理单场战斗的完整生命周期（支持热重载）
    /// Battle manager - Manages complete battle lifecycle (Hot reload compatible)
    /// </summary>
    public class BattleManager : MonoBehaviour
    {
        private static BattleManager _instance;
        
        public static BattleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<BattleManager>();
                }
                return _instance;
            }
        }

        [Header("Battle State")]
        public BattlePhase CurrentPhase = BattlePhase.Deployment;

        [Header("Units")]
        public List<UnitEntity> PlayerUnits = new List<UnitEntity>();
        public List<UnitEntity> EnemyUnits = new List<UnitEntity>();

        [Header("Battle Settings")]
        public Transform PlayerSpawnArea;
        public Transform EnemySpawnArea;

        [Header("Rewards")]
        public List<int> BattleRewards = new List<int>();

        [Header("Debug")]
        public bool EnableDebugLogs = true;

        private void Awake()
        {
            if (_instance == null)
            {
                // 热重载兼容：使用类型转换
                _instance = (BattleManager)(object)this;
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            RegisterEventListeners();
        }

        private void OnDestroy()
        {
            UnregisterEventListeners();
            
            if (ReferenceEquals(_instance, this))
            {
                _instance = null;
            }
        }

        /// <summary>
        /// 注册事件监听
        /// Register event listeners
        /// </summary>
        private void RegisterEventListeners()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.AddListener<UnitDeathEvent>(OnUnitDeath);
            }
        }

        /// <summary>
        /// 取消事件监听
        /// Unregister event listeners
        /// </summary>
        private void UnregisterEventListeners()
        {
            if (EventManager.Instance != null)
            {
                EventManager.Instance.RemoveListener<UnitDeathEvent>(OnUnitDeath);
            }
        }

        /// <summary>
        /// 开始战斗
        /// Start battle
        /// </summary>
        public void StartBattle(EnemySquadData enemies)
        {
            LogDebug("Starting battle...");
            
            CurrentPhase = BattlePhase.Deployment;
            
            // 生成敌人
            SpawnEnemies(enemies);
            
            // 触发战斗开始事件
            EventManager.Instance?.TriggerEvent(new CombatStartEvent
            {
                Enemies = new List<GameObject>()
            });
        }

        /// <summary>
        /// 生成敌人
        /// Spawn enemies
        /// </summary>
        private void SpawnEnemies(EnemySquadData enemies)
        {
            EnemyUnits.Clear();
            
            if (enemies?.EnemyIDs == null) return;
            
            foreach (int enemyID in enemies.EnemyIDs)
            {
                UnitConfig config = ConfigManager.Instance?.GetUnitConfig(enemyID);
                if (config != null && config.Prefab != null)
                {
                    Vector3 spawnPos = EnemySpawnArea != null 
                        ? EnemySpawnArea.position + Random.insideUnitSphere * 5f 
                        : Vector3.zero;
                    
                    GameObject enemyObj = Instantiate(config.Prefab, spawnPos, Quaternion.identity);
                    
                    UnitEntity enemy = enemyObj.GetComponent<UnitEntity>();
                    if (enemy != null)
                    {
                        enemy.Initialize(config);
                        enemy.IsPlayer = false;
                        EnemyUnits.Add(enemy);
                    }
                }
            }
            
            LogDebug($"Spawned {EnemyUnits.Count} enemies");
        }

        /// <summary>
        /// 部署玩家单位
        /// Deploy player unit
        /// </summary>
        public void DeployPlayerUnit(int unitID, Vector3 position)
        {
            if (CurrentPhase != BattlePhase.Deployment)
            {
                Debug.LogWarning("[Battle] Can only deploy units during deployment phase");
                return;
            }

            UnitConfig config = ConfigManager.Instance?.GetUnitConfig(unitID);
            if (config != null && config.Prefab != null)
            {
                GameObject unitObj = Instantiate(config.Prefab, position, Quaternion.identity);
                
                UnitEntity unit = unitObj.GetComponent<UnitEntity>();
                if (unit != null)
                {
                    unit.Initialize(config);
                    unit.IsPlayer = true;
                    PlayerUnits.Add(unit);
                    
                    LogDebug($"Deployed {config.UnitName}");
                }
            }
        }

        /// <summary>
        /// 开始战斗阶段
        /// Start battle phase
        /// </summary>
        public void BeginBattlePhase()
        {
            CurrentPhase = BattlePhase.Battle;
            LogDebug("Battle phase started!");
        }

        /// <summary>
        /// 单位死亡回调
        /// Unit death callback
        /// </summary>
        private void OnUnitDeath(UnitDeathEvent eventData)
        {
            if (eventData.IsPlayer)
            {
                PlayerUnits.RemoveAll(u => u == null || !u.IsAlive);
            }
            else
            {
                EnemyUnits.RemoveAll(u => u == null || !u.IsAlive);
            }

            CheckBattleEnd();
        }

        /// <summary>
        /// 检查战斗是否结束
        /// Check if battle has ended
        /// </summary>
        private void CheckBattleEnd()
        {
            if (PlayerUnits.Count == 0)
            {
                EndBattle(false);
            }
            else if (EnemyUnits.Count == 0)
            {
                EndBattle(true);
            }
        }

        /// <summary>
        /// 结束战斗
        /// End battle
        /// </summary>
        public void EndBattle(bool isWin)
        {
            CurrentPhase = BattlePhase.Settlement;
            
            LogDebug($"Battle ended. Victory: {isWin}");

            if (isWin)
            {
                GenerateRewards();
                SaveUnitStates();
            }
            else
            {
                ApplyDefeatPenalty();
            }

            // 触发战斗结束事件
            EventManager.Instance?.TriggerEvent(new CombatEndEvent
            {
                IsVictory = isWin,
                Rewards = new List<GameObject>()
            });
        }

        /// <summary>
        /// 生成奖励
        /// Generate rewards
        /// </summary>
        private void GenerateRewards()
        {
            LogDebug("Generating rewards...");
        }

        /// <summary>
        /// 保存单位状态
        /// Save unit states
        /// </summary>
        private void SaveUnitStates()
        {
            foreach (UnitEntity unit in PlayerUnits)
            {
                if (unit != null && unit.IsAlive)
                {
                    unit.InstanceData.CurrentHP = unit.Stats.CurrentHP;
                    LogDebug($"Saved {unit.Config.UnitName} HP: {unit.Stats.CurrentHP}");
                }
            }
        }

        /// <summary>
        /// 应用失败惩罚
        /// Apply defeat penalty
        /// </summary>
        private void ApplyDefeatPenalty()
        {
            LogDebug("Applying defeat penalty...");
        }

        /// <summary>
        /// 调试日志
        /// Debug log
        /// </summary>
        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[Battle] {message}");
            }
        }

        /// <summary>
        /// 获取战斗状态摘要（调试用）
        /// Get battle status summary (for debugging)
        /// </summary>
        public string GetBattleStatusSummary()
        {
            return $"Phase: {CurrentPhase}, Players: {PlayerUnits.Count}, Enemies: {EnemyUnits.Count}";
        }

        #region Hot Reload Support
        
        /// <summary>
        /// 热重载回调
        /// Hot reload callback
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] BattleManager reloaded. {GetBattleStatusSummary()}");
            
            // 重新注册事件监听
            UnregisterEventListeners();
            RegisterEventListeners();
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] BattleManager static reload");
        }

        /// <summary>
        /// 重置静态变量
        /// Reset static variables
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        #endregion
    }

    /// <summary>
    /// 战斗阶段
    /// Battle phase
    /// </summary>
    public enum BattlePhase
    {
        Deployment,     // 部署阶段
        Battle,         // 战斗阶段
        Settlement      // 结算阶段
    }

    /// <summary>
    /// 敌人小队数据
    /// Enemy squad data
    /// </summary>
    [System.Serializable]
    public class EnemySquadData
    {
        public int[] EnemyIDs;
        public int Difficulty;
    }
}
