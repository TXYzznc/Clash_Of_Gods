using System.Collections.Generic;
using UnityEngine;

namespace ClashOfGods.Utils
{
    /// <summary>
    /// 对象池 - 优化性能，减少实例化开销
    /// Object pool - Optimizes performance, reduces instantiation overhead
    /// </summary>
    public class ObjectPool : MonoBehaviour
    {
        [System.Serializable]
        public class Pool
        {
            public string Tag;
            public GameObject Prefab;
            public int Size;
        }

        public static ObjectPool Instance { get; private set; }

        [Header("Pools")]
        public List<Pool> Pools;

        private Dictionary<string, Queue<GameObject>> poolDictionary;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            InitializePools();
        }

        /// <summary>
        /// 初始化对象池
        /// Initialize pools
        /// </summary>
        private void InitializePools()
        {
            poolDictionary = new Dictionary<string, Queue<GameObject>>();

            foreach (Pool pool in Pools)
            {
                Queue<GameObject> objectPool = new Queue<GameObject>();

                for (int i = 0; i < pool.Size; i++)
                {
                    GameObject obj = Instantiate(pool.Prefab);
                    obj.SetActive(false);
                    objectPool.Enqueue(obj);
                }

                poolDictionary.Add(pool.Tag, objectPool);
            }

            Debug.Log($"[ObjectPool] Initialized {poolDictionary.Count} pools");
        }

        /// <summary>
        /// 从池中获取对象
        /// Spawn object from pool
        /// </summary>
        public GameObject SpawnFromPool(string tag, Vector3 position, Quaternion rotation)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPool] Pool with tag {tag} doesn't exist");
                return null;
            }

            GameObject objectToSpawn = poolDictionary[tag].Dequeue();

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;

            poolDictionary[tag].Enqueue(objectToSpawn);

            return objectToSpawn;
        }

        /// <summary>
        /// 将对象返回池中
        /// Return object to pool
        /// </summary>
        public void ReturnToPool(string tag, GameObject obj)
        {
            if (!poolDictionary.ContainsKey(tag))
            {
                Debug.LogWarning($"[ObjectPool] Pool with tag {tag} doesn't exist");
                return;
            }

            obj.SetActive(false);
        }
    }
}
