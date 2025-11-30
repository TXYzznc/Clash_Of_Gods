# 网络架构设计文档

## 概述

本项目采用**单机优先、联机可选**的网络架构设计，实现了单机模式与网络同步的完全解耦。

### 核心特点

- **默认单机模式**：无网络开销，性能最优
- **一键切换联机**：通过事件系统统一管理
- **组件可插拔**：网络同步组件默认禁用，按需启用
- **热重载友好**：所有组件支持 Fast Script Reload

---

## 架构图

```
┌─────────────────────────────────────────────────────────────┐
│                      游戏逻辑层                              │
│  ┌─────────────┐  ┌──────────────────┐  ┌───────────────┐  │
│  │ UnitEntity  │  │ SummonerController│  │ BattleManager │  │
│  │(MonoBehaviour)│ │  (MonoBehaviour)  │  │(MonoBehaviour)│  │
│  └─────────────┘  └──────────────────┘  └───────────────┘  │
└─────────────────────────────────────────────────────────────┘
           ↓ 可选（联机模式启用）
┌─────────────────────────────────────────────────────────────┐
│                      网络同步层                              │
│  ┌─────────────────┐  ┌─────────────────────┐               │
│  │ UnitNetworkSync │  │ SummonerNetworkSync │               │
│  │(NetworkSyncBase)│  │  (NetworkSyncBase)  │               │
│  └─────────────────┘  └─────────────────────┘               │
│                    ↑ 继承                                    │
│              ┌─────────────────┐                            │
│              │ NetworkSyncBase │ ← 自动监听模式切换事件      │
│              │(NetworkBehaviour)│                            │
│              └─────────────────┘                            │
└─────────────────────────────────────────────────────────────┘
           ↓ 管理
┌─────────────────────────────────────────────────────────────┐
│                      网络管理层                              │
│  ┌───────────────────┐  ┌──────────────────────┐            │
│  │NetworkModeManager │→ │ GameNetworkManager   │            │
│  │  (NetworkManager) │  │ (游戏特定网络功能)    │            │
│  └───────────────────┘  └──────────────────────┘            │
└─────────────────────────────────────────────────────────────┘
           ↓ 事件驱动
┌─────────────────────────────────────────────────────────────┐
│                      事件系统                                │
│              ┌──────────────────────┐                       │
│              │ NetworkModeChangedEvent │                    │
│              │ { IsOnline, IsHost }    │                    │
│              └──────────────────────┘                       │
└─────────────────────────────────────────────────────────────┘
```

---

## 核心组件

### 1. NetworkModeManager（网络模式管理器）

**职责**：控制单机/联机模式切换

```csharp
// 启动 PVP 主机
NetworkModeManager.Instance.StartPVPHost();

// 加入 PVP 房间
NetworkModeManager.Instance.JoinPVPRoom("192.168.1.100");

// 停止 PVP，返回单机
NetworkModeManager.Instance.StopPVP();

// 检查当前模式
bool isOnline = NetworkModeManager.Instance.IsOnlineMode;
```

### 2. NetworkSyncBase（网络同步基类）

**职责**：所有网络同步组件的基类，自动响应模式切换

```csharp
public abstract class NetworkSyncBase : NetworkBehaviour
{
    protected virtual void Awake()
    {
        enabled = false;  // 默认禁用
    }

    protected virtual void Start()
    {
        // 自动监听网络模式变更事件
        EventManager.Instance?.AddListener<NetworkModeChangedEvent>(OnNetworkModeChanged);
    }

    private void OnNetworkModeChanged(NetworkModeChangedEvent e)
    {
        if (e.IsOnline)
            EnableNetworkSync();
        else
            DisableNetworkSync();
    }
}
```

### 3. UnitNetworkSync / SummonerNetworkSync

**职责**：具体的网络同步实现

```csharp
[RequireComponent(typeof(UnitEntity))]
public class UnitNetworkSync : NetworkSyncBase
{
    [SyncVar(hook = nameof(OnHPChanged))]
    private float syncedHP;

    protected override void OnNetworkSyncEnabled()
    {
        // 初始化同步值
        if (isServer)
            syncedHP = unitEntity.Stats.CurrentHP;
    }
}
```

---

## 使用方式

### 场景设置

```
GameObject: Unit
├─ UnitEntity (MonoBehaviour)        ← 核心逻辑，始终启用
├─ UnitNetworkSync (NetworkSyncBase) ← 网络同步，默认禁用
├─ UnitAIController
└─ UnitAnimationController

GameObject: NetworkManager
├─ GameNetworkManager
└─ NetworkModeTestHelper             ← 测试工具（可选）
```

### 单机模式（默认）

```
1. UnitEntity.enabled = true
2. UnitNetworkSync.enabled = false
3. 无 NetworkIdentity
4. 性能最优，无网络开销
```

### 联机模式（一键切换）

```
1. 调用 NetworkModeManager.Instance.StartPVPHost()
2. 触发 NetworkModeChangedEvent { IsOnline = true }
3. 所有 NetworkSyncBase 子类自动启用
4. 自动添加 NetworkIdentity（如需要）
5. 启动 Mirror 网络
```

---

## 事件流程

### 切换到联机模式

```
用户操作 → StartPVPHost()
    ↓
设置 _isOnlineMode = true
    ↓
StartHost()
    ↓
触发 NetworkModeChangedEvent { IsOnline = true, IsHost = true }
    ↓
所有 NetworkSyncBase 组件收到事件
    ↓
调用 EnableNetworkSync()
    ↓
组件 enabled = true
    ↓
调用 OnNetworkSyncEnabled()（子类重写）
```

### 切换回单机模式

```
用户操作 → StopPVP()
    ↓
StopHost() / StopClient()
    ↓
设置 _isOnlineMode = false
    ↓
触发 NetworkModeChangedEvent { IsOnline = false }
    ↓
所有 NetworkSyncBase 组件收到事件
    ↓
调用 DisableNetworkSync()
    ↓
组件 enabled = false
```

---

## 快捷键（测试用）

| 按键 | 功能 |
|------|------|
| F1 | 切换网络模式（Host） |
| F2 | 生成测试单位 |
| F3 | 显示/隐藏调试信息 |

---

## 文件清单

### 网络管理
- `Network/NetworkModeManager.cs` - 网络模式管理器
- `Network/GameNetworkManager.cs` - 游戏网络管理器
- `Network/NetworkSyncBase.cs` - 网络同步基类

### 网络同步组件
- `Network/UnitNetworkSync.cs` - 单位网络同步
- `Network/SummonerNetworkSync.cs` - 召唤师网络同步
- `Network/BattleNetworkSync.cs` - 战斗管理器网络同步

### UI 和工具
- `UI/NetworkModeUI.cs` - 网络模式切换 UI
- `Utils/NetworkModeTestHelper.cs` - 测试助手

### 核心逻辑（单机）
- `Gameplay/Units/UnitEntity.cs` - 单位实体
- `Gameplay/Summoner/SummonerController.cs` - 召唤师控制器

---

## 优势

| 特性 | 重构前 | 重构后 |
|------|--------|--------|
| 单机性能 | 有网络开销 | 零网络开销 |
| 代码耦合 | 高 | 低 |
| 测试难度 | 复杂 | 简单 |
| 热重载 | 需要网络组件 | 完全支持 |
| 模式切换 | 需要重启 | 一键切换 |

---

## 注意事项

1. **Prefab 设置**：确保 NetworkSync 组件默认禁用
2. **事件监听**：NetworkSyncBase 会自动注册/注销事件监听
3. **NetworkIdentity**：联机模式会自动添加，无需手动设置
4. **热重载**：所有组件都支持 Fast Script Reload

---

## 扩展指南

### 添加新的网络同步组件

```csharp
public class MyNetworkSync : NetworkSyncBase
{
    private MyComponent myComponent;

    protected override void Awake()
    {
        base.Awake();
        myComponent = GetComponent<MyComponent>();
    }

    protected override void OnNetworkSyncEnabled()
    {
        // 初始化同步值
        if (isServer)
        {
            // 设置 SyncVar 初始值
        }
    }

    protected override void OnNetworkSyncDisabled()
    {
        // 清理工作
    }

    [SyncVar(hook = nameof(OnValueChanged))]
    private float syncedValue;

    [Server]
    public void ServerSyncValue(float value)
    {
        syncedValue = value;
    }

    private void OnValueChanged(float oldValue, float newValue)
    {
        if (!isServer)
        {
            // 客户端同步
        }
    }
}
```

---

**架构设计完成！** 🎯
