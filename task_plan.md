# 问剑江湖项目任务计划

## 当前目标

当前主线是先做 Unity 客户端真实原型，用客户端场景、输入、HUD、特效和联调反馈来反推后端接口、字段、配置和数据库。

下一步启动本地后端并做 Unity 运行态联调，验证 WebSocket 登录和区域快照能在 HUD 上真实显示；后端数据库和复杂协议暂不继续猜。

## 需求索引

只放还没结束、后面还要继续跟的需求。已经完成的历史需求不放这里，只留在下面日期记录里查证。

| 需求ID | 需求 | 当前状态 | 当前阶段 | 下一步 |
|--------|------|----------|----------|--------|
| R-20260606-unity-client | 先做 Unity 客户端真实原型 | 进行中 | 阶段 4：WebSocket 登录和区域快照 | 启动本地后端，运行 Unity 场景，确认 HUD 显示真实 `LOGIN_OK` / `REGION_SNAPSHOT` |

## 需求详情

### R-20260606-unity-client：先做 Unity 客户端真实原型

- 目标：先把 Unity 客户端跑起来，再决定后端还需要哪些接口、字段、配置和数据库设计。
- 计划文档：`docs/plans/wenjian-unity-proto-combatfield-client-plan.md`
- 当前：Unity 工程和第一张 `Proto_CombatField` 场景已生成，玩家可本地移动，相机可跟随，基础 HUD、调试面板和 WebSocket 客户端脚本已挂入场景；真实后端联调、特效和技能反馈还没做。

#### 阶段

- 【✔】 阶段 0：生成客户端开发计划；结果：已明确先补 Unity 工程、场景、输入、HUD、WebSocket、技能、秘境和后端反馈 <2026-06-06>
- 【✔】 阶段 1：检查 Unity 环境并创建真实 Unity 工程；结果：使用本机 Unity `2022.3.62f3c1` 创建 `wenjian-client/Proto_CombatField/UnityProject`，接入 `com.coplaydev.unity-mcp` 嵌入式包，Codex config 已写入 `unityMCP` server；影响范围：Unity 工程、Packages、Codex 本地配置；验证：Unity batchmode 能加载工程，`uvx --from mcpforunityserver==9.7.1 mcp-for-unity --help` 已成功；未完成：当前 Codex 会话未暴露 Unity MCP 工具，HTTP server 8080 未常驻启动 <2026-06-06>
- 【✔】 阶段 2：创建竹林战斗场、玩家、训练敌人和原型相机；结果：生成 `Assets/_Wenjian/Scenes/Proto_CombatField.unity`、Tilemap、占位地块、玩家、训练敌人和正交主相机；影响范围：Unity 场景、占位美术、场景生成器；验证：`unity-generate-proto-scene-after-movement.log` 生成成功，最终 EditMode 测试 `14/14` 通过；未完成：Pixel Perfect/URP 包尚未接入，后续画面 QA 时处理 <2026-06-06>
- 【✔】 阶段 3：实现本地 8 方向输入、HUD 和调试面板；结果：`LocalPlayerMotor` 支持原型本地 8 方向移动，`CameraFollow2D` 跟随玩家，`PrototypeHudPresenter` 显示连接状态、玩家血量、敌人血量、区域/秘境状态、serverTick、坐标和最近事件；影响范围：Unity 运行时 UI、场景工厂和 EditMode 测试；验证：最终 EditMode 测试 `17/17` 通过；未完成：Input System 包尚未接入，当前仍用 Legacy Input 原型输入 <2026-06-06>
- 【→】 阶段 4：接入 WebSocket 登录和进入区域；已完成：`FirstChainWsClient` 已实现连接 `ws://127.0.0.1:18080/ws/first-chain`、发送 `LOGIN` 和接收文本消息的客户端逻辑，`FirstChainHudController` 可把 `LOGIN_OK` / `REGION_SNAPSHOT` 更新到 HUD，并在 `LOGIN_OK` 后排队 `ENTER_REGION`；当前停在：还没启动本地后端做 Unity 运行态联调和截图；下一步：启动 gateway-ws，运行 Unity 场景，确认 HUD 显示真实返回并记录字段缺口 <2026-06-06>
- 【 】 阶段 5：接入技能事件、剑气特效和受击反馈
- 【 】 阶段 6：接入秘境开始和结算反馈
- 【 】 阶段 7：整理后端接口、配置和数据库反馈

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
