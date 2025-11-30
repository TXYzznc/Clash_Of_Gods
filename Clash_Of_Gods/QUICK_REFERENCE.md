# 快速参考卡片

## 网络模式系统

### 快捷键

| 按键 | 功能 |
|------|------|
| F1 | 切换网络模式（单机 ↔ 联机） |
| F2 | 生成测试单位 |
| F3 | 显示/隐藏调试信息 |

---

### 代码模板

#### 1. 创建新的游戏对象（需要网络同步）

```csharp
// 核心逻辑类
public class MyObject : MonoBehaviour
{
    public void DoAction()
    {
        // 执行逻辑
        
        // 触发事件
        EventManager.Instance?.TriggerEvent(new MyEvent { ... });
    }
}

// 网络同步类
[RequireComponent(typeof(MyObject))]
public class MyObjectNetworkSync : NetworkSyncBase
{
    private MyObject myObject;
    
    [SyncVar(hook = nameof(OnValueChanged))]
    private float syncedValue;
    
    protected override void Awake()
    {
        base.Awake();
        myObject = GetComponent<MyObject>();
    }
    
    protected override void OnNetworkSyncEnabled()
    {
        if (isServer)
            syncedValue = myObject.value;
        
        EventManager.Instance?.AddListener<MyEvent>(OnMyEvent);
    }
    
    protected override void OnNetworkSyncDisabled()
    {
        EventManager.Instance?.RemoveListener<MyEvent>(OnMyEvent);
    }
    
    private void OnMyEvent(MyEvent e)
    {
        if (isServer)
            syncedValue = e.newValue;
    }
    
    private void OnValueChanged(float oldValue, float newValue)
    {
        if (!isServer)
            myObject.value = newValue;
    }
}
```

#### 2. 客户端请求 → 服务器执行

```csharp
[Command]
public void CmdRequestAction(int param)
{
    // 服务器验证
    if (CanDoAction(param))
    {
        // 执行逻辑
        myObject.DoAction(param);
        
        // 广播给所有客户端
        RpcOnActionExecuted(param);
    }
}

[ClientRpc]
private void RpcOnActionExecuted(int param)
{
    // 播放特效、动画等
}
```

#### 3. 服务器广播事件

```csharp
// 服务器端
if (isServer)
{
    RpcBroadcastEvent(eventData);
}

[ClientRpc]
private void RpcBroadcastEvent(EventData data)
{
    // 所有客户端收到
}
```

---

### 场景设置清单

```
必需对象：
├─ NetworkManager (GameNetworkManager)
├─ EventManager (EventManager)
└─ BattleManager (BattleManager + BattleNetworkSync)

单位 Prefab：
├─ UnitEntity (enabled)
└─ UnitNetworkSync (disabled)

召唤师 Prefab：
├─ SummonerController (enabled)
└─ SummonerNetworkSync (disabled)
```

---

### API 速查

#### 网络模式切换

```csharp
// 启动 Host
NetworkModeManager.Instance.StartPVPHost();

// 加入房间
NetworkModeManager.Instance.JoinPVPRoom("192.168.1.100");

// 停止联机
NetworkModeManager.Instance.StopPVP();

// 检查当前模式
bool isOnline = NetworkModeManager.Instance.IsOnlineMode;
```

#### 事件系统

```csharp
// 注册监听
EventManager.Instance?.AddListener<MyEvent>(OnMyEvent);

// 触发事件
EventManager.Instance?.TriggerEvent(new MyEvent { ... });

// 移除监听
EventManager.Instance?.RemoveListener<MyEvent>(OnMyEvent);
```

#### 网络对象管理

```csharp
// 启用网络同步
GameNetworkManager gameNetMgr = NetworkModeManager.Instance as GameNetworkManager;
gameNetMgr?.EnableNetworkSyncOnObject(myObject);

// 生成网络对象（服务器）
GameObject obj = gameNetMgr.SpawnNetworkObject(prefab, position, rotation);

// 销毁网络对象（服务器）
gameNetMgr.DespawnNetworkObject(obj);
```

---

### 调试技巧

#### 1. 日志规范

```csharp
Debug.Log($"[Network] 网络相关");
Debug.Log($"[NetworkSync] 同步相关");
Debug.Log($"[Unit] 单位相关");
Debug.Log($"[Battle] 战斗相关");
Debug.Log($"[HotReload] 热重载相关");
```

#### 2. 检查网络状态

```csharp
// 是否是服务器
if (isServer) { ... }

// 是否是客户端
if (isClient) { ... }

// 是否是本地玩家
if (isLocalPlayer) { ... }

// 是否有权限
if (hasAuthority) { ... }
```

#### 3. 调试 GUI

```csharp
private void OnGUI()
{
    GUI.Box(new Rect(10, 10, 200, 100), "Debug");
    GUI.Label(new Rect(20, 35, 180, 60), 
        $"Mode: {(NetworkModeManager.Instance?.IsOnlineMode ?? false ? "Online" : "Offline")}\n" +
        $"Server: {NetworkServer.active}\n" +
        $"Client: {NetworkClient.active}");
}
```

---

### 常见错误

#### 错误 1: NetworkIdentity not found

**原因**：对象没有 NetworkIdentity 组件

**解决**：
```csharp
NetworkIdentity netId = obj.GetComponent<NetworkIdentity>();
if (netId == null)
    netId = obj.AddComponent<NetworkIdentity>();
```

#### 错误 2: ClientRpc/Command not working

**原因**：
- 不在 NetworkBehaviour 中
- 没有 NetworkIdentity
- 没有网络连接

**解决**：
1. 确保继承自 NetworkBehaviour
2. 确保对象有 NetworkIdentity
3. 确保已连接到网络

#### 错误 3: SyncVar not syncing

**原因**：
- 只在服务器修改
- Hook 函数名错误
- 网络未连接

**解决**：
```csharp
[SyncVar(hook = nameof(OnValueChanged))]  // 确保函数名正确
private float syncedValue;

[Server]
public void UpdateValue(float newValue)
{
    syncedValue = newValue;  // 只在服务器修改
}

private void OnValueChanged(float oldValue, float newValue)
{
    if (!isServer)  // 客户端更新
    {
        // 更新本地状态
    }
}
```

---

### 性能优化

#### 1. 减少同步频率

```csharp
public float syncInterval = 0.1f;
private float lastSyncTime;

private void Update()
{
    if (Time.time - lastSyncTime > syncInterval)
    {
        // 同步
        lastSyncTime = Time.time;
    }
}
```

#### 2. 只同步变化的值

```csharp
private void Update()
{
    if (isServer)
    {
        // 只在值变化时同步
        if (Vector3.Distance(syncedPosition, transform.position) > 0.1f)
        {
            syncedPosition = transform.position;
        }
    }
}
```

#### 3. 批量操作

```csharp
// ✓ 好
[Server]
public void SpawnMultiple(int[] ids, Vector3[] positions)
{
    for (int i = 0; i < ids.Length; i++)
        Spawn(ids[i], positions[i]);
}

// ✗ 不好
[Command]
public void CmdSpawn(int id, Vector3 position)
{
    // 每次调用都有网络开销
}
```

---

### 测试流程

#### 单机测试

```
1. 运行场景
2. 检查 Console 无错误
3. 按 F2 生成单位
4. 验证功能正常
5. 检查 Profiler 无网络开销
```

#### 联机测试

```
1. 按 F1 切换到联机模式
2. 观察 NetworkSync 组件启用
3. 按 F2 生成单位
4. 验证网络同步正常
5. Build 客户端版本测试多人
```

---

### 文档索引

- 📖 [网络架构文档](./NETWORK_ARCHITECTURE.md) - 架构设计详解
- 🛠️ [场景设置指南](./NETWORK_MODE_SETUP_GUIDE.md) - 详细设置步骤
- 🔥 [热重载指南](./FAST_SCRIPT_RELOAD_GUIDE.md) - 热重载使用
- 🧪 [热重载测试](./HOT_RELOAD_TEST_GUIDE.md) - 测试流程

---

**快速开发，高效调试！** ⚡
