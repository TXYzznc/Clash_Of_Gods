# Fast Script Reload 热重载开发指南

## 📖 概述

Fast Script Reload (FSR) 是一个 Unity 热重载工具，允许你在 Play 模式下修改代码并立即看到效果，无需停止游戏重新编译。

**当前版本**: 1.8.0  
**包来源**: `com.handzlikchris.fastscriptreload`  
**官方文档**: https://fastscriptreload.com

---

## 🚀 快速开始

### 1. 基本使用流程

```
1. 进入 Play 模式
2. 修改任意 .cs 脚本文件
3. 保存文件 (Ctrl+S)
4. 等待 2-3 秒，自动热重载
5. 立即看到效果！
```

### 2. 首次配置

1. 打开 Unity 后，欢迎窗口会自动弹出
2. 也可以通过菜单打开：`Window → Fast/Live Script Reload → Start Screen`
3. 按照欢迎窗口的指引完成配置
4. 打开示例场景测试：`Assets/Samples/Fast Script Reload/1.8.0/Basic Example/Scenes/ExampleScene.unity`

### 3. 重要设置：禁用 Unity 自动刷新

为了让 FSR 正常工作，**必须**调整 Unity 的自动刷新设置：

**方法 1 (推荐)**: 
```
Edit → Preferences → Asset Pipeline → Auto Refresh → Disabled 或 Enabled Outside Playmode
```

**方法 2**: 
```
Edit → Preferences → General → Script Changes While Playing → Recompile After Finished Playing
```

> ⚠️ **重要**: 如果 Unity 仍然在 Play 模式下显示 "Reloading script assemblies" 进度条，请启用：
> `Window → Fast Script Reload → Start Screen → Reload → Force prevent assembly reload during playmode`

---

## 🎯 核心功能

### 1. 自动热重载

默认情况下，FSR 会自动检测所有 .cs 文件的更改并热重载。更改会每 3 秒批量处理一次。

**手动控制热重载**:
1. 在 `Options → Reload` 页面取消勾选 `Enable auto Hot-Reload for changed files`
2. 使用 `Window → Fast Script Reload → Force Reload` 手动触发
3. 或在代码中调用：
```csharp
FastScriptReloadManager.TriggerReloadForChangedFiles();
```

### 2. 热重载回调方法

在脚本中添加特殊方法，可以在热重载时执行自定义逻辑：

```csharp
using UnityEngine;

public class MyScript : MonoBehaviour
{
    [SerializeField] private int resolution = 10;
    
    // 热重载时调用（有实例访问权限）
    void OnScriptHotReload()
    {
        // 可以通过 'this' 访问当前实例
        Debug.Log($"脚本已热重载！resolution 值为: {resolution}");
        
        // 适用场景：
        // - 重新初始化某些状态
        // - 刷新缓存数据
        // - 重新绑定事件
    }
    
    // 热重载时调用（静态方法，无实例）
    static void OnScriptHotReloadNoInstance()
    {
        Debug.Log("脚本已热重载（静态方法）");
        
        // 适用场景：
        // - 新添加的类型初始化
        // - 不需要实例的全局操作
        // - 重新加载场景
        // - 调用测试函数
    }
}
```

### 3. 动态添加新类型

当你在 Play 模式下添加一个全新的类时，可以使用特殊的回调方法：

```csharp
// 在 Play 模式下取消注释这个类来测试
public class NewMonoBehaviourTest : MonoBehaviour
{
    // 当这个新类型被添加到程序集时调用
    static void OnScriptHotReloadNewTypeAdded()
    {
        var go = new GameObject("TestDynamic");
        go.AddComponent<NewMonoBehaviourTest>();
    }

    void Start()
    {
        Debug.Log("新类型已创建！");
        GameObject.CreatePrimitive(PrimitiveType.Cube);
    }
    
    void OnScriptHotReload()
    {
        Debug.Log("新类型脚本热重载完成");
    }
}
```

---

## 🧪 实验性功能

### 1. 运行时添加新字段

**启用方法**:
```
Window → Fast Script Reload → Start Screen → New Fields → Enable experimental added field support
```

**使用示例** (来自官方示例 Graph.cs):
```csharp
public class Graph : MonoBehaviour
{
    [SerializeField] private int resolution = 10;
    
    // 实验性功能：在 Play 模式下取消注释以添加新字段
    // [SerializeField] [Range(-3, 3)] private float _testUMove = 0f;
    
    void Update()
    {
        // 使用新字段
        // var u = (x + 0.5f) * step - 1f + _testUMove;
    }
}
```

**限制**:
- 外部类无法调用运行时添加的新字段
- 新字段只有在被使用后才会在 Inspector 中显示
- 会有轻微的性能开销（使用动态字典查找）

### 2. 编辑器外热重载（非 Play 模式）

**启用方法**:
```
Window → Fast Script Reload → Start Screen → Editor Hot-Reload → Enable Hot-Reload outside of play mode
```

> ⚠️ 这是实验性功能，主要用于编辑器脚本的迭代开发，不是 Unity 编译机制的替代品。

---

## 🐛 调试支持

### 1. 基本调试

调试完全支持，但断点需要设置在**生成的文件**中，而不是原始文件。

**自动打开调试文件**:
```
Window → Fast Script Reload → Start Screen → Debugging → Auto open generated source file for debugging
```

热重载后，控制台会显示可点击的链接，点击即可打开生成的源文件设置断点。

### 2. 函数断点

如果普通断点不生效，可以尝试设置函数断点：

| 原始类型 | 函数断点类型名 | 函数名 |
|---------|--------------|--------|
| `MyClass` | `MyClass__Patched_` | 保持不变 |
| `Graph` | `Graph__Patched_` | `Update` |

> ⚠️ **Rider 用户注意**: Unity 2019 和 2020 版本的 Rider 调试有一些问题，只能使用自动打开功能打开可调试文件。

---

## ⚠️ 重要限制

### 1. 泛型不支持

```csharp
// ❌ 泛型类和方法不会被热重载
public class GenericClass<T>
{
    public void GenericMethod<U>(U arg) { }  // 不会热重载
}

// ✅ 继承自泛型基类的非泛型类可以热重载
public class ConcreteClass : GenericBase<int>
{
    public void NonGenericMethod() { }  // 可以热重载
    
    public void GenericMethod<T>(T arg) { }  // 不会热重载
}
```

### 2. `this` 引用问题

当传递 `this` 给期望具体类型的方法时可能出错：

```csharp
// ❌ 可能出错（热重载后类型变为 EnemyController__Patched_）
public class EnemyController : MonoBehaviour
{
    EnemyManager m_EnemyManager;
    
    void Start()
    {
        m_EnemyManager.RegisterEnemy(this);  // 类型不匹配
    }
}

public class EnemyManager : MonoBehaviour
{
    public void RegisterEnemy(EnemyController enemy) { }  // 期望具体类型
}

// ✅ 解决方案 1：使用接口
public interface IRegistrableEnemy { }

public class EnemyController : MonoBehaviour, IRegistrableEnemy
{
    void Start()
    {
        m_EnemyManager.RegisterEnemy(this);  // 使用接口类型
    }
}

public class EnemyManager : MonoBehaviour
{
    public void RegisterEnemy(IRegistrableEnemy enemy) { }  // 使用接口
}

// ✅ 解决方案 2：使用公共基类
public class EnemyManager : MonoBehaviour
{
    public void RegisterEnemy(MonoBehaviour enemy) { }  // 使用 MonoBehaviour
}
```

> 💡 默认情况下，实验性设置 `Enable method calls with 'this' as argument fix` 已开启，可以自动修复大部分 `this` 问题。

### 3. 单例模式问题

```csharp
// ❌ 可能出错
public class MySingleton : MonoBehaviour
{
    public static MySingleton Instance;
    
    void Start()
    {
        Instance = this;  // 类型不匹配
    }
}

// ✅ 解决方案：使用类型转换
public class MySingleton : MonoBehaviour
{
    public static MySingleton Instance;
    
    void Start()
    {
        Instance = (MySingleton)(object)this;  // 强制转换
    }
}
```

### 4. 完整限制列表

| 限制 | 说明 | 解决方案 |
|------|------|----------|
| 泛型类/方法 | 不支持热重载 | 移动代码到非泛型类/方法 |
| 新增公共方法 | 只有私有方法有效 | 先添加为私有方法 |
| 内部类访问 | 无法访问其他程序集的 internal 成员 | 使用 User Script Override 或改为 public |
| 嵌套类 | 大量嵌套类可能导致编译错误 | 将嵌套类移到顶层 |
| 扩展方法 | 传递 `this` 的扩展方法可能出错 | 改为实例方法 |
| IL2CPP | 不支持 IL2CPP 构建 | 开发时使用 Mono |
| 部分类 | 实验性支持，文件名需匹配类名 | 确保文件名包含类名 |
| private protected | 无法访问 | 改为 protected |

---

## 🔧 高级配置

### 1. 文件排除

**通过右键菜单**:
1. 右键点击 .cs 文件
2. 选择 `Fast Script Reload → Add Hot-Reload Exclusion`

**通过属性**:
```csharp
using FastScriptReload.Runtime;

[PreventHotReload]
public class MyClass : MonoBehaviour
{
    // 这个类不会被热重载
}
```

**查看所有排除**:
```
右键 .cs 文件 → Fast Script Reload → Show Exclusions
```

### 2. 监视特定文件夹

1. 在 `Reload` 选项卡勾选 `Specify watched folders/files manually`
2. 右键点击文件夹，选择 `Fast Script Reload → Watch File / Folder`

### 3. 用户脚本重写覆盖 (User Script Rewrite Override)

当遇到编译问题时，可以创建自定义覆盖：

1. 右键点击问题文件
2. 选择 `Fast Script Reload → Add / Open User Script Rewrite Override`
3. 在覆盖文件中修复问题

**示例 - 修复单例问题**:
```csharp
// 覆盖文件内容
public class MySingleton__Patched_ : MonoBehaviour
{   
    void Start()
    {
        // 修复后的代码
        Instance = (MySingleton)(object)this;
    }
}
```

**示例 - 添加内部接口**:
```csharp
// 当原始代码实现了 internal 接口时
// 在覆盖文件中重新定义该接口
interface IInterface
{
    // 添加需要的定义
}
```

### 4. 文件监视器实现

如果默认文件监视器有问题，可以切换实现：
```
Window → Fast Script Reload → Start Screen → File Watchers → File Watcher implementation
```

| 实现 | 说明 |
|------|------|
| DefaultUnity | 默认，某些版本可能较慢 |
| DirectWindowsApi | 实验性，更快，不支持符号链接 |
| CustomPolling | 实验性，手动轮询，最慢 |

### 5. 引用排除

如果遇到 "Type XYZ is defined in both assembly a.dll and b.dll" 错误：
```
Start Screen → Exclude References (Advanced) → 添加需要排除的引用
```

---

## 📋 《诸神对决》项目最佳实践

### 1. 推荐的热重载友好代码模式

```csharp
// ✅ 使用接口而非具体类型
public interface IUnit
{
    void TakeDamage(float damage);
    void OnDeath();
}

public class UnitEntity : MonoBehaviour, IUnit
{
    public void TakeDamage(float damage) { }
    public void OnDeath() { }
}

// ✅ 使用事件系统而非直接引用
public class BattleManager : MonoBehaviour
{
    void OnEnable()
    {
        EventManager.Subscribe<UnitDeathEvent>(OnUnitDeath);
    }
    
    void OnDisable()
    {
        EventManager.Unsubscribe<UnitDeathEvent>(OnUnitDeath);
    }
    
    void OnUnitDeath(UnitDeathEvent e)
    {
        // 通过事件系统解耦
    }
}
```

### 2. 热重载回调示例

```csharp
public class SummonerController : MonoBehaviour
{
    void OnScriptHotReload()
    {
        Debug.Log($"[SummonerController] 热重载完成！当前HP: {RuntimeData.CurrentHP}");
        // 重新初始化状态
        RefreshUI();
    }
}

public class BattleManager : MonoBehaviour
{
    void OnScriptHotReload()
    {
        Debug.Log($"[BattleManager] 热重载完成！当前阶段: {CurrentPhase}");
        // 刷新战斗状态
        RecalculateBattleState();
    }
}

public class UnitAIController : MonoBehaviour
{
    void OnScriptHotReload()
    {
        // 热重载后重新扫描敌人
        ScanForEnemies();
        Debug.Log($"[AI] 热重载后重新索敌，当前目标: {CurrentTarget?.name ?? "无"}");
    }
}
```

### 3. 需要注意的类

| 类名 | 注意事项 |
|------|----------|
| `GameLifecycleManager` | 继承 NetworkManager，网络相关代码可能需要重启 |
| `EventManager` | 单例模式，使用 `(EventManager)(object)this` 修复 |
| `SaveSystem` | 单例模式，注意 Instance 引用 |
| `ConfigManager` | 单例模式，配置数据可能需要重新加载 |

### 4. Mirror 网络代码注意事项

Mirror 使用 IL 织入来调整类，热重载可能会有问题：
- 网络同步变量 (`[SyncVar]`) 的更改可能不会正确同步
- RPC 方法的更改可能需要重启
- 建议在 `Reload` 选项中启用 `Disable added/removed fields check`

---

## 🎮 开发工作流建议

### 1. 日常开发流程

```
1. 打开 Unity，进入 Play 模式
2. 修改代码（如调整伤害计算、AI行为等）
3. 保存文件 (Ctrl+S)，等待 2-3 秒
4. 观察控制台确认 "Hot-reload completed"
5. 测试修改效果
6. 重复步骤 2-5
```

### 2. 适合热重载的修改

- ✅ 调整数值（伤害、速度、范围等）
- ✅ 修改逻辑流程
- ✅ 添加 Debug.Log 调试输出
- ✅ 修复 Bug
- ✅ 调整 AI 行为
- ✅ 修改 UI 逻辑
- ✅ 修改私有方法

### 3. 不适合热重载的修改

- ❌ 添加新的公共字段（实验性支持）
- ❌ 修改泛型类
- ❌ 修改网络同步代码（Mirror SyncVar/RPC）
- ❌ 大规模重构
- ❌ 添加新的公共方法
- ❌ 修改继承关系

### 4. 遇到问题时

1. 查看控制台错误信息
2. 检查是否触及限制（泛型、this 引用等）
3. 尝试使用 User Script Override
4. 如果无法解决，按 `Ctrl+R` 触发完整重新编译
5. 或停止 Play 模式重新编译

---

## 📊 性能说明

### 内存影响
- 每次热重载会增加少量内存（新编译的程序集）
- 除非在同一 Play 会话中进行数百次修改，否则影响可忽略
- 应用性能不会受到有意义的影响

### 文件监视开销
如果遇到性能问题：
1. 打开 `Window → Fast Script Reload → File Watcher (Advanced Setup)`
2. 缩小监视范围到特定脚本文件夹
3. 可以监视多个文件夹

---

## 🔗 常用菜单路径

| 功能 | 菜单路径 |
|------|----------|
| 开始界面 | `Window → Fast/Live Script Reload → Start Screen` |
| 强制重载 | `Window → Fast Script Reload → Force Reload` |
| 添加排除 | 右键 .cs 文件 → `Fast Script Reload → Add Hot-Reload Exclusion` |
| 监视文件夹 | 右键文件夹 → `Fast Script Reload → Watch File / Folder` |
| 脚本覆盖 | 右键 .cs 文件 → `Fast Script Reload → Add / Open User Script Rewrite Override` |
| 查看排除列表 | 右键 .cs 文件 → `Fast Script Reload → Show Exclusions` |
| 查看覆盖列表 | 右键 .cs 文件 → `Fast Script Reload → Show User Script Rewrite Overrides` |

---

## 📚 参考资源

- **官方文档**: https://fastscriptreload.com/projects/fast-script-reload/documentation
- **技术博客**: https://immersivevrtools.com/Blog/how-to-build-hot-reload-functionality-for-unity
- **示例场景**: `Assets/Samples/Fast Script Reload/1.8.0/Basic Example/Scenes/ExampleScene.unity`
- **示例脚本**: 
  - `Assets/Samples/Fast Script Reload/1.8.0/Basic Example/Scripts/Graph.cs`
  - `Assets/Samples/Fast Script Reload/1.8.0/Basic Example/Scripts/FunctionLibrary.cs`

---

## 🔄 Live Script Reload 扩展

如果需要在设备构建中使用热重载（如 Android、Quest 2、Windows 独立版），可以考虑 Live Script Reload 扩展：

- 包含 Fast Script Reload 的所有功能
- 支持通过网络在设备上热重载
- 支持 Android（包括 VR 头显）和 Windows 独立版

[了解更多](https://immersivevrtools.com/redirect/fast-script-reload/live-script-reload-extension)

---

## ✨ 总结

Fast Script Reload 是一个强大的开发工具，可以显著提高迭代速度。在《诸神对决》项目中：

1. **日常开发**: 直接使用，大部分代码修改都能热重载
2. **遇到问题**: 查看限制列表，使用 User Script Override
3. **最佳实践**: 使用接口、事件系统，添加热重载回调
4. **性能**: 影响可忽略，放心使用

**记住**: 热重载是开发工具，最终发布时代码会正常编译，不会有任何影响！

---

**文档版本**: 2.0  
**适用 FSR 版本**: 1.8.0  
**最后更新**: 2025年11月
