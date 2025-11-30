# 网络模式设置指南

## 概述

本文档介绍如何在场景中设置网络模式系统，以及后续开发的注意事项。

---

## 场景布置步骤

### 1. 创建网络管理器

#### 步骤 1.1：创建 NetworkManager GameObject

```
1. 在 Hierarchy 中右键 → Create Empty
2. 命名为 "NetworkManager"
3. 添加组件：GameNetworkManager
```

#### 步骤 1.2：配置 GameNetworkManager

在 Inspector 中配置以下参数：

```
【Network Info】
- Transport: 选择 KcpTransport（推荐）或其他 Mirror Transport
- Network Address: localhost
- Max Connections: 2（PVP 1v1）

【Spawn Info】
- Player Prefab: 拖入你的召唤师 Prefab
- Auto Create Player: ✓ 勾选
- Player Spawn Method: Round Robin

【Game Network Settings】
- Player Prefab: 拖入召唤师 Prefab（同上）
- Spawn Points: 设置 2 个出生点 Transform
  - SpawnPoint_Player1
  - SpawnPoint_Player2

【Network Mode】
- Is Online Mode: ✗ 不勾选（默认单机）
```

#### 步骤 1.3：设置出生点

```
1. 创建两个空 GameObject
   - SpawnPoint_Player1 (Position: -5, 0, 0)
   - SpawnPoint_Player2 (Position: 5, 0, 0)

2. 将它们拖入 GameNetworkManager 的 Spawn Points 数组
```

---

### 2. 创建事件管理器

```
1. 在 Hierarchy 中右键 → Create Empty
2. 命名为 "EventManager"
3. 添加组件：EventManager
```

**注意**：EventManager 会自动设置为 DontDestroyOnLoad

---

### 3. 设置单位 Prefab

#### 步骤 3.1：配置单位 Prefab

对于每个单位 Prefab（如 Warrior、Mage 等）：

```
【必需组件】
1. UnitEntity (MonoBehaviour) ← 核心逻辑
   - Config: 拖入对应的 UnitConfig ScriptableObject
   - Enable Debug Logs: ✓

2. UnitNetworkSync (NetworkBehaviour) ← 网络同步
   - enabled: ✗ 默认禁用
   - Auto Enable On Online Mode: ✓

【可选组件】
3. UnitAIController
4. UnitAnimationController
5. UnitBuffManager
```

#### 步骤 3.2：不要手动添加 NetworkIdentity

**重要**：不要在 Prefab 上手动添加 NetworkIdentity！

- 单机模式：不需要 NetworkIdentity
- 联机模式：GameNetworkManager 会自动添加

---

### 4. 设置召唤师 Prefab

#### 步骤 4.1：配置召唤师 Prefab

```
【必需组件】
1. SummonerController (MonoBehaviour) ← 核心逻辑
   - Runtime Data: 配置初始数据
   - Move Speed: 5
   - Enable Debug Logs: ✓

2. SummonerNetworkSync (NetworkBehaviour) ← 网络同步
   - enabled: ✗ 默认禁用
   - Auto Enable On Online Mode: ✓

3. CharacterController
   - Radius: 0.5
   - Height: 2
```

---

### 5. 设置战斗管理器

```
1. 在 Hierarchy 中右键 → Create Empty
2. 命名为 "BattleManager"
3. 添加组件：BattleManager
4. 添加组件：BattleNetworkSync
   - enabled: ✗ 默认禁用
   - Auto Enable On Online Mode: ✓

【配置】
- Player Spawn Area: 拖入玩家部署区域的 Transform
- Enemy Spawn Area: 拖入敌人生成区域的 Transform
- Enable Debug Logs: ✓
```

---

### 6. 添加测试工具（可选）

#### 步骤 6.1：创建测试助手

```
1. 在 Hierarchy 中右键 → Create Empty
2. 命名为 "NetworkTestHelper"
3. 添加组件：NetworkModeTestHelper

【配置】
- Test Unit Prefab: 拖入一个测试用的单位 Prefab
- Spawn Point: 拖入一个生成位置的 Transform
- Show Debug GUI: ✓
- Enable Debug Logs: ✓
```

#### 步骤 6.2：创建 UI（可选）

```
1. 创建 Canvas
2. 添加 NetworkModeUI 组件

【UI 元素】
- Host Button: 创建 Button，文本 "Start Host"
- Join Button: 创建 Button，文本 "Join Room"
- Disconnect Button: 创建 Button，文本 "Disconnect"
- Address Input: 创建 InputField，默认值 "localhost"
- Status Text: 创建 Text，显示网络状态

【绑定】
将这些 UI 元素拖入 NetworkModeUI 组件的对应字段
```

---

## 场景层级结构示例

```
Scene: BattleScene
├─ NetworkManager (GameNetworkManager)
│  └─ KcpTransport
├─ EventManager (EventManager)
├─ BattleManager (BattleManager + BattleNetworkSync)
├─ SpawnPoints
│  ├─ SpawnPoint_Player1
│  └─ SpawnPoint_Player2
├─ DeploymentAreas
│  ├─ PlayerDeployArea
│  └─ EnemySpawnArea
├─ NetworkTestHelper (NetworkModeTestHelper)
└─ Canvas (NetworkModeUI)
   ├─ HostButton
   ├─ JoinButton
   ├─ DisconnectButton
   ├─ AddressInput
   └─ StatusText
```

---

## 运行时测试

### 单机模式测试

```
1. 运行场景
2. 观察 Console：
   - [Network] PVP stopped - Offline mode
   - 所有 NetworkSync 组件应该是 disabled

3. 生成单位测试：
   - 按 F2 生成测试单位
   - 单位应该正常工作
   - 无网络开销

4. 检查性能：
   - Profiler 中不应该有 Mirror 网络相关的开销
```

### 联机模式测试

```
1. 运行场景
2. 按 F1 或点击 "Start Host" 按钮
3. 观察 Console：
   - [Network] PVP Host started - Online mode enabled
   - [NetworkSync] UnitNetworkSync enabled on XXX
   - [NetworkSync] SummonerNetworkSync enabled on XXX

4. 生成单位测试：
   - 按 F2 生成测试单位
   - 单位会自动添加 NetworkIdentity
   - 网络同步组件自动启用

5. 多客户端测试：
   - Build 一个客户端版本
   - Host 运行在 Editor
   - Client 运行 Build 版本
   - 输入 Host 的 IP 地址
   - 点击 "Join Room"
```

---

## 后续开发指南

### 1. 添加新的游戏对象

#### 如果需要网络同步：

```csharp
// 1. 创建核心逻辑类（MonoBehaviour）
public class MyGameObject : MonoBehaviour
{
    public void DoSomething()
    {
        // 核心逻辑
        
        // 触发事件（网络同步组件会监听）
        EventManager.Instance?.TriggerEvent(new MyEvent { ... });
    }
}

// 2. 创建网络同步类（NetworkSyncBase）
public class MyGameObjectNetworkSync : NetworkSyncBase
{
    private MyGameObject myObject;
    
    [SyncVar(hook = nameof(OnValueChanged))]
    private float syncedValue;
    
    protected override void Awake()
    {
        base.Awake();
        myObject = GetComponent<MyGameObject>();
    }
    
    protected override void OnNetworkSyncEnabled()
    {
        // 初始化同步值
        if (isServer)
        {
            syncedValue = myObject.someValue;
        }
        
        // 监听事件
        EventManager.Instance?.AddListener<MyEvent>(OnMyEvent);
    }
    
    protected override void OnNetworkSyncDisabled()
    {
        EventManager.Instance?.RemoveListener<MyEvent>(OnMyEvent);
    }
    
    private void OnMyEvent(MyEvent e)
    {
        if (isServer)
        {
            // 服务器同步
            syncedValue = e.newValue;
        }
    }
    
    private void OnValueChanged(float oldValue, float newValue)
    {
        if (!isServer)
        {
            // 客户端更新
            myObject.someValue = newValue;
        }
    }
}
```

#### 如果不需要网络同步：

```csharp
// 直接使用 MonoBehaviour，无需网络同步组件
public class MyLocalObject : MonoBehaviour
{
    // 纯单机逻辑
}
```

---

### 2. 事件驱动开发

**推荐模式**：使用事件系统解耦网络同步

```csharp
// 核心逻辑触发事件
public void TakeDamage(float damage)
{
    Stats.CurrentHP -= damage;
    
    // 触发事件
    EventManager.Instance?.TriggerEvent(new UnitDamagedEvent
    {
        Unit = gameObject,
        Damage = damage
    });
}

// 网络同步组件监听事件
protected override void OnNetworkSyncEnabled()
{
    EventManager.Instance?.AddListener<UnitDamagedEvent>(OnUnitDamaged);
}

private void OnUnitDamaged(UnitDamagedEvent e)
{
    if (e.Unit == gameObject && isServer)
    {
        // 同步到客户端
        syncedHP = unitEntity.Stats.CurrentHP;
    }
}
```

**优势**：
- 核心逻辑不依赖网络
- 网络同步可插拔
- 易于测试和维护

---

### 3. 网络同步最佳实践

#### 3.1 只同步必要的数据

```csharp
// ✓ 好的做法：只同步关键状态
[SyncVar(hook = nameof(OnHPChanged))]
private float syncedHP;

[SyncVar(hook = nameof(OnAliveChanged))]
private bool syncedIsAlive;

// ✗ 不好的做法：同步所有数据
[SyncVar] private float attack;
[SyncVar] private float defense;
[SyncVar] private float attackSpeed;
// ... 太多了！
```

#### 3.2 使用 Hook 更新客户端

```csharp
[SyncVar(hook = nameof(OnHPChanged))]
private float syncedHP;

private void OnHPChanged(float oldHP, float newHP)
{
    if (!isServer)
    {
        // 客户端更新本地状态
        unitEntity.Stats.CurrentHP = newHP;
    }
}
```

#### 3.3 服务器权威

```csharp
// 客户端请求
[Command]
public void CmdRequestAttack(NetworkIdentity target)
{
    // 服务器验证和执行
    if (CanAttack(target))
    {
        unitEntity.Attack(target.gameObject);
    }
}

// 服务器广播
[ClientRpc]
public void RpcPlayAttackAnimation()
{
    unitEntity.AnimationController?.TriggerAnimation("Attack");
}
```

---

### 4. 调试技巧

#### 4.1 使用调试 GUI

```csharp
private void OnGUI()
{
    if (!ShowDebugGUI) return;
    
    GUI.Box(new Rect(10, 10, 200, 100), "Debug Info");
    GUI.Label(new Rect(20, 35, 180, 60), 
        $"Mode: {(NetworkModeManager.Instance?.IsOnlineMode ?? false ? "Online" : "Offline")}\n" +
        $"Units: {FindObjectsOfType<UnitEntity>().Length}\n" +
        $"NetworkSync: {FindObjectsOfType<NetworkSyncBase>().Count(s => s.enabled)}");
}
```

#### 4.2 使用快捷键

```csharp
private void Update()
{
    if (Input.GetKeyDown(KeyCode.F1))
        ToggleNetworkMode();
    
    if (Input.GetKeyDown(KeyCode.F2))
        SpawnTestUnit();
    
    if (Input.GetKeyDown(KeyCode.F3))
        ShowDebugGUI = !ShowDebugGUI;
}
```

#### 4.3 日志规范

```csharp
// 使用统一的日志前缀
Debug.Log($"[Network] Message");
Debug.Log($"[NetworkSync] Message");
Debug.Log($"[Unit] Message");
Debug.Log($"[Battle] Message");
```

---

### 5. 性能优化

#### 5.1 减少同步频率

```csharp
[Header("Sync Settings")]
public float syncInterval = 0.1f;
private float lastSyncTime;

private void Update()
{
    if (!enabled || !isServer) return;
    
    if (Time.time - lastSyncTime > syncInterval)
    {
        // 同步位置
        if (Vector3.Distance(syncedPosition, transform.position) > 0.1f)
        {
            syncedPosition = transform.position;
        }
        lastSyncTime = Time.time;
    }
}
```

#### 5.2 使用 SyncVar 而不是频繁的 RPC

```csharp
// ✓ 好的做法：使用 SyncVar
[SyncVar(hook = nameof(OnHPChanged))]
private float syncedHP;

// ✗ 不好的做法：频繁调用 RPC
[ClientRpc]
public void RpcUpdateHP(float newHP)
{
    // 每次 HP 变化都调用 RPC，开销大
}
```

#### 5.3 批量操作

```csharp
// ✓ 好的做法：批量生成
[Server]
public void SpawnMultipleUnits(int[] unitIDs, Vector3[] positions)
{
    for (int i = 0; i < unitIDs.Length; i++)
    {
        SpawnUnit(unitIDs[i], positions[i]);
    }
}

// ✗ 不好的做法：逐个生成
[Command]
public void CmdSpawnUnit(int unitID, Vector3 position)
{
    // 每次调用都有网络开销
}
```

---

### 6. 常见问题

#### Q1: 为什么我的 NetworkSync 组件没有启用？

**A**: 检查以下几点：
1. NetworkModeManager 是否已启动联机模式？
2. NetworkSync 组件的 `autoEnableOnOnlineMode` 是否勾选？
3. EventManager 是否存在于场景中？
4. 是否正确监听了 `NetworkModeChangedEvent`？

#### Q2: 为什么客户端看不到服务器生成的对象？

**A**: 确保：
1. 对象有 NetworkIdentity 组件
2. 使用 `NetworkServer.Spawn(obj)` 生成对象
3. Prefab 已注册到 NetworkManager 的 Spawnable Prefabs 列表

#### Q3: 如何在单机模式下测试网络同步逻辑？

**A**: 
1. 使用 F1 切换到联机模式
2. 在 Editor 中启动 Host
3. 使用 ParrelSync 插件克隆项目，作为 Client 测试

#### Q4: 热重载后网络同步失效？

**A**: 
1. 确保所有组件都实现了 `OnScriptHotReload` 方法
2. 在热重载回调中重新注册事件监听
3. 重新获取组件引用

---

## 检查清单

### 场景设置检查

- [ ] NetworkManager 已添加 GameNetworkManager 组件
- [ ] EventManager 已添加到场景
- [ ] BattleManager 已添加 BattleNetworkSync 组件
- [ ] 所有单位 Prefab 都有 UnitNetworkSync 组件（默认禁用）
- [ ] 召唤师 Prefab 有 SummonerNetworkSync 组件（默认禁用）
- [ ] 出生点已设置
- [ ] Transport 已配置（KcpTransport 推荐）

### 运行时检查

- [ ] 单机模式下所有 NetworkSync 组件都是禁用的
- [ ] 按 F1 可以切换到联机模式
- [ ] 联机模式下 NetworkSync 组件自动启用
- [ ] 生成的单位自动添加 NetworkIdentity
- [ ] 事件系统正常工作
- [ ] 热重载功能正常

### 性能检查

- [ ] 单机模式下 Profiler 无 Mirror 开销
- [ ] 联机模式下网络流量合理
- [ ] 同步频率适中（不要每帧同步）
- [ ] 使用 SyncVar 而不是频繁 RPC

---

## 参考资料

- [Mirror 官方文档](https://mirror-networking.gitbook.io/docs/)
- [Fast Script Reload 指南](./FAST_SCRIPT_RELOAD_GUIDE.md)
- [热重载测试指南](./HOT_RELOAD_TEST_GUIDE.md)
- [网络架构文档](./NETWORK_ARCHITECTURE.md)

---

**设置完成！开始开发吧！** 🚀
