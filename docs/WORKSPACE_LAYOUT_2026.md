# 工作区归拢（2026-10-20）

## 当前入口

- 源码：`../src/THSSS.csproj`；运行：根目录 `sh tools/run-game.sh`。
- 游戏资源、配置与玩家数据：`../game/`，内部相对路径不变。
- 原版与历史：`../reference/original/`、`../reference/decompiled/`、`../reference/THSSS-Refactored/`。
- 报告：本目录；回归与工具：`../tests/`、`../tools/`。
- SDK：根目录 `global.json`，从根目录构建即可，不再依赖切换进旧项目目录。

## 搬迁保护

搬迁前生成 `/tmp/thsss-workspace-move-2026.json`，记录1,519个游戏资源/数据文件的SHA256；采用同文件系统重命名，不覆盖目标。原版EXE/DLL及.resources也按文件重命名保留。

整理验证期间检测到 `Replay/AutoSave.rpy`、`Score.dat`、`Score.dat.bak` 发生运行时更新，因来源无法仅靠文件时间确定，最终没有继续强制覆盖当前版本。搬迁前字节与观察到的更新版本均保存在：

- `game/backups/workspace-2026-10-20/before-move/`
- `game/backups/workspace-2026-10-20/observed-after-move/`

其余1,516个原游戏文件最终SHA256与搬迁前完全一致。旧数据备份按搬迁清单校验，不是恢复到原版初始存档。模型回归现在使用临时Model工作目录，不再切进live game数据根。

## 验证

- 根目录 `dotnet build src/THSSS.csproj`：0错误。
- `tools/run-shader-regressions.sh`：8套通过。
- `tools/run-behavior-regressions.sh`：5套通过。
- 从 `/tmp` 调用启动脚本，指定隔离测试数据：正常菜单退出0并保存测试Score，不依赖调用者工作目录。
- 根目录publish到 `/tmp/thsss-organized-publish`：1,514个发布资产逐文件匹配；发布包不含玩家Score、Replay、ScreenShot。
- 现存ImageSharp依赖安全告警与旧ini-parser兼容告警未在本次目录整理中升级或隐藏。

本轮没有删除实际bin/obj文件，也没有提交历史代码与玩家数据。新增.gitignore使构建、发布和缓存不再作为新文件混入版本控制。
