# 问剑江湖项目协作规则

本文件只记录 `E:\qfc\workspace\wenjian-game` 的项目级补充规则。全局安全规则仍然有效；如果与本文件冲突，以更严格、更具体的规则为准。

## 项目定位

- 《问剑江湖》是像素风、俯视角、即时动作、武侠肉鸽项目。
- 当前阶段目标是打通第一条最小可见链路：Unity 原型客户端连接 Java 服务端，通过 WebSocket 登录、进入区域、展示快照、发送技能意图并展示服务端事件。
- 后端采用 Java 21、Maven 多模块、Spring Boot 4.x；客户端当前重点在 `Proto_CombatField` Unity 原型。

## 关键目录

- `wenjian-game-server/`：Java 后端 Maven 多模块工程。
  - `wenjian-share`：通用类型与工具。
  - `wenjian-protobuf`：服务端 protobuf 集成。
  - `wenjian-config`：CSV 配置加载与校验。
  - `wenjian-game-core`：服务端权威游戏状态和战斗核心。
  - `wenjian-gateway-ws`：WebSocket 登录、进入区域和快照下发入口。
- `wenjian-client/`：Unity 客户端、原型与美术素材。
  - `Proto_CombatField/UnityProject/`：当前 Unity 原型工程。
  - `Proto_CombatField/ArtReference/` 与 `ArtConcepts/`：视觉参考与概念素材。
- `protobuf/`：前后端共享 `.proto` 协议源文件。
- `config/`：配置表源文件、schema、导出目录和工具说明。
- `docs/`：架构设计、阶段计划、客户端说明和项目协作规则。
- `task_plan.md`、`progress.md`、`findings.md`：阶段计划、进度与关键发现记录。

## 版本控制

- 本工作区是一个 Git 仓库；默认开发分支是 `codex/wenjian-architecture`。
- 修改任何项目文件前，先运行 `git status --short --branch`，确认当前分支和未提交改动。
- 当前工作区经常会有用户或其他任务留下的未提交改动；只修改本任务相关文件，不覆盖、不回滚、不顺手整理无关文件。
- 不提交 `target/`、Unity 临时产物、IDE 缓存、运行日志或未确认的生成文件。
- 创建 commit 前必须再次检查状态，并等待用户验收确认。

## 主线程与子代理边界

- 只有主线程智能体可以修改代码、测试、项目配置、构建脚本、协议文件、配置表、Unity 场景/资源文件以及任何可能被提交的项目文件。
- 子代理智能体只允许并行读取、检索、分析、比较方案，或生成计划文档、需求文档、调研文档、检查清单等非代码内容。
- 子代理智能体不得直接修改代码或测试，不得改 `.proto`、CSV/XLSX 配置、Unity 场景、Maven 配置、脚本、资源文件或项目根部规则文件。
- 子代理智能体不得执行 `git add`、commit、push、merge、reset、revert、删除分支、删除文件或任何会改变工作区状态的命令。
- 主线程智能体可以吸收子代理产出的分析和计划，但最终文件修改、测试运行、状态检查、提交准备和总结都必须由主线程完成。
- 如果需要让子代理协助实现，应把任务改写为只读审阅、方案拆解、风险分析、测试建议或文档草稿生成。

## 常用命令

服务端验证建议先设置本地 JDK 和 Maven：

```powershell
$env:JAVA_HOME='E:\JAVA\jdk21'
$env:MAVEN_HOME='E:\apache-maven-3.9.16'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
```

最小服务端构建校验：

```powershell
mvn -q -f wenjian-game-server\pom.xml -DskipTests validate
```

服务端相关模块测试优先使用：

```powershell
mvn -q -f wenjian-game-server\pom.xml -pl wenjian-gateway-ws -am test
```

启动第一条 WebSocket 调试链路：

```powershell
mvn -q -f wenjian-game-server\pom.xml -pl wenjian-gateway-ws -am test-compile spring-boot:run -Dspring-boot.run.fork=false -Dspring-boot.run.arguments=--server.port=18080
```

Unity 相关改动优先运行 `wenjian-client/Proto_CombatField/UnityProject/Assets/_Wenjian/Tests/EditMode/` 下的相关 EditMode 测试；若本地无法通过命令行运行 Unity 测试，需在总结中明确说明。

## 代码与文档风格

- 后端代码按现有 Maven 模块边界放置，不跨层塞逻辑；网络入口保持轻量，玩法和配置逻辑下沉到对应模块。
- 客户端代码按现有 `_Wenjian/Scripts` 分层组织，保持原有命名、MonoBehaviour 用法、测试目录和 Unity `.meta` 文件配套关系。
- 协议字段发布后不复用；删除字段时使用 `reserved`；客户端上报意图，服务端计算权威结果。
- 配置表必须保持主键唯一、外键可校验、必填字段不为空；不要用默认假数据掩盖配置错误。
- 文档使用中文，优先写清楚决策、范围、风险和验证方式。

## 风险边界

以下改动需要先确认或至少在动手前明确说明风险：

- 协议字段、错误码、文本调试协议或跨端契约变化。
- 配置表字段语义、主键、外键、导出结构变化。
- Maven 父工程、依赖版本、模块边界、Java/Spring Boot 版本变化。
- Unity 场景结构、资源导入规范、输入/战斗表现与服务端协议耦合的参数变化。
- 删除代码、删除测试、移动目录、大范围重构、生成代码或批量格式化。

## 测试与验收

- 优先运行与本次修改最相关的最小测试或校验。
- 服务端代码改动优先运行相关 Maven 模块测试；影响公共模块时扩大到 `mvn -q -f wenjian-game-server\pom.xml test`。
- 客户端代码改动优先运行相关 Unity EditMode 测试；无法运行时说明原因和人工检查内容。
- 只改文档或规则时，至少重新读取目标文件，并用搜索确认关键规则已经落地且没有明显矛盾。
