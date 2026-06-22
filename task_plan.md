# 问剑江湖项目任务计划

## 当前目标

当前主线是先做 Unity 客户端真实原型，用客户端场景、输入、HUD、特效和联调反馈来反推后端接口、字段、配置和数据库。

阶段 6 秘境开始、房间展示和结算反馈已通过 Unity MCP 运行态联调；阶段 8 已补客户端观感、HUD 安全区、角色轮廓、场景层次和战斗反馈可读性；阶段 9 已把主角效果图接入为 Unity Sprite，补了 4 向移动/攻击/受击帧、玩家边界、技能命中范围、敌人待机/受击状态和秘境房间场景反馈；阶段 10 已补训练敌人追击/避障、技能冷却、运行态视觉验收和后端字段反馈；阶段 11 已补敌人攻击玩家、玩家受击扣血、击杀敌人、清怪开门和胜利反馈；阶段 12 已补出口交互、奖励三选一、下一房间重置和多敌人小波次。下一步继续客户端优先，做可试玩打磨：让奖励真实影响战斗参数，补房间差异、失败/重开和更多表现反馈。

## 需求索引

只放还没结束、后面还要继续跟的需求。已经完成的历史需求不放这里，只留在下面日期记录里查证。

| 需求ID | 需求 | 当前状态 | 当前阶段 | 下一步 |
|--------|------|----------|----------|--------|
| R-20260606-unity-client | 先做 Unity 客户端真实原型 | 进行中 | 阶段 13：待开始单人秘境肉鸽试玩打磨 | 补奖励实际效果、房间差异、失败/重开、更多表现反馈和试玩验收 |

## 需求详情

### R-20260606-unity-client：先做 Unity 客户端真实原型

- 目标：先把 Unity 客户端跑起来，再决定后端还需要哪些接口、字段、配置和数据库设计。
- 计划文档：`docs/plans/wenjian-unity-proto-combatfield-client-plan.md`
- 当前：Unity 工程和第一张 `Proto_CombatField` 场景已生成，玩家可本地移动，相机可跟随，基础 HUD、WebSocket 登录/区域快照、技能意图、剑气占位、受击反馈、秘境开始和结算反馈已完成运行态联调；已补一轮客户端观感和战斗反馈可读性，并把青白轻剑主角效果图提取成 Unity 可见 Sprite，接入 4 向移动、攻击和受击帧切换；已补玩家移动边界、技能命中范围、敌人 Idle/Hit 状态、秘境房间场景反馈、训练敌人基础追击/避障、技能冷却 HUD、小战斗闭环、出口交互、奖励三选一、下一房间串联和多敌人小波次；下一步做单人秘境肉鸽试玩打磨，让奖励真实影响战斗参数，并补房间差异、失败/重开和更多表现反馈。

#### 阶段

- 【✔】 阶段 0：生成客户端开发计划；结果：已明确先补 Unity 工程、场景、输入、HUD、WebSocket、技能、秘境和后端反馈 <2026-06-06>
- 【✔】 阶段 1：检查 Unity 环境并创建真实 Unity 工程；结果：使用本机 Unity `2022.3.62f3c1` 创建 `wenjian-client/Proto_CombatField/UnityProject`，接入 `com.coplaydev.unity-mcp` 嵌入式包，Codex config 已写入 `unityMCP` server；影响范围：Unity 工程、Packages、Codex 本地配置；验证：Unity batchmode 能加载工程，`uvx --from mcpforunityserver==9.7.1 mcp-for-unity --help` 已成功；未完成：当前 Codex 会话未暴露 Unity MCP 工具，HTTP server 8080 未常驻启动 <2026-06-06>
- 【✔】 阶段 2：创建竹林战斗场、玩家、训练敌人和原型相机；结果：生成 `Assets/_Wenjian/Scenes/Proto_CombatField.unity`、Tilemap、占位地块、玩家、训练敌人和正交主相机；影响范围：Unity 场景、占位美术、场景生成器；验证：`unity-generate-proto-scene-after-movement.log` 生成成功，最终 EditMode 测试 `14/14` 通过；未完成：Pixel Perfect/URP 包尚未接入，后续画面 QA 时处理 <2026-06-06>
- 【✔】 阶段 3：实现本地 8 方向输入、HUD 和调试面板；结果：`LocalPlayerMotor` 支持原型本地 8 方向移动，`CameraFollow2D` 跟随玩家，`PrototypeHudPresenter` 显示连接状态、玩家血量、敌人血量、区域/秘境状态、serverTick、坐标和最近事件；影响范围：Unity 运行时 UI、场景工厂和 EditMode 测试；验证：最终 EditMode 测试 `17/17` 通过；未完成：Input System 包尚未接入，当前仍用 Legacy Input 原型输入 <2026-06-06>
- 【✔】 阶段 4：接入 WebSocket 登录和进入区域；结果：本地 gateway-ws 启动后，Unity MCP Play Mode 验证 HUD 显示 `Conn: Online`、`Tick 1`、`Ent 2` 和 `REGION_SNAPSHOT entities=2`；影响范围：Unity WebSocket 客户端、HUD 状态、后端字段反馈；验证：Unity MCP 运行态联调通过，EditMode 测试 `23/23` 通过；缺口：`REGION_SNAPSHOT` 仍缺实体详情列表 <2026-06-08>
- 【✔】 阶段 5：接入技能事件、剑气特效和受击反馈；结果：新增技能命令、技能输入控制器、剑气 Quad 占位、伤害飘字和敌方血条扣减，Unity MCP 运行态触发技能后收到 `SKILL_EVENT` / `DAMAGE_EVENT`；影响范围：客户端 Net、Presentation、场景工厂、场景文件和 EditMode 测试；验证：Unity EditMode 测试 `32/32` 通过，MCP Play Mode 验证 `Feedback => skill=2001 damage=-12`；缺口：服务端事件还缺 `eventId/serverTick/durationMs/vfxId/targetHp` 等字段 <2026-06-08>
- 【✔】 阶段 6：接入秘境开始和结算反馈；结果：新增 `START_ROGUE` / `FINISH_ROGUE` 命令、秘境调试控制器、房间摘要 HUD 和奖励摘要 HUD；影响范围：客户端 Net、Presentation、UI、场景工厂、场景文件和 EditMode 测试；验证：Unity EditMode 测试 `41/41` 通过，MCP Play Mode 验证 HUD 显示 `ROGUE_START instance=9000001 monsters=8` 与 `Reward item 6001 x3`，Console 无 error/warning；缺口：后端仍缺房间状态、怪物实体列表、出口状态和奖励详情 <2026-06-08>
- 【✔】 阶段 7：整理后端接口、配置和数据库反馈；结果：`docs/client/proto-combatfield-backend-feedback.md` 已覆盖阶段 4-6 主要缺口，后端下一轮优先级暂缓到客户端可见质量继续提升后再定；影响范围：客户端反馈文档和后续后端计划输入；验证：未运行，原因：文档/计划状态调整 <2026-06-10>
- 【✔】 阶段 8：补客户端观感、HUD 安全区、角色轮廓和战斗反馈可读性；结果：HUD 增加背板并缩小字号，地面降低格子压迫感并增加柔化层/草簇/石块/训练区域，玩家和训练敌人拆成 `VisualRoot` 多层轮廓，技能和受击反馈继续可见；影响范围：Unity 场景工厂、`Proto_CombatField` 场景、EditMode 测试；验证：Unity EditMode `46/46` 通过，MCP Play Mode 验证在线、技能、伤害、VFX 和世界血条，Console 无 error/warning <2026-06-10>
- 【✔】 阶段 9：继续客户端可玩性和场景内容扩展；结果：已将 `08-character-spec-standard.png` 的青白轻剑主角提取为 Unity Sprite，生成 20 张主角帧资源并接入 4 向移动/攻击/受击；本轮继续补玩家移动边界、技能命中范围预览、训练敌人 Idle/Hit 状态、受击位移/闪色和秘境房间色层/怪物标记/出口封印反馈；影响范围：Unity Presentation、Net、Prototype 场景工厂、EditMode 测试和 `Proto_CombatField` 场景；验证：Unity EditMode `61/61` 通过，Console 无 error/warning，临时相机截图确认房间/技能/受击反馈可见 <2026-06-10>
- 【✔】 阶段 10：补可玩战斗循环扩展，覆盖敌人基础 AI/追击、障碍碰撞、技能冷却/命中结果表现，并继续整理后端实体列表、技能事件和房间出口字段需求；结果：新增训练敌人追击/避障/边界约束组件，技能输入增加冷却拦截和 HUD 冷却文本，默认场景已接线并重建，运行态探针确认追击、冷却、命中范围、命中爆点和伤害飘字可用，后端字段反馈已补到 `docs/client/proto-combatfield-backend-feedback.md`；影响范围：Unity Presentation、Prototype 场景工厂、`Proto_CombatField` 场景、客户端后端反馈文档；验证：Unity Play Mode 运行态探针通过，临时截图确认画面可见，Unity Console 无 error/warning，阶段相关 EditMode `28/28` 通过，全量 EditMode `69/69` 通过 <2026-06-10>
- 【✔】 阶段 11：补小战斗闭环，覆盖敌人攻击玩家、玩家受击血量、清怪开门、胜利反馈和下一轮后端字段复核；结果：新增训练敌人攻击范围/冷却/伤害事件和 `PrototypeCombatLoopController`，HUD 可在敌人攻击时扣玩家血并播放受击帧，玩家击杀敌人后敌方 HP 归零、怪物数清零、房间切为 `Cleared`、出口封印隐藏、敌人视觉保持 `Dead`，阶段 11 后端字段反馈已补入客户端反馈文档；影响范围：Unity Presentation、Net、Prototype 场景工厂、`Proto_CombatField` 场景、EditMode 测试和客户端后端反馈文档；验证：阶段相关 EditMode `47/47` 通过，全量 EditMode `75/75` 通过，Unity MCP 运行态探针确认 `ENEMY_ATTACK hpDelta=-6`、玩家 HP `100->94`、击杀后 `ROOM_CLEARED`、出口隐藏和敌人 `Dead`；Unity Console 无项目 error，只有截图工具产生的 1 条 MCP fallback warning <2026-06-10>
- 【✔】 阶段 12：补胜利后交互和奖励展示，覆盖出口交互、奖励预告/拾取、下一房间串联和多敌人/波次雏形；结果：清怪后出现可交互出口，玩家靠近出口弹出 3 选 1 本地奖励面板，选择奖励后 HUD 刷新奖励摘要并进入下一房间，下一房间重置敌人、怪物数、房间状态和出口封印，默认场景扩展到 2-3 只训练敌人的小波次，并补离线原型技能命中，未启动后端也能推进小房间循环；影响范围：Unity Presentation、Net、UI、Prototype 场景工厂、`Proto_CombatField` 场景、EditMode 测试和客户端后端反馈文档；验证：阶段相关 EditMode `52/52` 通过，全量 EditMode `84/84` 通过，Unity MCP 离线场景探针确认 4 次本地技能命中清完 2 只怪、`afterFirst=1`、清怪开出口、奖励面板弹出、选择奖励后进入 `Room 2`、`monsters=3`、三只敌人激活且出口重新封印 <2026-06-10>
- 【 】 阶段 13：补单人秘境肉鸽试玩打磨，覆盖奖励实际效果、房间差异、失败/重开、怪物目标切换和更多战斗表现反馈

## 2026-06-10

### 执行记录

- 【✔】 R-20260606-unity-client / 阶段 8：补客户端观感与战斗反馈可读性；结果：新增 HUD 背板和安全区、地面柔化/草簇/石块/训练区域、多层玩家/敌人轮廓、敌人世界血条同步和技能/命中反馈；验证：Unity EditMode `46/46` 通过，MCP Play Mode 触发技能后 `DAMAGE_EVENT hpDelta=-12`、`wake=True`、`burst=True`、`enemyHp=0.88`，Console 无 error/warning <2026-06-10>
- 【✔】 R-20260606-unity-client / 阶段 9：补主角 4 向移动、攻击和受击帧；结果：已生成并接入 20 张青白轻剑主角帧资源，`LocalPlayerMotor` 可按移动方向切 idle/walk，`SkillIntentController` 触发攻击帧，`FirstChainHudController` 在玩家受击时触发 hit 帧；影响范围：Unity 角色资源、Presentation、Net、场景工厂和 `Proto_CombatField` 场景；验证：Unity EditMode `53/53` 通过，场景检查确认 `PlayerSpriteAnimator` 已被 motor/HUD 引用，Console 无 error/warning <2026-06-10>
- 【✔】 R-20260606-unity-client / 阶段 9：补玩家边界、技能范围、敌人状态和秘境房间反馈；结果：`LocalPlayerMotor` 增加战斗区域边界，技能事件显示命中范围预览，训练敌人有 Idle 脉动和 Hit 位移/闪色/状态文本，`RogueRoomScenePresenter` 让秘境开始/结算驱动房间色层、怪物标记和出口封印；影响范围：Unity Presentation、Net、Prototype 场景工厂、EditMode 测试和 `Proto_CombatField` 场景；验证：Unity EditMode `61/61` 通过，Console 无 error/warning，临时相机截图确认房间/技能/受击反馈可见 <2026-06-10>
- 【✔】 R-20260606-unity-client / 阶段 10：补可玩战斗循环基础实现；结果：`TrainingEnemyMotor` 支持训练敌人按仇恨范围追击玩家、遇到矩形障碍侧移避开并限制在战斗边界内，`SkillIntentController` 增加技能冷却和 `SkillCooldownText` HUD，场景反馈改为抖动敌人 `VisualRoot` 避免覆盖 AI 位移，`Proto_CombatField` 场景已重建；影响范围：Unity Presentation、Prototype 场景工厂、`Proto_CombatField` 场景；验证：阶段相关 EditMode `28/28` 通过，全量 EditMode `69/69` 通过，Unity Console 无 error/warning <2026-06-10>
- 【✔】 R-20260606-unity-client / 阶段 10：完成运行态视觉验收和后端字段反馈整理；结果：Unity Play Mode 探针确认敌人从 `x=4.0` 追击到 `x≈1.3`，技能第一次释放成功、第二次被冷却拦截、冷却结束后可再次释放，剑气、命中范围、命中爆点和伤害飘字均激活；`docs/client/proto-combatfield-backend-feedback.md` 已补实体权威状态、地图阻挡/出口、技能冷却和命中结果字段缺口；验证：临时截图 `C:\Users\Administrator\AppData\Local\Temp\wenjian-client-qa\stage10-runtime-qa.png` 可见，Unity Console 无 error/warning <2026-06-10>
- 【✔】 R-20260606-unity-client / 阶段 11：补小战斗闭环；结果：敌人进入攻击范围后触发本地攻击事件，`PrototypeCombatLoopController` 转给 HUD 扣玩家血；玩家伤害击杀敌人后触发清怪开门，房间状态变为 `Cleared`，敌人视觉保持 `Dead`，`docs/client/proto-combatfield-backend-feedback.md` 已补敌人攻击、清怪开门和胜利状态字段缺口；影响范围：Unity Presentation、Net、Prototype 场景工厂、`Proto_CombatField` 场景和客户端反馈文档；验证：阶段相关 EditMode `47/47` 通过，全量 EditMode `75/75` 通过，Unity MCP 运行态探针确认玩家 HP `100->94`、`ROOM_CLEARED`、出口隐藏和敌人 `Dead` <2026-06-10>
- 【✔】 R-20260606-unity-client / 阶段 12：补出口交互、奖励三选一、下一房间和小波次；结果：新增 `PrototypeRogueRewardController`，清怪后出口可交互，靠近出口弹出 3 选 1 本地奖励面板，选择奖励后 HUD 显示奖励摘要并进入下一房间，房间状态、敌人、怪物数和出口封印重置，默认场景扩展为 2-3 只训练敌人的小波次，并补离线原型技能命中，未启动后端也能推进小房间循环；影响范围：Unity Presentation、Net、UI、Prototype 场景工厂、`Proto_CombatField` 场景、EditMode 测试和客户端反馈文档；验证：阶段相关 EditMode `52/52` 通过，全量 EditMode `84/84` 通过，Unity MCP 离线场景探针确认 4 次本地技能命中清完 2 只怪、第一只死亡后剩余 `1`、第二只死亡后开出口、奖励面板弹出、选择奖励后进入 `Room 2` 且三只敌人激活 <2026-06-10>

### 问题记录

- 【✔】 R-20260606-unity-client / 阶段 8：用户反馈“确实能动，但是连半成品都算不上，太过简陋”；处理结果：本轮优先补 HUD 布局、角色轮廓、场景层次和战斗反馈，而不是继续推进后端协议 <2026-06-10>
- 【✔】 R-20260606-unity-client / 阶段 8：MCP 内置 `manage_camera` batchmode 截图请求未稳定落盘；处理结果：改用 Unity Camera 渲染临时 PNG 到系统临时目录做视觉 QA，未把截图产物保留在仓库 <2026-06-10>

### 变更记录

| 需求ID | 阶段 | 文件/配置 | 修改内容 | 回滚方法 |
|--------|------|-----------|----------|----------|
| R-20260606-unity-client | 阶段 8 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 增加 HUD 背板/安全区、地面柔化层、草簇/石块/训练区域、多层玩家/敌人轮廓，并重新生成场景 | 回退场景工厂改动后重新运行 `ProtoCombatFieldSceneGenerator.BuildDefaultScene()` |
| R-20260606-unity-client | 阶段 8 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs` | 增加 HUD 安全区、地面弱化、环境装饰和角色轮廓的 EditMode 断言 | 回退新增断言 |
| R-20260606-unity-client | 阶段 9 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Art/Characters/PlayerLightSword_Idle_Front.png`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Editor/Prototype/ProtoCombatFieldSceneGenerator.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 从角色规格效果图提取青白轻剑主角正面待机 Sprite，并让玩家 `VisualRoot/CharacterSprite` 使用该 Sprite | 删除角色 PNG/.meta，回退场景生成器和场景工厂后重新生成场景 |
| R-20260606-unity-client | 阶段 9 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Art/Characters/PlayerLightSword_*.png`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/PlayerSpriteAnimator.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/LocalPlayerMotor.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/SkillIntentController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Net/FirstChainHudController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 生成并接入 4 向 idle/walk、4 向攻击、4 向受击主角帧，新增玩家 Sprite 动画控制器，并让移动、技能和玩家受击驱动换帧 | 删除新增 `PlayerLightSword_*.png/.meta`、`PlayerSpriteAnimator.cs/.meta` 和对应测试，回退移动/技能/HUD/场景工厂改动后重新生成场景 |
| R-20260606-unity-client | 阶段 9 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/LocalPlayerMotor.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/PrototypeCombatFeedbackPresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/RogueRoomScenePresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Net/FirstChainHudController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 增加玩家世界边界、技能命中范围预览、敌人 Idle/Hit 状态反馈、秘境房间色层/怪物标记/出口封印，并接入场景默认生成 | 删除新增 `RogueRoomScenePresenter.cs/.meta` 和对应测试，回退移动/反馈/HUD/场景工厂改动后重新生成场景 |
| R-20260606-unity-client | 阶段 9 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Net/FirstChainHudControllerTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs` | 新增玩家边界、技能范围、敌人状态、秘境房间反馈和网络事件接线的 EditMode 测试 | 回退新增/修改的对应测试文件 |
| R-20260606-unity-client | 阶段 10 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/TrainingEnemyMotor.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/SkillIntentController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 新增训练敌人本地追击/避障/边界约束，技能输入增加冷却拦截和 HUD 冷却文本，并让战斗反馈只抖动敌人 `VisualRoot`，避免覆盖敌人根节点 AI 位移 | 删除 `TrainingEnemyMotor.cs/.meta` 和对应测试，回退技能冷却与场景工厂接线后重新生成场景 |
| R-20260606-unity-client | 阶段 10 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/TrainingEnemyMotorTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/SkillIntentControllerTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs` | 新增训练敌人追击/避障/边界、技能冷却和默认场景接线的 EditMode 测试 | 回退新增/修改的对应测试文件 |
| R-20260606-unity-client | 阶段 10 | `docs/client/proto-combatfield-backend-feedback.md`、`task_plan.md` | 补阶段 10 运行态验收结果，以及实体权威状态、地图阻挡/出口、技能冷却和命中结果字段缺口；阶段计划推进到阶段 11 | 回退文档中阶段 10 相关段落，并把 `task_plan.md` 当前阶段恢复为阶段 10 进行中 |
| R-20260606-unity-client | 阶段 11 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/TrainingEnemyMotor.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/PrototypeCombatLoopController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Net/FirstChainHudController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/PrototypeCombatFeedbackPresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/RogueRoomScenePresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 新增敌人攻击事件/冷却/范围提示、原型战斗 loop 事件桥接、玩家扣血、击杀清怪、开门和死亡终态表现，并重新生成默认场景 | 删除 `PrototypeCombatLoopController.cs/.meta`，回退阶段 11 相关脚本和场景工厂改动后重新生成场景 |
| R-20260606-unity-client | 阶段 11 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/TrainingEnemyMotorTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/PrototypeCombatLoopControllerTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Net/FirstChainHudControllerTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/RogueRoomScenePresenterTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/PrototypeCombatFeedbackPresenterTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs` | 新增敌人攻击、玩家扣血、击杀清怪、房间开门、敌人死亡和默认场景接线的 EditMode 测试 | 回退新增/修改的对应测试文件 |
| R-20260606-unity-client | 阶段 11 | `docs/client/proto-combatfield-backend-feedback.md`、`task_plan.md` | 补阶段 11 小战斗闭环验收结果，以及敌人攻击、清怪开门和胜利状态字段缺口；阶段计划推进到阶段 12 | 回退文档中阶段 11 相关段落，并把 `task_plan.md` 当前阶段恢复为阶段 11 进行中 |
| R-20260606-unity-client | 阶段 12 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/PrototypeRogueRewardController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/RogueRoomScenePresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/PrototypeCombatLoopController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/PrototypeCombatFeedbackPresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/SkillIntentController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Net/FirstChainHudController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/UI/PrototypeHudPresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 新增出口交互、3 选 1 奖励面板、下一房间重置、多敌人小波次、离线原型技能命中、HUD 奖励摘要和默认场景接线 | 删除 `PrototypeRogueRewardController.cs/.meta`，回退阶段 12 相关脚本和场景工厂改动后重新生成场景 |
| R-20260606-unity-client | 阶段 12 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/PrototypeRogueRewardControllerTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/RogueRoomScenePresenterTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/PrototypeCombatFeedbackPresenterTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/SkillIntentControllerTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Net/FirstChainHudControllerTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/UI/PrototypeHudPresenterTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs` | 新增出口交互、奖励选择、下一房间、波次推进、离线技能命中、HUD 展示和默认场景接线的 EditMode 测试 | 回退新增/修改的对应测试文件 |
| R-20260606-unity-client | 阶段 12 | `docs/client/proto-combatfield-backend-feedback.md`、`task_plan.md` | 补阶段 12 验收结果，以及出口交互、奖励候选、下一房间和局内奖励 modifier 字段缺口；阶段计划推进到阶段 13 | 回退文档中阶段 12 相关段落，并把 `task_plan.md` 当前阶段恢复为阶段 12 进行中 |

## 2026-06-08

### 执行记录

- 【✔】 R-20260606-unity-client / 阶段 4：完成 Unity MCP 运行态 WebSocket 联调；结果：本地 gateway-ws 监听 `18080` 后，Unity Play Mode HUD 显示 `Conn: Online`、`Tick 1`、`Ent 2`、`REGION_SNAPSHOT entities=2` <2026-06-08>
- 【✔】 R-20260606-unity-client / 阶段 5：按 TDD 补技能意图、剑气占位和受击反馈；结果：新增 `BuildSkill`、`SkillIntentController`、`PrototypeCombatFeedbackPresenter`，并让 `FirstChainWsClient` 在技能排队后立即发送命令；验证：Unity EditMode `32/32` 通过，MCP Play Mode 触发技能后收到 `DAMAGE_EVENT hpDelta=-12` <2026-06-08>
- 【✔】 R-20260606-unity-client / 阶段 6：按 TDD 接入秘境开始、房间展示和结算反馈；结果：新增 `BuildStartRogue` / `BuildFinishRogue`、`RogueDebugController`、秘境房间摘要和奖励摘要 HUD；验证：Unity EditMode `41/41` 通过，MCP Play Mode 验证 `START_ROGUE` 后显示 `Rogue 4001  Inst 9000001  Map 2001  Monsters 8  Pool 5001`，`FINISH_ROGUE` 后显示 `Reward item 6001 x3`，Console 无 error/warning <2026-06-08>
- 【→】 R-20260606-unity-client / 阶段 7：开始整理后端接口、配置和数据库反馈；当前处理：已把阶段 6 的秘境字段缺口补入 `docs/client/proto-combatfield-backend-feedback.md`；下一步：形成下一轮后端最小字段和配置开发计划 <2026-06-08>

### 问题记录

- 【✔】 R-20260606-unity-client / 阶段 4：gateway-ws 默认相对配置路径启动失败，提示缺少 `player_template.csv`；处理结果：联调时使用 `WENJIAN_CONFIG_SOURCE_DIR=E:\qfc\workspace\wenjian-game\config\source` 显式指定配置目录，并记录到客户端反馈文档 <2026-06-08>
- 【❓】 R-20260606-unity-client / 阶段 5：Unity MCP batchmode 不支持 `scene_view` 截图，`game_view` 截图请求未稳定落盘；当前处理：本轮用 MCP 读取 HUD/反馈组件状态和 Console 作为运行态证据；需要：后续如需视觉验收，改用可见 Unity Editor 或非 batchmode 截图 <2026-06-08>
- 【❓】 R-20260606-unity-client / 阶段 6：`ROGUE_START` 当前只给 `monsterId/monsters/rewardPoolId/entities` 摘要；当前处理：客户端只能显示房间摘要，不能真实生成怪物列表和出口；需要：后端补房间状态、清怪条件、怪物实体列表和出口状态 <2026-06-08>
- 【❓】 R-20260606-unity-client / 阶段 6：`ROGUE_FINISH` 当前只给 `itemId/count`，且调试链路允许未清怪直接结算；当前处理：客户端只显示奖励数字摘要；需要：后端补奖励详情来源、多奖励结构和服务端清怪/幂等校验 <2026-06-08>

### 变更记录

| 需求ID | 阶段 | 文件/配置 | 修改内容 | 回滚方法 |
|--------|------|-----------|----------|----------|
| R-20260606-unity-client | 阶段 5 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Net/` | 新增 `BuildSkill`，让技能命令可排队并通知 WebSocket 立即发送 | 回退 `FirstChainCommandBuilder.cs`、`FirstChainHudController.cs`、`FirstChainWsClient.cs` 的阶段 5 改动 |
| R-20260606-unity-client | 阶段 5 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/` | 新增技能输入控制器、剑气/伤害反馈组件和对应 EditMode 测试 | 删除新增脚本/测试及 `.meta` |
| R-20260606-unity-client | 阶段 5 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 场景工厂和场景接入 `CombatFeedback`、`SkillIntentController`、剑气占位和伤害飘字 | 回退场景工厂并重新生成场景 |
| R-20260606-unity-client | 阶段 5 | `docs/client/proto-combatfield-backend-feedback.md` | 新增阶段 4/5 后端接口反馈初稿 | 删除该文档 |
| R-20260606-unity-client | 阶段 6 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Net/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Net/` | 新增秘境开始/结算命令和 `ROGUE_START` / `ROGUE_FINISH` 响应解析测试 | 回退 `FirstChainCommandBuilder.cs`、`FirstChainHudController.cs` 和对应测试的阶段 6 改动 |
| R-20260606-unity-client | 阶段 6 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/RogueDebugController.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/RogueDebugControllerTests.cs` | 新增秘境调试控制器，支持运行态触发开始和结算 | 删除新增脚本/测试及 `.meta` |
| R-20260606-unity-client | 阶段 6 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/UI/PrototypeHudPresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | HUD 和场景接入秘境房间摘要、奖励摘要和 `RogueDebugController` | 回退 UI/场景工厂改动并重新生成场景 |
| R-20260606-unity-client | 阶段 6 | `docs/client/proto-combatfield-backend-feedback.md` | 补充秘境开始、房间状态、结算奖励和后端优先级反馈 | 回退该文档的阶段 6 新增段落 |

## 2026-06-06

### 执行记录

- 【✔】 R-20260606-unity-client / 阶段 0：生成 Unity 客户端优先开发计划；结果：新增 `docs/plans/wenjian-unity-proto-combatfield-client-plan.md`，明确先补客户端，用客户端反馈约束后端 <2026-06-06 14:59>
- 【✔】 R-20260606-task-log-cleanup / 阶段 0：用户反馈旧日志只有日期，没有需求和阶段；结果：新版 skill 改为需求索引式日志模型 <2026-06-06>
- 【✔】 R-20260606-task-log-cleanup / 阶段 1：第一次按新版 skill 整理 `task_plan.md`；结果：结构完整但太复杂 <2026-06-06>
- 【✔】 R-20260606-task-log-cleanup / 阶段 2：用户反馈文件看不懂、结构太复杂、已结束需求不应放在索引；处理结果：索引只保留未结束需求，历史需求只留日期证据 <2026-06-06>
- 【✔】 R-20260606-task-log-cleanup / 阶段 2：用户确认“现在好多了，可以执行客户端开发”；结果：日志整理需求结束，从需求索引和需求详情移出，只保留日期证据 <2026-06-06 16:40>
- 【✔】 R-20260606-unity-client / 阶段 1：安装 Unity MCP 并检查 Unity 客户端环境；结果：`com.coplaydev.unity-mcp` 已作为嵌入式包接入 Unity 工程，Codex `config.toml` 已配置 `unityMCP`，`uv/uvx` 和 `unity-mcp-skill` 已安装；验证：Unity batchmode 加载工程成功，`uvx --from mcpforunityserver==9.7.1 mcp-for-unity --help` 已成功；未完成：当前会话未暴露 Unity MCP 工具，MCP server 8080 未常驻启动，需重启 Codex 后继续验证 <2026-06-06 16:40>
- 【✔】 R-20260606-unity-client / 阶段 2：生成 `Proto_CombatField` 第一张场景；结果：生成 Tilemap、占位地块、玩家、训练敌人、主相机和 Build Settings 场景入口；验证：场景文件、占位 PNG、Tile asset、`EditorBuildSettings.asset` 均存在，`unity-generate-proto-scene-after-movement.log` 记录生成成功 <2026-06-06 16:40>
- 【✔】 R-20260606-unity-client / 阶段 3：补玩家本地移动和相机跟随；结果：已 TDD 新增 `LocalPlayerMotor`、`CameraFollow2D` 和场景工厂挂载逻辑；验证：EditMode 测试 `14/14` 通过 <2026-06-06 16:40>
- 【✔】 R-20260606-unity-client / 阶段 3：补基础 HUD 和调试面板；结果：新增 `PrototypeHudPresenter`、HUD 快照、连接/玩家/区域/调试/最近事件文本、玩家和敌人血条，并序列化到 `Proto_CombatField` 场景；验证：场景生成器运行成功，最终 EditMode 测试 `17/17` 通过 <2026-06-06 17:36>
- 【→】 R-20260606-unity-client / 阶段 4：补 WebSocket 客户端侧接入；当前处理：已新增 `FirstChainCommandBuilder`、`FirstChainHudController`、`FirstChainWsClient`，场景生成器已写入 `FirstChainClient` 节点；验证：最终 EditMode 测试 `23/23` 通过；下一步：启动本地后端并运行 Unity 场景做真实联调 <2026-06-06 17:46>

### 问题记录

- 【✔】 R-20260606-unity-client / 阶段 0：用户反馈“前端 Unity 还没有场景、特效”；处理结果：确认当前只有视觉标准和原型壳，没有真实 Unity 工程，并生成客户端优先计划 <2026-06-06 14:59>
- 【✔】 R-20260606-task-log-cleanup / 阶段 2：用户反馈 `task_plan.md` 太复杂；处理结果：移除已结束需求索引和历史需求详情，只保留当前需求与下一步 Unity 需求 <2026-06-06>
- 【❓】 R-20260606-unity-client / 阶段 1：Unity MCP 包在 Unity 日志中提示 `No process found listening on port 8080`；当前处理：Unity 包、Codex 配置和 `uvx` server 依赖均已安装，当前开发暂用 Unity batchmode 验证；需要：重启 Codex 后确认是否出现 `unityMCP` 工具，并由 Unity MCP/uvx 启动 HTTP server <2026-06-06 16:40>
- 【❓】 R-20260606-unity-client / 阶段 3：HUD 当前只能显示原型快照；缺口：真实服务端快照还没提供实体 `hp/maxHp/name/visualId/state` 和技能冷却数据；需要：阶段 4 联调时把缺口写入后端反馈清单 <2026-06-06 17:36>
- 【❓】 R-20260606-unity-client / 阶段 4：`REGION_SNAPSHOT` 仍只有 `entities` 数量，没有实体详情；当前处理：客户端只能在 HUD 显示 entity 摘要并标记 `LastSnapshotMissingEntityDetails`；需要：后端后续提供实体列表、坐标、hp/maxHp、entityType、visualId、facing、state <2026-06-06 17:46>

### 变更记录

| 需求ID | 阶段 | 文件/配置 | 修改内容 | 回滚方法 |
|--------|------|-----------|----------|----------|
| R-20260606-unity-client | 阶段 0 | `docs/plans/wenjian-unity-proto-combatfield-client-plan.md` | 新增 Unity 客户端优先开发计划 | 删除该文档 |
| R-20260606-task-log-cleanup | 阶段 2 | `task_plan.md` | 索引只保留未结束需求；已完成需求从索引和详情中移除，历史记录压缩到日期记录 | 恢复上一版详细需求索引 |
| R-20260606-unity-client | 阶段 1 | `wenjian-client/Proto_CombatField/UnityProject/Packages/manifest.json`、`wenjian-client/Proto_CombatField/UnityProject/Packages/packages-lock.json`、`wenjian-client/Proto_CombatField/UnityProject/Packages/com.coplaydev.unity-mcp/` | 接入 Unity MCP 嵌入式包 | 移除 manifest 依赖、packages-lock 条目和嵌入式包目录 |
| R-20260606-unity-client | 阶段 1 | `.gitignore` | 给 Unity MCP 嵌入式包 `Editor/Tools/Build/` 加白名单，避免通用 Unity `Build/` 忽略规则漏掉安装包源码 | 删除对应白名单两行 |
| R-20260606-unity-client | 阶段 1 | 外部配置：`C:\Users\Administrator\.codex\config.toml`、`C:\Users\Administrator\.codex\skills\unity-mcp-skill\` | 配置 Codex `unityMCP` server 并安装 Unity MCP skill | 删除对应 `unityMCP` 配置和 skill 目录 |
| R-20260606-unity-client | 阶段 2 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Editor/Prototype/ProtoCombatFieldSceneGenerator.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Art/`、`wenjian-client/Proto_CombatField/UnityProject/ProjectSettings/EditorBuildSettings.asset` | 新增场景生成器、占位美术、Tile asset、第一张战斗场景和 Build Settings 入口 | 删除对应生成资源并恢复 Build Settings |
| R-20260606-unity-client | 阶段 3 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Presentation/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Presentation/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs` | 新增本地玩家移动、相机跟随和对应 EditMode 测试，并让场景工厂挂载组件 | 删除新增 Presentation 脚本/测试并回退场景工厂挂载逻辑 |
| R-20260606-unity-client | 阶段 3 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/UI/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/UI/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 新增基础 HUD、调试面板、血条和对应测试，并让场景工厂挂载 HUD | 删除新增 UI 脚本/测试并回退场景工厂 HUD 挂载逻辑后重新生成场景 |
| R-20260606-unity-client | 阶段 4 | `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Net/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Net/`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/UI/PrototypeHudPresenter.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scripts/Prototype/CombatFieldSceneFactory.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/Prototype/CombatFieldSceneFactoryTests.cs`、`wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Scenes/Proto_CombatField.unity` | 新增 WebSocket 客户端、登录/进入区域命令、服务端响应到 HUD 的控制器和对应测试，并让场景工厂挂载 `FirstChainClient` | 删除新增 Net 脚本/测试并回退场景工厂 `FirstChainClient` 挂载逻辑后重新生成场景 |

## 历史日期记录

这里仅用于查证过去做过什么，不作为当前计划入口。

### 2026-06-05

- 【✔】 R-20260605-core-config / 阶段 0：完成版本控制、Java、Maven 和修改前基线检查 <2026-06-05 17:38>
- 【✔】 R-20260605-core-config / 阶段 1：完成后端第一链路 core 下沉与配置驱动；验证：core、gateway、全量后端测试和 `git diff --check` 通过 <2026-06-05 17:38>

### 2026-06-04

- 【✔】 R-20260604-core-plan / 阶段 0：分析现有计划、进度、发现记录和当前代码 <2026-06-04 11:44>
- 【✔】 R-20260604-core-plan / 阶段 1：生成阶段一 D core 下沉计划 `docs/plans/wenjian-cursor-stage1-core-plan.md` <2026-06-04 11:44>

### 2026-05-30

- 【❓】 R-20260530-protobuf-config-plan / 阶段 0：拉取远端失败；阻塞点：GitHub 443 连接失败；需要：网络恢复后重新 fetch <2026-05-30>
- 【✔】 R-20260530-protobuf-config-plan / 阶段 1：生成 Cursor 第一轮阶段开发计划 <2026-05-30>
- 【✔】 R-20260530-protobuf-config-plan / 阶段 2：修正 protobuf 生成路径、protoc 获取方式、测试依赖和 DTO 类型引用问题；验证：protobuf、gateway、全量后端测试通过 <2026-05-30>
- 【✔】 R-20260530-protobuf-config-plan / 阶段 3：生成配置阶段计划 `docs/plans/wenjian-cursor-stage1-config-plan.md` <2026-05-30>

### 2026-05-21

- 【✔】 R-20260521-minimal-chain / 阶段 0：确认方案 C，并新增前后端并行最小链路实施计划 <2026-05-21 12:05>
- 【✔】 R-20260521-minimal-chain / 阶段 1：安装 Maven 3.9.16，验证 Java 21，创建后端 Maven 多模块骨架 <2026-05-21 12:05>
- 【✔】 R-20260521-minimal-chain / 阶段 2：完成登录与进入区域最小链路和 Unity 原型壳说明 <2026-05-21 15:38>
- 【✔】 R-20260521-minimal-chain / 阶段 3：完成 WebSocket 登录和进入区域集成测试、入口、Handler 和手动验收 <2026-05-21 16:34>
- 【✔】 R-20260521-minimal-chain / 阶段 4：完成单技能意图、技能事件和伤害事件文本调试协议 <2026-05-21 19:07>
- 【✔】 R-20260521-minimal-chain / 阶段 5：完成单人秘境开始与结算协议、最小内存实例状态和验证 <2026-05-21 20:15>

### 2026-05-20

- 【✔】 R-20260520-art-standard / 阶段 0：复盘首批概念美术素材，确认不继续沿用偏写实、偏暗、偏厚重方向 <2026-05-20 18:57>
- 【✔】 R-20260520-art-standard / 阶段 1：确认第二轮美术方向为明快清爽的像素武侠、轻量动作肉鸽、战斗读得清楚 <2026-05-20 18:57>
- 【✔】 R-20260520-art-standard / 阶段 2：生成第二轮 7 张可执行美术标准素材并保存到项目目录 <2026-05-20 18:57>
- 【 】 R-20260520-art-standard / 阶段 3：等待用户评审第二轮标准素材，决定是否冻结第一版美术执行标准 <2026-05-20 18:57>
