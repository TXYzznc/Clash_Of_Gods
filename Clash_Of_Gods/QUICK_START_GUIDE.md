# 诸神对决 - 快速开始指南

## 🎯 项目状态
✅ **框架搭建完成** - 30个核心脚本已创建，可以开始具体实现

## 📋 前置要求

### Unity版本
- Unity 2021.3 LTS 或更高版本
- 支持 C# 9.0

### 必需插件
1. **Mirror Networking** (网络同步)
   - 安装方式: Package Manager -> Add package from git URL
   - URL: `https://github.com/vis2k/Mirror.git`

2. **TextMeshPro** (UI文本)
   - Unity内置，首次使用时会自动导入

3. **NavMesh Components** (导航网格)
   - Package Manager -> Unity Registry -> AI Navigation

## 🚀 快速启动步骤

### 步骤1: 创建启动场景

1. 创建新场景 `Boot.unity`
2. 创建空物体命名为 `Bootstrap`
3. 添加 `Bootstrap.cs` 脚本
4. 保存场景到 `Assets/Scenes/Boot.unity`

### 步骤2: 创建Manager预制体

#### 创建 GameLifecycleManager 预制体
```
1. 创建空物体 "GameLifecycleManager"
2. 添加组件: GameLifecycleManager.cs
3. 添加组件: NetworkManager (Mirror)
4. 保存为预制体到 Assets/Prefabs/System/
5. 在Bootstrap中引用此预制体
```

#### 创建其他Manager预制体（可选）
```
- EventManager
- SaveSystem  
- ConfigManager
```

### 步骤3: 创建配置资源

#### 创建单位配置
```
1. 右键 -> Create -> ClashOfGods -> Configs -> Unit Config
2. 设置单位属性（ID、名称、基础属性等）
3. 保存到 Assets/Configs/Units/
```

#### 创建卡牌配置
```
1. 右键 -> Create -> ClashOfGods -> Configs -> Card Config
2. 设置卡牌属性（ID、名称、效果等）
3. 保存到 Assets/Configs/Cards/
```

#### 创建技能配置
```
1. 右键 -> Create -> ClashOfGods -> Configs -> Skill Config
2. 设置技能属性（ID、名称、冷却等）
3. 保存到 Assets/Configs/Skills/
```

### 步骤4: 配置ConfigManager

1. 选中ConfigManager预制体
2. 在Inspector中：
   - 将创建的UnitConfig拖入 `Unit Configs` 数组
   - 将创建的CardConfig拖入 `Card Configs` 数组
   - 将创建的SkillConfig拖入 `Skill Configs` 数组

### 步骤5: 创建玩家预制体

#### 创建召唤师预制体
```
1. 创建空物体 "Summoner"
2. 添加组件:
   - SummonerController.cs
   - SummonerSkillSystem.cs
   - CharacterController (Unity内置)
3. 选择职业，添加对应脚本:
   - BerserkerClass.cs (狂战)
   - WarlockClass.cs (术士)
   - ChaosClass.cs (混沌)
   - DruidClass.cs (德鲁伊)
4. 添加Tag: "Player"
5. 保存为预制体到 Assets/Prefabs/Player/
```

### 步骤6: 创建单位预制体

#### 创建战斗单位预制体
```
1. 创建空物体 "Unit_Example"
2. 添加组件:
   - UnitEntity.cs
   - UnitAIController.cs
   - UnitAnimationController.cs
   - UnitBuffManager.cs
   - NavMeshAgent (Unity内置)
   - Animator (Unity内置)
3. 配置NavMeshAgent:
   - Speed: 3.5
   - Angular Speed: 120
   - Acceleration: 8
4. 保存为预制体到 Assets/Prefabs/Units/
```

### 步骤7: 创建测试场景

#### 创建探索场景
```
1. 创建新场景 "TestExploration.unity"
2. 添加地面 (Plane)
3. 添加 MapManager 空物体
   - 添加 MapManager.cs 脚本
4. 添加 NavMeshSurface 组件到地面
   - 点击 "Bake" 烘焙导航网格
5. 实例化 Summoner 预制体
6. 添加几个敌人（EnemyOverworldController）
7. 添加事件点（MapEventPoint）
```

#### 创建战斗场景
```
1. 创建新场景 "TestCombat.unity"
2. 添加地面 (Plane)
3. 添加 BattleManager 空物体
   - 添加 BattleManager.cs 脚本
4. 添加 CardManager 空物体
   - 添加 CardManager.cs 脚本
5. 创建玩家部署区域（空物体）
6. 创建敌人生成区域（空物体）
7. 在BattleManager中引用这两个区域
```

## 🎮 运行测试

### 测试1: 系统初始化
```
1. 打开 Boot.unity 场景
2. 点击 Play
3. 查看Console，应该看到:
   === [Bootstrap] Starting System Initialization ===
   [Bootstrap] ✓ EventManager initialized
   [Bootstrap] ✓ ConfigManager initialized
   [Bootstrap] ✓ SaveSystem initialized
   [Bootstrap] ✓ GameLifecycleManager initialized
   === [Bootstrap] All Systems Initialized Successfully ===
```

### 测试2: 探索系统
```
1. 打开 TestExploration.unity 场景
2. 点击 Play
3. 使用WASD或点击地面移动召唤师
4. 接近敌人观察警戒/追击行为
5. 接近事件点按E键交互
```

### 测试3: 战斗系统
```
1. 打开 TestCombat.unity 场景
2. 点击 Play
3. 在部署阶段放置召唤物
4. 点击开始战斗
5. 观察自动战斗和AI行为
```

## 📝 开发检查清单

### 核心系统 ✅
- [x] 游戏生命周期管理
- [x] 事件系统
- [x] 存档系统
- [x] 配置管理

### 召唤师系统 ✅
- [x] 召唤师控制器
- [x] 技能系统
- [x] 四大职业实现

### 单位系统 ✅
- [x] 单位实体
- [x] AI控制器
- [x] 动画控制器
- [x] Buff管理器

### 战斗系统 ✅
- [x] 战斗管理器
- [x] 卡牌管理器

### 地图系统 ✅
- [x] 地图管理器
- [x] 事件点系统
- [x] 敌人大地图AI

### 网络系统 ✅
- [x] 网络管理器
- [x] 时间同步

### UI系统 ✅
- [x] UI管理器
- [x] 战斗UI
- [x] 探索UI

### 工具类 ✅
- [x] 单例模式
- [x] 对象池

## 🔧 常见问题

### Q: Mirror插件报错
**A**: 确保已正确安装Mirror插件，并且Unity版本支持。可以尝试：
```
1. Window -> Package Manager
2. 点击 "+" -> Add package from git URL
3. 输入: https://github.com/vis2k/Mirror.git
```

### Q: NavMesh无法烘焙
**A**: 确保已安装AI Navigation包：
```
1. Window -> Package Manager
2. Unity Registry -> 搜索 "AI Navigation"
3. 点击 Install
```

### Q: 脚本编译错误
**A**: 检查以下几点：
1. 所有脚本的命名空间是否正确
2. Mirror插件是否正确安装
3. Unity版本是否支持C# 9.0

### Q: 找不到某个脚本
**A**: 确保脚本路径正确：
```
Assets/Scripts/
├── Core/
├── Data/
├── Gameplay/
│   ├── Summoner/
│   │   └── Classes/
│   ├── Units/
│   ├── Combat/
│   └── Map/
├── Network/
├── UI/
└── Utils/
```

## 📚 下一步学习

### 推荐阅读顺序
1. `README.md` - 详细架构文档
2. `ARCHITECTURE_OVERVIEW.md` - 架构总览
3. 各个脚本的注释 - 了解具体实现

### 推荐实现顺序
1. **第一周**: 实现基础战斗流程
   - 单位自动攻击
   - 伤害计算
   - 胜负判定

2. **第二周**: 实现探索系统
   - 玩家移动
   - 敌人AI
   - 事件触发

3. **第三周**: 实现卡牌系统
   - 手牌管理
   - 卡牌释放
   - 效果执行

4. **第四周**: 实现职业技能
   - 被动技能
   - 主动技能
   - 进阶系统

## 🎨 美术资源建议

### 临时测试资源
- 使用Unity基础几何体（Cube、Sphere、Capsule）
- 使用Unity默认材质
- 使用简单的颜色区分敌我

### 正式美术资源
- Unity Asset Store
- AI生成（Stable Diffusion、Midjourney）
- 购买可商用资源包
- 使用ProBuilder绘制地图

## 💡 开发技巧

### 1. 使用Debug.Log追踪流程
```csharp
Debug.Log($"[ClassName] Method called with param: {value}");
```

### 2. 使用Gizmos可视化
```csharp
private void OnDrawGizmosSelected()
{
    Gizmos.color = Color.red;
    Gizmos.DrawWireSphere(transform.position, attackRange);
}
```

### 3. 使用ContextMenu快速测试
```csharp
[ContextMenu("Test Function")]
private void TestFunction()
{
    // 测试代码
}
```

### 4. 使用Inspector调试
- 将需要观察的变量设为public或添加[SerializeField]
- 运行时可以在Inspector中实时查看和修改

## 🤝 获取帮助

### 文档资源
- Unity官方文档: https://docs.unity3d.com/
- Mirror文档: https://mirror-networking.gitbook.io/
- C#文档: https://docs.microsoft.com/zh-cn/dotnet/csharp/

### 社区资源
- Unity中文社区
- Mirror Discord
- GitHub Issues

## ✨ 总结

恭喜！你已经完成了《诸神对决》的框架搭建。现在可以开始具体实现各个系统的功能了。

**记住**: 
- 先实现核心玩法循环
- 逐步添加功能
- 经常测试和迭代
- 保持代码整洁

祝开发顺利！🎮
