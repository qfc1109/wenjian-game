# 问剑江湖 Cursor 阶段一 D 开发计划：Core 最小服务下沉与配置驱动

## 1. PM 结论

阶段一 A/B 已完成 protobuf Java 生成和 gateway protobuf mapper，阶段一 C 已完成 CSV 配置加载与校验。下一轮 Cursor 只执行阶段一 D：把当前 `wenjian-gateway-ws` 内的第一链路业务规则下沉到 `wenjian-game-core`，并让 core 通过 `wenjian-config` 读取真实 CSV 配置。

本阶段完成后必须停止，交给 PM 审查；不要继续做服务端权威 Tick、移动同步、KCP、数据库、账号持久化、Unity 工程或二进制 WebSocket 协议切换。

一句话目标：core 负责登录、进入区域、单技能、秘境开始和秘境结算的最小业务规则；gateway 只保留文本协议接入、参数解析、调用 core、protobuf mapper 和文本响应格式化。

## 2. 现有计划分析结论

- `docs/plans/wenjian-development-roadmap.md` 的阶段一目标是把文本调试 Demo 变成 protobuf、config、core 真实生效的工程链路。
- `docs/plans/wenjian-cursor-stage-development-plan.md` 已覆盖阶段一 A/B，当前代码中已有 protobuf 生成流程和 `FirstChainProtocolMapper`。
- `docs/plans/wenjian-cursor-stage1-config-plan.md` 已覆盖阶段一 C，当前代码中已有 `CsvConfigLoader`、`GameConfigRepository`、配置 model 和配置校验测试。
- `docs/plans/minimal-dual-track-chain-plan.md` 的文本调试链路已完成，下一步不是继续扩展临时文本 Demo，而是把这条链路迁移到真实模块边界。
- `progress.md` 和 `task_plan.md` 中部分旧描述仍停留在“准备进入配置加载”，后续执行应以本文档和路线文档为准。

## 3. 当前工程事实

- `wenjian-game-core` 当前只有 `pom.xml`，尚无业务代码。
- `wenjian-game-core` 已依赖 `wenjian-config`、`wenjian-protobuf`、`wenjian-share`，依赖方向可以承载本阶段下沉。
- `wenjian-gateway-ws` 已依赖 `wenjian-game-core`，不需要新增模块。
- `FirstChainGatewayService` 仍持有玩家、区域、技能、怪物、秘境、奖励等硬编码常量。
- `GatewayDtos.java` 中的 DTO 仍在 gateway 包内，当前 mapper 和 WebSocket 文本响应都依赖这些类型。
- `FirstChainWebSocketHandler` 仍负责文本协议解析和文本响应格式化，这一层本阶段可以保留。
- 真实配置中 `player_template.csv` 和 `region.csv` 的出生点是 `3200,2400`，旧 gateway 测试仍断言 `10,12`。迁移到配置驱动时必须同步更新测试期望。

## 4. 本阶段边界

本轮可以做：

- 新增 `wenjian-game-core/src/main/java/com/wenjian/core/firstchain/...`。
- 新增 `wenjian-game-core/src/test/java/com/wenjian/core/firstchain/...`。
- 修改 `wenjian-game-core/pom.xml`，补充测试依赖。
- 修改 `FirstChainGatewayService`，让它成为 gateway 到 core 的薄适配层。
- 修改 `FirstChainWebSocketConfig`，加载 `GameConfigRepository` 并创建 core service。
- 修改 gateway 单测、mapper 测试和 WebSocket 集成测试的期望值。
- 如有必要，更新 `wenjian-game-server/README.md` 中的当前行为说明。

本轮不要做：

- 不切换外部 WebSocket 文本协议为二进制 protobuf。
- 不新增 protobuf 字段。
- 不做 KCP。
- 不做服务端 Tick、输入帧队列、移动校验、技能冷却/前摇/命中判定。
- 不做数据库、账号、角色持久化、背包、任务、邮件、商城、排行榜。
- 不创建 Unity 正式工程。
- 不新增 `wenjian-battle`、`wenjian-world`、`wenjian-rogue` 等模块。
- 不提交 `target/`、生成 Java 文件或临时脚本。

## 5. 推荐设计

### 5.1 Core 包结构

推荐新增：

```text
wenjian-game-server/wenjian-game-core/src/main/java/com/wenjian/core/firstchain/
  FirstChainCoreService.java
  FirstChainCoreDtos.java
```

`FirstChainCoreDtos` 可以先用一个 public final class 承载最小 nested enum/record，避免第一轮为 DTO 创建过多文件。建议包含：

- `ResultCode`
- `EntityKind`
- `GridPosition`
- `GridVector`
- `LoginResult`
- `EntitySnapshot`
- `EnterRegionResult`
- `SkillEvent`
- `DamageEvent`
- `SkillCastResult`
- `RogueStartResult`
- `RewardItem`
- `RogueFinishResult`

后续进入正式战斗竖切时，再根据复杂度拆成领域文件。

### 5.2 Core 服务职责

`FirstChainCoreService` 负责：

- 持有 `GameConfigRepository`。
- 从 `player_template` 读取初始区域、出生点、最大生命和默认技能。
- 从 `skill` 读取技能伤害。
- 从 `monster` 读取训练怪物生命。
- 从 `rogue` 读取秘境地图、出生点、怪物、怪物数量和奖励池。
- 从 `reward_pool` 读取奖励道具和数量。
- 维护最小内存秘境实例状态，保持“未启动不能结算”的现有行为。

本阶段可以继续保留的临时固定值：

- 开发玩家 ID：`1000001`，因为账号和角色系统尚未建立。
- 训练敌人实体 ID：`2000001`，因为实体分配器尚未建立。
- 默认秘境实例 ID：`9000001`，因为实例 ID 生成器尚未建立。
- 区域快照 tick：`1`，因为服务端 Tick 尚未建立。

这些固定值应集中在 core 内，并在注释或测试名中表明是第一链路临时值；不要继续散落在 gateway。

### 5.3 Gateway 适配策略

为降低改动面，本阶段建议暂时保留 `GatewayDtos.java` 和 `FirstChainProtocolMapper` 的外部形状：

- `FirstChainGatewayService` 内部持有 `FirstChainCoreService`。
- `FirstChainGatewayService.createDefault()` 或 Spring 配置负责创建配置仓库和 core service。
- `FirstChainGatewayService` 把 core DTO 转成 gateway DTO。
- `FirstChainWebSocketHandler` 继续调用 `FirstChainGatewayService`，文本请求和响应格式不变。
- `FirstChainProtocolMapper` 继续从 gateway DTO 映射到 protobuf，避免本轮同时重写 mapper 边界。

下一轮可再决定是否删除 `GatewayDtos.java`，让 gateway 直接使用 core DTO 或 protobuf 类型。

### 5.4 配置加载入口

推荐在 `FirstChainWebSocketConfig` 中通过 Spring 属性指定配置目录：

```text
wenjian.config.source-dir=../config/source
```

要求：

- 默认值只能服务本地 README 中的启动方式。
- 配置目录不存在或 CSV 错误时必须启动失败，不允许静默兜底。
- 测试中可以显式设置该属性，保证 WebSocket 集成测试加载真实 `config/source`。

## 6. 执行步骤

### Checkpoint 0：基线确认

执行：

```powershell
git status --short --branch
$env:JAVA_HOME='E:\JAVA\jdk21'
$env:MAVEN_HOME='E:\apache-maven-3.9.16'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
java -version
mvn -v
cd E:\qfc\workspace\wenjian-game\wenjian-game-server
mvn -q test
```

通过标准：

- 当前分支为 `codex/` 前缀分支，不在 `main/master`。
- 没有与本轮无关的未提交改动。
- Java/Maven 可运行。
- 修改前全量后端测试通过。

失败处理：

- 如果修改前测试失败，停止并记录失败输出，不继续改代码。
- 如果工作区已有无关改动，停止并说明文件列表。

### Checkpoint 1：新增 core 最小服务和单测

预计修改：

- `wenjian-game-server/wenjian-game-core/pom.xml`
- `wenjian-game-server/wenjian-game-core/src/main/java/com/wenjian/core/firstchain/FirstChainCoreDtos.java`
- `wenjian-game-server/wenjian-game-core/src/main/java/com/wenjian/core/firstchain/FirstChainCoreService.java`
- `wenjian-game-server/wenjian-game-core/src/test/java/com/wenjian/core/firstchain/FirstChainCoreServiceTest.java`

测试必须覆盖：

- `login` 返回开发玩家 `1000001`、初始区域 `1001`、出生点 `3200,2400`、最大生命 `100`。
- `enterRegion` 返回玩家自身和训练怪物，玩家坐标来自配置，训练怪物生命来自 `monster.csv`。
- 非开发玩家或错误区域返回 `NOT_FOUND`。
- `castSkill` 使用 `skill.csv` 中技能 `2001` 的伤害 `12`，返回 `hpDelta=-12`。
- 错误技能返回 `NOT_FOUND`。
- `startRogue` 使用 `rogue.csv` 中秘境 `4001`，返回地图 `2001`、出生点 `1500,1500`、怪物 `3001 x 8`、奖励池 `5001`。
- `finishRogue` 只有在实例启动后成功，奖励来自 `reward_pool.csv`：`itemId=6001 count=3`。
- 未启动实例直接结算返回 `NOT_FOUND`。

建议命令：

```powershell
mvn -q -pl wenjian-game-core -am test
```

### Checkpoint 2：把 gateway service 改成 core 适配层

预计修改：

- `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/FirstChainGatewayService.java`
- `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/FirstChainWebSocketConfig.java`
- `wenjian-game-server/wenjian-gateway-ws/src/test/java/com/wenjian/gateway/ws/FirstChainGatewayServiceTest.java`

要求：

- `FirstChainGatewayService` 不再保存技能伤害、怪物血量、秘境配置、奖励配置等业务配置常量。
- gateway service 只做 core DTO 到 gateway DTO 的转换。
- 保留现有 public/package-private 方法名：`login`、`enterRegion`、`castSkill`、`startRogue`、`finishRogue`，减少 WebSocket Handler 改动。
- 测试期望更新为配置值，例如登录和区域快照出生点从 `10,12` 改为 `3200,2400`。
- 旧文本协议格式继续可用。

建议命令：

```powershell
mvn -q -pl wenjian-gateway-ws -am test
```

### Checkpoint 3：更新 mapper、集成测试和 README

预计修改：

- `wenjian-game-server/wenjian-gateway-ws/src/test/java/com/wenjian/gateway/ws/FirstChainProtocolMapperTest.java`
- `wenjian-game-server/wenjian-gateway-ws/src/test/java/com/wenjian/gateway/ws/FirstChainWebSocketIntegrationTest.java`
- `wenjian-game-server/README.md`

要求：

- mapper 测试继续覆盖错误码、坐标、实体、技能、伤害、秘境奖励。
- 集成测试继续覆盖 `LOGIN`、`ENTER_REGION`、`SKILL`、`START_ROGUE`、`FINISH_ROGUE`。
- 集成测试断言配置驱动值：登录坐标 `3200,2400`、技能伤害 `-12`、秘境 `4001`、地图 `2001`、奖励 `6001 x 3`。
- README 明确当前临时文本协议未变，但响应值来自 `config/source`。

建议命令：

```powershell
mvn -q -pl wenjian-gateway-ws -am test
```

### Checkpoint 4：全量验证与交付

执行：

```powershell
cd E:\qfc\workspace\wenjian-game\wenjian-game-server
mvn -q -pl wenjian-game-core -am test
mvn -q -pl wenjian-gateway-ws -am test
mvn -q test
cd E:\qfc\workspace\wenjian-game
git diff --check
git status --short --branch
```

通过标准：

- core 模块测试通过。
- gateway 模块测试通过。
- 后端全量测试通过。
- `git diff --check` 通过；如仅有 Windows LF/CRLF 提示，需要在总结中说明。
- `git status` 只出现本轮相关文件。

## 7. PM 审查重点

PM 会重点检查：

- core 是否真的依赖 `GameConfigRepository`，而不是把 gateway 硬编码搬到 core 后继续伪配置化。
- gateway 是否变薄，是否还持有技能伤害、怪物血量、秘境奖励等配置常量。
- 是否保留现有文本协议兼容，避免本轮同时改变客户端接入方式。
- 配置错误是否仍由 `wenjian-config` 明确失败，没有静默默认值。
- 测试是否覆盖配置驱动值，而不是只断言对象非空。
- 是否没有越界进入战斗 Tick、KCP、数据库、Unity 或新模块拆分。

## 8. 本阶段完成后下一步

阶段一 D 完成并通过 PM 审查后，再进入阶段二的第一个竖切任务：服务端权威战斗最小 Tick。该任务应单独开计划，重点包括输入帧、实体生命状态、技能冷却、命中判定、死亡状态和秘境清怪后结算。

不要在本阶段顺手实现阶段二内容。
