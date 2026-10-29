# 东方夏夜祭 · 跨平台移植

移植原版thsss（东方夏夜祭）到更现代的dotnet并且适配权平台

## 运行

```sh
sh tools/run-game.sh
```

需要 .NET 8 SDK 和系统 OpenGL / OpenAL。启动脚本自动使用 `game/` 的资源、配置与存档；不必再手工设置工作目录。SDK版本统一由根目录 `global.json` 固定。

## 配置

```sh
sh tools/run-config.sh
```

原版 `Config.exe` 的跨平台替代（Avalonia，Linux/macOS/Windows 同一份程序）。它读写同一个 `game/Setting.INI`：显示（全屏启动/垂直同步/绘制周期）、键盘与手柄按键、字体。点“应用”保存，点“应用并启动”保存后启动游戏。中文值按 GBK 保留，未改动的音量等键不会被重写。

## 找东西

| 目录 | 内容 |
|---|---|
| [`src/`](src/) | **当前唯一开发项目**：游戏 `src/THSSS.csproj`、配置程序 `src/Config/THSSS.Config.csproj` |
| [`game/`](game/) | **实际运行数据**：图片、音乐、模型、脚本，以及 `Setting.INI`、`Score.dat`、`Replay/`、`ScreenShot/` |
| [`reference/`](reference/) | **仅供参考**：`original/` 原版EXE/SlimDX/提取资源，`decompiled/` 反编译源码，`THSSS-Refactored/` 早期Windows尝试 |
| [`docs/`](docs/) | 审计与修复记录、原作说明；先看 [`docs/README.md`](docs/README.md) |
| [`tests/`](tests/) · [`tools/`](tools/) | 回归测试、启动与验收脚本 |

## 构建与发布

```sh
dotnet build src/THSSS.csproj
dotnet build src/Config/THSSS.Config.csproj
dotnet publish src/THSSS.csproj -c Release -o artifacts/publish
dotnet publish src/Config/THSSS.Config.csproj -c Release -o artifacts/publish
```

发布版会附带只读资源和初始配置，运行后在自己的目录创建存档/录像；**不会把 `game/Score.dat` 或你的录像打进发布包**。已有玩家数据只需保留在 `game/`，不要拿测试数据覆盖。

## 验证

```sh
sh tools/run-shader-regressions.sh
sh tools/run-behavior-regressions.sh
```

无图形桌面时可用 `ALSOFT_DRIVERS=null xvfb-run -a sh ...`。硬件手柄、扬声器听感、多刷新率和原版逐帧对照的边界见当前修复报告。

本次目录整理保留所有文件内容；没有删除或重置原版资源、存档、录像、配置和弹幕脚本。
