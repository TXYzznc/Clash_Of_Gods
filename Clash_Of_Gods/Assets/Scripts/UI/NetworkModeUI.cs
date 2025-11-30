using UnityEngine;
using UnityEngine.UI;
using ClashOfGods.Network;
using ClashOfGods.Core;

namespace ClashOfGods.UI
{
    /// <summary>
    /// 网络模式切换 UI - 提供单机/联机模式切换界面
    /// Network mode UI - Provides offline/online mode switching interface
    /// </summary>
    public class NetworkModeUI : MonoBehaviour
    {
        [Header("UI References")]
        public Button HostButton;
        public Button JoinButton;
        public Button DisconnectButton;
        public InputField AddressInput;
        public Text StatusText;
        public GameObject OnlineModePanel;
        public GameObject OfflineModePanel;

        [Header("Settings")]
        public string DefaultAddress = "localhost";
        public float StatusUpdateInterval = 0.5f;

        private float lastStatusUpdate;

        private void Start()
        {
            InitializeUI();
            RegisterEventListeners();
            UpdateUI();
        }

        private void OnDestroy()
        {
            UnregisterEventListeners();
        }

        private void Update()
        {
            // 定期更新状态显示
            if (Time.time - lastStatusUpdate > StatusUpdateInterval)
            {
                UpdateStatusText();
                lastStatusUpdate = Time.time;
            }
        }

        private void InitializeUI()
        {
            // 设置默认地址
            if (AddressInput != null)
                AddressInput.text = DefaultAddress;

            // 绑定按钮事件
            if (HostButton != null)
                HostButton.onClick.AddListener(OnHostClicked);

            if (JoinButton != null)
                JoinButton.onClick.AddListener(OnJoinClicked);

            if (DisconnectButton != null)
                DisconnectButton.onClick.AddListener(OnDisconnectClicked);
        }

        private void RegisterEventListeners()
        {
            EventManager.Instance?.AddListener<NetworkModeChangedEvent>(OnNetworkModeChanged);
        }

        private void UnregisterEventListeners()
        {
            EventManager.Instance?.RemoveListener<NetworkModeChangedEvent>(OnNetworkModeChanged);
        }

        /// <summary>
        /// 点击 Host 按钮
        /// </summary>
        private void OnHostClicked()
        {
            if (NetworkModeManager.Instance != null)
            {
                NetworkModeManager.Instance.StartPVPHost();
                Debug.Log("[NetworkModeUI] Starting as Host");
            }
        }

        /// <summary>
        /// 点击 Join 按钮
        /// </summary>
        private void OnJoinClicked()
        {
            string address = AddressInput?.text ?? DefaultAddress;
            
            if (NetworkModeManager.Instance != null)
            {
                NetworkModeManager.Instance.JoinPVPRoom(address);
                Debug.Log($"[NetworkModeUI] Joining room at {address}");
            }
        }

        /// <summary>
        /// 点击 Disconnect 按钮
        /// </summary>
        private void OnDisconnectClicked()
        {
            if (NetworkModeManager.Instance != null)
            {
                NetworkModeManager.Instance.StopPVP();
                Debug.Log("[NetworkModeUI] Disconnecting");
            }
        }

        /// <summary>
        /// 网络模式变更回调
        /// </summary>
        private void OnNetworkModeChanged(NetworkModeChangedEvent e)
        {
            UpdateUI();
            Debug.Log($"[NetworkModeUI] Mode changed - Online: {e.IsOnline}, Host: {e.IsHost}");
        }

        /// <summary>
        /// 更新 UI 状态
        /// </summary>
        private void UpdateUI()
        {
            bool isOnline = NetworkModeManager.Instance?.IsOnlineMode ?? false;

            // 切换面板显示
            if (OnlineModePanel != null)
                OnlineModePanel.SetActive(isOnline);

            if (OfflineModePanel != null)
                OfflineModePanel.SetActive(!isOnline);

            // 更新按钮状态
            if (HostButton != null)
                HostButton.interactable = !isOnline;

            if (JoinButton != null)
                JoinButton.interactable = !isOnline;

            if (DisconnectButton != null)
                DisconnectButton.interactable = isOnline;

            if (AddressInput != null)
                AddressInput.interactable = !isOnline;

            UpdateStatusText();
        }

        /// <summary>
        /// 更新状态文本
        /// </summary>
        private void UpdateStatusText()
        {
            if (StatusText == null) return;

            string status = "Offline (Single Player)";

            if (NetworkModeManager.Instance != null && NetworkModeManager.Instance.IsOnlineMode)
            {
                if (Mirror.NetworkServer.active && Mirror.NetworkClient.active)
                    status = "Online - Host";
                else if (Mirror.NetworkServer.active)
                    status = "Online - Server";
                else if (Mirror.NetworkClient.active)
                    status = "Online - Client";
                else
                    status = "Online - Connecting...";
            }

            StatusText.text = status;
        }

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log("[HotReload] NetworkModeUI reloaded");
            UpdateUI();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] NetworkModeUI static reload");
        }

        #endregion
    }
}
