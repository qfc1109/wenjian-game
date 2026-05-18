# 问剑江湖项目进度记录

## 当前摘要

项目处于第一阶段规划期。整体架构设计文档已完成并提交，下一步转向客户端视觉方向、效果图和首批美术素材规划。

## 最新状态

- 日期：2026-05-19
- 分支：codex/wenjian-architecture
- 工作区：创建本文档前为干净状态
- 当前重点：客户端视觉方向与概念素材生成规划

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

## 进行中

### 客户端视觉方向与美术素材规划

目标是先生成并审阅视觉方向，再决定 Unity 客户端工程和资源目录如何创建。

计划覆盖：

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

### 多智能体协作机制

- 已由主控 Codex 会话承担项目经理角色。
- 已启动美术总监、架构师、后端高级工程师、Unity 前端工程师四个并行智能体进行预研。
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

1. 根据视觉方向拆分第一阶段实施计划。
2. 创建目录与协议骨架实施计划。
3. 生成第二批补充视觉素材。
4. 规划 Unity `Proto_CombatField` 原型任务。

## 当前风险

- 前端视觉与动作规格尚未确定，可能影响战斗 Tick、碰撞范围、技能前后摇和协议字段。
- 生成类素材适合做方向预览，不能直接等同于最终可用资产。
- 外部生成工具的使用结果需要记录来源、文件名和用途，避免后续遗忘素材来历。
- 本批概念图使用内置图像生成能力生成，未将外部网站密钥写入仓库。

## 操作原则

- 每次完成文档、素材、目录、工程骨架或关键决策后，都更新本文档。
- 重大设计变化同步更新 `task_plan.md`。
- 外部工具和验证结果记录到 `findings.md`。
- 多智能体输出由项目经理汇总后再写入正式文档。
