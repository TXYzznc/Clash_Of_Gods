using UnityEngine;
using Mirror;
using ClashOfGods.Core;

namespace ClashOfGods.Network
{
    /// <summary>
    /// 网络同步基类 - 所有需要网络同步的对象继承此类
    /// 默认禁用，切换到联机模式时自动启用
    /// </summary>
    public abstract class NetworkSyncBase : NetworkBehaviour
    {
        [Header("Network Sync")]
        [SerializeField] protected bool autoEnableOnOnlineMode = true;

        protected virtual void Awake()
        {
            // 默认禁用网络同步组件
            enabled = false;
        }

        protected virtual void Start()
        {
            // 监听网络模式变更事件
            EventManager.Instance?.AddListener<NetworkModeChangedEvent>(OnNetworkModeChanged);
            
            // 检查当前是否已经是联机模式
            if (NetworkModeManager.Instance != null && NetworkModeManager.Instance.IsOnlineMode)
            {
                EnableNetworkSync();
            }
        }

        protected virtual void OnDestroy()
        {
            EventManager.Instance?.RemoveListener<NetworkModeChangedEvent>(OnNetworkModeChanged);
        }

        /// <summary>
        /// 网络模式变更回调
        /// </summary>
        private void OnNetworkModeChanged(NetworkModeChangedEvent e)
        {
            if (e.IsOnline && autoEnableOnOnlineMode)
            {
                EnableNetworkSync();
            }
            else
            {
                DisableNetworkSync();
            }
        }

        /// <summary>
        /// 启用网络同步
        /// </summary>
        protected virtual void EnableNetworkSync()
        {
            enabled = true;
            OnNetworkSyncEnabled();
            Debug.Log($"[NetworkSync] {GetType().Name} enabled on {gameObject.name}");
        }

        /// <summary>
        /// 禁用网络同步
        /// </summary>
        protected virtual void DisableNetworkSync()
        {
            enabled = false;
            OnNetworkSyncDisabled();
            Debug.Log($"[NetworkSync] {GetType().Name} disabled on {gameObject.name}");
        }

        /// <summary>
        /// 网络同步启用时调用（子类重写）
        /// </summary>
        protected virtual void OnNetworkSyncEnabled() { }

        /// <summary>
        /// 网络同步禁用时调用（子类重写）
        /// </summary>
        protected virtual void OnNetworkSyncDisabled() { }

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] {GetType().Name} reloaded. Enabled: {enabled}");
        }

        #endregion
    }
}
