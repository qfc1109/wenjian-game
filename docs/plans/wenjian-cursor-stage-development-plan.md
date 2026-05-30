# 问剑江湖 Cursor 阶段开发计划

## 1. PM 结论

本计划以 `docs/plans/wenjian-development-roadmap.md` 为主线。Cursor 第一轮只执行“阶段一 A/B”：建立 protobuf Java 生成流程，并让当前网关链路开始使用生成后的协议类型。完成后先停下，把结果交给 PM 检查，再决定是否进入配置加载和 core 下沉。

当前远端同步状态：2026-05-30 尝试 `git pull --ff-only`、`git fetch origin --prune`、`git ls-remote --heads origin` 均因 GitHub 443 连接失败未完成实时同步。本地 `HEAD` 为 `af8cfc4 中文：新增问剑江湖后续开发路线文档`，当前计划基于该本地版本生成。

当前 CodeGraph 状态：已执行 `codegraph init -i`，索引 13 个代码文件、113 个节点、170 条边。Cursor 完成结构性代码变更后，建议运行 `codegraph sync` 更新索引。

## 2. 当前工程事实

- `wenjian-gateway-ws` 已有文本调试链路：`LOGIN`、`ENTER_REGION`、`SKILL`、`START_ROGUE`、`FINISH_ROGUE`。
- `FirstChainGatewayService` 仍持有玩家、区域、技能、怪物、秘境、奖励等硬编码常量。
- `FirstChainWebSocketHandler` 同时负责文本解析、调用服务、格式化响应。
- `GatewayDtos.java` 中的 record/enum 是当前服务内部返回结构，尚未使用 protobuf 生成类型。
- `wenjian-protobuf`、`wenjian-config`、`wenjian-game-core` 目前基本是模块壳。
- `protobuf/` 已有 `common`、`account`、`world`、`battle`、`rogue` 五组 proto，覆盖当前最小链路。
- `config/source/` 已有玩家模板、地图、区域、技能、怪物、秘境、奖励池、道具 CSV，但服务端尚未正式加载。

## 3. Cursor 第一轮目标

一句话目标：让后端构建能稳定生成 Java protobuf 类型，并让现有登录、进入区域、技能、秘境链路开始经过 protobuf 类型映射验证，同时保持现有 WebSocket 文本集成测试继续通过。

本轮不做：

- 不接入 KCP。
- 不创建数据库、账号持久化、背包、任务、邮件、商城、排行榜。
- 不创建 Unity 正式工程。
- 不把 WebSocket 外部协议直接改成二进制 protobuf。
- 不新增无依据 proto 字段。
- 不提交 `target/` 或生成后的 Java 文件。

## 4. Cursor 执行步骤

### Checkpoint 0：基线确认

执行：

```powershell
git status --short --branch
$env:JAVA_HOME='E:\JAVA\jdk21'
$env:MAVEN_HOME='E:\apache-maven-3.9.16'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
java -version
mvn -v
cd D:\workspace\AI_project\wenjian-game\wenjian-game-server
mvn -q test
```

通过标准：

- 当前分支不是 `main/master`。
- 没有与本轮无关的未提交改动。
- Java/Maven 指向 Java 21 + Maven 3.9.x。
- 当前全量后端测试通过。

失败处理：

- 如果测试在修改前失败，停止并记录失败输出，不继续改代码。
- 如果 Git 状态已有无关改动，停止并说明文件列表。

### Checkpoint 1：建立 protobuf Java 生成流程

预计修改文件：

- `wenjian-game-server/pom.xml`
- `wenjian-game-server/wenjian-protobuf/pom.xml`
- 新增 `wenjian-game-server/wenjian-protobuf/src/test/java/com/wenjian/protobuf/ProtobufGeneratedTypesTest.java`

实现要求：

- 在 Maven 中固化 protobuf Java 生成流程，proto 源目录指向仓库根目录下的 `protobuf/`。
- 生成物输出到 `target/generated-sources/...`，不要把生成后的 Java 文件提交到仓库。
- `wenjian-protobuf` 需要依赖 `protobuf-java`。
- 测试至少构造并断言以下生成类型可用：
  - `com.wenjian.protobuf.account.LoginResp`
  - `com.wenjian.protobuf.world.EnterRegionResp`
  - `com.wenjian.protobuf.battle.SkillEvent`
  - `com.wenjian.protobuf.battle.DamageEvent`
  - `com.wenjian.protobuf.rogue.StartRogueResp`
  - `com.wenjian.protobuf.rogue.FinishRoguePush`

推荐检查命令：

```powershell
cd D:\workspace\AI_project\wenjian-game\wenjian-game-server
mvn -q -pl wenjian-protobuf -am test
```

通过标准：

- protobuf 生成类型能编译。
- smoke test 通过。
- `git status --short` 不出现 `target/` 或生成 Java 文件。

停止点：

- 如果 Maven 插件或 protoc 依赖无法下载，不手写生成文件；停止并汇报依赖解析错误。

### Checkpoint 2：让现有链路开始使用生成类型

预计修改文件：

- `wenjian-game-server/wenjian-gateway-ws/pom.xml`（如果缺少测试或 protobuf 依赖再改）
- 新增 `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/FirstChainProtocolMapper.java`
- 新增或修改 `wenjian-game-server/wenjian-gateway-ws/src/test/java/com/wenjian/gateway/ws/FirstChainProtocolMapperTest.java`
- 视需要小改 `FirstChainWebSocketHandler.java`

实现要求：

- 保留当前文本 WebSocket 请求和响应格式，保证现有集成测试不被客户端协议切换影响。
- 新增 mapper，把当前 `LoginResult`、`EnterRegionResult`、`SkillCastResult`、`DamageEvent`、`RogueStartResult`、`RogueFinishResult` 映射为 protobuf 生成类型。
- 文本格式化可以继续输出当前字符串，但数据来源应经过 protobuf 类型或 mapper 测试覆盖。
- 不急着删除 `GatewayDtos.java` 中的内部 record；它们下一轮随 core 下沉再整理。
- 映射时要显式转换错误码：`ResultCode.OK -> ERROR_CODE_OK`，`ResultCode.NOT_FOUND -> ERROR_CODE_NOT_FOUND`。
- 坐标、实体类型、奖励项必须逐项断言，不只检查对象非空。

推荐检查命令：

```powershell
cd D:\workspace\AI_project\wenjian-game\wenjian-game-server
mvn -q -pl wenjian-gateway-ws -am test
```

通过标准：

- mapper 单测通过。
- 原 `FirstChainWebSocketIntegrationTest` 仍通过。
- 登录、区域快照、技能事件、伤害事件、秘境开始、秘境结算的关键字段没有回归。

### Checkpoint 3：全量验证与交付给 PM

执行：

```powershell
cd D:\workspace\AI_project\wenjian-game\wenjian-game-server
mvn -q test
cd D:\workspace\AI_project\wenjian-game
git diff --check
git status --short --branch
```

通过标准：

- `mvn -q test` 通过。
- `git diff --check` 通过；如果只出现 Windows LF/CRLF 提示，需要在总结中说明。
- 只出现本轮相关文件变更。
- 不包含 `target/`、`.codegraph/`、生成 Java 文件或临时脚本。

Cursor 完成后需要回报：

- 实际修改文件列表。
- 新增 Maven 插件/依赖名称和版本。
- protobuf 生成目录。
- 新增测试类和覆盖范围。
- 三个检查命令的结果。
- 是否需要 PM 运行 `codegraph sync` 或复查调用关系。

## 5. PM 验收清单

Cursor 完成第一轮后，PM 重点检查：

- 是否真的从 `protobuf/` 生成 Java 类型，而不是手写同名类。
- `wenjian-protobuf` 是否保持为生成类型承载模块，没有混入业务规则。
- `wenjian-gateway-ws` 是否只做临时文本接入和映射，没有继续扩大硬编码业务。
- 现有文本链路是否保持兼容。
- mapper 是否覆盖错误码、坐标、实体、技能、伤害、秘境奖励。
- 是否没有引入 KCP、数据库、Unity、背包等越界内容。

PM 验收通过后，再进入下一轮。

## 6. 后续阶段草案

### 阶段一 C：CSV 配置加载与校验

目标：`wenjian-config` 正式加载 `config/source/*.csv`，提供主键唯一、必填字段、外键关系校验。完成后 `FirstChainGatewayService` 不再直接硬编码技能伤害、秘境怪物、奖励等配置值。

检查点：

- 配置加载单测。
- 错误配置失败测试。
- 网关链路仍通过。

### 阶段一 D：core 最小服务下沉

目标：把登录、进入区域、技能、秘境的规则从 `wenjian-gateway-ws` 下沉到 `wenjian-game-core`，网关只保留接入、解析、映射和返回。

检查点：

- core 单测覆盖四条链路。
- gateway 集成测试只验证接入层。
- `FirstChainGatewayService` 变薄或被 core facade 替代。

### 阶段二：服务端权威战斗竖切

目标：建立 tick、输入帧、移动校验、技能冷却/前摇/命中/伤害、实体状态和快照输出。秘境必须清怪后才能结算。

检查点：

- 无客户端也能用测试驱动 tick。
- 技能命中由服务端计算。
- 怪物死亡后才能结算奖励。

### 阶段三：Unity Proto_CombatField 原型

目标：Unity 正式连接本地服务端，展示玩家、敌人、技能方向、受击反馈、秘境结算。

检查点：

- 客户端能登录进入区域。
- 画面可读，输入延迟可接受。
- 客户端只表现服务端结果。

## 7. 当前建议

请 Cursor 先只执行本文件的 Checkpoint 0 到 Checkpoint 3。完成后不要继续扩展配置加载、core 下沉或 Unity 工程，把结果交给 PM 检查。
