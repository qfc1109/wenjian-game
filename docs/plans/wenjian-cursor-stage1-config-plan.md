# 问剑江湖 Cursor 阶段一 C 开发计划：CSV 配置加载与校验

## 1. PM 结论

阶段一第一轮 protobuf 生成与 gateway mapper 已通过 PM 修正和本地验证。下一轮 Cursor 只执行阶段一 C：让 `wenjian-config` 正式加载并校验 `config/source/*.csv`，形成可被 core/gateway 使用的配置访问层。

本阶段完成后必须停止，交给 PM 审查；不要继续做 core 下沉、网关硬编码迁移、KCP、数据库或 Unity。

## 2. 本阶段目标

一句话目标：`wenjian-config` 能从仓库 `config/source/` 加载当前 8 张 CSV，提供类型化访问，并在主键、必填字段、数值格式和外键关系错误时明确失败。

当前 CSV 文件：

- `player_template.csv`
- `map.csv`
- `region.csv`
- `skill.csv`
- `monster.csv`
- `rogue.csv`
- `reward_pool.csv`
- `item.csv`

## 3. 本阶段边界

本轮可以做：

- 修改 `wenjian-config/pom.xml`。
- 新增 `wenjian-config/src/main/java/com/wenjian/config/...`。
- 新增 `wenjian-config/src/test/java/com/wenjian/config/...`。
- 如有必要，更新 `wenjian-game-server/README.md` 的配置验证命令。

本轮不要做：

- 不修改 `FirstChainGatewayService` 的硬编码行为。
- 不让 gateway 直接依赖配置加载结果。
- 不创建数据库、账号、角色持久化。
- 不创建 Unity 工程。
- 不改动 protobuf 字段。
- 不新增大模块。
- 不提交 `target/` 或临时生成文件。

## 4. 推荐设计

### 4.1 包结构

推荐新增：

```text
wenjian-game-server/wenjian-config/src/main/java/com/wenjian/config/
  ConfigException.java
  CsvConfigLoader.java
  GameConfigRepository.java
  model/
    PlayerTemplateConfig.java
    MapConfig.java
    RegionConfig.java
    SkillConfig.java
    MonsterConfig.java
    RogueConfig.java
    RewardPoolConfig.java
    ItemConfig.java
```

### 4.2 访问方式

推荐接口：

```java
GameConfigRepository configs = CsvConfigLoader.load(Path sourceDir);
configs.requirePlayerTemplate(1);
configs.requireRegion(1001);
configs.requireSkill(2001);
configs.requireRogue(4001);
```

要求：

- `requireXxx(id)` 找不到时抛 `ConfigException`，不要返回 null，也不要静默兜底。
- 只读集合用 `Map.copyOf` / `List.copyOf` 固化。
- CSV 解析优先使用成熟库，例如 Apache Commons CSV；不要用随意 `split(",")` 做正式解析。
- 当前字段名以 CSV header 为准，不额外猜字段。

## 5. 校验规则

### Checkpoint 0：基线确认

执行：

```powershell
git status --short --branch
$env:JAVA_HOME='D:\JAVA\jdk-24'
$env:MAVEN_HOME='D:\maven\apache-maven-3.6.3'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
java -version
mvn -v
cd D:\workspace\AI_project\wenjian-game\wenjian-game-server
mvn -q test
```

通过标准：

- 当前分支不是 `main/master`。
- 没有与本轮无关的未提交改动。
- 全量测试在修改前通过。

### Checkpoint 1：建立 CSV 加载基础

预计修改：

- `wenjian-config/pom.xml`
- `CsvConfigLoader`
- `GameConfigRepository`
- `ConfigException`
- model records

要求：

- loader 接收 `Path sourceDir`。
- 每张表必须存在。
- 每张表必须包含预期 header。
- 主键字段必须唯一。
- 必填字段不能为空。
- int/long/boolean 必须严格解析，错误时抛 `ConfigException` 并带文件名、字段名、原始值。

建议测试：

```powershell
mvn -q -pl wenjian-config -am test
```

### Checkpoint 2：实现外键与业务基础校验

必须校验：

- `player_template.init_region_id -> region.region_id`
- `player_template.default_skill_id -> skill.skill_id`
- `region.map_id -> map.map_id`
- `rogue.map_id -> map.map_id`
- `rogue.monster_id -> monster.monster_id`
- `rogue.reward_pool_id -> reward_pool.reward_pool_id`
- `reward_pool.item_id -> item.item_id`

建议校验：

- `max_hp`、`move_speed`、`damage`、`monster_count`、`count`、地图宽高必须为正数。
- `region_type` 只允许当前 CSV 中已出现且明确支持的值。
- `hit_shape` 只允许当前 CSV 中已出现且明确支持的值。
- boolean 只接受 `true` / `false`。

测试至少覆盖：

- 当前真实 `config/source` 加载成功。
- 重复主键失败。
- 缺失外键失败。
- 非法数字失败。
- 缺少必填字段失败。

测试夹具可以放在：

```text
wenjian-game-server/wenjian-config/src/test/resources/config-fixtures/
```

### Checkpoint 3：提供 typed accessors

`GameConfigRepository` 至少提供：

- `requirePlayerTemplate(int templateId)`
- `requireMap(int mapId)`
- `requireRegion(int regionId)`
- `requireSkill(int skillId)`
- `requireMonster(int monsterId)`
- `requireRogue(int rogueId)`
- `requireRewardPool(int rewardPoolId)`
- `requireItem(int itemId)`

当前真实数据断言至少覆盖：

- 玩家模板 `1`：初始区域 `1001`、出生点 `3200,2400`、默认技能 `2001`。
- 区域 `1001`：地图 `1001`，`pk_enabled=false`。
- 技能 `2001`：冷却 `600`，伤害 `12`。
- 怪物 `3001`：最大生命 `60`。
- 秘境 `4001`：地图 `2001`，怪物 `3001`，数量 `8`，奖励池 `5001`。
- 奖励池 `5001`：道具 `6001`，数量 `3`。
- 道具 `6001`：名称 `试炼铜钱`，类型 `CURRENCY`。

### Checkpoint 4：全量验证

执行：

```powershell
cd D:\workspace\AI_project\wenjian-game\wenjian-game-server
mvn -q -pl wenjian-config -am test
mvn -q test
cd D:\workspace\AI_project\wenjian-game
git diff --check
git status --short --branch
```

通过标准：

- config 模块测试通过。
- 后端全量测试通过。
- `git diff --check` 通过；如仅有 Windows LF/CRLF 提示，汇报中说明。
- `git status` 只出现本轮相关文件。

## 6. Cursor 完成后汇报格式

完成后必须停止，并按以下格式汇报给 PM：

- 当前分支和 Git 状态：
- 实际修改文件列表：
- 新增依赖及版本：
- CSV 加载入口：
- Repository typed accessors：
- 已实现的校验规则：
- 测试类与测试覆盖：
- 执行过的检查命令及结果：
- 是否出现 target/、临时文件或无关文件变更：
- 遇到的问题、风险和未确认点：
- 是否建议进入阶段一 D，但不要自行继续：

## 7. PM 审查重点

PM 会重点检查：

- 是否使用可靠 CSV 解析方式，而不是随意字符串拆分。
- 是否明确失败，不做静默默认值。
- 是否覆盖主键、必填、数值、外键错误。
- 是否没有修改 gateway 行为。
- 是否没有扩大到 core 下沉或玩法扩展。
- 是否可以在下一阶段把 `FirstChainGatewayService` 的硬编码迁移到配置和 core。
