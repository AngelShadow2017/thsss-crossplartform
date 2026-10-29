# 测试改为 DejaGnu 驱动（2026-10-25）

## 结构

断言仍然留在各自的 C# 测试程序里；DejaGnu 只负责进程、超时、退出码和统一报告。

C# 测试程序在 `tests/<suite>/`，DejaGnu 驱动在 `tests/dejagnu/<tool>/`。

```
tests/dejagnu/
├── shader/shader.exp      渲染（9 套）
├── behavior/behavior.exp  行为（8 套 + GL/VK 各一次的窗口缩放）
└── audio/audio.exp        音频（OpenAL、Wine 原生 XAudio2、错误选择）
```

每个 toolkit 一个独立源目录。`runtest --srcdir` 会 source 目录下所有 `.exp`，共用一个目录会导致行为 runner 跑起音频用例——这是重构过程中实际踩到的。

## 三条约束

1. **必须打印各自的 PASS 标记**，且退出码为 0。只判断 `PASS` 任意出现不够：测试在打印前崩溃会被误判通过。所以每个用例匹配自己的标记，例如 `PASS SaveCompatibility`、`PASS: [0-9]+ legacy enumeration`。
2. **超时按用例设置**。窗口缩放要跑真实游戏循环，给 180 秒；其余默认 45 或 240 秒。
3. **运行目录与数据隔离**。渲染用例用 `../game/FX/Blur.fx` 这类相对路径，驱动先 `cd src/`；缩放用例由驱动建立临时数据根（符号链接资源、复制 `Setting.INI`），玩家的 `Score.dat` 与录像不被读写。

## 入口

```sh
sh tools/run-shader-regressions.sh
sh tools/run-behavior-regressions.sh
sh tools/run-audio-regressions.sh

# 保留报告（默认写入临时目录）
THSSS_TEST_REPORT_DIR=/tmp/reports sh tools/run-behavior-regressions.sh

# 无显示环境
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-behavior-regressions.sh
```

## 结果（本次实测）

| Runner | 结果 |
|---|---|
| `run-shader-regressions.sh` | `# of expected passes 9`，exit 0 |
| `run-behavior-regressions.sh` | `# of expected passes 10`，exit 0 |
| `run-audio-regressions.sh` | `# of expected passes 3`，exit 0 |
| `run-resize-regressions.sh` | GL/VK 各 8 项断言通过，exit 0 |

## 重构中修掉的问题

| 问题 | 原因 |
|---|---|
| 行为 runner 报 `THSSS_AUDIO_TEST_DLL` 缺失 | 共用 srcdir，行为 runner 执行了音频用例 |
| `WindowResizeCompatibility` 报 `NowLoading-0` 缺失 | 驱动没给它传隔离数据根 |
| 缩放用例超时 | 统一 45 秒不够跑真实游戏循环 |
| `ShaderAudit` / `EffectCompatibility` 崩溃 | 驱动没 `cd src/`，相对资源路径失效 |

## 未覆盖

- DejaGnu 的 `native` 变体没有拆分；同一套用例只在 Linux 主机上跑
- Windows 侧仍依靠手工/脚本在虚拟机里执行，没有接入 DejaGnu
