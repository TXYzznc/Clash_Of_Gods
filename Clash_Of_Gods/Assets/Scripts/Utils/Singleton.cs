using UnityEngine;

namespace ClashOfGods.Utils
{
    /// <summary>
    /// 单例模式基类 - 支持热重载
    /// Singleton pattern base class - Hot reload compatible
    /// </summary>
    public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static T instance;
        private static object lockObject = new object();
        private static bool applicationIsQuitting = false;

        public static T Instance
        {
            get
            {
                if (applicationIsQuitting)
                {
                    Debug.LogWarning($"[Singleton] Instance of {typeof(T)} already destroyed. Returning null.");
                    return null;
                }

                lock (lockObject)
                {
                    if (instance == null)
                    {
                        instance = FindObjectOfType<T>();

                        if (instance == null)
                        {
                            GameObject singletonObject = new GameObject();
                            instance = singletonObject.AddComponent<T>();
                            singletonObject.name = $"{typeof(T).Name} (Singleton)";

                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return instance;
                }
            }
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                // 热重载兼容：使用类型转换避免类型不匹配
                instance = (T)(object)this;
                DontDestroyOnLoad(gameObject);
                OnSingletonAwake();
            }
            else if (!ReferenceEquals(instance, this))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 单例初始化时调用（子类可重写）
        /// Called when singleton initializes (override in subclass)
        /// </summary>
        protected virtual void OnSingletonAwake()
        {
        }

        protected virtual void OnDestroy()
        {
            if (ReferenceEquals(instance, this))
            {
                applicationIsQuitting = true;
                instance = null;
            }
        }

        /// <summary>
        /// 热重载回调 - 子类可重写以处理热重载后的状态恢复
        /// Hot reload callback - Override in subclass to handle state restoration
        /// </summary>
        protected virtual void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] {typeof(T).Name} hot reloaded");
        }

        /// <summary>
        /// 静态热重载回调 - 无需实例
        /// Static hot reload callback - No instance required
        /// </summary>
        protected static void OnScriptHotReloadNoInstance()
        {
            Debug.Log($"[HotReload] {typeof(T).Name} static hot reload");
        }

        /// <summary>
        /// 重置应用退出标志（用于编辑器重新进入Play模式）
        /// Reset application quitting flag (for editor re-entering play mode)
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            applicationIsQuitting = false;
            instance = null;
        }
    }
}
