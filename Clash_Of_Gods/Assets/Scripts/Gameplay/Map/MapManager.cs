using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

namespace ClashOfGods.Gameplay.Map
{
    /// <summary>
    /// 地图管理器 - 管理探索地图状态、迷雾和动态寻路（支持热重载）
    /// Map manager - Manages exploration map state, fog, and dynamic pathfinding (Hot reload compatible)
    /// </summary>
    public class MapManager : MonoBehaviour
    {
        private static MapManager _instance;
        
        public static MapManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<MapManager>();
                return _instance;
            }
        }

        [Header("Map Data")]
        public string CurrentMapID;
        public GridSystem MapGrid;

        [Header("Events")]
        public List<MapEventPoint> ActiveEvents = new List<MapEventPoint>();

        [Header("Fog of War")]
        public bool UseFogOfWar = true;
        public Material FogMaterial;

        [Header("NavMesh")]
        public NavMeshSurface NavMeshSurface;

        private void Awake()
        {
            if (_instance == null)
            {
                _instance = (MapManager)(object)this;
            }
            else if (!ReferenceEquals(_instance, this))
            {
                Destroy(gameObject);
            }
        }

        private void OnDestroy()
        {
            if (ReferenceEquals(_instance, this))
                _instance = null;
        }

        /// <summary>
        /// 加载地图
        /// Load map
        /// </summary>
        public void LoadMap(string mapId)
        {
            CurrentMapID = mapId;
            Debug.Log($"[MapManager] Loading map: {mapId}");
            
            // TODO: 加载地图场景
            // TODO: 初始化事件点
            InitializeEvents();
            
            // 烘焙导航网格
            BakeNavMeshDynamic();
        }

        /// <summary>
        /// 初始化事件点
        /// Initialize event points
        /// </summary>
        private void InitializeEvents()
        {
            // TODO: 从配置加载事件点
            ActiveEvents.Clear();
            
            MapEventPoint[] eventPoints = FindObjectsOfType<MapEventPoint>();
            ActiveEvents.AddRange(eventPoints);
            
            Debug.Log($"[MapManager] Initialized {ActiveEvents.Count} event points");
        }

        /// <summary>
        /// 消除迷雾
        /// Reveal fog
        /// </summary>
        public void RevealFog(Vector3 position, float radius)
        {
            if (!UseFogOfWar) return;
            
            // TODO: 实现迷雾消除逻辑
            Debug.Log($"[MapManager] Revealing fog at {position} with radius {radius}");
        }

        /// <summary>
        /// 动态烘焙导航网格
        /// Bake NavMesh dynamically
        /// </summary>
        public void BakeNavMeshDynamic()
        {
            if (NavMeshSurface != null)
            {
                NavMeshSurface.BuildNavMesh();
                Debug.Log("[MapManager] NavMesh baked");
            }
        }

        /// <summary>
        /// 获取最近的事件点
        /// Get nearest event point
        /// </summary>
        public MapEventPoint GetNearestEvent(Vector3 position, float maxDistance = 100f)
        {
            MapEventPoint nearest = null;
            float nearestDistance = maxDistance;

            foreach (MapEventPoint eventPoint in ActiveEvents)
            {
                if (eventPoint == null || !eventPoint.IsActive) continue;

                float distance = Vector3.Distance(position, eventPoint.transform.position);
                if (distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearest = eventPoint;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 移除事件点
        /// Remove event point
        /// </summary>
        public void RemoveEvent(MapEventPoint eventPoint)
        {
            if (ActiveEvents.Contains(eventPoint))
            {
                ActiveEvents.Remove(eventPoint);
                Debug.Log($"[MapManager] Removed event: {eventPoint.EventType}");
            }
        }

        #region Hot Reload Support
        
        /// <summary>
        /// 热重载回调
        /// Hot reload callback
        /// </summary>
        void OnScriptHotReload()
        {
            Debug.Log($"[HotReload] MapManager reloaded. Map: {CurrentMapID}, Events: {ActiveEvents.Count}");
        }

        /// <summary>
        /// 静态热重载回调
        /// Static hot reload callback
        /// </summary>
        static void OnScriptHotReloadNoInstance()
        {
            Debug.Log("[HotReload] MapManager static reload");
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
    /// 网格系统（用于迷雾）
    /// Grid system (for fog of war)
    /// </summary>
    [System.Serializable]
    public class GridSystem
    {
        public int Width = 100;
        public int Height = 100;
        public float CellSize = 1f;
        
        private bool[,] revealedCells;

        public void Initialize()
        {
            revealedCells = new bool[Width, Height];
        }

        public void RevealCell(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                revealedCells[x, y] = true;
            }
        }

        public bool IsCellRevealed(int x, int y)
        {
            if (x >= 0 && x < Width && y >= 0 && y < Height)
            {
                return revealedCells[x, y];
            }
            return false;
        }
    }
}
