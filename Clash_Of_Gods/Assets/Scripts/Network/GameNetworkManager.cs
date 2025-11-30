using UnityEngine;
using Mirror;
using ClashOfGods.Core;
using ClashOfGods.Gameplay.Units;
using ClashOfGods.Gameplay.Summoner;
using System.Collections.Generic;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 游戏网络管理器 - 扩展 NetworkModeManager，提供游戏特定的网络功能
    /// Game network manager - Extends NetworkModeManager with game-specific network features
    /// 
    /// 使用方式：
    /// 1. 单机模式（默认）：所有 NetworkSync 组件禁用，无网络开销
    /// 2. 联机模式：调用 StartPVPHost() 或 JoinPVPRoom() 一键切换
    /// </summary>
    public class GameNetworkManager : NetworkModeManager
    {
        [Header("Game Network Settings")]
        public GameObject PlayerPrefab;
        public Transform[] SpawnPoints;

        [Header("Runtime")]
        private List<NetworkIdentity> spawnedNetworkObjects = new List<NetworkIdentity>();

        /// <summary>
        /// 玩家连接时调用
        /// </summary>
        public override void OnServerAddPlayer(NetworkConnectionToClient conn)
        {
            // 选择出生点
            Transform spawnPoint = GetSpawnPoint(conn.connectionId);
            
            // 生成玩家对象
            GameObject player = Instantiate(PlayerPrefab, spawnPoint.position, spawnPoint.rotation);
            NetworkServer.AddPlayerForConnection(conn, player);
            
            // 确保网络同步组件启用
            EnableNetworkSyncOnObject(player);
            
            Debug.Log($"[GameNetwork] Player {conn.connectionId} spawned at {spawnPoint.position}");
        }

        /// <summary>
        /// 玩家断开连接时调用
        /// </summary>
        public override void OnServerDisconnect(NetworkConnectionToClient conn)
        {
            Debug.Log($"[GameNetwork] Player {conn.connectionId} disconnected");
            
            // 触发玩家断线事件（可用于判定胜负）
            EventManager.Instance?.TriggerEvent(new PlayerDisconnectedEvent 
            { 
                ConnectionId = conn.connectionId 
            });
            
            base.OnServerDisconnect(conn);
        }

        /// <summary>
        /// 获取出生点
        /// </summary>
        private Transform GetSpawnPoint(int connectionId)
        {
            if (SpawnPoints == null || SpawnPoints.Length == 0)
                return transform;
            
            return SpawnPoints[connectionId % SpawnPoints.Length];
        }

        /// <summary>
        /// 为场景中所有活动对象启用网络同步
        /// Enable network sync for all active objects in scene
        /// </summary>
        public void EnableNetworkSyncForAllObjects()
        {
            // 查找所有 UnitEntity
            UnitEntity[] units = FindObjectsOfType<UnitEntity>();
            foreach (var unit in units)
            {
                EnableNetworkSyncOnObject(unit.gameObject);
            }

            // 查找所有 SummonerController
            SummonerController[] summoners = FindObjectsOfType<SummonerController>();
            foreach (var summoner in summoners)
            {
                EnableNetworkSyncOnObject(summoner.gameObject);
            }

            Debug.Log($"[GameNetwork] Enabled network sync for {units.Length} units and {summoners.Length} summoners");
        }

        /// <summary>
        /// 为单个对象启用网络同步
        /// </summary>
        public void EnableNetworkSyncOnObject(GameObject obj)
        {
            // 添加 NetworkIdentity（如果没有）
            NetworkIdentity netId = obj.GetComponent<NetworkIdentity>();
            if (netId == null)
            {
                netId = obj.AddComponent<NetworkIdentity>();
            }

            // 启用所有 NetworkSyncBase 子类组件
            NetworkSyncBase[] syncComponents = obj.GetComponents<NetworkSyncBase>();
            foreach (var sync in syncComponents)
            {
                sync.enabled = true;
            }

            // 如果是服务器且对象未注册，则注册到网络
            if (NetworkServer.active && !netId.isServer)
            {
                NetworkServer.Spawn(obj);
                spawnedNetworkObjects.Add(netId);
            }
        }

        /// <summary>
        /// 禁用所有网络同步
        /// </summary>
        public void DisableNetworkSyncForAllObjects()
        {
            NetworkSyncBase[] allSyncs = FindObjectsOfType<NetworkSyncBase>(true);
            foreach (var sync in allSyncs)
            {
                sync.enabled = false;
            }

            spawnedNetworkObjects.Clear();
            Debug.Log($"[GameNetwork] Disabled network sync for {allSyncs.Length} components");
        }

        /// <summary>
        /// 动态生成网络对象
        /// </summary>
        [Server]
        public GameObject SpawnNetworkObject(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            GameObject obj = Instantiate(prefab, position, rotation);
            EnableNetworkSyncOnObject(obj);
            return obj;
        }

        /// <summary>
        /// 销毁网络对象
        /// </summary>
        [Server]
        public void DespawnNetworkObject(GameObject obj)
        {
            NetworkIdentity netId = obj.GetComponent<NetworkIdentity>();
            if (netId != null)
            {
                spawnedNetworkObjects.Remove(netId);
                NetworkServer.Destroy(obj);
            }
            else
            {
                Destroy(obj);
            }
        }

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] GameNetworkManager reloaded. Online: {IsOnlineMode}, Spawned: {spawnedNetworkObjects.Count}");
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] GameNetworkManager static reload");
        }

        #endregion
    }

    /// <summary>
    /// 玩家断线事件
    /// </summary>
    public class PlayerDisconnectedEvent
    {
        public int ConnectionId;
    }
}
