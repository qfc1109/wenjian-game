# Minimal Dual-Track Chain Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Build the smallest front-end/back-end validation chain for 《问剑江湖》 so Unity prototype output and Java server output can verify each other early.

**Architecture:** Use contract-first, dual-track delivery. The back end starts with a minimal Maven/Spring Boot service and WebSocket chain; the Unity side starts with `Proto_CombatField` and consumes the same protobuf/config assumptions. KCP, complete roguelite systems, inventory, chat, economy, and production art are deferred until the first chain is proven.

**Tech Stack:** Java 21, Maven 3.9.x, Spring Boot 4.x line, protobuf source files under `protobuf/`, CSV config under `config/source/`, Unity LTS 2D URP/Tilemap prototype, Git + Markdown progress logs.

---

## 1. Project Manager Decision

已采用方案 C：契约先行，前后端最小链路并行。

这意味着后端和 Unity 不再串行等待。每一阶段都必须有共同验收点：

- 后端输出协议、服务和可观察响应。
- 前端输出场景、输入、展示和可观察行为。
- 项目经理维护日志、风险和验收结论。

## 2. Scope Boundary

### In Scope

- Maven 最小后端骨架。
- WebSocket 登录和进入区域链路。
- 竹林测试区域区域快照。
- Unity `Proto_CombatField` 原型计划与最小接入。
- 单技能意图和服务端技能事件。
- 单人秘境开始/结算的最小协议验证。
- 每个闭环的计划日志、验证记录和阶段汇报。

### Out of Scope

- 完整 KCP 实现。
- 背包、任务、聊天、商城、邮件、排行榜。
- 完整随机肉鸽房间生成。
- 最终商业美术资源拆分。
- 大型 `ddl-server` 式业务目录完整复制。

## 3. File Responsibility Map

### Current Planning Files

- `docs/plans/minimal-dual-track-chain-plan.md`：本计划，记录前后端并行最小链路。
- `task_plan.md`：每日任务、阻塞、验证和回滚记录。
- `progress.md`：阶段性状态摘要。
- `findings.md`：技术发现、参考项目结构、风险和取舍。

### Future Back-End Files

- `wenjian-game-server/pom.xml`：后端父工程。
- `wenjian-game-server/wenjian-share/`：通用类型、错误码、玩家上下文。
- `wenjian-game-server/wenjian-protobuf/`：protobuf 生成物承载模块。
- `wenjian-game-server/wenjian-config/`：CSV 配置加载。
- `wenjian-game-server/wenjian-game-core/`：区域、实体、输入帧、技能事件核心模型。
- `wenjian-game-server/wenjian-gateway-ws/`：WebSocket 登录、进入区域和快照下发。

### Future Unity Files

- `wenjian-client/Proto_CombatField/README.md`：Unity 原型任务说明。
- `wenjian-client/Proto_CombatField/ArtReference/`：引用第二轮标准图。
- `wenjian-client/Proto_CombatField/ConfigSamples/`：前端读取或镜像的最小配置样例。

## 4. Shared Contract

第一条链路只使用已有协议域，不扩展低频业务：

- `account.LoginReq`
- `account.LoginResp`
- `world.EnterRegionReq`
- `world.EnterRegionResp`
- `world.RegionSnapshot`
- `battle.InputFrame`
- `battle.SkillIntent`
- `battle.SkillEvent`
- `battle.DamageEvent`
- `rogue.StartRogueReq`
- `rogue.StartRogueResp`
- `rogue.FinishRoguePush`

第一阶段验收顺序：

1. 登录成功。
2. 进入竹林测试区域。
3. 后端返回区域快照。
4. Unity 显示玩家、测试敌人和基础场景。
5. Unity 发送一个技能意图。
6. 后端返回一个技能事件和可选伤害事件。
7. Unity 播放基础剑气占位效果。
8. 后端发送单人秘境开始和结算消息。

## 5. Development Tasks

### Task 1: Plan And Log Baseline

**Files:**
- Create: `docs/plans/minimal-dual-track-chain-plan.md`
- Modify: `task_plan.md`
- Modify: `progress.md`
- Modify: `findings.md`

- [x] **Step 1: Record Scheme C as the chosen route**

Run:

```powershell
git status --short --branch
```

Expected: current branch is `codex/wenjian-architecture`, not `main` or `master`.

- [x] **Step 2: Write the formal plan**

Create this document and keep all tasks small enough to verify independently.

- [x] **Step 3: Run documentation checks**

Run:

```powershell
git diff --check
rg -n "方案 C|前后端|最小链路|Proto_CombatField|WebSocket" docs/plans/minimal-dual-track-chain-plan.md task_plan.md progress.md findings.md
```

Expected: no whitespace errors; key plan terms are present.

Verified on 2026-05-21:

- `git diff --check` passed with only Windows LF/CRLF conversion warnings.
- Key terms were found in the plan and project logs.
- Placeholder scan for `TBD` / `TODO` / `implement later` / `fill in details` returned no matches.
- Protobuf basic check covered 5 files.
- CSV basic check covered 8 files.

### Task 2: Protocol Contract Review

**Files:**
- Modify if needed: `protobuf/account/account.proto`
- Modify if needed: `protobuf/world/world.proto`
- Modify if needed: `protobuf/battle/battle.proto`
- Modify if needed: `protobuf/rogue/rogue.proto`
- Modify: `findings.md`

- [x] **Step 1: Inspect existing messages**

Run:

```powershell
rg -n "message LoginReq|message LoginResp|message EnterRegionReq|message RegionSnapshot|message InputFrame|message SkillIntent|message StartRogueReq|message FinishRoguePush" protobuf
```

Expected: every minimal-chain message is present.

- [x] **Step 2: Only add missing fields that block the chain**

Allowed additions are limited to player id, region id, entity id, position, direction, skill id, target position, damage value, reward id, and error code. Do not add inventory, chat, quest, mail, shop, guild, or ranking fields.

Review result on 2026-05-21: no protocol fields were added. Existing messages already cover the first minimal chain.

- [x] **Step 3: Verify protobuf basics**

Run:

```powershell
$protoFiles = Get-ChildItem -LiteralPath 'protobuf' -Recurse -Filter '*.proto'
foreach ($f in $protoFiles) {
  $text = Get-Content -LiteralPath $f.FullName -Raw -Encoding UTF8
  if ($text -notmatch 'syntax = "proto3";' -or $text -notmatch 'option java_package') {
    'PROTO_CHECK_FAIL: ' + $f.FullName
  }
}
'proto files checked: ' + $protoFiles.Count
```

Expected: no `PROTO_CHECK_FAIL`.

Verified on 2026-05-21: 5 `.proto` files checked, no `PROTO_CHECK_FAIL`.

### Task 3: Back-End Maven Skeleton

**Files:**
- Create: `wenjian-game-server/pom.xml`
- Create: `wenjian-game-server/wenjian-share/pom.xml`
- Create: `wenjian-game-server/wenjian-protobuf/pom.xml`
- Create: `wenjian-game-server/wenjian-config/pom.xml`
- Create: `wenjian-game-server/wenjian-game-core/pom.xml`
- Create: `wenjian-game-server/wenjian-gateway-ws/pom.xml`
- Modify: `wenjian-game-server/README.md`
- Modify: `task_plan.md`

- [x] **Step 0: Resolve local Java/Maven environment blocker**

Current blocker found on 2026-05-21:

- `mvn -v` fails because Maven is not available on PATH.
- `java -version` reports Java 1.8.0_181.
- The planned Spring Boot 4.x / Java 21 direction cannot be validated in this environment yet.

Before creating the Maven skeleton, install or expose Java 21 and Maven 3.9.x, or approve a Maven Wrapper bootstrap path.

Resolved on 2026-05-21:

- JDK 21 verified at `E:\JAVA\jdk21`.
- Maven 3.9.16 installed at `E:\apache-maven-3.9.16`.
- `mvn -v` reports Maven 3.9.16 and Java 21.0.11.

- [x] **Step 1: Create only the minimum modules**

The first Maven skeleton must include only:

```text
wenjian-share
wenjian-protobuf
wenjian-config
wenjian-game-core
wenjian-gateway-ws
```

Do not create KCP, admin, database, mail, bag, task, guild, or payment modules in this task.

- [x] **Step 2: Verify Maven structure**

Run:

```powershell
Get-ChildItem -LiteralPath 'wenjian-game-server' -Directory | Select-Object -ExpandProperty Name
```

Expected: only the minimal modules above plus any existing non-module documentation files.

- [x] **Step 3: Run Maven validation**

Run from `wenjian-game-server`:

```powershell
mvn -q -DskipTests validate
```

Expected: build validates. If Maven is not installed, record the tool environment blocker in `task_plan.md`.

Verified on 2026-05-21:

- Module directories are exactly `wenjian-config`, `wenjian-game-core`, `wenjian-gateway-ws`, `wenjian-protobuf`, and `wenjian-share`.
- `mvn -q -DskipTests validate` passed from `wenjian-game-server`.

### Task 4: Back-End First WebSocket Chain

**Files:**
- Create under `wenjian-game-server/wenjian-gateway-ws/src/main/java/`
- Create under `wenjian-game-server/wenjian-game-core/src/main/java/`
- Create tests under matching `src/test/java/`
- Modify: `task_plan.md`
- Modify: `findings.md`

- [ ] **Step 1: Add a test for login and enter-region flow**

The test must prove:

- login returns a player id and success code.
- enter region returns region id and at least one player entity.

- [ ] **Step 2: Implement the smallest in-memory service**

Use in-memory data only. No database, Redis, account persistence, or external service.

- [ ] **Step 3: Verify the module**

Run from `wenjian-game-server`:

```powershell
mvn -q -pl wenjian-gateway-ws -am test
```

Expected: gateway tests pass.

### Task 5: Unity Proto_CombatField Plan And Shell

**Files:**
- Create: `wenjian-client/Proto_CombatField/README.md`
- Create: `wenjian-client/Proto_CombatField/ArtReference/README.md`
- Modify: `wenjian-client/README.md`
- Modify: `task_plan.md`

- [ ] **Step 1: Define Unity prototype acceptance**

The README must state:

- player sprite target: 48x48.
- tile target: 32x32.
- first scene: bamboo combat field.
- first HUD: health, inner force, skill slots, minimal map indicator.
- first network goal: display server region snapshot.

- [ ] **Step 2: Link second-round art standards**

Reference:

```text
wenjian-client/ArtConcepts/round2-visual-standards/
docs/client/wenjian-executable-art-standards.md
```

- [ ] **Step 3: Verify documentation**

Run:

```powershell
rg -n "48x48|32x32|竹林|RegionSnapshot|round2-visual-standards" wenjian-client/Proto_CombatField wenjian-client/README.md
```

Expected: all prototype constraints are present.

### Task 6: First Front-End/Back-End Acceptance

**Files:**
- Modify after implementation: `task_plan.md`
- Modify after implementation: `progress.md`
- Modify after implementation: `findings.md`

- [ ] **Step 1: Start the back-end gateway**

Run:

```powershell
mvn -pl wenjian-game-server/wenjian-gateway-ws -am spring-boot:run
```

Expected: local WebSocket gateway starts and logs the listening port.

- [ ] **Step 2: Verify the first client interaction**

Use either a Unity client or a temporary WebSocket test client to send login and enter-region messages.

Expected:

- login response is successful.
- enter-region response contains a region snapshot.
- player position matches `config/source/player_template.csv`.

- [ ] **Step 3: Record acceptance**

Update `task_plan.md` with:

- command run.
- result.
- known gap.
- next action.

### Task 7: Single Skill Chain

**Files:**
- Modify if needed: `protobuf/battle/battle.proto`
- Modify back-end core/gateway tests and implementation.
- Modify Unity prototype documentation or scripts after project exists.
- Modify: `task_plan.md`

- [ ] **Step 1: Add a failing test for one skill intent**

The first skill must use existing `config/source/skill.csv`, preferably `default_skill_id` from `player_template.csv`.

- [ ] **Step 2: Implement one in-memory skill event**

The server returns a `SkillEvent` and optional `DamageEvent`; it does not calculate full combat balance.

- [ ] **Step 3: Verify front-end readability**

Unity or a documented visual check must confirm the VFX does not hide the player, enemies, or collision area.

### Task 8: Minimal Rogue Instance Closure

**Files:**
- Modify if needed: `protobuf/rogue/rogue.proto`
- Modify back-end tests and implementation.
- Modify: `task_plan.md`
- Modify: `progress.md`

- [ ] **Step 1: Start rogue instance from config**

Use `config/source/rogue.csv`, `monster.csv`, and `reward_pool.csv`.

- [ ] **Step 2: Return deterministic finish result**

The first finish result can be deterministic and config-driven. Do not implement random room generation in this task.

- [ ] **Step 3: Record the closed loop**

Update progress once login, enter region, skill event, and rogue finish are all demonstrated.

## 6. Progress Logging Rules

Every task must update logs before moving to a new module:

1. `task_plan.md` records daily task state, exact files changed, verification commands, result, rollback path.
2. `progress.md` records only milestone-level status.
3. `findings.md` records technical discoveries, blockers, and architecture decisions.
4. If verification fails, do not commit unless the user explicitly asks; record the blocker first.
5. Each conversation task should produce at most one commit unless the user approves a split.

## 7. Stop Points

Stop and report after each of these:

- Plan and log baseline is written.
- Maven skeleton validates.
- WebSocket login/enter-region test passes.
- Unity `Proto_CombatField` shell is created.
- First region snapshot is displayed or test-client verified.
- Single skill event is verified.
- Minimal rogue closure is verified.

## 8. Current Recommendation

Execute Task 1 first, then stop for a short review. After Task 1 is committed, start Task 2 and Task 3 together only if their file ranges remain separate.
