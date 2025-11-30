using UnityEngine;
using Mirror;
using ClashOfGods.Gameplay.Summoner;
using ClashOfGods.Core;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 召唤师网络同步组件 - 负责同步 SummonerController 的状态
    /// 默认禁用，联机模式时自动启用
    /// </summary>
    [RequireComponent(typeof(SummonerController))]
    public class SummonerNetworkSync : NetworkSyncBase
    {
        private SummonerController summoner;

        [Header("Synced Values")]
        [SyncVar(hook = nameof(OnHPChanged))]
        private float syncedHP;

        [SyncVar(hook = nameof(OnSpiritChanged))]
        private float syncedSpirit;

        [SyncVar]
        private Vector3 syncedPosition;

        protected override void Awake()
        {
            base.Awake();
            summoner = GetComponent<SummonerController>();
        }

        protected override void OnNetworkSyncEnabled()
        {
            if (isServer && summoner?.RuntimeData != null)
            {
                syncedHP = summoner.RuntimeData.CurrentHP;
                syncedSpirit = summoner.RuntimeData.CurrentSpirit;
                syncedPosition = transform.position;
            }
        }

        private void Update()
        {
            if (!enabled) return;

            // 服务器定期同步位置
            if (isServer)
            {
                syncedPosition = transform.position;
            }
        }

        #region Server -> Client Sync

        [Server]
        public void ServerSyncHP(float newHP)
        {
            syncedHP = newHP;
        }

        [Server]
        public void ServerSyncSpirit(float newSpirit)
        {
            syncedSpirit = newSpirit;
        }

        private void OnHPChanged(float oldHP, float newHP)
        {
            if (!isServer && summoner?.RuntimeData != null)
            {
                summoner.RuntimeData.CurrentHP = newHP;
            }
        }

        private void OnSpiritChanged(float oldSpirit, float newSpirit)
        {
            if (!isServer && summoner?.RuntimeData != null)
            {
                summoner.RuntimeData.CurrentSpirit = newSpirit;
            }
        }

        #endregion

        #region Client -> Server Commands

        [Command]
        public void CmdRequestMove(Vector3 targetPosition)
        {
            summoner.Move(targetPosition);
            RpcOnMove(targetPosition);
        }

        [Command]
        public void CmdRequestCastSkill(int skillID, Vector3 target)
        {
            summoner.CastSummonerSkill(skillID, target);
            RpcOnSkillCast(skillID, target);
        }

        [Command]
        public void CmdRequestTeleport(Vector3 position)
        {
            summoner.Teleport(position);
            RpcOnTeleport(position);
        }

        #endregion

        #region Server -> All Clients RPCs

        [ClientRpc]
        private void RpcOnMove(Vector3 targetPosition)
        {
            if (!isServer)
            {
                summoner.Move(targetPosition);
            }
        }

        [ClientRpc]
        private void RpcOnSkillCast(int skillID, Vector3 target)
        {
            // 播放技能特效
            Debug.Log($"[NetworkSync] Skill {skillID} cast at {target}");
        }

        [ClientRpc]
        private void RpcOnTeleport(Vector3 position)
        {
            if (!isServer)
            {
                summoner.Teleport(position);
            }
        }

        #endregion

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] SummonerNetworkSync reloaded. Enabled: {enabled}");
            summoner = GetComponent<SummonerController>();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] SummonerNetworkSync static reload");
        }

        #endregion
    }
}
