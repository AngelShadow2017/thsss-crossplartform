# 测试目录（2026-10-26）

从 `tests/` 运行。SDK 版本由根 `global.json` 固定；安装的 SDK 10 workload manifest 不完整，那是环境问题，不是代码问题。

## 布局

```
tests/
├── <suite>/          C# 测试项目，`<suite>/<suite>.csproj`
└── dejagnu/<tool>/   DejaGnu 驱动，每个 toolkit 单独 srcdir
```

项目引用为 `../../src/THSSS.csproj`。曾使用 `test/testcase/` 的旧布局已合并，不要再引用该路径。

DejaGnu 每个 toolkit 必须有独立源目录，因为 `runtest --srcdir` 会执行其中所有 `.exp`；共用目录会让一个 runner 跑到别的 toolkit 用例。

| Toolkit | Runner | 用例 |
|---|---|---|
| `shader` | `tools/run-shader-regressions.sh` | 9 套渲染 |
| `behavior` | `tools/run-behavior-regressions.sh` | 8 套行为，另加 GL/Vulkan 各一次窗口缩放 |
| `audio` | `tools/run-audio-regressions.sh` | Linux OpenAL、错误选择、可选 Wine 原生 XAudio2 |
| 缩放 | `tools/run-resize-regressions.sh` | GL/Vulkan 各 8 项 |

驱动只负责进程、超时和退出码；数值断言留在 C# 里。每个用例必须匹配自己的 PASS 标记，进程在打印前崩溃不能被当成通过。渲染套件依赖相对资源路径，需在 `src` 下运行；缩放和音频需要独立的临时数据根，不得写入 `game/`。

## 关键套件

- `ListCompatibility`：pre-4.5 实时索引枚举（删除、同轮追加、真实事件/背景）以及工作线程停止超时。生产调用点用 `LegacyItems`，不是快照。
- `SaveCompatibility`：只用临时存档与录像，校验 CP936/UTF-8 字节保留、坏档回滚、原子备份与 Replay 数字/EOF 契约。
- `TextScreenshotCompatibility`：171 条断言，含 Point 单位、逻辑后台缓冲区缩放，以及 P 键截图在交换之后取帧。
- `WindowInputCompatibility --glfw --game-smoke`：隐藏 GLFW 真实窗口、两种键盘模式、改键/F8/退出、焦点与手柄快照；Linux 下过滤鼠标/触控板误识别。实体手柄与可变刷新率仍需人工验证。
- `VulkanBackend`：直接调用游戏实际 `Sprite`/`MySprite`、菜单图集与透明叠加，覆盖 400 次绘制一次提交、不同变换/颜色快照、跳过 Present 的 EndFrame、静态字形缓存失效。`THSSS_DIAG_FPS=1` 输出实际 FPS 与每帧提交/draw；性能测试不要同时开像素读回诊断。
- `AudioBackendCompatibility`：后端中立契约，含精确循环、暂停游标、并发 voice、故障注入与幂等释放；另断言 XAudio2/Win32 类型只在 Windows 产物出现。

`BehaviorParityAudit` 是修复前的诊断快照，不属于验收套件，已排除在 runner 之外。

## 验收

| 套件 | 断言 |
|---|---|
| `AudioCompatibility` | 162 条 OpenAL loopback |
| `SaveCompatibility` | 84 条，含合法结局 `LastStage=All` 与三位逗号小数 |
| `TextScreenshotCompatibility` | 171 条 |
| `WindowInputCompatibility` | 4721 条，含两种键盘模式、144 种确定性计时场景与 Alt+F4 完整进程 |

当前行为矩阵见 `../docs/BEHAVIOR_PARITY_FIXES_2026.md`；渲染状态见 `../docs/RENDER_BACKEND_STATUS_2026.md`；音频见 `../docs/AUDIO_BACKENDS_2026.md`。所有实际游戏测试使用临时配置、Score 与独立录像目录，不写 live 玩家数据。
