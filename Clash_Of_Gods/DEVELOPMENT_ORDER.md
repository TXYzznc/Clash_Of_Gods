# 开发顺序速查表

## 🎯 推荐开发顺序

```
第一阶段：核心战斗 (MVP) ─────────────────────────────
│
├─ 1. 召唤物系统 ★★★ [Week 1]
│     └─ 扩展 UnitEntity，添加技能/进化/装备
│
├─ 2. 战斗系统核心 ★★★ [Week 2]
│     └─ 自动战斗AI，伤害计算，胜负判定
│
├─ 3. 策略卡系统 ★★★ [Week 3]
│     └─ 手牌管理，灵力消耗，卡牌效果
│
└─ 4. 战斗UI ★★★ [Week 4]
      └─ 部署界面，战斗界面，结算界面

第二阶段：地图探索 ─────────────────────────────────
│
├─ 5. 地图系统 ★★☆ [Week 5]
│     └─ 场景加载，玩家移动，战争迷雾
│
├─ 6. 敌人AI ★★☆ [Week 6]
│     └─ 状态机（巡逻/警戒/追击/战斗）
│
├─ 7. 事件系统 ★★☆ [Week 7]
│     └─ 宝箱/商人/祭坛/圣泉
│
└─ 8. 撤离结算 ★★☆ [Week 8]
      └─ 钥匙系统，成功/失败结算

第三阶段：养成系统 ─────────────────────────────────
│
├─ 9. 召唤师职业 ★★☆ [Week 9]
│     └─ 狂战/术士/混沌/德鲁伊
│
├─ 10. 科技树 ★☆☆ [Week 10]
│      └─ 全局强化，解锁功能
│
└─ 11. 基地系统 ★☆☆ [Week 11]
       └─ 3D基地，功能建筑

第四阶段：内容填充 ─────────────────────────────────
│
├─ 12. 神话角色 ★☆☆ [Week 12]
├─ 13. 策略卡内容 ★☆☆ [Week 13]
├─ 14. 地图/Boss ★☆☆ [Week 14]
└─ 15. UI打磨 ★☆☆ [Week 15]

第五阶段：联机优化 ─────────────────────────────────
│
├─ 16. PVP联机 ☆☆☆ [Week 16]
├─ 17. 云存档 ☆☆☆ [Week 17]
└─ 18. 性能优化 ☆☆☆ [Week 18]
```

---

## 📋 第一周任务（立即开始）

### 目标：完成召唤物系统基础

| 天数 | 任务 | 产出 |
|------|------|------|
| Day 1 | 扩展 UnitConfig | 添加人口/进化/技能字段 |
| Day 2 | 创建 EvolutionConfig | 进化分支配置 |
| Day 3 | 实现 UnitSkillSystem | 技能基类和释放逻辑 |
| Day 4 | 实现技能类型 | 主动/被动/进化技能 |
| Day 5 | 实现 UnitEvolution | 局内等级和进化 |
| Day 6 | 创建测试角色 | 孙悟空三形态 |
| Day 7 | 测试调试 | 修复Bug |

---

## 🔧 当前可用的基础


### 已完成的系统

```
✅ Core/
   ├─ EventManager      - 事件系统
   ├─ ConfigManager     - 配置管理
   ├─ SaveSystem        - 存档系统
   └─ GameLifecycleManager - 生命周期

✅ Gameplay/Units/
   ├─ UnitEntity        - 单位实体（HP/攻击/防御）
   ├─ UnitStats         - 属性计算
   ├─ UnitAIController  - AI控制器（基础）
   └─ UnitBuffManager   - Buff管理

✅ Gameplay/Combat/
   ├─ BattleManager     - 战斗管理（基础）
   └─ BattlePhase       - 战斗阶段

✅ Gameplay/Summoner/
   ├─ SummonerController - 召唤师控制
   └─ SummonerRuntimeData - 运行时数据

✅ Network/
   ├─ NetworkModeManager - 单机/联机切换
   ├─ NetworkSyncBase    - 同步基类
   └─ 各种NetworkSync组件
```

### 需要新增的系统

```
⏳ Gameplay/Units/
   ├─ UnitSkillSystem   - 技能系统
   ├─ UnitEvolution     - 进化系统
   └─ UnitEquipment     - 装备系统

⏳ Gameplay/Cards/
   ├─ CardManager       - 卡牌管理
   ├─ CardEffect        - 卡牌效果
   └─ CardDeck          - 卡组

⏳ Data/
   ├─ EvolutionConfig   - 进化配置
   └─ SummonerClassConfig - 职业配置

⏳ AI/
   ├─ EnemyStateMachine - 敌人状态机
   └─ CombatAI          - 战斗AI
```

---

## 💡 开发建议

### 1. 先做核心，后做内容

```
核心系统 → 测试验证 → 内容填充 → 打磨优化
```

### 2. 每周一个可测试的里程碑

```
Week 1: 能召唤单位并看到技能释放
Week 2: 能进行一场完整的自动战斗
Week 3: 能使用策略卡影响战斗
Week 4: 有完整的战斗UI
```

### 3. 使用热重载加速开发

```
修改代码 → 自动热重载 → 立即看到效果
无需重启Unity，大幅提升开发效率
```

### 4. 单机优先，联机可选

```
所有功能先在单机模式下开发测试
联机功能通过 NetworkSync 组件可插拔添加
```

---

## 📚 相关文档

- [开发路线图](./DEVELOPMENT_ROADMAP.md) - 详细开发计划
- [网络架构](./NETWORK_ARCHITECTURE.md) - 网络系统设计
- [场景设置指南](./NETWORK_MODE_SETUP_GUIDE.md) - 场景配置
- [快速参考](./QUICK_REFERENCE.md) - 代码模板
- [热重载指南](./FAST_SCRIPT_RELOAD_GUIDE.md) - 热重载使用

---

**开始第一个任务：扩展 UnitConfig，添加召唤物特有字段！** 🚀
