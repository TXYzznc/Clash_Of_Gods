using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace ClashOfGods.Core
{
    /// <summary>
    /// 数据持久化系统 - 负责序列化和反序列化玩家数据（支持热重载）
    /// Data persistence system - Handles serialization and deserialization (Hot reload compatible)
    /// </summary>
    public class SaveSystem : MonoBehaviour
    {
        private static SaveSystem _instance;
        
        public static SaveSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindObjectOfType<SaveSystem>();
                }
                return _instance;
            }
        }

        private const string SAVE_FILE_NAME = "save.dat";
        private string SavePath => Path.Combine(Application.persistentDataPath, SAVE_FILE_NAME);

        // 缓存当前玩家数据
        private PlayerData cachedPlayerData;

        private void Awake()
        {
            if (_instance == null)
            {
                // 热重载兼容：使用类型转换
                _instance = (SaveSystem)(object)this;
                DontDestroyOnLoad(gameObject);
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// 获取当前玩家数据（带缓存）
        /// Get current player data (with cache)
        /// </summary>
        public PlayerData GetCurrentPlayerData()
        {
            if (cachedPlayerData == null)
            {
                cachedPlayerData = LoadGame();
            }
            return cachedPlayerData;
        }

        /// <summary>
        /// 保存游戏数据（本地/云端）
        /// Save game data (local/cloud)
        /// </summary>
        public void SaveGame(PlayerData data, bool toCloud = false)
        {
            try
            {
                data.LastSaveTimestamp = DateTime.Now.Ticks;
                byte[] binaryData = SerializeToBinary(data);
                File.WriteAllBytes(SavePath, binaryData);
                
                cachedPlayerData = data;
                
                Debug.Log($"[SaveSystem] Game saved to: {SavePath}");

                if (toCloud)
                {
                    // TODO: 实现云存档上传
                    Debug.Log("[SaveSystem] Cloud save not implemented yet");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Save failed: {e.Message}");
            }
        }

        /// <summary>
        /// 快速保存当前缓存数据
        /// Quick save current cached data
        /// </summary>
        public void QuickSave()
        {
            if (cachedPlayerData != null)
            {
                SaveGame(cachedPlayerData);
            }
        }

        /// <summary>
        /// 读取存档
        /// Load save data
        /// </summary>
        public PlayerData LoadGame()
        {
            try
            {
                if (File.Exists(SavePath))
                {
                    byte[] binaryData = File.ReadAllBytes(SavePath);
                    PlayerData data = DeserializeFromBinary<PlayerData>(binaryData);
                    
                    cachedPlayerData = data;
                    Debug.Log("[SaveSystem] Game loaded successfully");
                    return data;
                }
                else
                {
                    Debug.Log("[SaveSystem] No save file found, creating new data");
                    cachedPlayerData = new PlayerData();
                    return cachedPlayerData;
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[SaveSystem] Load failed: {e.Message}");
                cachedPlayerData = new PlayerData();
                return cachedPlayerData;
            }
        }

        /// <summary>
        /// 二进制序列化
        /// Binary serialization
        /// </summary>
        public byte[] SerializeToBinary(object data)
        {
            using (MemoryStream stream = new MemoryStream())
            {
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete
                BinaryFormatter formatter = new BinaryFormatter();
                formatter.Serialize(stream, data);
#pragma warning restore SYSLIB0011
                return stream.ToArray();
            }
        }

        /// <summary>
        /// 二进制反序列化
        /// Binary deserialization
        /// </summary>
        public T DeserializeFromBinary<T>(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
            {
#pragma warning disable SYSLIB0011 // BinaryFormatter is obsolete
                BinaryFormatter formatter = new BinaryFormatter();
                return (T)formatter.Deserialize(stream);
#pragma warning restore SYSLIB0011
            }
        }

        /// <summary>
        /// JSON序列化（备选方案）
        /// JSON serialization (alternative)
        /// </summary>
        public string SerializeToJson(object data)
        {
            return JsonUtility.ToJson(data, true);
        }

        /// <summary>
        /// JSON反序列化（备选方案）
        /// JSON deserialization (alternative)
        /// </summary>
        public T DeserializeFromJson<T>(string json)
        {
            return JsonUtility.FromJson<T>(json);
        }

        /// <summary>
        /// 删除存档
        /// Delete save file
        /// </summary>
        public void DeleteSave()
        {
            if (File.Exists(SavePath))
            {
                File.Delete(SavePath);
                cachedPlayerData = null;
                Debug.Log("[SaveSystem] Save file deleted");
            }
        }

        /// <summary>
        /// 检查存档是否存在
        /// Check if save file exists
        /// </summary>
        public bool HasSaveFile()
        {
            return File.Exists(SavePath);
        }

        #region Hot Reload Support
        
        /// <summary>
        /// 热重载回调
        /// Hot reload callback
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] SaveSystem reloaded. Cached data: {(cachedPlayerData != null ? "Yes" : "No")}");
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] SaveSystem static reload");
        }

        /// <summary>
        /// 重置静态变量
        /// Reset static variables
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            _instance = null;
        }

        #endregion
    }

    /// <summary>
    /// 玩家数据 - 可序列化
    /// Player data - Serializable
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public string PlayerName = "Player";
        public int GlobalLevel = 1;
        public int[] UnlockedTechIDs = new int[0];
        public UnitInstanceData[] OwnedUnits = new UnitInstanceData[0];
        public InventoryData Inventory = new InventoryData();
        public int HolyWaterCount = 3;
        public long LastSaveTimestamp;

        public PlayerData()
        {
            LastSaveTimestamp = DateTime.Now.Ticks;
        }
    }

    /// <summary>
    /// 背包数据
    /// Inventory data
    /// </summary>
    [Serializable]
    public class InventoryData
    {
        public int[] ItemIDs = new int[0];
        public int[] ItemCounts = new int[0];
        public int Gold = 0;
    }

    /// <summary>
    /// 单位实例数据
    /// Unit instance data
    /// </summary>
    [Serializable]
    public class UnitInstanceData
    {
        public int ConfigID;
        public int Experience;
        public int Tier = 1;
        public int[] EquippedItemIDs = new int[5]; // 3宝物+2装备
        public float CurrentHP;
    }
}
