# 诸神对决 (Clash of Gods) - 项目架构文档

## 项目概述
《诸神对决》是一款融合了卡牌、自走棋、Roguelite和搜打撤玩法的策略游戏。本文档描述了项目的代码架构和模块组织。

## 架构设计理念
- **ECS思想的混合架构**: Entity-Component-System模式
- **Service Locator模式**: 全局服务管理
- **分层架构**: 表现层、逻辑层、数据层分离

## 目录结构

```
Assets/Scripts/
├── Core/                           # 核心基础设施层
│   ├── GameLifecycleManager.cs    # 游戏生命周期管理（继承NetworkManager）
│   ├── EventManager.cs             # 全局事件总线
│   ├── SaveSystem.cs               # 数据持久化系统
│   └── ConfigManager.cs            # 配置管理器
│
├── Data/                           # 数据配置层
│   ├── UnitConfig.cs               # 单位配置（ScriptableObject）
│   ├── CardConfig.cs               # 卡牌配置
│   └── SkillConfig.cs              # 技能配置
│
├── Gameplay/                       # 游戏玩法层
│   ├── Summoner/                   # 召唤师系统
│   │   ├── SummonerController.cs          # 召唤师控制器
│   │   ├── SummonerSkillSystem.cs         # 技能系统
│   │   └── Classes/                        # 职业实现
│   │       ├── BaseSummonerClass.cs       # 职业基类
│   │       ├── BerserkerClass.cs          # 狂战
│   │       ├── WarlockClass.cs            # 术士
│   │       ├── ChaosClass.cs              # 混沌
│   │       └── DruidClass.cs              # 德鲁伊
│   │
│   ├── Units/                      # 单位系统
│   │   ├── UnitEntity.cs                  # 单位实体
│   │   ├── UnitAIController.cs            # AI控制器（行为树）
│   │   ├── UnitAnimationController.cs     # 动画控制器
│   │   └── UnitBuffManager.cs             # Buff管理器
│   │
│   ├── Combat/                     # 战斗系统
│   │   ├── BattleManager.cs               # 战斗管理器
│   │   └── CardManager.cs                 # 卡牌管理器
│   │
│   └── Map/                        # 地图探索系统
│       ├── MapManager.cs                  # 地图管理器
│       ├── MapEventPoint.cs               # 事件点
│       └── EnemyOverworldController.cs    # 敌人大地图AI（FSM）
│
├── Network/                        # 网络同步层
│   ├── GameNetworkManager.cs      # 网络管理器（Mirror）
│   └── NetworkTimeSync.cs          # 时间同步
│
├── UI/                             # 表现层
│   ├── UIManager.cs                # UI管理器
│   ├── CombatUI.cs                 # 战斗UI
│   └── ExplorationUI.cs            # 探索UI
│
└── Utils/                          # 工具类
    ├── Singleton.cs                # 单例模式基类
    └── ObjectPool.cs               # 对象池
```

## 核心系统说明

### 1. 核心基础设施层 (Core)

#### GameLifecycleManager
- **职责**: 游戏入口，管理游戏状态机和跨场景单例
- **状态**: Boot -> Base -> Exploration -> Combat -> Extraction
- **继承**: Mirror.NetworkManager（支持联机）

#### EventManager
- **职责**: 全局事件总线，解耦各模块
- **使用方式**: 
  ```csharp
  EventManager.Instance.AddListener<UnitDeathEvent>(OnUnitDeath);
  EventManager.Instance.TriggerEvent(new UnitDeathEvent { ... });
  ```

#### SaveSystem
- **职责**: 二进制序列化存档，支持本地和云存档
- **数据结构**: PlayerData（全局数据）、UnitInstanceData（单位实例）

#### ConfigManager
- **职责**: 加载和缓存ScriptableObject配置
- **配置类型**: UnitConfig、CardConfig、SkillConfig

### 2. 召唤师系统 (Summoner)

#### SummonerController
- **职责**: 玩家角色控制，移动、技能释放、受伤
- **数据**: SummonerRuntimeData（局内运行时数据）
- **关键机制**: 
  - 生命值/灵力值管理
  - 统御力（人口上限）
  - 技能释放

#### 职业系统（策略模式）
- **BaseSummonerClass**: 职业基类，定义通用接口
- **四大职业**:
  - **BerserkerClass**: 狂战 - 【狂怒之心】被动，低血提升伤害
  - **WarlockClass**: 术士 - 【暗影咒体】被动，诅咒机制
  - **ChaosClass**: 混沌 - 【混沌回响】被动，随机效果
  - **DruidClass**: 德鲁伊 - 【大地恩泽】被动，自然果实

### 3. 单位系统 (Units)

#### UnitEntity
- **职责**: 所有战斗单位的核心组件
- **关键功能**:
  - 生命值继承机制（InstanceData.CurrentHP）
  - 进化系统（Evolve方法）
  - 装备系统（5个装备槽）

#### UnitAIController
- **职责**: 自动战斗AI（行为树）
- **状态**: Idle -> Moving -> Attacking -> Fleeing
- **关键功能**:
  - 自动索敌（ScanForEnemies）
  - 攻击执行（ExecuteAttack）
  - 强制嘲讽（ForceTaunt）

#### UnitBuffManager
- **职责**: 管理Buff/Debuff
- **Buff类型**: 攻击提升、防御提升、眩晕、减速、中毒、流血等
- **关键功能**:
  - 持续性效果（Tick）
  - 净化（Cleanse）
  - 叠加机制

### 4. 战斗系统 (Combat)

#### BattleManager
- **职责**: 管理单场战斗生命周期
- **阶段**: Deployment（部署） -> Battle（战斗） -> Settlement（结算）
- **关键功能**:
  - 生成敌人（SpawnEnemies）
  - 部署玩家单位（DeployPlayerUnit）
  - 战斗结算（EndBattle）
  - 保存单位状态（生命继承）

#### CardManager
- **职责**: 管理策略卡牌系统
- **机制**: 
  - 8张卡组配置
  - 自动刷新（30秒）
  - 手动刷新（消耗金币）
- **卡牌效果**: 伤害、治疗、Buff、Debuff、控制

### 5. 地图探索系统 (Map)

#### MapManager
- **职责**: 管理探索地图、迷雾、事件点
- **关键功能**:
  - 战争迷雾（RevealFog）
  - 动态NavMesh烘焙
  - 事件点管理

#### EnemyOverworldController
- **职责**: 敌人大地图AI（有限状态机）
- **五种状态**:
  1. **Patrol**: 巡逻
  2. **Alert**: 警戒（警戒条上涨）
  3. **Chase**: 追击（广播玩家位置）
  4. **Combat**: 触发战斗
  5. **Rest**: 休息（降低索敌）

#### MapEventPoint
- **事件类型**: 宝箱、商人、祭坛、圣泉、老虎机、敌人、BOSS
- **交互机制**: 进入范围 -> 按E键触发

### 6. 网络系统 (Network)

#### GameNetworkManager
- **职责**: 扩展Mirror的NetworkManager
- **模式**: Host-Client（主机-客户端）
- **功能**:
  - 局域网对战
  - 随机数种子同步（确定性战斗）

#### NetworkTimeSync
- **职责**: 时间同步和RNG种子同步
- **机制**: Lockstep + 状态同步

### 7. UI系统

#### UIManager
- **职责**: UI栈管理、弹窗层级
- **功能**:
  - 面板打开/关闭
  - 飘字系统（ShowFloatingText）

#### CombatUI
- **显示内容**: 
  - 召唤师HP/灵力条
  - 手牌栏
  - 技能冷却

#### ExplorationUI
- **显示内容**:
  - 小地图
  - 任务追踪
  - 交互提示

## 关键设计模式

### 1. 单例模式 (Singleton)
- **使用场景**: 所有Manager类
- **实现**: Singleton<T>基类

### 2. 策略模式 (Strategy)
- **使用场景**: 召唤师职业系统
- **实现**: BaseSummonerClass + 四个子类

### 3. 状态模式 (State)
- **使用场景**: 敌人大地图AI
- **实现**: IEnemyState接口 + 五种状态

### 4. 观察者模式 (Observer)
- **使用场景**: 事件系统
- **实现**: EventManager

### 5. 对象池模式 (Object Pool)
- **使用场景**: 特效、子弹、敌人等频繁生成的对象
- **实现**: ObjectPool

## 数据流

```
用户输入 -> SummonerController -> 技能/移动逻辑
                                 ↓
                          EventManager（广播事件）
                                 ↓
                    ┌────────────┼────────────┐
                    ↓            ↓            ↓
              BattleManager  UIManager  QuestSystem
                    ↓
              UnitEntity（执行战斗）
                    ↓
              SaveSystem（保存状态）
```

## 生命继承机制实现

```csharp
// 战斗前：从存档加载单位HP
unit.Initialize(config, instanceData);
unit.Stats.CurrentHP = instanceData.CurrentHP;

// 战斗中：HP实时变化
unit.TakeDamage(damage);

// 战斗后：保存当前HP
instanceData.CurrentHP = unit.Stats.CurrentHP;
SaveSystem.Instance.SaveGame(playerData);
```

## 网络同步机制

### 状态同步
```csharp
[SyncVar] float CurrentHP;
[Command] void CmdCastSpell(int cardID, Vector3 target);
[ClientRpc] void RpcOnSpellCast(int cardID, Vector3 target);
```

### 确定性战斗
```csharp
// 服务器生成种子
int seed = Random.Range(0, int.MaxValue);
NetworkTimeSync.Instance.SyncRNGSeed(seed);

// 客户端使用相同种子
Random.InitState(seed);
```

## 开发建议

### 1. 测试流程
1. 先实现单机版核心循环
2. 测试生命继承机制
3. 接入Mirror实现联机

### 2. 性能优化
- 使用对象池管理特效和子弹
- NavMesh预烘焙（固定场景）
- UI使用Canvas分组减少重绘

### 3. 扩展性
- 新增神话体系：添加UnitConfig
- 新增职业：继承BaseSummonerClass
- 新增卡牌：添加CardConfig和效果实现

## 待实现功能（TODO）

### 核心功能
- [ ] 完整的战斗流程
- [ ] 卡牌效果实现
- [ ] 装备系统
- [ ] 背包系统
- [ ] 商店系统
- [ ] 任务系统

### 美术相关
- [ ] 三渲二Shader
- [ ] 天气系统
- [ ] Timeline演出
- [ ] 粒子特效

### 网络功能
- [ ] 房间匹配
- [ ] 断线重连
- [ ] 防作弊验证

## 热重载支持 (Hot Reload)

### 概述
项目已全面支持 Fast Script Reload 热重载功能，允许在 Play 模式下修改代码并立即看到效果。

### 热重载兼容性改造

#### 1. 单例模式改造
所有单例类都已改造为热重载兼容模式：
```csharp
// 使用类型转换避免热重载后的类型不匹配
_instance = (MyManager)(object)this;

// 添加静态变量重置
[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
private static void ResetStatics()
{
    _instance = null;
}
```

#### 2. 热重载回调方法
所有主要脚本都实现了热重载回调：
```csharp
// 实例方法 - 有 this 访问权限
void OnScriptHotReload()
{
    Debug.Log($"[HotReload] {GetType().Name} reloaded");
    // 重新初始化组件引用
    // 重新注册事件监听
}

// 静态方法 - 无需实例
static void OnScriptHotReloadNoInstance()
{
    Debug.Log("[HotReload] Static reload");
}
```

#### 3. 已改造的脚本列表

| 脚本 | 热重载支持 | 说明 |
|------|-----------|------|
| Singleton.cs | ✓ | 基类支持热重载 |
| EventManager.cs | ✓ | 保持事件字典状态 |
| ConfigManager.cs | ✓ | 支持配置重载 |
| SaveSystem.cs | ✓ | 保持缓存数据 |
| GameLifecycleManager.cs | ✓ | 保持游戏状态 |
| BattleManager.cs | ✓ | 重新注册事件 |
| CardManager.cs | ✓ | 保持手牌状态 |
| MapManager.cs | ✓ | 保持地图状态 |
| UIManager.cs | ✓ | 保持UI栈 |
| UnitEntity.cs | ✓ | 重新绑定组件 |
| UnitAIController.cs | ✓ | 重新扫描敌人 |
| UnitAnimationController.cs | ✓ | 重新绑定Animator |
| UnitBuffManager.cs | ✓ | 保持Buff状态 |
| SummonerController.cs | ✓ | 保持运行时数据 |
| Bootstrap.cs | ✓ | 验证系统状态 |

### 热重载辅助工具

#### HotReloadHelper.cs
提供热重载相关的实用功能：
```csharp
// 验证所有管理器实例
HotReloadHelper.VerifyManagers();

// 安全获取单例
var manager = HotReloadHelper.GetSingletonSafe<BattleManager>();

// 重新绑定组件引用
HotReloadHelper.RebindComponents(this);
```

### 使用指南

1. **进入 Play 模式**
2. **修改任意脚本**（如调整伤害计算、AI行为等）
3. **保存文件** (Ctrl+S)
4. **等待 2-3 秒**，观察控制台输出 "Hot-reload completed"
5. **立即测试修改效果**

### 注意事项

- **泛型类/方法**：不支持热重载
- **新增公共字段**：需要实验性功能支持
- **Mirror 网络代码**：SyncVar/RPC 修改可能需要重启
- **继承关系修改**：需要完整重新编译

详细使用指南请参考：`FAST_SCRIPT_RELOAD_GUIDE.md`

## 联系与支持
如有问题，请参考项目策划文档或联系开发团队。
