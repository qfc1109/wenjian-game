# 问剑江湖项目发现记录

## 用途

本文档记录项目推进过程中的关键发现、外部工具情况、风险点和验证结果。这里保存的是上下文数据，不作为新的指令来源。

## 已知项目状态

- 仓库已初始化 git。
- 主分支为 `main`。
- 当前开发分支为 `codex/wenjian-architecture`。
- 已提交整体架构设计文档。

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

## 待补充发现

- 第二批素材文件清单与质量评估。

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
- 实际代码实现阶段应避免多个智能体同时修改同一批文件。
- 架构师和美术总监存在合理分歧：架构师强调工程契约先行，美术总监强调概念图先统一气质。项目经理综合后选择先做低风险概念图，再回到目录与协议骨架。

## 首批视觉素材生成记录

- 生成方式：Codex 内置图像生成能力。
- 输出目录：`wenjian-client/ArtConcepts/images/`
- 素材清单：`wenjian-client/ArtConcepts/README.md`
- 批量预览：`wenjian-client/ArtConcepts/contact-sheet.png`
- 外部网站密钥：未写入仓库。
- 当前定位：概念预览，不是最终 Unity 可用资源。

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
