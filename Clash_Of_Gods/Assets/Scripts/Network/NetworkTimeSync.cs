using UnityEngine;
using Mirror;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 网络时间同步 - 确保客户端和服务器时间一致
    /// Network time sync - Ensures client and server time consistency
    /// </summary>
    public class NetworkTimeSync : NetworkBehaviour
    {
        [SyncVar]
        public double ServerTime;

        [SyncVar]
        public int RandomSeed;

        private void Update()
        {
            if (isServer)
            {
                ServerTime = NetworkTime.time;
            }
        }

        /// <summary>
        /// 同步随机数种子（Lockstep机制）
        /// Sync RNG seed (Lockstep mechanism)
        /// </summary>
        [Server]
        public void SyncRNGSeed(int seed)
        {
            RandomSeed = seed;
            RpcSyncRNGSeed(seed);
        }

        [ClientRpc]
        private void RpcSyncRNGSeed(int seed)
        {
            Random.InitState(seed);
            Debug.Log($"[NetworkTimeSync] RNG seed synced: {seed}");
        }

        /// <summary>
        /// 获取同步后的时间
        /// Get synced time
        /// </summary>
        public double GetSyncedTime()
        {
            if (isServer)
            {
                return NetworkTime.time;
            }
            else
            {
                return ServerTime;
            }
        }
    }
}
