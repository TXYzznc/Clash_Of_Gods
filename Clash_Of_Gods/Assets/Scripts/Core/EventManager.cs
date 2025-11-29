using System;
using System.Collections.Generic;
using UnityEngine;

namespace ClashOfGods.Core
{
    /// <summary>
    /// 全局事件总线 - 解耦各个模块（支持热重载）
    /// Global event bus - Decouples modules (Hot reload compatible)
    /// </summary>
    public class EventManager : MonoBehaviour
    {
        private static EventManager _instance;
        
        public static EventManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<EventManager>();
                }
                return _instance;
            }
        }

        private Dictionary<Type, Delegate> eventDictionary = new Dictionary<Type, Delegate>();

        private void Awake()
        {
            if (_instance == null)
            {
                // 热重载兼容：使用类型转换
                _instance = (EventManager)(object)this;
                DontDestroyOnLoad(gameObject);
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 添加事件监听
        /// Add event listener
        /// </summary>
        public void AddListener<T>(Action<T> listener)
        {
            Type eventType = typeof(T);
            
            if (eventDictionary.ContainsKey(eventType))
            {
                eventDictionary[eventType] = Delegate.Combine(eventDictionary[eventType], listener);
            }
            else
            {
                eventDictionary[eventType] = listener;
            }
        }

        /// <summary>
        /// 移除事件监听
        /// Remove event listener
        /// </summary>
        public void RemoveListener<T>(Action<T> listener)
        {
            Type eventType = typeof(T);
            
            if (eventDictionary.ContainsKey(eventType))
            {
                eventDictionary[eventType] = Delegate.Remove(eventDictionary[eventType], listener);
                
                if (eventDictionary[eventType] == null)
                {
                    eventDictionary.Remove(eventType);
                }
            }
        }

        /// <summary>
        /// 触发事件
        /// Trigger event
        /// </summary>
        public void TriggerEvent<T>(T eventData)
        {
            Type eventType = typeof(T);
            
            if (eventDictionary.ContainsKey(eventType))
            {
                var callback = eventDictionary[eventType] as Action<T>;
                callback?.Invoke(eventData);
            }
        }

        /// <summary>
        /// 清空所有事件
        /// Clear all events
        /// </summary>
        public void ClearAllEvents()
        {
            eventDictionary.Clear();
        }

        /// <summary>
        /// 获取已注册的事件数量（调试用）
        /// Get registered event count (for debugging)
        /// </summary>
        public int GetRegisteredEventCount()
        {
            return eventDictionary.Count;
        }

        #region Hot Reload Support
        
        /// <summary>
        /// 热重载回调 - 保持事件字典状态
        /// Hot reload callback - Preserve event dictionary state
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] EventManager reloaded. Registered events: {eventDictionary.Count}");
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] EventManager static reload");
        }

        /// <summary>
        /// 重置静态变量（编辑器重新进入Play模式时）
        /// Reset static variables (when editor re-enters play mode)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        #endregion
    }

    #region Event Data Classes
    
    /// <summary>
    /// 单位死亡事件
    /// Unit death event
    /// </summary>
    public class UnitDeathEvent
    {
        public GameObject Unit;
        public bool IsPlayer;
    }

    /// <summary>
    /// 战斗开始事件
    /// Combat start event
    /// </summary>
    public class CombatStartEvent
    {
        public List<GameObject> Enemies;
    }

    /// <summary>
    /// 战斗结束事件
    /// Combat end event
    /// </summary>
    public class CombatEndEvent
    {
        public bool IsVictory;
        public List<GameObject> Rewards;
    }

    /// <summary>
    /// 单位受伤事件
    /// Unit damaged event
    /// </summary>
    public class UnitDamagedEvent
    {
        public GameObject Unit;
        public float Damage;
        public GameObject Source;
    }

    /// <summary>
    /// 技能释放事件
    /// Skill cast event
    /// </summary>
    public class SkillCastEvent
    {
        public int SkillID;
        public GameObject Caster;
        public Vector3 TargetPosition;
    }

    #endregion
}
