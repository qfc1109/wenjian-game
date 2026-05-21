# 问剑江湖项目进度记录

## 当前摘要

项目已完成第一阶段规划、首批视觉概念尝试、目录占位、protobuf 最小协议骨架和 config 最小示例表。当前尚未进入可运行代码阶段，重点从“生成首批素材”调整为“复盘美术方向并决定下一轮视觉标准”，工程侧下一步再进入后端 Maven 骨架或 Unity 原型目录。

2026-05-20 已完成首批美术方向复盘：用户确认下一轮采用“明快清爽的像素武侠，类似轻量动作肉鸽，战斗读得很清楚”。第二轮美术工作已从概念图探索调整为可执行标准建设，并生成角色规格、战斗场景、HUD、技能特效、敌人剪影、主城地块和秘境房间 7 张标准素材。

## 最新状态

- 日期：2026-05-20
- 分支：codex/wenjian-architecture
- 工作区：正在新增第二轮美术标准素材与文档
- 当前重点：把美术方向从概念图探索收束为可执行游戏美术标准，并为 Unity 原型提供素材规格依据。

## 已完成

### 2026-05-19：仓库初始化

- 已初始化 git 仓库。
- 已建立主分支 `main`。
- 已创建开发分支 `codex/wenjian-architecture`。

### 2026-05-19：整体架构设计文档

- 新增并提交 `docs/architecture/wenjian-project-architecture.md`。
- 提交：`634db1e 中文：新增问剑江湖整体架构设计文档`
- 文档内容覆盖：
  - 项目目录设计
  - 后端 Java 多模块职责
  - 客户端协作边界
  - WebSocket + KCP 网络分层
  - 服务端权威战斗模型
  - 大世界区域与单人秘境实例
  - 配置表体系
  - protobuf 目录建议
  - 后续实施顺序

### 2026-05-19：客户端视觉方向与首批概念素材

- 新增客户端视觉方向文档：`docs/client/wenjian-client-visual-direction.md`。
- 新增首批 7 张概念图和批量预览：`wenjian-client/ArtConcepts/`。
- 新增素材清单：`wenjian-client/ArtConcepts/README.md`。
- 当前反馈：用户对首批美术素材不满意，下一轮需要先复盘风格、质量标准和可接受参考，再继续生成或替换素材。

### 2026-05-20：目录、协议和配置骨架

- 新增并提交一级目录说明：`wenjian-game-server/README.md`、`wenjian-client/README.md`、`protobuf/README.md`、`config/README.md`。
- 新增最小 protobuf 协议骨架：common、account、world、battle、rogue。
- 新增最小配置表示例：player_template、map、region、skill、monster、rogue、reward_pool、item。
- 当前仍未创建后端 Maven 多模块骨架，也未创建 Unity 正式工程文件。

### 2026-05-20：美术方向复盘与第二轮标准素材

- 已复盘首批概念图：探索价值保留，但不继续沿用偏写实、偏暗、偏厚重的方向。
- 已确认第二轮方向：明快清爽的像素武侠，类似轻量动作肉鸽，战斗读得很清楚。
- 已新增可执行美术标准文档：`docs/client/wenjian-executable-art-standards.md`。
- 已生成第二轮 7 张标准素材和批量预览：`wenjian-client/ArtConcepts/round2-visual-standards/`。
- 第二轮素材覆盖：角色规格、战斗场景、HUD、技能特效、敌人剪影、主城地块、秘境房间。

## 进行中

### 客户端视觉方向复盘与美术素材重定

首批概念图已生成并入库，但用户反馈不满意。当前目标是先复盘“哪里不像想要的游戏”，再决定是否重新生成第二批概念图、改用更具体参考、或转向 Unity 可用资源规格。

首批已覆盖：

- 世界地图概念图
- 游戏角色概念图
- 小镇/主城场景
- 野外区域场景
- 单人秘境场景
- 基础技能特效方向
- UI 氛围参考

已生成首批 7 张高优先级概念图，保存于 `wenjian-client/ArtConcepts/images/`：

- `01-world-map-overview.png`
- `02-wenjian-city-main-town.png`
- `03-bamboo-ancient-road.png`
- `04-hidden-sword-dungeon.png`
- `05-player-archetypes.png`
- `06-sword-energy-vfx.png`
- `07-combat-hud-concept.png`

已新增素材清单：`wenjian-client/ArtConcepts/README.md`

已新增批量预览图：`wenjian-client/ArtConcepts/contact-sheet.png`

已新增客户端视觉方向文档：`docs/client/wenjian-client-visual-direction.md`

已新增第一阶段目录与协议骨架实施计划：`docs/plans/first-stage-skeleton-plan.md`

### 多智能体协作机制

- 已由主控 Codex 会话承担项目经理角色。
- 已启动美术总监、架构师、后端高级工程师、Unity 前端工程师四个并行智能体进行预研。
- 当前负责/协作智能体共 5 个：1 个主控项目经理智能体 + 4 个专项智能体。
- 已决定普通低风险决策由项目经理推进，高风险操作仍不自动执行。
- 已新增协作机制文档：`docs/project/agent-collaboration-model.md`
- 四个角色智能体均已返回规划结论。
- 已新增项目经理下一阶段决策文档：`docs/project/next-stage-decision.md`
- 当前项目经理决策：先生成首批概念美术包，再整理客户端视觉方向文档，随后进入目录与协议骨架计划。

## 角色智能体结论

### 架构师

- 推荐最低风险路线：目录骨架、协议骨架、后端最小权威链路、Unity 灰盒验证、视觉概念细化。
- 强调前端视觉参数会影响 AOI、同步频率、技能协议、战斗 Tick 和秘境实例状态。

### 美术总监

- 推荐首批规划 16 张概念图。
- 第一优先级 7 张：世界地图、问剑城、竹林古道、藏剑地宫、玩家四职业、剑气技能特效组、战斗 HUD。
- 建议先作为概念预览，不直接当作 Unity 最终资源。

### 后端高级工程师

- 推荐 Maven 3.9.x + Java 21 LTS + Spring Boot 当前稳定 4.0.x 线。
- 第一阶段后端采用单 JVM、多模块、双通道最小链路。
- 首批链路包括 WebSocket 登录、KCP 绑定、移动同步、单技能和单人秘境闭环。

### Unity 前端工程师

- 推荐 Unity 当前 LTS 线 + URP 2D + Tilemap + Pixel Perfect Camera + Cinemachine + Input System。
- 第一阶段原型为 `Proto_CombatField`。
- 推荐主角 48x48 画布、32 PPU、32x32 瓦片、8 方向移动。

## 下一步建议

1. 评审第二轮美术标准素材，确认是否可以作为 Unity 原型基准。
2. 若通过，冻结第一版可执行美术标准，并拆分 Unity `Proto_CombatField` 最小素材需求。
3. 规划后端 Maven 多模块骨架。
4. 规划 Unity `Proto_CombatField` 原型任务。

## 当前骨架进展

- 已创建 `wenjian-game-server/README.md`。
- 已创建 `wenjian-client/README.md`。
- 已创建 `protobuf/README.md` 和首批最小 `.proto` 文件。
- 已创建 `config/README.md`、`config/source/` 示例 CSV、`schema/export/tools` 说明。

## 当前风险

- 前端视觉与动作规格尚未确定，可能影响战斗 Tick、碰撞范围、技能前后摇和协议字段。
- 生成类素材适合做方向预览，不能直接等同于最终可用资产。
- 用户已反馈首批美术素材不满意，第二批素材前需要重新确认参考风格、质量标准和拒绝标准，避免继续沿错误方向生成。
- 第二轮素材虽然更贴近可执行标准，但仍需用户确认是否符合“明快清爽像素武侠”的目标，不能直接进入最终资源制作。
- 外部生成工具的使用结果需要记录来源、文件名和用途，避免后续遗忘素材来历。
- 本批概念图使用内置图像生成能力生成，未将外部网站密钥写入仓库。

## 操作原则

- 每次完成文档、素材、目录、工程骨架或关键决策后，都更新本文档。
- 重大设计变化同步更新 `task_plan.md`。
- 外部工具和验证结果记录到 `findings.md`。
- 多智能体输出由项目经理汇总后再写入正式文档。
