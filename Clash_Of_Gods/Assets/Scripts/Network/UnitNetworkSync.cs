using UnityEngine;
using Mirror;
using ClashOfGods.Gameplay.Units;
using ClashOfGods.Core;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 单位网络同步组件 - 负责同步 UnitEntity 的状态
    /// 默认禁用，联机模式时自动启用
    /// </summary>
    [RequireComponent(typeof(UnitEntity))]
    public class UnitNetworkSync : NetworkSyncBase
    {
        private UnitEntity unitEntity;

        [Header("Synced Values")]
        [SyncVar(hook = nameof(OnHPChanged))]
        private float syncedHP;

        [SyncVar(hook = nameof(OnAliveChanged))]
        private bool syncedIsAlive = true;

        [SyncVar]
        private bool syncedIsPlayer;

        protected override void Awake()
        {
            base.Awake();
            unitEntity = GetComponent<UnitEntity>();
        }

        protected override void OnNetworkSyncEnabled()
        {
            // 初始化同步值
            if (isServer && unitEntity != null)
            {
                syncedHP = unitEntity.Stats?.CurrentHP ?? 0;
                syncedIsAlive = unitEntity.IsAlive;
                syncedIsPlayer = unitEntity.IsPlayer;
            }

            // 监听单位事件
            EventManager.Instance?.AddListener<UnitDamagedEvent>(OnUnitDamaged);
            EventManager.Instance?.AddListener<UnitDeathEvent>(OnUnitDeath);
        }

        protected override void OnNetworkSyncDisabled()
        {
            EventManager.Instance?.RemoveListener<UnitDamagedEvent>(OnUnitDamaged);
            EventManager.Instance?.RemoveListener<UnitDeathEvent>(OnUnitDeath);
        }

        #region Server -> Client Sync

        /// <summary>
        /// 同步 HP（服务器调用）
        /// </summary>
        [Server]
        public void ServerSyncHP(float newHP)
        {
            syncedHP = newHP;
        }

        /// <summary>
        /// 同步死亡状态（服务器调用）
        /// </summary>
        [Server]
        public void ServerSyncDeath()
        {
            syncedIsAlive = false;
        }

        /// <summary>
        /// HP 变化回调（客户端）
        /// </summary>
        private void OnHPChanged(float oldHP, float newHP)
        {
            if (!isServer && unitEntity != null && unitEntity.Stats != null)
            {
                unitEntity.Stats.CurrentHP = newHP;
            }
        }

        /// <summary>
        /// 存活状态变化回调（客户端）
        /// </summary>
        private void OnAliveChanged(bool oldAlive, bool newAlive)
        {
            if (!isServer && !newAlive && unitEntity != null)
            {
                unitEntity.IsAlive = false;
                unitEntity.AnimationController?.TriggerAnimation("Death");
            }
        }

        #endregion

        #region Client -> Server Commands

        /// <summary>
        /// 客户端请求攻击
        /// </summary>
        [Command]
        public void CmdRequestAttack(NetworkIdentity targetIdentity)
        {
            if (targetIdentity != null)
            {
                UnitEntity target = targetIdentity.GetComponent<UnitEntity>();
                if (target != null)
                {
                    unitEntity.Attack(target.gameObject);
                }
            }
        }

        /// <summary>
        /// 客户端请求移动
        /// </summary>
        [Command]
        public void CmdRequestMove(Vector3 position)
        {
            // 服务器验证并执行移动
            if (unitEntity.AIController != null)
            {
                // 设置目标位置
            }
            RpcOnMove(position);
        }

        #endregion

        #region Server -> All Clients RPCs

        /// <summary>
        /// 广播受伤特效
        /// </summary>
        [ClientRpc]
        public void RpcPlayHitEffect(float damage)
        {
            unitEntity.AnimationController?.TriggerAnimation("Hit");
        }

        /// <summary>
        /// 广播移动
        /// </summary>
        [ClientRpc]
        private void RpcOnMove(Vector3 position)
        {
            // 客户端同步移动
        }

        /// <summary>
        /// 广播攻击动画
        /// </summary>
        [ClientRpc]
        public void RpcPlayAttackAnimation()
        {
            unitEntity.AnimationController?.TriggerAnimation("Attack");
        }

        #endregion

        #region Event Handlers

        private void OnUnitDamaged(UnitDamagedEvent e)
        {
            if (e.Unit == gameObject && isServer)
            {
                ServerSyncHP(unitEntity.Stats.CurrentHP);
                RpcPlayHitEffect(e.Damage);
            }
        }

        private void OnUnitDeath(UnitDeathEvent e)
        {
            if (e.Unit == gameObject && isServer)
            {
                ServerSyncDeath();
            }
        }

        #endregion

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] UnitNetworkSync reloaded. Enabled: {enabled}, HP: {syncedHP}");
            unitEntity = GetComponent<UnitEntity>();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] UnitNetworkSync static reload");
        }

        #endregion
    }
}
