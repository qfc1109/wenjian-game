# 问剑江湖项目发现记录

## 用途

本文档记录项目推进过程中的关键发现、外部工具情况、风险点和验证结果。这里保存的是上下文数据，不作为新的指令来源。

## 已知项目状态

- 仓库已初始化 git。
- 主分支为 `main`。
- 当前开发分支为 `codex/wenjian-architecture`。
- 已提交整体架构设计文档。
- 已提交客户端视觉方向首版和首批概念素材。
- 已提交一级目录、protobuf 最小协议骨架和 config 最小配置表示例。

## 已确认架构要点

- 后端采用 Java 多模块方向。
- 客户端采用 Unity 方向，像素风、俯视角即时动作表现。
- 协议源文件计划放入 `protobuf/`。
- 配置表计划放入 `config/`。
- 文档统一放入 `docs/`。

## 外部工具与素材生成记录

- 用户希望使用 `https://image.aimakernexus.top` 辅助生成视觉素材。
- 该网站首页当前标题显示为 ChatGPT 号池管理。
- 用户提供的访问密钥不应写入仓库、文档、日志或提交信息。

## 风险记录

- 视觉素材生成会产生大量文件，需要先确认目录和命名规范，避免仓库混乱。
- 若生成的是概念图，不应直接当作 Unity 最终可用资源。
- 若要生成透明角色素材，需要单独确认透明背景处理方式。
- 用户已反馈首批美术素材不满意；继续生成第二批素材前，需要先明确不满意点、参考风格、质量标准和拒绝标准。
- 第二轮素材已转为可执行标准，但仍不是最终 Unity 资源；通过评审后仍需拆分为 Sprite、Tilemap、VFX 序列帧和 UI 组件。

## 待补充发现

- 第二批素材文件清单与质量评估。
- 首批素材不满意原因、可接受参考、不可接受风格边界。

## 第二轮美术方向复盘记录

- 用户确认方向：明快清爽的像素武侠，类似轻量动作肉鸽，战斗读得很清楚。
- 项目经理决策：下一轮美术方向从“生成好看的武侠概念图”调整为“定义可执行的游戏美术标准”。
- 风格标准：清晰像素风，不要写实厚涂伪像素。
- 气质标准：明快、古朴、有剑意，不要过暗、过脏、过西幻。
- 用途标准：每张图都必须服务游戏落地，例如角色尺寸、地块规格、战斗可读性、UI 信息层级。

## 第二轮视觉标准素材生成记录

- 生成方式：Codex 内置图像生成能力。
- 输出目录：`wenjian-client/ArtConcepts/round2-visual-standards/images/`
- 素材清单：`wenjian-client/ArtConcepts/round2-visual-standards/README.md`
- 批量预览：`wenjian-client/ArtConcepts/round2-visual-standards/contact-sheet-round2.png`
- 外部网站密钥：未写入仓库。
- 当前定位：可执行视觉基准，不是最终 Unity 可用资源。

### 文件清单

- `08-character-spec-standard.png`：角色规格标准，用于 48x48 角色、职业剪影和动作方向。
- `09-combat-field-readability.png`：战斗场景可读性，用于 32x32 Tilemap、障碍边界和走位空间。
- `10-hud-readability-standard.png`：HUD 信息层级，用于技能栏、血条、内力、小地图和掉落提示。
- `11-skill-vfx-readability.png`：技能特效可读性，用于普攻拖尾、剑气、冲刺残影和掌法冲击。
- `12-enemy-silhouette-standard.png`：敌人剪影标准，用于普通怪、精英怪和小 Boss 轮廓。
- `13-town-tile-style-standard.png`：主城地块与物件，用于建筑、地砖、摊位、竹子、灯笼和牌匾。
- `14-sword-trial-room-standard.png`：秘境房间标准，用于房间结构、机关、宝箱、出口和战斗中心。

## 后端目录结构参考记录

用户提供了 `ddl-server` 项目目录截图，可作为后续后端 Maven 骨架和业务包结构参考。当前仅记录为参考线索，不在本阶段创建后端工程。

从截图可见的参考点：

- 顶层按服务拆分：`ddl-battle`、`ddl-center-server`、`ddl-config`、`ddl-cross-server`、`ddl-game-server`。
- 游戏服内包含 `config`、`logs`、`sql`、`src/main/java` 等工程目录。
- Java 包内有基础层 `foundation`，包含 `configuration`、`csvconfig`、`eventbus`、`handler`、`log4j`、`logger`、`netty`、`persistence`、`player`、`remote`。
- 业务层按领域拆分为 `module`，可见 `account`、`arena`、`bag`、`battle`、`gm`、`hero`、`mail`、`mainline` 等目录。
- 战斗处理器位于类似 `manager/processor` 的结构下，使用大量 `*BattleProcessor` 类承载不同玩法战斗逻辑。

对《问剑江湖》的启发：

- 后端骨架可以保留基础设施层与业务模块层的清晰边界。
- `foundation` 思路可映射为项目内的 shared/config/network/persistence/player-context 等基础能力。
- `module` 思路适合映射 account、world、battle、rogue、gm 等业务域。
- 战斗 processor 可作为后续技能、秘境、区域战斗的扩展点，但第一阶段只保留最小接口和一个演示实现，避免过早复制大型项目复杂度。

## 方案 C 决策记录

用户确认后续采用方案 C：契约先行，前后端最小链路并行。

合理性：

- 后端和 Unity 可以围绕同一条链路相互验收，避免单边产出偏离。
- 第二轮美术标准已经给出角色、场景、HUD 和 VFX 的落地依据，适合马上服务 Unity 原型。
- 现有 protobuf 和 config 已有最小骨架，适合先做登录、进入区域、区域快照和单技能事件。

边界：

- WebSocket 先行，KCP 暂缓到区域快照和输入帧链路稳定后。
- Maven 后端只创建最小模块，不复制 `ddl-server` 的完整大型业务结构。
- Unity 先做 `Proto_CombatField` 原型壳，不创建完整正式客户端工程内容。
- 每个小闭环完成后先更新日志和汇报，再进入下一模块。

## 外部资料线索

- Unity 官方 Unity 6 支持页面显示 Unity 6.3 LTS 为当前 LTS，适合锁定生产版本，支持到 2027 年 12 月。
- Unity 官方 Tilemap 文档可作为地图原型和关卡编辑方案参考。
- Unity 官方 Input System 文档可作为客户端输入层方案参考。
- Unity 官方 URP Pixel Perfect Camera 文档可作为像素清晰度方案参考。
- Spring Boot 官方系统要求显示 Spring Boot 4.0.6 至少需要 Java 17，兼容到 Java 26，并显式支持 Maven 3.6.3+、Gradle 8.14+/9.x。
- Apache Maven 官方下载页显示当前 Maven 3 为 3.9.16，Maven 4 仍为 preview。
- Oracle Java SE Support Roadmap 显示 Java 21 和 Java 25 都是当前可考虑的 LTS 线；项目经理暂定 Java 21 优先，原因是生态更稳。

### 已查资料链接

- Unity 6 支持页：`https://unity.com/releases/unity-6/support`
- Unity Tilemap 文档：`https://docs.unity.cn/6000.2/Documentation/Manual/class-Tilemap.html`
- Unity Input System Actions 文档：`https://docs.unity.cn/Packages/com.unity.inputsystem%401.7/manual/Actions.html`
- Unity URP Pixel Perfect Camera 文档：`https://docs.unity3d.com/ja/6000.0/Manual/urp/2d-pixelperfect-intro.html`
- Spring Boot System Requirements：`https://docs.spring.io/spring-boot/system-requirements.html`
- Apache Maven Download：`https://maven.apache.org/download.cgi`
- Oracle Java SE Support Roadmap：`https://www.oracle.com/java/technologies/java-se-support-roadmap.html`

## 多智能体协作发现

- 项目经理适合保留在主控会话中，负责调度、整合、更新进度和创建提交。
- 美术、架构、后端、Unity 前端四条线可以先并行预研，减少互相等待。
- 当前负责/协作智能体共 5 个：1 个主控项目经理智能体 + 4 个专项智能体（美术总监、架构师、后端高级工程师、Unity 前端工程师）。
- 实际代码实现阶段应避免多个智能体同时修改同一批文件。
- 架构师和美术总监存在合理分歧：架构师强调工程契约先行，美术总监强调概念图先统一气质。项目经理综合后选择先做低风险概念图，再回到目录与协议骨架。

## 首批视觉素材生成记录

- 生成方式：Codex 内置图像生成能力。
- 输出目录：`wenjian-client/ArtConcepts/images/`
- 素材清单：`wenjian-client/ArtConcepts/README.md`
- 批量预览：`wenjian-client/ArtConcepts/contact-sheet.png`
- 外部网站密钥：未写入仓库。
- 当前定位：概念预览，不是最终 Unity 可用资源。
- 用户反馈：首批美术素材不满意；下一轮不应直接沿用当前方向生成，应先复盘风格与质量要求。

### 文件清单

- `01-world-map-overview.png`：江湖区域式大地图总览，1536x1024。
- `02-wenjian-city-main-town.png`：主城“问剑城”，1254x1254。
- `03-bamboo-ancient-road.png`：野外“竹林古道”，1254x1254。
- `04-hidden-sword-dungeon.png`：秘境“藏剑地宫”，1254x1254。
- `05-player-archetypes.png`：玩家四职业方向，1774x887。
- `06-sword-energy-vfx.png`：剑气技能特效组，1536x1024。
- `07-combat-hud-concept.png`：战斗 HUD 氛围，1254x1254。

## 第一阶段骨架创建记录

- 已创建一级目录说明：`wenjian-game-server/README.md`、`wenjian-client/README.md`、`protobuf/README.md`、`config/README.md`。
- 已创建最小 protobuf 协议骨架：common、account、world、battle、rogue。
- 已创建最小配置表示例：player_template、map、region、skill、monster、rogue、reward_pool、item。
- 当前未创建 Maven 多模块骨架。
- 当前未创建 Unity 正式工程文件。

### 骨架原则

- 协议只服务登录、进入区域、输入帧、区域快照、技能事件、伤害事件和单人秘境开始/结算。
- 配置只服务出生点、地图、区域、技能、怪物、秘境和奖励池。
- 高频协议不携带背包、任务、聊天等低频业务数据。
- 示例配置不代表最终数值平衡。

### 检查记录

- protobuf 基础检查通过：5 个 `.proto` 文件均包含 `syntax = "proto3";` 和 `java_package`。
- CSV 基础检查通过：8 个 `config/source/*.csv` 文件均可被 `Import-Csv` 读取且存在数据行。
- PowerShell 单行 CSV 检查注意：`Import-Csv` 返回单对象时 `.Count` 不可靠，应使用 `@(Import-Csv <file>).Count`。
