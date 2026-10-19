# 文档入口

当前状态优先看“修复与验收”，审计文件保留的是修复前快照。

最新复查（2026-10-21）：[PARITY_RECHECK_2026.md](PARITY_RECHECK_2026.md)。发现默认字体单位与 FixedFPS 追帧仍有差异；此前修复报告不能据此解释为完全一致。

| 主题 | 当前修复 | 原始审计 |
|---|---|---|
| 原作行为、输入、音频、存档、录像 | [BEHAVIOR_PARITY_FIXES_2026.md](BEHAVIOR_PARITY_FIXES_2026.md) | [BEHAVIOR_PARITY_AUDIT_2026.md](BEHAVIOR_PARITY_AUDIT_2026.md) |
| shader、模型、光照、后处理 | [SHADER_FIXES_2026.md](SHADER_FIXES_2026.md) | [SHADER_AUDIT_2026.md](SHADER_AUDIT_2026.md) |

原作文字说明：`角色设定&ExStory.txt`（原文件内容与编码保留）。

目录整理后的入口：源码 `../src/`、运行数据 `../game/`、原版参考 `../reference/`。根目录 README 提供启动、构建和回归命令。

本轮目录迁移与数据备份说明：[WORKSPACE_LAYOUT_2026.md](WORKSPACE_LAYOUT_2026.md)。
