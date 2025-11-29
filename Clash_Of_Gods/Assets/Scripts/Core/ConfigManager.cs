using System.Collections.Generic;
using UnityEngine;

namespace ClashOfGods.Core
{
    /// <summary>
    /// 配置管理器 - 加载和缓存静态配置数据（支持热重载）
    /// Config manager - Loads and caches static configuration data (Hot reload compatible)
    /// </summary>
    public class ConfigManager : MonoBehaviour
    {
        private static ConfigManager _instance;
        
        public static ConfigManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<ConfigManager>();
                }
                return _instance;
            }
        }

        [Header("Config Resources")]
        public UnitConfig[] UnitConfigs;
        public CardConfig[] CardConfigs;
        public SkillConfig[] SkillConfigs;

        private Dictionary<int, UnitConfig> unitConfigDict = new Dictionary<int, UnitConfig>();
        private Dictionary<int, CardConfig> cardConfigDict = new Dictionary<int, CardConfig>();
        private Dictionary<int, SkillConfig> skillConfigDict = new Dictionary<int, SkillConfig>();

        private bool isInitialized = false;

        private void Awake()
        {
            if (_instance == null)
            {
                // 热重载兼容：使用类型转换
                _instance = (ConfigManager)(object)this;
                DontDestroyOnLoad(gameObject);
                InitializeConfigs();
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化配置字典
        /// Initialize config dictionaries
        /// </summary>
        private void InitializeConfigs()
        {
            if (isInitialized) return;

            unitConfigDict.Clear();
            cardConfigDict.Clear();
            skillConfigDict.Clear();

            // 加载单位配置
            if (UnitConfigs != null)
            {
                foreach (var config in UnitConfigs)
                {
                    if (config != null)
                    {
                        unitConfigDict[config.UnitID] = config;
                    }
                }
            }

            // 加载卡牌配置
            if (CardConfigs != null)
            {
                foreach (var config in CardConfigs)
                {
                    if (config != null)
                    {
                        cardConfigDict[config.CardID] = config;
                    }
                }
            }

            // 加载技能配置
            if (SkillConfigs != null)
            {
                foreach (var config in SkillConfigs)
                {
                    if (config != null)
                    {
                        skillConfigDict[config.SkillID] = config;
                    }
                }
            }

            isInitialized = true;
            Debug.Log($"[ConfigManager] Loaded {unitConfigDict.Count} units, {cardConfigDict.Count} cards, {skillConfigDict.Count} skills");
        }

        /// <summary>
        /// 重新加载配置（热重载时可用）
        /// Reload configs (useful for hot reload)
        /// </summary>
        public void ReloadConfigs()
        {
            isInitialized = false;
            InitializeConfigs();
        }

        /// <summary>
        /// 获取单位配置
        /// Get unit config
        /// </summary>
        public UnitConfig GetUnitConfig(int unitID)
        {
            if (unitConfigDict.TryGetValue(unitID, out UnitConfig config))
            {
                return config;
            }
            
            Debug.LogWarning($"[ConfigManager] Unit config not found: {unitID}");
            return null;
        }

        /// <summary>
        /// 获取卡牌配置
        /// Get card config
        /// </summary>
        public CardConfig GetCardConfig(int cardID)
        {
            if (cardConfigDict.TryGetValue(cardID, out CardConfig config))
            {
                return config;
            }
            
            Debug.LogWarning($"[ConfigManager] Card config not found: {cardID}");
            return null;
        }

        /// <summary>
        /// 获取技能配置
        /// Get skill config
        /// </summary>
        public SkillConfig GetSkillConfig(int skillID)
        {
            if (skillConfigDict.TryGetValue(skillID, out SkillConfig config))
            {
                return config;
            }
            
            Debug.LogWarning($"[ConfigManager] Skill config not found: {skillID}");
            return null;
        }

        /// <summary>
        /// 获取所有单位配置
        /// Get all unit configs
        /// </summary>
        public IEnumerable<UnitConfig> GetAllUnitConfigs()
        {
            return unitConfigDict.Values;
        }

        /// <summary>
        /// 获取所有卡牌配置
        /// Get all card configs
        /// </summary>
        public IEnumerable<CardConfig> GetAllCardConfigs()
        {
            return cardConfigDict.Values;
        }

        #region Hot Reload Support
        
        /// <summary>
        /// 热重载回调
        /// Hot reload callback
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] ConfigManager reloaded. Units: {unitConfigDict.Count}, Cards: {cardConfigDict.Count}, Skills: {skillConfigDict.Count}");
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] ConfigManager static reload");
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
}
