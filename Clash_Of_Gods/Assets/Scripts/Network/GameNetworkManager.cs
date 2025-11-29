using UnityEngine;
using Mirror;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 游戏网络管理器 - 扩展Mirror的NetworkManager
    /// Game network manager - Extends Mirror's NetworkManager
    /// </summary>
    public class GameNetworkManager : NetworkManager
    {
        public static new GameNetworkManager singleton { get; private set; }

        [Header("Network Settings")]
        public int RandomSeed;

        public override void Awake()
        {
            base.Awake();
            singleton = this as GameNetworkManager;
        }

        /// <summary>
        /// 玩家连接时调用
        /// Called when player connects
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            base.OnServerAddPlayer(conn);
            
            Debug.Log($"[Network] Player connected: {conn.connectionId}");
            
            // TODO: 生成玩家召唤师对象
        }

        /// <summary>
        /// 玩家断开连接时调用
        /// Called when player disconnects
        /// </summary>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.Log($"[Network] Player disconnected: {conn.connectionId}");
            
            // TODO: 处理掉线（判定对方胜利）
            
            base.OnServerDisconnect(conn);
        }

        /// <summary>
        /// 启动Host（PVE模式）
        /// Start host (PVE mode)
        /// </summary>
        public void StartHostPVE()
        {
            Debug.Log("[Network] Starting Host (PVE)");
            NetworkServer.Listen(maxConnections);
        }

        /// <summary>
        /// 启动Client（PVP模式）
        /// Start client (PVP mode)
        /// </summary>
        public void StartClientPVP(string address)
        {
            Debug.Log($"[Network] Starting Client (PVP) - Connecting to {address}");
            networkAddress = address;
            StartClient();
        }

        /// <summary>
        /// 同步随机数种子
        /// Sync RNG seed
        /// </summary>
        [Server]
        public void SyncRNGSeed(int seed)
        {
            RandomSeed = seed;
        }
    }
}
