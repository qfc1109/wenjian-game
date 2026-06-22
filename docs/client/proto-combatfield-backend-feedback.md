# Proto_CombatField 后端接口反馈

更新时间：2026-06-10

## 已验证链路

- Unity MCP 可加载 `Proto_CombatField` 场景并进入 Play Mode。
- 客户端已连接 `ws://127.0.0.1:18080/ws/first-chain`。
- `LOGIN local-dev-key <clientTimeMs>` 已验证返回 `LOGIN_OK`，客户端使用 `playerId`、`regionId`、`x`、`y` 更新 HUD。
- `ENTER_REGION 1000001 1001` 已验证返回 `REGION_SNAPSHOT`，客户端使用 `regionId`、`self`、`entities`、`serverTick` 更新 HUD。
- `SKILL 1000001 2001 1 0` 已验证返回 `SKILL_EVENT` 和 `DAMAGE_EVENT`，客户端可播放剑气占位、显示伤害反馈并更新敌方血条。
- `START_ROGUE 1000001 4001` 已验证返回 `ROGUE_START`，客户端可显示秘境实例、地图、怪物数量和奖励池占位。
- `FINISH_ROGUE 1000001 9000001` 已验证返回 `ROGUE_FINISH`，客户端可显示结算状态和 `itemId/count` 奖励摘要。
- 阶段 10 运行态探针已验证：训练敌人可从 `x=4.0` 追击到 `x≈1.3`，技能第一次释放成功、第二次被客户端冷却拦截、冷却结束后可再次释放；`SwordQi`、`HitRangePreview`、`HitBurst` 和伤害飘字均可激活。临时截图：`C:\Users\Administrator\AppData\Local\Temp\wenjian-client-qa\stage10-runtime-qa.png`。
- 阶段 11 运行态探针已验证：本地训练敌人进入攻击范围后触发 `ENEMY_ATTACK hpDelta=-6`，玩家 HP 从 100 降到 94；玩家击杀训练敌人后敌方 HP 为 0、秘境怪物数为 0、房间状态变为 `Cleared`、出口封印隐藏、敌人视觉状态保持 `Dead`。
- 阶段 12 场景探针已验证：不开后端时，离线原型技能可本地触发 `SKILL_EVENT` / `DAMAGE_EVENT` 并推进小波次；第一只怪死亡后 HUD 显示剩余 `1`，第二只怪死亡后房间进入 `Cleared` 并显示可交互出口；玩家靠近出口弹出 3 选 1 本地奖励面板；选择奖励后进入 `Room 2`，HUD 刷新奖励摘要和怪物数 `3`，三只训练敌人激活，出口重新封印。

## 阶段 4：区域快照缺口

当前 `REGION_SNAPSHOT` 只有 `entities=2` 这类摘要，客户端不能据此真实创建或刷新实体列表。

后端下一步需要在快照中提供：

| 字段 | 用途 |
|------|------|
| `entityId` | 客户端复用或销毁场景对象 |
| `entityType` | 区分玩家、怪物、NPC、掉落物 |
| `x` / `y` | 映射到 Unity 世界坐标 |
| `hp` / `maxHp` | 血条和死亡状态 |
| `visualId` | 客户端选择 prefab 或 sprite |
| `facing` | 朝向和技能默认方向 |
| `state` | idle、move、cast、hit、dead 等表现状态 |

## 阶段 5：技能和伤害事件缺口

当前客户端能用 `SKILL_EVENT` 播放短剑气，但表现仍是猜的。

建议 `SkillEvent` 增补：

| 字段 | 用途 |
|------|------|
| `eventId` | 客户端去重和调试 |
| `serverTick` | 和快照、伤害、动画时间线对齐 |
| `castMs` | 前摇表现 |
| `durationMs` | 特效持续时间 |
| `range` | 剑气长度 |
| `radius` | 命中宽度 |
| `hitShape` | 圆形、矩形、扇形等形状 |
| `vfxId` | 选择客户端特效 |

当前 `DamageEvent` 只有 `hpDelta=-12`，客户端只能从本地旧血量推算敌方剩余血量。

建议 `DamageEvent` 增补：

| 字段 | 用途 |
|------|------|
| `targetHp` | 精确刷新目标血条 |
| `targetMaxHp` | 首次显示和血条比例 |
| `isDead` | 死亡表现和对象回收 |
| `damageType` | 普攻、技能、暴击、DOT 等表现 |
| `hitVfxId` | 受击特效 |
| `floatingTextStyle` | 普通伤害、暴击、治疗等飘字样式 |

## 阶段 6：秘境开始和结算缺口

当前 `ROGUE_START` 可让客户端显示秘境已开始，但还不足以真正生成房间和怪物。

建议 `RogueStart` 增补：

| 字段 | 用途 |
|------|------|
| `roomId` / `roomIndex` | 客户端展示当前房间、房间进度和调试定位 |
| `roomState` | 区分准备、战斗中、已清理、可退出、失败等状态 |
| `clearCondition` | 展示清怪、存活、限时等结算条件 |
| `monsterEntities` | 真实生成怪物对象，至少包含 entityId、monsterId、x、y、hp、maxHp、visualId |
| `exitState` | 控制出口或下一房间入口是否可见、可交互 |
| `seed` | 需要客户端复现房间布局或调试随机结果时使用 |

当前 `ROGUE_FINISH` 只有 `itemId/count`，客户端只能显示数字，不能显示奖励名称、类型或来源。

建议 `RogueFinish` 增补：

| 字段 | 用途 |
|------|------|
| `rewards[]` | 支持多奖励，不把结算限制成单个物品 |
| `itemId` | 客户端查配置或服务端直接返回详情 |
| `itemName` | 如果客户端不直接加载物品配置，需要随结算返回 |
| `itemType` | 区分装备、材料、货币、经验等展示样式 |
| `rarity` | 奖励颜色、音效和结算重点展示 |
| `source` | 展示来自奖励池、首通、怪物掉落或保底 |

当前后端允许未清怪直接 `FINISH_ROGUE`，只能作为调试链路。正式逻辑需要服务端校验清怪条件、房间状态和结算幂等，客户端只负责展示结果，不能替服务端判断是否可结算。

## 阶段 10：可玩战斗循环缺口

客户端现在已经有本地训练敌人追击、障碍避让、技能冷却和命中反馈，但这些仍是原型侧模拟。要进入真正可玩循环，服务端需要逐步接管权威状态，客户端只做预测、表现和插值。

### 实体权威状态

训练敌人本地 AI 暴露出的字段需求：

| 字段 | 用途 |
|------|------|
| `entityId` | 持续匹配同一个怪物对象，避免重复创建 |
| `entityType` | 区分玩家、怪物、机关、掉落物和出口 |
| `monsterId` / `templateId` | 选择怪物配置、碰撞半径、移动速度和表现资源 |
| `x` / `y` | 服务端权威坐标，客户端插值到 Unity 世界坐标 |
| `targetId` | 客户端显示怪物当前追击目标或仇恨指向 |
| `moveSpeed` | 客户端播放移动动画和预测位移 |
| `collisionRadius` | 角色间距、技能命中和避障表现 |
| `state` | idle、chase、attack、hit、dead 等状态 |
| `facing` | 怪物朝向、攻击方向和受击转向 |
| `aiState` | idle、aggro、chase、blocked、leash、return 等调试状态 |

### 地图阻挡与出口

本地障碍现在写死在客户端场景工厂里。后端至少需要给客户端同一套阻挡语义，否则怪物追击、技能落点和出口交互会在前后端产生偏差。

| 字段 | 用途 |
|------|------|
| `mapId` | 客户端加载地图和阻挡配置 |
| `blockedCells[]` 或 `obstacleRects[]` | 表达墙、竹丛、石块等不可走区域 |
| `spawnPoints[]` | 玩家和怪物初始位置 |
| `exitEntityId` | 把出口也作为实体同步 |
| `exitState` | locked、sealed、open、used |
| `clearConditionProgress` | 显示还剩几只怪、是否满足开门条件 |

### 技能冷却和命中结果

客户端当前只做本地冷却拦截，服务端仍必须权威校验冷却、距离、目标和命中结果。

| 字段 | 用途 |
|------|------|
| `skillId` | 匹配客户端技能按钮和配置 |
| `cooldownMs` | HUD 显示完整冷却 |
| `cooldownRemainingMs` | 登录、重连或快照刷新时恢复冷却状态 |
| `castStartTick` / `resolveTick` | 前摇、命中和伤害时间线对齐 |
| `casterId` | 技能来源 |
| `aimX` / `aimY` | 技能方向 |
| `originX` / `originY` | 技能起点，避免客户端用旧位置猜 |
| `hitTargets[]` | 一次技能命中的目标列表 |
| `targetHp` / `targetMaxHp` | 精确刷新血条 |
| `isDead` | 死亡动画、掉落和出口解锁 |

### 客户端仍可本地处理的内容

- 方向输入、按键冷却灰显、攻击帧和受击帧播放。
- 根据服务端字段选择 `vfxId`、`hitVfxId`、飘字样式和音效。
- 对服务端权威位置做短时间插值，避免实体跳动。

客户端不应长期决定怪物是否命中、是否死亡、出口是否开启、奖励是否发放。

## 阶段 11：小战斗闭环缺口

客户端现在已经补齐原型侧的小战斗闭环：怪物攻击玩家、玩家血量下降、玩家击杀怪物、清怪开门和胜利反馈。但这些结果目前仍由客户端模拟，正式链路需要服务端权威下发事件和房间状态。

### 敌人攻击玩家

本地 `TrainingEnemyMotor` 目前只需要 `attackRange`、`attackCooldown`、`attackDamage` 就能表现敌人攻击。正式服务端建议把敌人普攻也作为技能或动作事件下发。

| 字段 | 用途 |
|------|------|
| `eventId` | 客户端去重、回放和调试 |
| `serverTick` | 对齐攻击前摇、命中帧和扣血时间 |
| `sourceId` | 攻击来源怪物 |
| `targetId` | 被攻击玩家或实体 |
| `attackId` / `skillId` | 选择怪物攻击表现和配置 |
| `aimX` / `aimY` | 攻击方向和命中范围预览 |
| `range` / `radius` / `hitShape` | 客户端显示攻击范围提示 |
| `cooldownMs` | 怪物攻击节奏和调试展示 |
| `damage` / `hpDelta` | 飘字和基础扣血 |
| `targetHp` / `targetMaxHp` | 精确刷新玩家血条 |
| `hitVfxId` | 玩家受击特效和音效 |

### 击杀、清怪和开门

当前客户端在敌方 HP 到 0 时直接把房间切为 `Cleared` 并隐藏出口封印。正式链路应该由服务端确认死亡、剩余怪物数量和出口状态。

| 字段 | 用途 |
|------|------|
| `targetId` | 标记哪只怪物死亡 |
| `isDead` | 死亡动画和禁用后续受击 |
| `deathState` | dead、despawn、corpse、revivePending 等表现 |
| `monsterRemaining` | HUD 和房间目标提示 |
| `roomState` | fighting、cleared、reward、failed |
| `clearConditionProgress` | 展示清怪条件完成度 |
| `exitEntityId` | 客户端定位出口对象 |
| `exitState` | sealed、opening、open、used |
| `roomClearedEventId` | 清房事件去重，避免重复开门或重复胜利反馈 |

### 胜利反馈和奖励预告

清怪后客户端需要知道下一步是出门、进入下一房间还是结算奖励。这个状态不应由客户端从按钮或本地怪物数猜。

| 字段 | 用途 |
|------|------|
| `victoryState` | roomCleared、stageCleared、rogueFinished |
| `nextAction` | openExit、chooseReward、finishRogue、nextRoom |
| `rewardPreview[]` | 清房后先展示可见奖励或掉落预告 |
| `unlockReason` | 出口开启原因，便于调试和 UI 提示 |
| `canFinishRogue` | 是否允许结算 |
| `canEnterNextRoom` | 是否允许进入下一房间 |

后端实现时可以先用文本调试协议返回这些字段；字段稳定后再映射到 protobuf。客户端目前最需要的是权威 `targetHp/isDead/monsterRemaining/roomState/exitState`，数据库设计可以等服务端房间状态机跑通后再落。

## 阶段 12：出口交互、奖励选择和下一房间缺口

客户端现在已经能串起原型侧最小肉鸽循环：清怪、开出口、靠近出口、奖励三选一、进入下一房间和刷新小波次。但奖励、房间推进和波次仍是本地假数据。正式链路需要后端提供权威房间状态、奖励候选和下一房间内容。

### 出口交互

| 字段 | 用途 |
|------|------|
| `exitEntityId` | 客户端定位并复用出口对象 |
| `exitState` | sealed、open、interactable、used 等表现状态 |
| `interactionRadius` | 控制玩家靠近多少距离弹出交互 UI |
| `nextAction` | chooseReward、nextRoom、finishRogue 等后续动作 |
| `canInteract` | 防止客户端本地误判可交互 |
| `unlockEventId` | 出口解锁事件去重和调试 |

### 奖励三选一

| 字段 | 用途 |
|------|------|
| `choiceId` | 玩家选择时回传，避免用展示文本或本地索引决定奖励 |
| `rewardType` | buff、item、currency、heal、skillUpgrade 等 |
| `rewardId` | 客户端查配置或服务端直接给详情 |
| `displayName` | 面板展示名称 |
| `description` | 面板展示效果说明 |
| `rarity` | 奖励颜色、音效和排序 |
| `iconId` / `vfxId` | 奖励图标和领取特效 |
| `effectPreview` | 选择前预览会影响 HP、伤害、冷却等哪个属性 |

客户端不应该长期内置奖励池；本地 3 选 1 只是为了验证交互和 HUD 需求。

### 下一房间和小波次

| 字段 | 用途 |
|------|------|
| `roomIndex` | 显示当前第几房，并驱动房间推进 |
| `roomSeed` | 复现房间布局、怪物刷新和奖励抽取 |
| `roomState` | fighting、cleared、rewardChoosing、transitioning、failed |
| `waveIndex` / `waveCount` | 支持一房多波 |
| `monsterRemaining` | HUD 显示剩余怪物和开门条件 |
| `monsterEntities[]` | 真实生成本房怪物，包含 entityId、monsterId、spawnX、spawnY、hp、maxHp、visualId |
| `spawnGroupId` | 客户端选择小波次刷怪点和房间节奏 |
| `appliedRewards[]` | 进入下一房时刷新已生效奖励或局内 buff |

阶段 12 说明奖励效果会反向影响后端字段：如果奖励能改最大血量、技能冷却、伤害、移速或额外剑气，服务端需要在玩家局内状态里同步这些临时 modifier，并在快照或房间切换事件中返回。

## 下一轮后端优先级建议

| 优先级 | 建议 | 原因 |
|--------|------|------|
| P0 | 补 `REGION_SNAPSHOT` / `ROGUE_START` 的实体详情列表 | 没有实体详情，Unity 不能真实生成怪物、血条、死亡和位置刷新 |
| P0 | 补 `DamageEvent.targetHp/targetMaxHp/isDead` | 客户端不能长期从本地旧血量猜结果 |
| P0 | 补怪物 `state/aiState/targetId/facing` 和权威坐标 | 阶段 10 已出现本地追击需求，后端需要接管怪物行为 |
| P0 | 补房间 `monsterRemaining/roomState/exitState` | 阶段 11 已需要服务端权威确认清怪开门和胜利状态 |
| P0 | 补奖励候选 `rewardChoices[]` 和选择回执 | 阶段 12 已出现 3 选 1 面板，不能长期用客户端假数据发奖 |
| P0 | 补下一房间 `roomIndex/wave/monsterEntities` | 阶段 12 已串起下一房间和小波次，需要后端决定房间内容 |
| P1 | 补技能冷却和事件时间线字段 | `cooldownRemainingMs/castStartTick/resolveTick/durationMs/range/radius/vfxId` 会直接影响技能可读性 |
| P1 | 补敌人攻击事件字段 | 客户端需要从服务端知道怪物何时攻击、打谁、扣多少血以及如何播放命中特效 |
| P1 | 补局内奖励 modifier 同步 | 奖励如果影响 HP、伤害、冷却、移速，客户端 HUD 和战斗表现都需要权威数值 |
| P2 | 明确奖励详情来自服务端还是客户端配置 | 决定是否需要物品 CSV 导出到客户端 |
| P2 | 再评估数据库持久化 | 客户端当前只证明需要秘境实例结果和奖励落库，不足以设计完整长期表 |

## 启动和配置反馈

本地 Spring Boot 启动时，默认相对路径可能找不到 `player_template.csv`。本轮联调用环境变量显式指定配置目录：

```powershell
$env:WENJIAN_CONFIG_SOURCE_DIR='E:\qfc\workspace\wenjian-game\config\source'
```

建议后端文档把本地启动命令改成显式配置目录，避免 Maven/Spring Boot 工作目录差异导致联调失败。

## 暂不建议推进的后端内容

- 暂不设计最终数据库表。
- 暂不扩展复杂战斗 Tick 持久化。
- 暂不切 KCP 或 protobuf 二进制作为唯一链路。
- 先补实体快照、技能事件、伤害事件和秘境房间/结算字段，再决定数据库边界。
