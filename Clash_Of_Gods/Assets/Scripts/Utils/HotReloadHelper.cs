using UnityEngine;
using System;
using System.Collections.Generic;

namespace ClashOfGods.Utils
{
    /// <summary>
    /// 热重载辅助工具 - 提供热重载相关的实用功能
    /// Hot reload helper - Provides utility functions for hot reload
    /// </summary>
    public static class HotReloadHelper
    {
        /// <summary>
        /// 热重载事件 - 当任何脚本热重载时触发
        /// Hot reload event - Triggered when any script is hot reloaded
        /// </summary>
        public static event Action OnAnyScriptHotReload;

        /// <summary>
        /// 触发全局热重载事件
        /// Trigger global hot reload event
        /// </summary>
        public static void NotifyHotReload(string scriptName)
        {
            Debug.Log($"[HotReload] Script reloaded: {scriptName}");
            OnAnyScriptHotReload?.Invoke();
        }

        /// <summary>
        /// 安全获取单例实例（热重载兼容）
        /// Safely get singleton instance (hot reload compatible)
        /// </summary>
        public static T GetSingletonSafe<T>() where T : MonoBehaviour
        {
            T instance = UnityEngine.Object.FindObjectOfType<T>();
            if (instance == null)
            {
                Debug.LogWarning($"[HotReload] Singleton {typeof(T).Name} not found in scene");
            }
            return instance;
        }

        /// <summary>
        /// 重新绑定组件引用
        /// Rebind component references
        /// </summary>
        public static void RebindComponents(MonoBehaviour target)
        {
            if (target == null) return;

            var fields = target.GetType().GetFields(
                System.Reflection.BindingFlags.Instance | 
                System.Reflection.BindingFlags.Public | 
                System.Reflection.BindingFlags.NonPublic
            );

            foreach (var field in fields)
            {
                if (typeof(Component).IsAssignableFrom(field.FieldType))
                {
                    var currentValue = field.GetValue(target) as Component;
                    if (currentValue == null)
                    {
                        var component = target.GetComponent(field.FieldType);
                        if (component != null)
                        {
                            field.SetValue(target, component);
                            Debug.Log($"[HotReload] Rebound {field.Name} on {target.name}");
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 验证所有管理器实例
        /// Verify all manager instances
        /// </summary>
        public static void VerifyManagers()
        {
            Debug.Log("=== [HotReload] Manager Verification ===");
            
            var managerTypes = new List<string>
            {
                "EventManager",
                "ConfigManager", 
                "SaveSystem",
                "GameLifecycleManager",
                "BattleManager",
                "UIManager",
                "CardManager",
                "MapManager"
            };

            foreach (var typeName in managerTypes)
            {
                var type = Type.GetType($"ClashOfGods.Core.{typeName}") ?? 
                           Type.GetType($"ClashOfGods.Gameplay.Combat.{typeName}") ??
                           Type.GetType($"ClashOfGods.Gameplay.Map.{typeName}") ??
                           Type.GetType($"ClashOfGods.UI.{typeName}");
                
                if (type != null)
                {
                    var instance = UnityEngine.Object.FindObjectOfType(type);
                    Debug.Log($"  {typeName}: {(instance != null ? "✓" : "✗")}");
                }
            }
            
            Debug.Log("=== [HotReload] Verification Complete ===");
        }

        /// <summary>
        /// 重置静态变量（用于编辑器重新进入Play模式）
        /// Reset static variables (for editor re-entering play mode)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            OnAnyScriptHotReload = null;
        }
    }

    /// <summary>
    /// 热重载接口 - 实现此接口以获得热重载通知
    /// Hot reload interface - Implement this interface to receive hot reload notifications
    /// </summary>
    public interface IHotReloadable
    {
        /// <summary>
        /// 热重载时调用
        /// Called on hot reload
        /// </summary>
        void OnHotReload();
    }
}
