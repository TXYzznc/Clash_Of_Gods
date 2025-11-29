using System.Collections.Generic;
using UnityEngine;

namespace ClashOfGods.UI
{
    /// <summary>
    /// UI管理器 - 管理UI栈和弹窗层级（支持热重载）
    /// UI manager - Manages UI stack and panel hierarchy (Hot reload compatible)
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        private static UIManager _instance;
        
        public static UIManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<UIManager>();
                return _instance;
            }
        }

        [Header("UI Panels")]
        public GameObject MainMenuPanel;
        public GameObject CombatUIPanel;
        public GameObject ExplorationUIPanel;
        public GameObject InventoryPanel;
        public GameObject SettingsPanel;

        [Header("Floating Text")]
        public GameObject FloatingTextPrefab;
        public Transform FloatingTextContainer;

        [Header("Debug")]
        public bool EnableDebugLogs = true;

        private Stack<GameObject> uiStack = new Stack<GameObject>();

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = (UIManager)(object)this;
                DontDestroyOnLoad(gameObject);
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        public void OpenPanel(string panelName)
        {
            GameObject panel = GetPanelByName(panelName);
            
            if (panel != null)
            {
                if (uiStack.Count > 0)
                    uiStack.Peek().SetActive(false);

                panel.SetActive(true);
                uiStack.Push(panel);
                
                LogDebug($"Opened panel: {panelName}");
            }
        }

        public void CloseCurrentPanel()
        {
            if (uiStack.Count > 0)
            {
                GameObject currentPanel = uiStack.Pop();
                currentPanel.SetActive(false);

                if (uiStack.Count > 0)
                    uiStack.Peek().SetActive(true);

                LogDebug($"Closed panel: {currentPanel.name}");
            }
        }

        public void CloseAllPanels()
        {
            while (uiStack.Count > 0)
            {
                GameObject panel = uiStack.Pop();
                panel.SetActive(false);
            }
            LogDebug("Closed all panels");
        }

        public void ShowFloatingText(Vector3 worldPos, string text, Color color)
        {
            if (FloatingTextPrefab == null || Camera.main == null) return;

            Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
            GameObject floatingText = Instantiate(FloatingTextPrefab, FloatingTextContainer);
            floatingText.transform.position = screenPos;
            Destroy(floatingText, 2f);
        }

        private GameObject GetPanelByName(string panelName)
        {
            switch (panelName)
            {
                case "MainMenu": return MainMenuPanel;
                case "Combat": return CombatUIPanel;
                case "Exploration": return ExplorationUIPanel;
                case "Inventory": return InventoryPanel;
                case "Settings": return SettingsPanel;
                default:
                    Debug.LogWarning($"[UIManager] Panel not found: {panelName}");
                    return null;
            }
        }

        public int GetOpenPanelCount() => uiStack.Count;

        private void LogDebug(string message)
        {
            if (EnableDebugLogs)
                Debug.Log($"[UIManager] {message}");
        }

        #region Hot Reload Support
        
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] UIManager reloaded. Open panels: {uiStack.Count}");
        }

        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] UIManager static reload");
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        #endregion
    }
}
