using UnityEngine;
using ClashOfGods.Network;
using ClashOfGods.Core;
using ClashOfGods.Gameplay.Units;
using ClashOfGods.Gameplay.Summoner;

namespace ClashOfGods.Utils
{
    /// <summary>
    /// 网络模式测试助手 - 提供快捷键和调试功能
    /// Network mode test helper - Provides hotkeys and debug features
    /// 
    /// 快捷键：
    /// F1 - 切换网络模式（Host）
    /// F2 - 生成测试单位
    /// F3 - 显示/隐藏调试信息
    /// </summary>
    public class NetworkModeTestHelper : MonoBehaviour
    {
        [Header("Test Settings")]
        public GameObject TestUnitPrefab;
        public Transform SpawnPoint;

        [Header("Hotkeys")]
        public KeyCode ToggleNetworkKey = KeyCode.F1;
        public KeyCode SpawnUnitKey = KeyCode.F2;
        public KeyCode ToggleDebugKey = KeyCode.F3;

        [Header("Debug")]
        public bool ShowDebugGUI = true;
        public bool EnableDebugLogs = true;

        private void Start()
        {
            RegisterEventListeners();
            LogDebug("NetworkModeTestHelper initialized");
        }

        private void OnDestroy()
        {
            UnregisterEventListeners();
        }

        private void Update()
        {
            HandleHotkeys();
        }

        private void RegisterEventListeners()
        {
            EventManager.Instance?.AddListener<NetworkModeChangedEvent>(OnNetworkModeChanged);
        }

        private void UnregisterEventListeners()
        {
            EventManager.Instance?.RemoveListener<NetworkModeChangedEvent>(OnNetworkModeChanged);
        }

        private void HandleHotkeys()
        {
            if (Input.GetKeyDown(ToggleNetworkKey))
            {
                ToggleNetworkMode();
            }

            if (Input.GetKeyDown(SpawnUnitKey))
            {
                SpawnTestUnit();
            }

            if (Input.GetKeyDown(ToggleDebugKey))
            {
                ShowDebugGUI = !ShowDebugGUI;
            }
        }

        /// <summary>
        /// 切换网络模式
        /// </summary>
        [ContextMenu("Toggle Network Mode")]
        public void ToggleNetworkMode()
        {
            if (NetworkModeManager.Instance == null)
            {
                Debug.LogError("[TestHelper] NetworkModeManager not found!");
                return;
            }

            if (NetworkModeManager.Instance.IsOnlineMode)
            {
                NetworkModeManager.Instance.StopPVP();
                LogDebug("Switched to Offline Mode");
            }
            else
            {
                NetworkModeManager.Instance.StartPVPHost();
                LogDebug("Switched to Online Mode (Host)");
            }
        }

        /// <summary>
        /// 生成测试单位
        /// </summary>
        [ContextMenu("Spawn Test Unit")]
        public void SpawnTestUnit()
        {
            if (TestUnitPrefab == null)
            {
                Debug.LogWarning("[TestHelper] No test unit prefab assigned");
                return;
            }

            Vector3 spawnPos = SpawnPoint != null 
                ? SpawnPoint.position 
                : transform.position + Random.insideUnitSphere * 3f;
            spawnPos.y = transform.position.y;

            GameObject unit = Instantiate(TestUnitPrefab, spawnPos, Quaternion.identity);

            // 如果是联机模式，启用网络同步
            if (NetworkModeManager.Instance?.IsOnlineMode == true)
            {
                GameNetworkManager gameNetMgr = NetworkModeManager.Instance as GameNetworkManager;
                gameNetMgr?.EnableNetworkSyncOnObject(unit);
            }

            LogDebug($"Spawned test unit at {spawnPos}");
        }

        /// <summary>
        /// 网络模式变更回调
        /// </summary>
        private void OnNetworkModeChanged(NetworkModeChangedEvent e)
        {
            LogDebug($"Network mode changed - Online: {e.IsOnline}, Host: {e.IsHost}");
        }

        /// <summary>
        /// 获取状态信息
        /// </summary>
        public string GetStatusInfo()
        {
            string mode = "Offline";
            if (NetworkModeManager.Instance?.IsOnlineMode == true)
            {
                if (Mirror.NetworkServer.active && Mirror.NetworkClient.active)
                    mode = "Host";
                else if (Mirror.NetworkServer.active)
                    mode = "Server";
                else if (Mirror.NetworkClient.active)
                    mode = "Client";
            }

            int unitCount = FindObjectsOfType<UnitEntity>().Length;
            int summonerCount = FindObjectsOfType<SummonerController>().Length;
            int networkSyncCount = FindObjectsOfType<NetworkSyncBase>(true).Length;
            int enabledSyncCount = 0;
            foreach (var sync in FindObjectsOfType<NetworkSyncBase>(true))
            {
                if (sync.enabled) enabledSyncCount++;
            }

            return $"Mode: {mode}\n" +
                   $"Units: {unitCount}\n" +
                   $"Summoners: {summonerCount}\n" +
                   $"NetworkSync: {enabledSyncCount}/{networkSyncCount}";
        }

        private void OnGUI()
        {
            if (!ShowDebugGUI) return;

            // 状态信息框
            GUI.Box(new Rect(10, 10, 200, 120), "Network Status");
            GUI.Label(new Rect(20, 35, 180, 80), GetStatusInfo());

            // 快捷键提示
            GUI.Box(new Rect(10, 140, 200, 80), "Hotkeys");
            GUI.Label(new Rect(20, 165, 180, 50),
                $"{ToggleNetworkKey} - Toggle Network\n" +
                $"{SpawnUnitKey} - Spawn Unit\n" +
                $"{ToggleDebugKey} - Toggle Debug");
        }

        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[TestHelper] {message}");
        }

        #region Hot Reload Support

        void OnScriptHotReload()
        {
            Debug.Log("[HotReload] NetworkModeTestHelper reloaded");
            RegisterEventListeners();
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] NetworkModeTestHelper static reload");
        }

        #endregion
    }
}
