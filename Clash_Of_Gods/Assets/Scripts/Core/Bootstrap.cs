using UnityEngine;
using ClashOfGods.Core;

namespace ClashOfGods
{
    /// <summary>
    /// 引导脚本 - 初始化所有核心系统（支持热重载）
    /// Bootstrap script - Initializes all core systems (Hot reload compatible)
    /// </summary>
    public class Bootstrap : MonoBehaviour
    {
        [Header("Manager Prefabs")]
        public GameObject GameLifecycleManagerPrefab;
        public GameObject EventManagerPrefab;
        public GameObject SaveSystemPrefab;
        public GameObject ConfigManagerPrefab;

        [Header("Initialization Order")]
        [SerializeField] private bool autoInitialize = true;

        private void Awake()
        {
            if (autoInitialize)
            {
                InitializeAllSystems();
            }
        }

        /// <summary>
        /// 初始化所有系统
        /// Initialize all systems
        /// </summary>
        public void InitializeAllSystems()
        {
            Debug.Log("=== [Bootstrap] Starting System Initialization ===");

            // 1. 初始化事件管理器（最先初始化，其他系统可能需要它）
            InitializeEventManager();

            // 2. 初始化配置管理器（加载所有配置数据）
            InitializeConfigManager();

            // 3. 初始化存档系统
            InitializeSaveSystem();

            // 4. 初始化游戏生命周期管理器（包含网络管理）
            InitializeGameLifecycleManager();

            Debug.Log("=== [Bootstrap] All Systems Initialized Successfully ===");
        }

        /// <summary>
        /// 初始化事件管理器
        /// Initialize event manager
        /// </summary>
        private void InitializeEventManager()
        {
            if (EventManager.Instance == null)
            {
                if (EventManagerPrefab != null)
                {
                    Instantiate(EventManagerPrefab);
                }
                else
                {
                    GameObject eventManagerObj = new GameObject("EventManager");
                    eventManagerObj.AddComponent<EventManager>();
                }
                Debug.Log("[Bootstrap] ✓ EventManager initialized");
            }
        }

        /// <summary>
        /// 初始化配置管理器
        /// Initialize config manager
        /// </summary>
        private void InitializeConfigManager()
        {
            if (ConfigManager.Instance == null)
            {
                if (ConfigManagerPrefab != null)
                {
                    Instantiate(ConfigManagerPrefab);
                }
                else
                {
                    GameObject configManagerObj = new GameObject("ConfigManager");
                    configManagerObj.AddComponent<ConfigManager>();
                }
                Debug.Log("[Bootstrap] ✓ ConfigManager initialized");
            }
        }

        /// <summary>
        /// 初始化存档系统
        /// Initialize save system
        /// </summary>
        private void InitializeSaveSystem()
        {
            if (SaveSystem.Instance == null)
            {
                if (SaveSystemPrefab != null)
                {
                    Instantiate(SaveSystemPrefab);
                }
                else
                {
                    GameObject saveSystemObj = new GameObject("SaveSystem");
                    saveSystemObj.AddComponent<SaveSystem>();
                }
                Debug.Log("[Bootstrap] ✓ SaveSystem initialized");
            }
        }

        /// <summary>
        /// 初始化游戏生命周期管理器
        /// Initialize game lifecycle manager
        /// </summary>
        private void InitializeGameLifecycleManager()
        {
            if (GameLifecycleManager.Instance == null)
            {
                if (GameLifecycleManagerPrefab != null)
                {
                    GameObject manager = Instantiate(GameLifecycleManagerPrefab);
                    manager.GetComponent<GameLifecycleManager>()?.InitializeGame();
                }
                else
                {
                    Debug.LogWarning("[Bootstrap] GameLifecycleManager prefab not assigned!");
                }
                Debug.Log("[Bootstrap] ✓ GameLifecycleManager initialized");
            }
        }

        /// <summary>
        /// 验证系统状态
        /// Verify system status
        /// </summary>
        [ContextMenu("Verify All Systems")]
        public void VerifyAllSystems()
        {
            Debug.Log("=== [Bootstrap] System Verification ===");
            
            Debug.Log($"EventManager: {(EventManager.Instance != null ? "✓" : "✗")}");
            Debug.Log($"ConfigManager: {(ConfigManager.Instance != null ? "✓" : "✗")}");
            Debug.Log($"SaveSystem: {(SaveSystem.Instance != null ? "✓" : "✗")}");
            Debug.Log($"GameLifecycleManager: {(GameLifecycleManager.Instance != null ? "✓" : "✗")}");
            
            Debug.Log("=== [Bootstrap] Verification Complete ===");
        }

        #region Hot Reload Support
        
        /// <summary>
        /// 热重载回调
        /// Hot reload callback
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log("[HotReload] Bootstrap reloaded");
            VerifyAllSystems();
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] Bootstrap static reload");
        }

        #endregion
    }
}
