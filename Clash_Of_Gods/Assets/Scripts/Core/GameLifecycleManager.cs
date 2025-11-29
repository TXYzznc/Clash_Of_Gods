using UnityEngine;
using Mirror;

namespace ClashOfGods.Core
{
    /// <summary>
    /// 游戏生命周期管理器 - 游戏入口，管理跨场景单例和游戏状态机（支持热重载）
    /// Game Entry Point - Manages cross-scene singletons and game state machine (Hot reload compatible)
    /// </summary>
    public class GameLifecycleManager : NetworkManager
    {
        private static GameLifecycleManager _instance;
        
        public static GameLifecycleManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<GameLifecycleManager>();
                }
                return _instance;
            }
        }

        public enum GameState
        {
            Boot,           // 启动
            Base,           // 基地
            Exploration,    // 探索
            Combat,         // 战斗
            Extraction      // 撤离结算
        }

        [Header("Game State")]
        public GameState CurrentState = GameState.Boot;
        public bool IsOnline = false;

        [Header("Debug")]
        public bool EnableDebugLogs = true;

        private void Awake()
        {
            if (_instance == null)
            {
                // 热重载兼容：使用类型转换
                _instance = (GameLifecycleManager)(object)this;
                DontDestroyOnLoad(gameObject);
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 初始化所有Manager
        /// Initialize all managers
        /// </summary>
        public void InitializeGame()
        {
            LogDebug("Game Initialized");
            ChangeState(GameState.Boot);
        }

        /// <summary>
        /// 场景切换处理（带Loading界面）
        /// Scene switching with loading screen
        /// </summary>
        public void SwitchScene(string sceneName)
        {
            LogDebug($"Switching to scene: {sceneName}");
            // TODO: 实现场景切换逻辑
        }

        /// <summary>
        /// 切换游戏状态
        /// Switch game state
        /// </summary>
        public void ChangeState(GameState newState)
        {
            GameState previousState = CurrentState;
            CurrentState = newState;
            
            LogDebug($"State changed: {previousState} -> {newState}");
            
            // 触发状态变更事件
            OnStateChanged(previousState, newState);
        }

        /// <summary>
        /// 状态变更处理
        /// Handle state change
        /// </summary>
        private void OnStateChanged(GameState from, GameState to)
        {
            switch (to)
            {
                case GameState.Boot:
                    // 初始化启动
                    break;
                case GameState.Base:
                    // 进入基地
                    break;
                case GameState.Exploration:
                    // 进入探索
                    break;
                case GameState.Combat:
                    // 进入战斗
                    break;
                case GameState.Extraction:
                    // 撤离结算
                    break;
            }
        }

        /// <summary>
        /// 调试日志
        /// Debug log
        /// </summary>
        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
            {
                Debug.Log($"[GameLifecycle] {message}");
            }
        }

        #region Mirror Network Callbacks
        
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            LogDebug($"Player connected: {conn.connectionId}");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            base.OnServerDisconnect(conn);
            LogDebug($"Player disconnected: {conn.connectionId}");
        }

        #endregion

        #region Hot Reload Support
        
        /// <summary>
        /// 热重载回调
        /// Hot reload callback
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] GameLifecycleManager reloaded. Current state: {CurrentState}");
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] GameLifecycleManager static reload");
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
