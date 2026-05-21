# 问剑江湖项目任务计划

## 当前目标

围绕《问剑江湖》完成第一阶段项目规划、目录骨架设计、协议与配置最小骨架，并在第二轮美术标准确认后，采用“契约先行，前后端最小链路并行”的方案推进工程初始化。

## 2026-05-21 工作记录

### 任务列表

- 【✔】 用户确认采用方案 C：契约先行，前后端最小链路并行 <2026-05-21 12:05>
- 【✔】 新增前后端并行最小链路实施计划 <2026-05-21 12:05>
- 【✔】 执行 Task 1 检查：文档关键内容、proto 基础状态、CSV 基础状态 <2026-05-21 12:05>
- 【✔】 完成 Task 2 协议契约复查，现有 protobuf 已覆盖最小链路消息 <2026-05-21 12:05>
- 【❓】 Task 3 后端 Maven 骨架受本机环境阻塞：未找到 Maven，当前 Java 为 1.8.0_181 <2026-05-21 12:05>
- 【✔】 已验证 `E:\JAVA\jdk21` 可用，安装 Maven 3.9.16 到 `E:\apache-maven-3.9.16` 并通过 SHA-512 校验 <2026-05-21 12:05>
- 【✔】 创建后端 Maven 最小多模块骨架：share、protobuf、config、game-core、gateway-ws <2026-05-21 12:05>
- 【✔】 后端 Maven 骨架 `mvn -q -DskipTests validate` 验证通过 <2026-05-21 12:05>
- 【✔】 Task 4 按 TDD 新增登录与进入区域最小链路测试，RED 阶段确认缺少实现类型 <2026-05-21 15:38>
- 【✔】 Task 4 新增最小内存实现，GREEN 阶段网关模块测试通过 <2026-05-21 15:38>
- 【✔】 Task 5 创建 Unity `Proto_CombatField` 原型壳说明和 ArtReference 引用目录 <2026-05-21 15:38>
- 【✔】 Task 6 按 TDD 新增 WebSocket 登录/进入区域集成测试，RED 阶段确认缺少 Spring Boot 启动配置 <2026-05-21 16:34>
- 【✔】 Task 6 新增 Spring Boot WebSocket 最小入口、Handler 和固定路径 `/ws/first-chain` <2026-05-21 16:34>
- 【✔】 Task 6 补齐 Spring Boot Maven 插件配置，修复 `spring-boot:run` 无法解析和父工程误启动问题 <2026-05-21 16:34>
- 【✔】 Task 6 用 Node 原生 WebSocket 临时客户端完成登录和进入区域手动验收 <2026-05-21 16:34>
- 【 】 下一步进入 Task 7：单技能意图、技能事件和 Unity 展示侧接入规划 <2026-05-21 16:34>

### 验证记录

- 【✔】 `git diff --check` 已执行，通过；仅出现 Windows LF/CRLF 换行转换提示 <2026-05-21 12:05>
- 【✔】 `rg -n "方案 C|前后端|最小链路|Proto_CombatField|WebSocket" ...` 已执行，关键内容存在 <2026-05-21 12:05>
- 【✔】 计划文档占位词扫描已执行，未发现 `TBD`、`TODO`、`implement later`、`fill in details` <2026-05-21 12:05>
- 【✔】 protobuf 基础检查已执行，5 个 `.proto` 文件检查完成且无失败输出 <2026-05-21 12:05>
- 【✔】 CSV 基础检查已执行，8 个 `config/source/*.csv` 文件检查完成且无失败输出 <2026-05-21 12:05>
- 【✔】 协议契约复查已执行，最小链路所需 Login、EnterRegion、RegionSnapshot、InputFrame、SkillEvent、DamageEvent、StartRogue、FinishRogue 消息均存在 <2026-05-21 12:05>
- 【❓】 `mvn -v` 已执行，失败：`mvn` 不在 PATH <2026-05-21 12:05>
- 【❓】 `java -version` 已执行，当前为 Java 1.8.0_181，不满足 Java 21 方向 <2026-05-21 12:05>
- 【✔】 使用 `E:\JAVA\jdk21` 和 `E:\apache-maven-3.9.16` 重新执行 `java -version` / `mvn -v`，环境满足 Java 21 + Maven 3.9.x <2026-05-21 12:05>
- 【✔】 `Get-ChildItem -LiteralPath 'wenjian-game-server' -Directory` 已执行，仅包含 5 个最小 Maven 模块 <2026-05-21 12:05>
- 【✔】 `mvn -q -DskipTests validate` 已在 `wenjian-game-server` 下执行，通过 <2026-05-21 12:05>
- 【✔】 RED：`mvn -q -pl wenjian-gateway-ws -am test` 首次执行失败，原因是 `FirstChainGatewayService` 和 DTO 类型不存在 <2026-05-21 15:38>
- 【✔】 GREEN：`mvn -q -pl wenjian-gateway-ws -am test` 再次执行通过 <2026-05-21 15:38>
- 【❓】 第一链路当前还不能试玩游戏，只能证明后端最小登录/进入区域行为；试玩需要 Unity `Proto_CombatField` 原型壳和客户端连接展示 <2026-05-21 15:38>
- 【✔】 `rg -n "48x48|32x32|竹林|RegionSnapshot|round2-visual-standards" wenjian-client/Proto_CombatField wenjian-client/README.md` 已执行，原型壳约束存在 <2026-05-21 15:38>
- 【✔】 RED：`mvn -q -pl wenjian-gateway-ws -am test` 曾因缺少 `@SpringBootConfiguration` 失败，验证 WebSocket 集成测试先于实现生效 <2026-05-21 16:34>
- 【✔】 GREEN：`mvn -q -pl wenjian-gateway-ws -am test` 通过，随机端口 Spring Boot WebSocket 集成测试完成登录和进入区域断言 <2026-05-21 16:34>
- 【✔】 手动启动：`mvn -q -pl wenjian-gateway-ws -am test-compile spring-boot:run -Dspring-boot.run.fork=false -Dspring-boot.run.arguments=--server.port=18080` 可启动本地网关 <2026-05-21 16:34>
- 【✔】 临时客户端验收：Node 原生 WebSocket 收到 `type=LOGIN_OK playerId=1000001 regionId=1001` 和 `type=REGION_SNAPSHOT regionId=1001 self=1000001 entities=2` <2026-05-21 16:34>
- 【❓】 Windows PowerShell 自带 `ClientWebSocket` 对 Tomcat 返回的 `Connection: upgrade, keep-alive` 兼容性不佳，本轮改用 Node 原生 WebSocket 验收 <2026-05-21 16:34>

### 文档修改

| 文件 | 修改内容 | 回滚方法 |
|------|----------|----------|
| `docs/plans/minimal-dual-track-chain-plan.md` | 新增方案 C 的正式实施计划，定义前后端并行最小链路、任务拆分、日志规则和停靠点。 | 删除该文档 |
| `task_plan.md` | 更新当前目标、2026-05-21 工作记录和验证记录。 | 移除 2026-05-21 工作记录并恢复当前目标旧描述 |
| `progress.md` | 同步当前重点为前后端并行最小链路计划。 | 恢复最新状态旧描述 |
| `findings.md` | 记录方案 C 决策和执行边界。 | 删除对应方案 C 记录 |
| `docs/plans/minimal-dual-track-chain-plan.md` | 标记 Task 2 协议复查完成，并在 Task 3 前增加 Java/Maven 环境阻塞项。 | 恢复 Task 2 勾选状态，删除 Task 3 Step 0 |
| `.gitignore` | 新增 Maven/Java 构建产物和 IDE 文件忽略规则。 | 删除该文件或移除新增规则 |
| `wenjian-game-server/pom.xml` | 新增后端 Maven 父工程，声明 5 个最小模块和 Java 21/Spring Boot 4.0.6 依赖管理。 | 删除该文件 |
| `wenjian-game-server/wenjian-share/pom.xml` | 新增共享模块骨架。 | 删除该模块目录 |
| `wenjian-game-server/wenjian-protobuf/pom.xml` | 新增 protobuf 集成占位模块骨架。 | 删除该模块目录 |
| `wenjian-game-server/wenjian-config/pom.xml` | 新增配置模块骨架，依赖 `wenjian-share`。 | 删除该模块目录 |
| `wenjian-game-server/wenjian-game-core/pom.xml` | 新增游戏核心模块骨架，依赖 share/protobuf/config。 | 删除该模块目录 |
| `wenjian-game-server/wenjian-gateway-ws/pom.xml` | 新增 WebSocket 网关模块骨架，依赖核心模块和 Spring WebSocket starter。 | 删除该模块目录 |
| `wenjian-game-server/README.md` | 更新后端最小 Maven 模块说明和本地验证命令。 | 恢复旧说明 |
| `wenjian-game-server/wenjian-gateway-ws/pom.xml` | 新增 `spring-boot-starter-test` 测试依赖。 | 移除该测试依赖 |
| `wenjian-game-server/wenjian-gateway-ws/src/test/java/com/wenjian/gateway/ws/FirstChainGatewayServiceTest.java` | 新增登录与进入区域最小链路测试。 | 删除该测试文件 |
| `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/FirstChainGatewayService.java` | 新增内存版登录与进入区域服务。 | 删除该实现文件 |
| `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/GatewayDtos.java` | 新增最小链路 DTO 和枚举。 | 删除该实现文件 |
| `wenjian-client/Proto_CombatField/README.md` | 新增 Unity 第一阶段原型壳说明与验收标准。 | 删除该文件 |
| `wenjian-client/Proto_CombatField/ArtReference/README.md` | 新增第二轮美术标准引用说明。 | 删除该文件 |
| `wenjian-client/README.md` | 补充第二轮美术标准和 `Proto_CombatField` 原型壳说明。 | 移除对应新增条目 |
| `wenjian-game-server/pom.xml` | 新增 Spring Boot Maven 插件版本管理，并让父工程和库模块默认跳过 `spring-boot:run`。 | 移除 `spring-boot.run.skip` 属性和 `spring-boot-maven-plugin` 插件管理项 |
| `wenjian-game-server/wenjian-gateway-ws/pom.xml` | 打开网关模块 `spring-boot:run`，声明 Spring Boot Maven 插件。 | 移除 `spring-boot.run.skip=false` 和插件声明 |
| `wenjian-game-server/wenjian-gateway-ws/src/test/java/com/wenjian/gateway/ws/FirstChainWebSocketIntegrationTest.java` | 新增 WebSocket 集成测试，验证登录和进入区域响应。 | 删除该测试文件 |
| `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/WenjianGatewayWsApplication.java` | 新增 Spring Boot 应用入口。 | 删除该文件 |
| `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/FirstChainWebSocketConfig.java` | 新增 `/ws/first-chain` WebSocket 路由配置。 | 删除该文件 |
| `wenjian-game-server/wenjian-gateway-ws/src/main/java/com/wenjian/gateway/ws/FirstChainWebSocketHandler.java` | 新增最小文本协议 Handler，转接登录和进入区域服务。 | 删除该文件 |

## 2026-05-20 工作记录

### 任务列表

- 【✔】 复盘首批概念美术素材，确认不继续沿用偏写实、偏暗、偏厚重方向 <2026-05-20 18:57>
- 【✔】 确认第二轮美术方向为明快清爽的像素武侠、轻量动作肉鸽、战斗读得清楚 <2026-05-20 18:57>
- 【✔】 生成第二轮 7 张可执行美术标准素材并保存到项目目录 <2026-05-20 18:57>
- 【 】 用户评审第二轮标准素材，决定是否冻结第一版美术执行标准 <2026-05-20 18:57>

### 文档修改

| 文件 | 修改内容 | 回滚方法 |
|------|----------|----------|
| `docs/client/wenjian-executable-art-standards.md` | 新增第二轮可执行美术标准，记录方向、Unity 原型约束和拒绝标准。 | 删除该文档 |
| `wenjian-client/ArtConcepts/round2-visual-standards/README.md` | 新增第二轮素材清单、用途、初步评估和拒绝标准。 | 删除该文档 |
| `docs/client/wenjian-client-visual-direction.md` | 补充 2026-05-20 美术方向修订结论。 | 移除对应新增小节 |
| `progress.md` | 同步第二轮美术方向与素材进度。 | 移除对应新增记录 |
| `task_plan.md` | 记录本轮任务与下一步评审项。 | 移除本工作记录小节 |
| `findings.md` | 记录第二轮素材生成与风险发现。 | 移除对应新增记录 |

## 当前阶段

- 状态：第一阶段骨架初始化与前后端最小链路计划中
- 当前分支：codex/wenjian-architecture
- 已完成：整体架构设计文档、客户端视觉方向首版、首批概念图、第一阶段实施计划、一级目录 README、protobuf 最小协议骨架、config 最小示例表、首批美术复盘、第二轮可执行美术标准素材、前后端并行最小链路计划
- 下一步：执行方案 C 的 Task 1 检查；随后进入协议契约复查、后端 Maven 最小骨架和 Unity `Proto_CombatField` 原型壳。

## 阶段计划

### 阶段 1：整体架构与目录设计

- 状态：已完成
- 输出：`docs/architecture/wenjian-project-architecture.md`
- 说明：已确认单仓库结构、后端模块方向、客户端协作边界、网络分层、配置表体系和后续顺序。

### 阶段 2：客户端视觉方向与效果图规划

- 状态：已完成首版和第二轮标准素材，待用户评审
- 目标：明确世界地图、游戏角色、秘境、小镇/主城、技能特效和 UI 氛围的首批视觉方向。
- 计划输出：
  - 已完成客户端视觉方向文档：`docs/client/wenjian-client-visual-direction.md`
  - 已完成首批概念图和素材预览文件夹：`wenjian-client/ArtConcepts/`
  - 已完成素材命名与目录规范：`wenjian-client/ArtConcepts/README.md`
- 当前反馈：用户确认第二轮方向为明快清爽的像素武侠，第二轮素材已改为可执行标准建设，不再沿用首批偏概念图路线。

### 阶段 3：第一阶段实施计划

- 状态：已完成首版
- 目标：在视觉方向和架构方向确认后，拆分后端骨架、protobuf、config、Unity 客户端原型的创建顺序。
- 输出：`docs/plans/first-stage-skeleton-plan.md`

### 阶段 4：项目骨架初始化

- 状态：进行中
- 目标：创建一级目录、后端多模块骨架、协议目录、配置目录和客户端原型目录。
- 已完成：一级目录 README、protobuf 最小协议骨架、config 最小示例表。
- 下一步：按 `docs/plans/minimal-dual-track-chain-plan.md` 推进后端 Maven 最小骨架和 Unity `Proto_CombatField` 原型壳。

## 决策记录

- 游戏名：问剑江湖
- 类型：像素风、俯视角即时动作、武侠肉鸽
- 服务器模型：同一个服务器 = 同一个江湖世界
- 第一版架构：区域式大地图 + 单人秘境实例
- 实时模型：服务端全权威
- 网络方案：WebSocket + KCP
- 当前推进方案：方案 C，契约先行，前后端最小链路并行
- 当前开发分支：codex/wenjian-architecture

## 待确认问题

- 第二轮可执行美术标准素材是否符合用户预期，是否可以冻结为第一版美术执行标准。
- 下一轮视觉素材是继续概念图优先，还是直接按 Unity 可用资源规格生成。
- 角色比例、像素尺寸、视角角度和动作帧规格。
- 技能特效的夸张程度、颜色体系和武侠风格边界。

## 错误与阻塞

暂无工程阻塞。首批美术方向不满意的问题已复盘，第二轮标准素材已生成，待用户评审后决定是否进入 Unity 原型拆分。

## 多智能体协作

- 状态：已启用
- 主控角色：项目经理
- 并行角色：美术总监、架构师、后端高级工程师、Unity 前端工程师
- 当前负责/协作智能体数量：5 个，包括 1 个主控项目经理智能体和 4 个专项智能体。
- 协作机制文档：`docs/project/agent-collaboration-model.md`

### 当前并行任务

- 美术总监：已完成首批概念图、美术关键词、提示词和素材用途规划；因用户反馈不满意，需要重新整理下一轮视觉标准。
- 架构师：已完成下一阶段推进顺序和架构风险评估。
- 后端高级工程师：已完成后端多模块骨架和最小可运行链路预研。
- Unity 前端工程师：已完成 Unity 2D 原型、资源规格和验证方式规划。

## 项目经理当前决策

- 决策文档：`docs/project/next-stage-decision.md`
- 当前路线：低风险双轨推进。
- 已执行：生成首批 7 张概念美术图，完成首批复盘；生成第二轮 7 张可执行美术标准素材；完成一级目录、protobuf 和 config 最小骨架。
- 下一步：评审第二轮美术标准，随后决定后端 Maven 骨架与 Unity 原型推进顺序。
