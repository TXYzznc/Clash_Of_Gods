using UnityEngine;
using Mirror;
using ClashOfGods.Core;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 网络模式管理器 - 控制单机/联机模式切换
    /// Network mode manager - Controls offline/online mode switching
    /// </summary>
    public class NetworkModeManager : NetworkManager
    {
        private static NetworkModeManager _instance;
        
        public static NetworkModeManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<NetworkModeManager>();
                return _instance;
            }
        }

        [Header("Network Mode")]
        [SerializeField] private bool _isOnlineMode = false;
        public bool IsOnlineMode => _isOnlineMode;

        [Header("Settings")]
        public int RandomSeed;

        public override void Awake()
        {
            if (_instance == null)
            {
                _instance = (NetworkModeManager)(object)this;
                base.Awake();
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 启动 PVP 主机模式
        /// Start PVP as host
        /// </summary>
        public void StartPVPHost()
        {
            _isOnlineMode = true;
            StartHost();
            
            // 广播网络模式变更事件
            EventManager.Instance?.TriggerEvent(new NetworkModeChangedEvent { IsOnline = true, IsHost = true });
            
            Debug.Log("[Network] PVP Host started - Online mode enabled");
        }

        /// <summary>
        /// 加入 PVP 房间
        /// Join PVP room as client
        /// </summary>
        public void JoinPVPRoom(string address)
        {
            _isOnlineMode = true;
            networkAddress = address;
            StartClient();
            
            // 广播网络模式变更事件
            EventManager.Instance?.TriggerEvent(new NetworkModeChangedEvent { IsOnline = true, IsHost = false });
            
            Debug.Log($"[Network] Joining PVP room at {address}");
        }

        /// <summary>
        /// 停止 PVP 模式，返回单机
        /// Stop PVP mode, return to offline
        /// </summary>
        public void StopPVP()
        {
            if (NetworkServer.active && NetworkClient.isConnected)
                StopHost();
            else if (NetworkClient.isConnected)
                StopClient();
            else if (NetworkServer.active)
                StopServer();

            _isOnlineMode = false;
            
            // 广播网络模式变更事件
            EventManager.Instance?.TriggerEvent(new NetworkModeChangedEvent { IsOnline = false, IsHost = false });
            
            Debug.Log("[Network] PVP stopped - Offline mode");
        }

        /// <summary>
        /// 设置随机数种子（确定性战斗）
        /// Set RNG seed for deterministic combat
        /// 注意：NetworkManager 不支持 ClientRpc，需要通过其他方式同步
        /// </summary>
        public void SetRNGSeed(int seed)
        {
            RandomSeed = seed;
            Random.InitState(seed);
            Debug.Log($"[Network] RNG seed set: {seed}");
        }

        #region Mirror Callbacks

        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            Debug.Log($"[Network] Player connected: {conn.connectionId}");
        }

        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.Log($"[Network] Player disconnected: {conn.connectionId}");
            base.OnServerDisconnect(conn);
        }

        public override void OnClientConnect()
        {
            base.OnClientConnect();
            Debug.Log("[Network] Connected to server");
        }

        public override void OnClientDisconnect()
        {
            base.OnClientDisconnect();
            _isOnlineMode = false;
            EventManager.Instance?.TriggerEvent(new NetworkModeChangedEvent { IsOnline = false, IsHost = false });
            Debug.Log("[Network] Disconnected from server");
        }

        #endregion

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] NetworkModeManager reloaded. Online: {_isOnlineMode}");
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] NetworkModeManager static reload");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        #endregion
    }

    /// <summary>
    /// 网络模式变更事件
    /// Network mode changed event
    /// </summary>
    public class NetworkModeChangedEvent
    {
        public bool IsOnline;
        public bool IsHost;
    }
}
