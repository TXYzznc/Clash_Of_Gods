using UnityEngine;
using Mirror;
using ClashOfGods.Gameplay.Combat;
using ClashOfGods.Core;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 战斗网络同步组件 - 负责同步 BattleManager 的状态
    /// 默认禁用，联机模式时自动启用
    /// </summary>
    [RequireComponent(typeof(BattleManager))]
    public class BattleNetworkSync : NetworkSyncBase
    {
        private BattleManager battleManager;

        [Header("Synced Values")]
        [SyncVar(hook = nameof(OnPhaseChanged))]
        private BattlePhase syncedPhase;

        [SyncVar]
        private int syncedPlayerUnitCount;

        [SyncVar]
        private int syncedEnemyUnitCount;

        protected override void Awake()
        {
            base.Awake();
            battleManager = GetComponent<BattleManager>();
        }

        protected override void OnNetworkSyncEnabled()
        {
            if (isServer && battleManager != null)
            {
                syncedPhase = battleManager.CurrentPhase;
                syncedPlayerUnitCount = battleManager.PlayerUnits.Count;
                syncedEnemyUnitCount = battleManager.EnemyUnits.Count;
            }

            // 监听战斗事件
            EventManager.Instance?.AddListener<CombatStartEvent>(OnCombatStart);
            EventManager.Instance?.AddListener<CombatEndEvent>(OnCombatEnd);
        }

        protected override void OnNetworkSyncDisabled()
        {
            EventManager.Instance?.RemoveListener<CombatStartEvent>(OnCombatStart);
            EventManager.Instance?.RemoveListener<CombatEndEvent>(OnCombatEnd);
        }

        #region Server -> Client Sync

        /// <summary>
        /// 同步战斗阶段
        /// </summary>
        [Server]
        public void ServerSyncPhase(BattlePhase phase)
        {
            syncedPhase = phase;
        }

        /// <summary>
        /// 同步单位数量
        /// </summary>
        [Server]
        public void ServerSyncUnitCounts(int playerCount, int enemyCount)
        {
            syncedPlayerUnitCount = playerCount;
            syncedEnemyUnitCount = enemyCount;
        }

        /// <summary>
        /// 战斗阶段变化回调
        /// </summary>
        private void OnPhaseChanged(BattlePhase oldPhase, BattlePhase newPhase)
        {
            if (!isServer && battleManager != null)
            {
                battleManager.CurrentPhase = newPhase;
                Debug.Log($"[BattleNetworkSync] Phase synced: {newPhase}");
            }
        }

        #endregion

        #region Client -> Server Commands

        /// <summary>
        /// 客户端请求部署单位
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdRequestDeployUnit(int unitID, Vector3 position, NetworkConnectionToClient sender = null)
        {
            if (battleManager.CurrentPhase == BattlePhase.Deployment)
            {
                battleManager.DeployPlayerUnit(unitID, position);
                RpcOnUnitDeployed(unitID, position);
            }
        }

        /// <summary>
        /// 客户端请求开始战斗
        /// </summary>
        [Command(requiresAuthority = false)]
        public void CmdRequestStartBattle(NetworkConnectionToClient sender = null)
        {
            if (battleManager.CurrentPhase == BattlePhase.Deployment)
            {
                battleManager.BeginBattlePhase();
                ServerSyncPhase(BattlePhase.Battle);
                RpcOnBattleStarted();
            }
        }

        #endregion

        #region Server -> All Clients RPCs

        /// <summary>
        /// 广播单位部署
        /// </summary>
        [ClientRpc]
        private void RpcOnUnitDeployed(int unitID, Vector3 position)
        {
            Debug.Log($"[BattleNetworkSync] Unit {unitID} deployed at {position}");
        }

        /// <summary>
        /// 广播战斗开始
        /// </summary>
        [ClientRpc]
        private void RpcOnBattleStarted()
        {
            Debug.Log("[BattleNetworkSync] Battle started!");
        }

        /// <summary>
        /// 广播战斗结束
        /// </summary>
        [ClientRpc]
        public void RpcOnBattleEnded(bool isVictory)
        {
            Debug.Log($"[BattleNetworkSync] Battle ended. Victory: {isVictory}");
        }

        #endregion

        #region Event Handlers

        private void OnCombatStart(CombatStartEvent e)
        {
            if (isServer)
            {
                ServerSyncPhase(BattlePhase.Battle);
            }
        }

        private void OnCombatEnd(CombatEndEvent e)
        {
            if (isServer)
            {
                ServerSyncPhase(BattlePhase.Settlement);
                RpcOnBattleEnded(e.IsVictory);
            }
        }

        #endregion

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] BattleNetworkSync reloaded. Phase: {syncedPhase}");
            battleManager = GetComponent<BattleManager>();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] BattleNetworkSync static reload");
        }

        #endregion
    }
}
