# Fast Script Reload 热重载测试指南

## 测试目标
验证 Fast Script Reload 是否正常工作，能否在 Play 模式下修改代码并立即看到效果。

## 测试环境准备

### 1. 确认 Fast Script Reload 已安装
- 打开 Unity
- 检查菜单栏是否有 `Window → Fast/Live Script Reload`
- 如果没有，说明插件未正确安装

### 2. 配置 Unity 自动刷新设置
**必须完成此步骤，否则热重载不会工作！**

**方法 1（推荐）**:
```
Edit → Preferences → Asset Pipeline → Auto Refresh → Enabled Outside Playmode
```

**方法 2**:
```
Edit → Preferences → General → Script Changes While Playing → Recompile After Finished Playing
```

### 3. 打开 Fast Script Reload 设置
```
Window → Fast/Live Script Reload → Start Screen
```

确认以下设置：
- ✅ `Enable auto Hot-Reload for changed files` 已勾选
- ✅ `Batch reload interval` 设置为 3 秒（默认）

---

## 测试流程：UnitEntity 热重载测试

### 准备阶段

#### 1. 创建测试场景
1. 创建新场景或使用现有场景
2. 在场景中创建一个空物体，命名为 `TestUnit`
3. 添加 `UnitEntity` 组件到 `TestUnit` 上

#### 2. 配置 UnitEntity
在 Inspector 中设置：
- `Enable Debug Logs`: ✅ 勾选
- `Test Damage Multiplier`: `1.0`
- `Test Message`: `"Original Message"`
- `Test Color`: 红色 (Red)

#### 3. 创建简单的 UnitConfig（可选）
如果没有 UnitConfig，可以跳过此步骤，测试代码会处理 null 情况。

---

### 测试步骤

#### 测试 1：验证基础功能

**步骤**:
1. 点击 Unity 的 **Play** 按钮进入 Play 模式
2. 观察控制台，应该看到 UnitEntity 初始化的日志
3. 按下 **空格键**
4. 观察控制台输出

**预期结果**:
```
=== [Hot Reload Test] ===
Unit Name: No Config
HP: 0/0
Test Damage Multiplier: 1
Test Message: Original Message
Test Color: RGBA(1.000, 0.000, 0.000, 1.000)
Is Alive: True
========================
```

---

#### 测试 2：热重载修改数值

**步骤**:
1. **保持 Play 模式运行**（不要停止）
2. 打开 `UnitEntity.cs` 脚本
3. 找到 `TestHotReload()` 方法
4. 修改输出内容，例如：
```csharp
private void TestHotReload()
{
    Debug.Log("=== [Hot Reload Test - MODIFIED!] ===");  // 修改这行
    Debug.Log($"Unit Name: {Config?.UnitName ?? "No Config"}");
    Debug.Log($"HP: {Stats?.CurrentHP ?? 0}/{Stats?.MaxHP ?? 0}");
    Debug.Log($"Test Damage Multiplier: {TestDamageMultiplier} (x2 = {TestDamageMultiplier * 2})");  // 修改这行
    Debug.Log($"Test Message: {TestMessage}");
    Debug.Log($"Test Color: {TestColor}");
    Debug.Log($"Is Alive: {IsAlive}");
    Debug.Log("=== END OF TEST ===");  // 添加这行
}
```
5. **保存文件** (Ctrl+S)
6. 等待 2-3 秒，观察控制台

**预期结果**:
控制台应该显示：
```
[HotReload] UnitEntity reloaded. Unit: Unknown: HP=0/0, Alive=True
Hot-reload completed (took XXms)
```

7. 再次按下 **空格键**

**预期结果**:
```
=== [Hot Reload Test - MODIFIED!] ===
Unit Name: No Config
HP: 0/0
Test Damage Multiplier: 1 (x2 = 2)
Test Message: Original Message
Test Color: RGBA(1.000, 0.000, 0.000, 1.000)
Is Alive: True
=== END OF TEST ===
```

✅ **如果看到修改后的输出，说明热重载成功！**

---

#### 测试 3：热重载修改逻辑

**步骤**:
1. **保持 Play 模式运行**
2. 修改 `TestHotReload()` 方法，添加新的逻辑：
```csharp
private void TestHotReload()
{
    Debug.Log("=== [Hot Reload Test] ===");
    Debug.Log($"Unit Name: {Config?.UnitName ?? "No Config"}");
    Debug.Log($"HP: {Stats?.CurrentHP ?? 0}/{Stats?.MaxHP ?? 0}");
    Debug.Log($"Test Damage Multiplier: {TestDamageMultiplier}");
    Debug.Log($"Test Message: {TestMessage}");
    Debug.Log($"Test Color: {TestColor}");
    Debug.Log($"Is Alive: {IsAlive}");
    
    // 新增逻辑：根据伤害倍率显示不同消息
    if (TestDamageMultiplier > 2.0f)
    {
        Debug.Log("⚔️ HIGH DAMAGE MODE!");
    }
    else if (TestDamageMultiplier > 1.0f)
    {
        Debug.Log("⚡ NORMAL DAMAGE MODE");
    }
    else
    {
        Debug.Log("🛡️ LOW DAMAGE MODE");
    }
    
    Debug.Log("========================");
}
```
3. **保存文件** (Ctrl+S)
4. 等待热重载完成
5. 按下 **空格键**

**预期结果**:
```
=== [Hot Reload Test] ===
...
🛡️ LOW DAMAGE MODE
========================
```

6. 在 Inspector 中修改 `Test Damage Multiplier` 为 `1.5`
7. 再次按下 **空格键**

**预期结果**:
```
⚡ NORMAL DAMAGE MODE
```

8. 修改为 `2.5`，再次测试

**预期结果**:
```
⚔️ HIGH DAMAGE MODE!
```

✅ **如果逻辑正确执行，说明热重载完全正常！**

---

#### 测试 4：热重载回调测试

**步骤**:
1. **保持 Play 模式运行**
2. 修改 `OnScriptHotReload()` 方法：
```csharp
void OnScriptHotReload()
{
    Debug.Log($"[HotReload] UnitEntity reloaded! {GetStatusSummary()}");
    Debug.Log($"[HotReload] Test Multiplier: {TestDamageMultiplier}");
    Debug.Log($"[HotReload] Test Message: {TestMessage}");
    
    // 重新初始化组件引用
    InitializeComponents();
}
```
3. **保存文件** (Ctrl+S)
4. 等待热重载完成

**预期结果**:
控制台应该显示：
```
[HotReload] UnitEntity reloaded! Unknown: HP=0/0, Alive=True
[HotReload] Test Multiplier: 1
[HotReload] Test Message: Original Message
Hot-reload completed (took XXms)
```

✅ **如果看到自定义的热重载回调输出，说明回调机制正常！**

---

#### 测试 5：修改 Inspector 可见字段

**步骤**:
1. **保持 Play 模式运行**
2. 在 Inspector 中修改以下值：
   - `Test Damage Multiplier`: `3.0`
   - `Test Message`: `"Modified in Inspector"`
   - `Test Color`: 蓝色 (Blue)
3. 按下 **空格键**

**预期结果**:
```
=== [Hot Reload Test] ===
...
Test Damage Multiplier: 3
Test Message: Modified in Inspector
Test Color: RGBA(0.000, 0.000, 1.000, 1.000)
...
⚔️ HIGH DAMAGE MODE!
========================
```

4. 现在修改代码，添加新的输出：
```csharp
private void TestHotReload()
{
    Debug.Log("=== [Hot Reload Test] ===");
    Debug.Log($"Test Message: {TestMessage} (Length: {TestMessage.Length})");  // 添加长度
    // ... 其他代码
}
```
5. **保存文件** (Ctrl+S)
6. 等待热重载
7. 按下 **空格键**

**预期结果**:
```
Test Message: Modified in Inspector (Length: 22)
```

✅ **Inspector 中修改的值在热重载后仍然保持，说明状态保持正常！**

---

## 常见问题排查

### 问题 1：热重载没有触发

**症状**: 保存文件后没有看到 "Hot-reload completed" 消息

**解决方案**:
1. 检查 Unity 自动刷新设置（见准备阶段第2步）
2. 检查 Fast Script Reload 是否启用：
   ```
   Window → Fast Script Reload → Start Screen → Reload → Enable auto Hot-Reload for changed files
   ```
3. 尝试手动触发：
   ```
   Window → Fast Script Reload → Force Reload
   ```

### 问题 2：修改后代码没有生效

**症状**: 热重载完成了，但代码修改没有生效

**解决方案**:
1. 检查是否修改了泛型方法（不支持热重载）
2. 检查是否添加了新的公共字段（需要实验性支持）
3. 尝试停止 Play 模式，重新进入

### 问题 3：控制台显示编译错误

**症状**: 保存后显示编译错误

**解决方案**:
1. 检查代码语法是否正确
2. 检查是否有类型不匹配问题
3. 如果是 `this` 引用问题，参考 `FAST_SCRIPT_RELOAD_GUIDE.md` 的解决方案

### 问题 4：Unity 仍然触发完整重新编译

**症状**: 保存后看到 "Reloading script assemblies" 进度条

**解决方案**:
启用强制防止重新加载：
```
Window → Fast Script Reload → Start Screen → Reload → Force prevent assembly reload during playmode
```

---

## 测试成功标准

完成以上所有测试后，如果满足以下条件，说明热重载配置成功：

- ✅ 修改代码后 2-3 秒内看到 "Hot-reload completed" 消息
- ✅ 修改的代码逻辑立即生效
- ✅ Inspector 中修改的值在热重载后保持不变
- ✅ 热重载回调方法正确执行
- ✅ 没有触发 Unity 的完整重新编译

---

## 清理测试代码

测试完成后，可以移除测试代码：

1. 删除 `UnitEntity.cs` 中的以下内容：
```csharp
[Header("Hot Reload Test")]
public float TestDamageMultiplier = 1.0f;
public string TestMessage = "Original Message";
public Color TestColor = Color.red;

private void Update()
{
    if (Input.GetKeyDown(KeyCode.Space))
    {
        TestHotReload();
    }
}

private void TestHotReload()
{
    // ... 整个方法
}
```

2. 保存文件

---

## 下一步

热重载测试成功后，你可以：

1. 在实际开发中使用热重载功能
2. 为其他脚本添加热重载回调
3. 参考 `FAST_SCRIPT_RELOAD_GUIDE.md` 了解更多高级用法
4. 查看 `HOT_RELOAD_REFACTORING_SUMMARY.md` 了解项目的热重载改造细节

---

**祝你开发愉快！🚀**
