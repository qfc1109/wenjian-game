# wenjian-game-server

Java 后端多模块工程目录。

第一阶段目标是建立服务端全权威的最小可运行链路：

- WebSocket 登录和业务请求。
- KCP 实时输入和状态同步。
- 区域式大世界。
- 单人秘境实例。
- 服务端权威战斗 Tick。
- 配置加载与校验。
- protobuf 生成代码。

后续计划采用 Maven 3.9.x、Java 21 LTS 和 Spring Boot 当前稳定 4.0.x 线。

当前已创建最小 Maven 多模块骨架：

- `wenjian-share`：通用类型与工具。
- `wenjian-protobuf`：服务端 protobuf 集成占位。
- `wenjian-config`：CSV 配置加载与校验。
- `wenjian-game-core`：服务端权威游戏状态和战斗核心。
- `wenjian-gateway-ws`：WebSocket 登录、进入区域和快照下发入口。

第一阶段只保留这些最小模块，暂不创建 KCP、后台、背包、任务、聊天、邮件、支付、排行榜等低频或外围模块。

本地验证建议使用：

```powershell
$env:JAVA_HOME='E:\JAVA\jdk21'
$env:MAVEN_HOME='E:\apache-maven-3.9.16'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
mvn -q -f wenjian-game-server\pom.xml -DskipTests validate
```

或进入本目录后执行：

```powershell
$env:JAVA_HOME='E:\JAVA\jdk21'
$env:MAVEN_HOME='E:\apache-maven-3.9.16'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
mvn -q -DskipTests validate
```

启动第一条 WebSocket 验收链路时，进入本目录后执行：

```powershell
$env:JAVA_HOME='E:\JAVA\jdk21'
$env:MAVEN_HOME='E:\apache-maven-3.9.16'
$env:Path="$env:JAVA_HOME\bin;$env:MAVEN_HOME\bin;$env:Path"
mvn -q -pl wenjian-gateway-ws -am test-compile spring-boot:run -Dspring-boot.run.fork=false -Dspring-boot.run.arguments=--server.port=18080
```

当前临时 WebSocket 路径为 `ws://127.0.0.1:18080/ws/first-chain`。第一阶段文本调试消息：

- `LOGIN local-dev-key 1700000000000`
- `ENTER_REGION 1000001 1001`
- `SKILL 1000001 2001 1 0`
- `START_ROGUE 1000001 4001`
- `FINISH_ROGUE 1000001 9000001`

`SKILL` 调试消息当前使用 `config/source/player_template.csv` 中的默认技能 `2001`，对应 `config/source/skill.csv` 的首个剑气技能。服务端会返回一个 `SKILL_EVENT`，并在命中训练敌人时追加一个 `DAMAGE_EVENT`。

`START_ROGUE` 调试消息当前使用 `config/source/rogue.csv` 中的默认秘境 `4001`，返回固定实例 `9000001`、地图 `2001`、出生点 `1500,1500`、怪物 `3001 x 8` 和奖励池 `5001`。`FINISH_ROGUE` 会在该实例已启动时返回 `ROGUE_FINISH`，并按 `config/source/reward_pool.csv` 返回固定奖励 `itemId=6001 count=3`。
