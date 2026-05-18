# 问剑江湖第一阶段目录与协议骨架实施计划

## 1. 目标

本计划用于指导《问剑江湖》从当前规划期进入工程骨架期。

目标不是一次性实现完整游戏，而是建立一个可持续迭代的最小工程骨架，让后端、Unity 客户端、protobuf 协议、配置表和美术资源可以在同一套目录与契约下推进。

## 2. 当前输入

已完成文档：

- `docs/architecture/wenjian-project-architecture.md`
- `docs/project/agent-collaboration-model.md`
- `docs/project/next-stage-decision.md`
- `docs/client/wenjian-client-visual-direction.md`

已完成素材：

- `wenjian-client/ArtConcepts/`

关键决策：

- 游戏类型：像素风、俯视角即时动作、武侠肉鸽。
- 后端：Java 多模块。
- 构建方向：Maven 3.9.x + Java 21 LTS + Spring Boot 当前稳定 4.0.x 线。
- 客户端：Unity 当前 LTS 线 + URP 2D + Tilemap + Pixel Perfect + Cinemachine + Input System。
- 网络：WebSocket + KCP。
- 实时模型：服务端全权威。
- 第一版世界：区域式大地图 + 单人秘境实例。

## 3. 总体推进顺序

```text
目录占位 → protobuf 最小协议 → config 最小配置表 → 后端 Maven 多模块骨架 → Unity 原型目录 → 最小链路联调
```

理由：

- 目录占位先明确边界。
- protobuf 先稳定前后端最小契约。
- config 先承载地图、区域、技能和秘境参数。
- 后端骨架依赖协议和配置。
- Unity 原型依赖视觉规格、输入协议和资源目录。

## 4. 阶段拆分

### 阶段 1：创建一级目录与说明

目标：

- 创建项目一级目录。
- 每个目录保留 `README.md` 说明职责。
- 不创建复杂工程配置。

预计新增：

```text
wenjian-game-server/README.md
wenjian-client/README.md
protobuf/README.md
config/README.md
docs/plans/first-stage-skeleton-plan.md
```

检查：

- `git status --short --branch`
- 文件存在检查
- README 内容检查

风险：

- 低风险。只新增目录说明，不涉及构建配置。

### 阶段 2：创建 protobuf 最小协议骨架

目标：

- 创建首批 `.proto` 文件。
- 只覆盖真实最小链路，不定义复杂背包、任务、装备、聊天或后台协议。

预计新增：

```text
protobuf/common/common.proto
protobuf/account/account.proto
protobuf/world/world.proto
protobuf/battle/battle.proto
protobuf/rogue/rogue.proto
```

最小协议范围：

- 登录返回玩家会话、KCP token、默认区域和出生点。
- 进入区域返回自身实体、周围实体和 serverTick。
- KCP 输入帧承载移动意图和技能意图。
- 战斗事件承载技能释放、伤害和死亡结果。
- 秘境请求承载进入、开始实例和结算奖励。

检查：

- 字段命名是否清晰。
- 是否避免猜测性兼容字段。
- 高频协议是否避免背包、任务等低频数据。
- 后续生成代码前再接入 protoc 或 Maven 插件。

风险：

- 中风险。协议属于前后端契约，字段设计会影响后续实现。
- 本阶段只做最小链路字段，降低返工成本。

### 阶段 3：创建 config 最小配置骨架

目标：

- 创建配置源目录、schema 目录、导出目录和工具目录。
- 创建首批 CSV 示例表。
- 配置字段只服务最小链路。

预计新增：

```text
config/source/player_template.csv
config/source/map.csv
config/source/region.csv
config/source/skill.csv
config/source/monster.csv
config/source/rogue.csv
config/source/reward_pool.csv
config/source/item.csv
config/schema/README.md
config/export/README.md
config/tools/README.md
```

首批字段：

- `player_template`：template_id、init_region_id、spawn_x、spawn_y、max_hp、move_speed、default_skill_id。
- `map`：map_id、width、height、collision_ref。
- `region`：region_id、map_id、region_type、pk_enabled、spawn_x、spawn_y。
- `skill`：skill_id、cooldown_ms、cast_ms、hit_shape、range、radius、damage。
- `monster`：monster_id、max_hp、move_speed。
- `rogue`：rogue_id、map_id、spawn_x、spawn_y、monster_id、monster_count、reward_pool_id。
- `reward_pool`：reward_pool_id、item_id、count。
- `item`：item_id、name、type。

检查：

- 主键唯一。
- 引用字段能对应到存在的表。
- 关键数值不为空。
- 不写默认假数据兜底配置错误。

风险：

- 中风险。配置字段会影响服务端加载和客户端表现。
- 第一版只做示例数据，不做热更新。

### 阶段 4：创建后端 Maven 多模块骨架

目标：

- 创建 Java 后端父工程和模块。
- 使用 Maven Wrapper 固定 Maven 版本。
- 使用 Java 21。
- 不实现完整业务，只保证骨架可构建。

预计模块：

```text
wenjian-gateway-ws
wenjian-gateway-kcp
wenjian-game-core
wenjian-world
wenjian-rogue
wenjian-battle
wenjian-config
wenjian-protobuf3
wenjian-share
wenjian-admin
```

第一阶段依赖原则：

- `share` 不依赖业务模块。
- `protobuf3` 只放生成代码。
- `config` 可被 gameplay 模块读取。
- `battle` 不依赖网关。
- `world` 和 `rogue` 可以调用 `battle`。
- 网关只做网络接入和路由。

检查：

- `mvn -version`
- `mvn verify`
- Maven Enforcer 检查 Java 和 Maven 版本。
- 模块依赖方向检查。

风险：

- 高风险。涉及构建配置和多模块工程。
- 需要在前面目录、协议、配置骨架之后执行。

### 阶段 5：创建 Unity 原型目录

目标：

- 创建 Unity 资源目录规范。
- 先建立原型目录和说明，不急于创建完整 Unity 工程文件。

预计目录：

```text
wenjian-client/Assets/_Wenjian/Art/
wenjian-client/Assets/_Wenjian/Prefabs/
wenjian-client/Assets/_Wenjian/Scenes/Prototype/
wenjian-client/Assets/_Wenjian/Scripts/
wenjian-client/Assets/_Wenjian/ScriptableObjects/
wenjian-client/Assets/_Wenjian/Tests/
```

原型目标：

- `Proto_CombatField`
- 8 方向移动
- Tilemap 场地
- Pixel Perfect 相机
- 普攻剑光
- 直线剑气
- 圆形斩击
- 战斗 HUD
- MockTransport 输入回放

检查：

- Unity 目录结构完整。
- 场景命名规范。
- 资源目录和概念图目录分离。

风险：

- 中高风险。正式 Unity 工程文件需要根据本机 Unity 版本创建。
- 当前先做目录计划，避免锁死工程版本。

### 阶段 6：最小链路联调

目标：

- 跑通“登录 → 进入区域 → KCP 绑定 → 移动输入 → 服务端快照 → 单技能 → 秘境开始与结算”的最小闭环。

验收标准：

- 服务端能启动。
- WebSocket 登录返回会话和 KCP token。
- KCP 或 Mock KCP 能绑定会话。
- 移动输入只上报意图，服务端返回权威坐标。
- 单技能由服务端判定冷却、范围、伤害和死亡。
- 单人秘境实例能创建、结束并返回奖励。
- Unity 原型能显示输入序号、serverTick 和基本 HUD。

风险：

- 高风险。涉及后端、协议、配置、Unity 和网络联调。
- 必须拆小任务执行，不一次性做完整系统。

## 5. 项目经理执行决策

立即执行顺序：

1. 创建一级目录与 README 说明。
2. 创建 protobuf 最小协议骨架。
3. 创建 config 最小配置骨架。
4. 复核并提交。

暂缓执行：

- 后端 Maven 多模块骨架。
- Unity 正式工程初始化。
- KCP 库选型和接入。
- 配置导出脚本。
- 管理后台。

原因：

- 前三步是低到中风险，可快速建立边界。
- 后端构建和 Unity 工程会引入版本与构建配置，风险更高，应在骨架说明和协议配置确认后执行。

## 6. 检查清单

每个阶段完成后必须执行：

- `git status --short --branch`
- 敏感信息扫描，确认没有密钥或 token。
- Markdown 文档标题检查。
- 文件清单检查。

涉及后端骨架时追加：

- `mvn -version`
- `mvn verify`

涉及 Unity 工程时追加：

- Unity 场景文件存在检查。
- 资源目录检查。
- 截图或 Play Mode 检查。

## 7. 当前结论

下一步不直接写业务代码，而是执行目录、协议和配置骨架。这样既能保持项目继续推进，也能避免在技术栈、Unity 工程和 KCP 接入未充分验证时做过重实现。
