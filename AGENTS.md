# 根本目标

移植原版thsss（东方夏夜祭）到更新的dotnet并且适配权平台

任何时候要尽可能贴合原版thsss

## 工作区入口（2026-10-20）

- `src/THSSS.csproj` 是唯一当前开发项目；`reference/` 下的原版、反编译和早期尝试只用于对照，不作为启动入口。
- `game/` 是运行数据根：资产、配置、Score/Replay/ScreenShot一起保留。测试必须复制/使用临时数据，不得覆盖这里的玩家数据。
- 从根运行 `sh tools/run-game.sh`；SDK唯一配置是根 `global.json`。回归入口仍为 `tools/run-shader-regressions.sh` 和 `tools/run-behavior-regressions.sh`。
- 当前报告集中在 `docs/`，`docs/README.md` 提供人类阅读索引。旧审计是历史快照，以对应FIXES文档为当前状态。
- 不因目录整理或版本控制噪声删除/恢复既有数据变化；特别保留 `game/CS/`、`game/Score.dat`、`game/Setting.INI` 与录像。
