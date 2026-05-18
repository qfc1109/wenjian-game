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

- 视觉生成工具实际使用方式。
- 首批素材输出目录。
- 首批素材文件清单与质量评估。

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
