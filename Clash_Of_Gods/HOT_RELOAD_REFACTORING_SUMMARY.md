# 热重载重构总结

## 概述

本次重构对 `Assets/Scripts` 目录下的所有脚本进行了热重载兼容性改造，使项目在开发过程中可以充分利用 Fast Script Reload 的热重载功能。

## 重构日期
2025年11月

## 改造内容

### 1. 单例模式改造

所有使用单例模式的类都进行了以下改造：

#### 改造前
```csharp
public static MyManager Instance { get; private set; }

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
    }
}
```

#### 改造后
```csharp
private static MyManager _instance;

public static MyManager Instance
{
    get
    {
        if (_instance == null)
            _instance = FindObjectOfType<MyManager>();
        return _instance;
    }
}

private void Awake()
{
    if (_instance == null)
    {
        // 热重载兼容：使用类型转换避免类型不匹配
        _instance = (MyManager)(object)this;
        DontDestroyOnLoad(gameObject);
    }
    else if (!ReferenceEquals(_instance, this))
    {
        Destroy(gameObject);
    }
}

// 重置静态变量（编辑器重新进入Play模式时）
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    _instance = null;
}
```

### 2. 热重载回调方法

所有主要脚本都添加了热重载回调方法：

```csharp
#region Hot Reload Support

/// <summary>
/// 热重载回调 - 有实例访问权限
/// </summary>
void OnScriptHotReload()
{
    Debug.Log($"[HotReload] {GetType().Name} reloaded");
    // 重新初始化组件引用
    // 重新注册事件监听
    // 刷新状态
}

/// <summary>
/// 静态热重载回调 - 无需实例
/// </summary>
static void OnScriptHotReloadNoInstance()
{
    Debug.Log("[HotReload] Static reload");
}

#endregion
```

## 改造的脚本列表

### Core 层
| 脚本 | 改造内容 |
|------|----------|
| `Singleton.cs` | 基类支持热重载，添加 `ResetStatics` |
| `EventManager.cs` | 单例改造，保持事件字典状态 |
| `ConfigManager.cs` | 单例改造，支持配置重载 |
| `SaveSystem.cs` | 单例改造，保持缓存数据 |
| `GameLifecycleManager.cs` | 单例改造，保持游戏状态 |
| `Bootstrap.cs` | 添加热重载回调，验证系统状态 |

### Gameplay 层
| 脚本 | 改造内容 |
|------|----------|
| `BattleManager.cs` | 单例改造，重新注册事件监听 |
| `CardManager.cs` | 单例改造，保持手牌状态 |
| `MapManager.cs` | 单例改造，保持地图状态 |
| `UnitEntity.cs` | 添加热重载回调，重新绑定组件 |
| `UnitAIController.cs` | 添加热重载回调，重新扫描敌人 |
| `UnitAnimationController.cs` | 添加热重载回调，重新绑定Animator |
| `UnitBuffManager.cs` | 添加热重载回调，保持Buff状态 |
| `SummonerController.cs` | 添加热重载回调，保持运行时数据 |

### UI 层
| 脚本 | 改造内容 |
|------|----------|
| `UIManager.cs` | 单例改造，保持UI栈状态 |

### Utils 层
| 脚本 | 改造内容 |
|------|----------|
| `Singleton.cs` | 基类全面支持热重载 |
| `HotReloadHelper.cs` | 新增热重载辅助工具类 |

## 新增文件

### HotReloadHelper.cs
提供热重载相关的实用功能：
- `VerifyManagers()` - 验证所有管理器实例
- `GetSingletonSafe<T>()` - 安全获取单例
- `RebindComponents()` - 重新绑定组件引用
- `IHotReloadable` - 热重载接口

## 关键改造点

### 1. 类型转换解决 `this` 引用问题
```csharp
// 热重载后类型变为 MyClass__Patched_
// 使用 (object) 中间转换避免类型不匹配
_instance = (MyManager)(object)this;
```

### 2. ReferenceEquals 替代 != 比较
```csharp
// 使用 ReferenceEquals 进行引用比较
// 避免热重载后的类型比较问题
if (!ReferenceEquals(_instance, this))
{
    Destroy(gameObject);
}
```

### 3. RuntimeInitializeOnLoadMethod 重置静态变量
```csharp
// 编辑器重新进入 Play 模式时重置静态变量
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    _instance = null;
}
```

### 4. 事件监听重新注册
```csharp
void OnScriptHotReload()
{
    // 热重载后重新注册事件监听
    UnregisterEventListeners();
    RegisterEventListeners();
}
```

### 5. 组件引用重新绑定
```csharp
void OnScriptHotReload()
{
    // 热重载后重新获取组件引用
    if (Animator == null)
        Animator = GetComponent<Animator>();
}
```

## 使用指南

### 开发流程
1. 进入 Play 模式
2. 修改任意脚本（如调整伤害计算、AI行为等）
3. 保存文件 (Ctrl+S)
4. 等待 2-3 秒，观察控制台输出 "Hot-reload completed"
5. 立即测试修改效果

### 适合热重载的修改
- ✅ 调整数值（伤害、速度、范围等）
- ✅ 修改逻辑流程
- ✅ 添加 Debug.Log 调试输出
- ✅ 修复 Bug
- ✅ 调整 AI 行为
- ✅ 修改 UI 逻辑
- ✅ 修改私有方法

### 不适合热重载的修改
- ❌ 添加新的公共字段（实验性支持）
- ❌ 修改泛型类
- ❌ 修改网络同步代码（Mirror SyncVar/RPC）
- ❌ 大规模重构
- ❌ 添加新的公共方法
- ❌ 修改继承关系

## 注意事项

### Mirror 网络代码
Mirror 使用 IL 织入来调整类，热重载可能会有问题：
- 网络同步变量 (`[SyncVar]`) 的更改可能不会正确同步
- RPC 方法的更改可能需要重启
- 建议在 FSR 设置中启用 `Disable added/removed fields check`

### 泛型类
泛型类和方法不支持热重载，如需修改请停止 Play 模式重新编译。

### 调试
断点需要设置在生成的文件中，可以启用自动打开调试文件：
`Window → Fast Script Reload → Start Screen → Debugging → Auto open generated source file for debugging`

## 相关文档
- `FAST_SCRIPT_RELOAD_GUIDE.md` - 详细使用指南
- `Assets/Scripts/README.md` - 项目架构文档（已更新热重载章节）

## 验证方法

在 Play 模式下，可以使用以下方法验证热重载是否正常工作：

```csharp
// 在任意脚本中调用
HotReloadHelper.VerifyManagers();
```

或者在 Bootstrap 组件上右键选择 "Verify All Systems"。
