# 问剑江湖 Unity Proto_CombatField 客户端优先开发计划

## 1. PM 结论

用户反馈是正确的：当前前端 Unity 还没有真实工程、场景、特效、HUD 或网络脚本。如果继续只推进后端，很容易在没有画面、输入、特效和 UI 约束的情况下猜接口、猜配置、猜数据库字段。

下一阶段必须先补客户端，以 `Proto_CombatField` 做真实 Unity 可运行原型。目标不是做最终美术版本，而是做一个能暴露后端数据需求、接口缺口、坐标尺度、战斗可读性和持久化需求的可玩竖切。

本计划完成后，后续后端战斗 Tick、协议字段、配置表、数据库和持久化设计都应以 Unity 原型反馈为输入，不再凭空设计。

## 2. 当前事实

- `wenjian-client` 当前只有 README、概念图、第二轮可执行视觉标准和 `Proto_CombatField` 原型壳说明。
- 当前没有 Unity 工程文件、`Assets/`、Scene、C# 脚本、Tilemap、Prefab、Animator、VFX、HUD 或客户端网络连接。
- 后端当前已有本地 WebSocket 文本调试链路：
  - `LOGIN local-dev-key 1700000000000`
  - `ENTER_REGION 1000001 1001`
  - `SKILL 1000001 2001 1 0`
  - `START_ROGUE 1000001 4001`
  - `FINISH_ROGUE 1000001 9000001`
- 后端阶段一 D 已让这些响应来自 `wenjian-game-core` 和 `config/source`，但仍缺少真实 Tick、移动、碰撞、冷却、命中、死亡、清怪结算和持久化。
- 当前关键未知点在客户端侧：
  - 后端坐标 `3200,2400` 到 Unity 世界坐标如何映射。
  - 32x32 Tilemap、48x48 角色、32 PPU 与后端配置单位如何对齐。
  - 技能范围、碰撞半径、前摇、特效持续时间在画面上是否可读。
  - HUD 需要哪些字段，例如最大血量、当前血量、技能冷却、秘境进度、奖励名称。
  - 后端事件是否足以驱动画面表现，例如缺少 serverTick、eventId、duration、vfxId、target remaining hp 等。

## 3. 技术方向

推荐使用：

- Unity 6.3 LTS，原因是官方当前 LTS 支持到 2027 年 12 月，适合锁定生产版本。
- 2D URP。
- Tilemap，先做 32x32 竹林战斗场和秘境房间。
- Pixel Perfect Camera，保持像素画面清晰稳定。
- Input System，统一 8 方向移动、技能键和调试按钮。
- WebSocket 文本协议先行，先接现有后端链路，不在本阶段强行切 protobuf 二进制或 KCP。

官方参考：

- Unity 6 LTS 支持页：`https://unity.com/releases/unity-6/support`
- Unity Tilemap 文档：`https://docs.unity.cn/6000.2/Documentation/Manual/class-Tilemap.html`
- Unity URP Pixel Perfect Camera 文档：`https://docs.unity3d.com/Manual/urp/2d-pixelperfect-intro.html`
- Unity Input System Actions 文档：`https://docs.unity.cn/Packages/com.unity.inputsystem%401.7/manual/Actions.html`

如果本机已安装的 Unity 版本不是 6.3 LTS，先记录实际版本和差异，不直接改项目版本。

## 4. 工程目录建议

建议将真实 Unity 工程放在：

```text
wenjian-client/Proto_CombatField/UnityProject/
```

原因：

- 与 `ArtConcepts/` 概念素材分离。
- 与现有 `Proto_CombatField/README.md` 原型壳说明保持同域。
- 后续可单独忽略 Unity 生成目录，例如 `Library/`、`Temp/`、`Obj/`、`Logs/`、`UserSettings/`。

Unity 内部建议目录：

```text
Assets/_Wenjian/
  Art/Placeholders/
  Art/Tiles/
  Art/VFX/
  Prefabs/
  Scenes/
  Scripts/
    Core/
    Net/
    Presentation/
    Prototype/
    UI/
  ScriptableObjects/
  Tests/
```

## 5. 本阶段总目标

做出一个第一版可运行客户端：

1. 打开 Unity 工程后直接进入 `Proto_CombatField` 场景。
2. 场景里有竹林战斗场、玩家、训练敌人、基础 HUD。
3. 玩家可以本地 8 方向移动，摄像机跟随且像素清晰。
4. 客户端能连接本地后端 WebSocket。
5. 客户端能发送登录和进入区域，按服务端返回显示玩家和训练敌人。
6. 客户端能发送技能意图，并表现服务端返回的技能事件和伤害事件。
7. 客户端能触发秘境开始和结算，显示简单房间/奖励反馈。
8. 产出一份后端接口反馈清单，明确后端下一步必须补哪些字段、接口、配置和数据库表。

## 6. 范围边界

本轮可以做：

- 创建 Unity 工程和 `Assets/_Wenjian` 目录。
- 创建 `Proto_CombatField` 场景。
- 使用占位美术，不追求最终资源。
- 创建 32x32 Tilemap 场景、48x48 玩家占位、训练敌人占位。
- 创建基础剑气 VFX 占位和受击反馈。
- 创建基础 HUD。
- 使用现有 WebSocket 文本协议接后端。
- 记录后端数据/接口/数据库反馈。

本轮不要做：

- 不做最终角色序列帧。
- 不做完整商业美术拆分。
- 不接 KCP。
- 不切 protobuf 二进制协议。
- 不做账号注册、角色创建、背包、任务、商城、聊天、排行榜。
- 不做复杂肉鸽房间生成。
- 不把客户端临时表现逻辑写成不可替换的大框架。

## 7. 阶段拆分

### 阶段 0：Unity 环境与项目创建

目标：

- 检查本机 Unity Hub / Unity Editor。
- 确认是否存在 Unity 6.3 LTS 或可用 Unity 6 LTS。
- 创建 `wenjian-client/Proto_CombatField/UnityProject/`。
- 使用 2D URP 模板或创建后安装 URP/2D 相关包。
- 更新 `.gitignore`，忽略 Unity 生成目录。

建议检查：

```powershell
Get-ChildItem "C:\Program Files\Unity\Hub\Editor" -Directory
```

验收：

- Unity 工程能打开。
- `ProjectSettings/ProjectVersion.txt` 记录实际 Unity 版本。
- `Packages/manifest.json` 存在并包含必要包。
- 没有提交 `Library/`、`Temp/`、`Obj/`、`Logs/`。

### 阶段 1：竹林战斗场与像素尺度

目标：

- 创建 `Assets/_Wenjian/Scenes/Proto_CombatField.unity`。
- 创建 32x32 Tilemap，占位竹林地面、障碍和边界。
- 创建 Pixel Perfect Camera。
- 建立临时坐标映射组件 `WorldCoordinateMapper`。

初始坐标策略：

- 不把后端 `3200,2400` 直接当 Unity 世界坐标。
- 先用可调参数映射，例如 `serverUnitsPerTile=100`、`tileWorldSize=1`。
- 把映射参数做成可调配置，阶段验收后再反推后端配置单位。

验收：

- 画面能清楚区分可走区域、障碍区域和战斗中心。
- 玩家出生点能从后端配置坐标映射到场景中合理位置。
- 输出坐标映射结论：后端配置单位是否需要调整，或是否需要新增客户端显示坐标字段。

### 阶段 2：玩家、训练敌人与本地输入

目标：

- 创建 48x48 玩家占位 sprite/prefab。
- 创建训练敌人占位 sprite/prefab。
- 使用 Input System 实现本地 8 方向移动。
- 创建基础相机跟随。

验收：

- 玩家移动方向清楚，角色不被背景吞掉。
- 训练敌人与玩家在 48x48/64x64 尺度下可区分。
- 记录后端需要补充的数据：
  - `move_speed` 单位是否适合 Unity。
  - 实体碰撞半径。
  - 实体朝向。
  - 实体视觉资源 id。

### 阶段 3：HUD 与调试面板

目标：

- 创建基础 HUD：血条、内力占位、技能槽、连接状态、区域/秘境状态。
- 创建调试面板：当前玩家 id、区域 id、serverTick、坐标、最近事件。
- HUD 不遮挡中央战斗区域。

验收：

- 玩家血量、敌人血量、技能槽和连接状态可读。
- 记录后端需要补充的数据：
  - `maxHp` 是否应随 EntitySnapshot 返回。
  - 技能冷却和剩余冷却。
  - 区域名称、地图名、敌人名称。
  - item 名称和类型是否随奖励返回，还是客户端查配置。

### 阶段 4：WebSocket 登录与区域快照

目标：

- 创建 `FirstChainWsClient`。
- 连接 `ws://127.0.0.1:18080/ws/first-chain`。
- 发送 `LOGIN local-dev-key <clientTimeMs>`。
- 发送 `ENTER_REGION <playerId> 1001`。
- 解析 `type=LOGIN_OK` 和 `type=REGION_SNAPSHOT` 文本响应。
- 在场景中按服务端响应生成或刷新玩家和训练敌人。

验收：

- 后端启动后，Unity 客户端能连接并显示玩家/训练敌人。
- 断开后能显示连接失败状态，不假装在线。
- 记录后端接口缺口：
  - `REGION_SNAPSHOT` 只有实体数量，不足以真实渲染实体列表；后续需要实体详情列表或 protobuf 二进制快照。
  - 需要明确 `position`、`hp`、`maxHp`、`entityType`、`visualId`、`facing`、`state`。

### 阶段 5：技能意图、剑气特效与受击反馈

目标：

- 绑定技能键，向当前朝向发送 `SKILL <playerId> 2001 <aimX> <aimY>`。
- 解析 `SKILL_EVENT`，播放短剑气占位特效。
- 解析 `DAMAGE_EVENT`，播放敌人闪烁、扣血数字或血条变化。
- 记录特效遮挡和持续时间。

验收：

- 技能方向可读。
- 剑气不遮挡玩家主体、敌人轮廓和血量位置。
- 受击反馈能被看见，但不过度遮挡战斗。
- 记录后端接口缺口：
  - `SkillEvent` 需要 `eventId`、`serverTick`、`castMs`、`durationMs`、`hitShape`、`range`、`radius`、`vfxId`。
  - `DamageEvent` 需要目标剩余血量或目标状态，否则客户端只能猜。

### 阶段 6：秘境开始、房间展示与结算反馈

目标：

- 创建秘境入口调试按钮。
- 发送 `START_ROGUE 1000001 4001`。
- 切换或重置到秘境房间展示。
- 显示怪物数量和奖励池占位。
- 发送 `FINISH_ROGUE 1000001 9000001`。
- 显示奖励弹窗或结算栏。

验收：

- 客户端能展示秘境开始和结算结果。
- 奖励反馈能读清楚。
- 记录后端接口缺口：
  - 秘境实例需要房间状态、清怪条件、怪物实体列表、出口状态。
  - 结算奖励需要 item 配置来源，至少需要 itemId、name、type、count。
  - 后端不能长期允许未清怪直接结算。

### 阶段 7：后端接口、配置与数据库反馈文档

目标：

新增：

```text
docs/client/proto-combatfield-backend-feedback.md
```

内容必须覆盖：

- 客户端已验证的字段。
- 客户端无法表现的字段缺口。
- 建议新增或调整的 protobuf 字段。
- 建议新增或调整的 CSV 配置字段。
- 建议后端数据库/持久化先不要做和必须做的边界。

建议反馈分类：

| 类别 | 客户端发现 | 后端影响 |
|------|------------|----------|
| 坐标尺度 | 后端配置单位与 Unity 世界单位映射 | 影响地图、移动、碰撞、技能范围 |
| 实体展示 | 缺 visualId/facing/state/maxHp | 影响快照协议和角色表 |
| 技能表现 | 缺 cast/duration/range/vfx | 影响 skill 配置和战斗事件 |
| HUD | 缺冷却、名称、区域信息 | 影响协议和配置导出 |
| 秘境 | 缺房间/清怪/奖励展示字段 | 影响 rogue 实例和持久化 |
| 持久化 | 客户端只需要阶段性保存点 | 影响账号、角色、背包、秘境记录 |

验收：

- 文档能直接指导下一轮后端协议和数据库设计。
- 不用主观描述替代字段清单。

### 阶段 8：阶段验收与下一轮决策

目标：

- 截图或录屏记录 Unity 原型。
- 汇总前端体验问题和后端接口缺口。
- 决定下一轮是补后端 protobuf 二进制、服务端 Tick，还是继续完善 Unity 表现。

验收：

- 本地能复现：启动后端、打开 Unity、连接、登录、进入区域、释放技能、秘境开始/结算。
- `task_plan.md`、`progress.md`、`findings.md` 更新。
- 如果 Unity 环境不可用，记录阻塞项而不是继续后端猜设计。

## 8. 对后端的约束调整

在客户端原型跑起来前，后端暂停以下工作：

- 不设计最终数据库表。
- 不扩展复杂 battle/world/rogue 模块。
- 不提前做完整 KCP。
- 不追加大量 protobuf 字段。

允许后端只做必要支撑：

- 保证 WebSocket 本地链路可启动。
- 修复客户端连接时发现的阻塞 bug。
- 根据 Unity 原型反馈做最小字段调整。

## 9. 第一轮客户端验收命令和材料

后端启动命令：

```powershell
$env:JAVA_HOME='E:\JAVA\jdk21'
$env:MAVEN_HOME='E:\apache-maven-3.9.16'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
cd E:\qfc\workspace\wenjian-game\wenjian-game-server
mvn -q -pl wenjian-gateway-ws -am test-compile spring-boot:run -Dspring-boot.run.fork=false -Dspring-boot.run.arguments=--server.port=18080
```

Unity 验收材料：

- `Proto_CombatField` 场景截图。
- 登录和进入区域后的截图。
- 技能释放和受击反馈截图。
- 秘境开始和结算截图。
- 后端接口反馈文档。

## 10. 推荐立即执行顺序

1. 检查 Unity Editor 安装情况。
2. 创建 Unity 工程和 `.gitignore` 更新。
3. 创建 `Proto_CombatField` 场景、Tilemap、相机、玩家和敌人占位。
4. 做本地输入和 HUD。
5. 接 WebSocket 登录/进入区域。
6. 接技能事件和受击反馈。
7. 接秘境开始/结算。
8. 写后端接口反馈文档。

## 11. 阶段目标一句话

下一阶段不是继续猜后端，而是先做一个能看、能动、能连后端、能暴露接口问题的 Unity `Proto_CombatField` 原型，用客户端真实需求反推后端协议、配置和数据库设计。
