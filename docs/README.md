# 文档索引

## 当前文档

| 主题 | 文档 | 内容 |
|---|---|---|
| macOS | [MACOS_2026.md](MACOS_2026.md) | 原生Metal/AudioQueue、MSL发布缓存、API软件驱动与验证边界 |
| 渲染 | [RENDER_BACKEND_STATUS_2026.md](RENDER_BACKEND_STATUS_2026.md) | 渲染后端与能力边界 |
| 渲染 | [SHADER_FIXES_2026.md](SHADER_FIXES_2026.md) | FX、混合、深度与光照 |
| 窗口 | [WINDOW_SCALING_2026.md](WINDOW_SCALING_2026.md) | 4:3缩放与Framebuffer/DPI |
| 音频 | [AUDIO_BACKENDS_2026.md](AUDIO_BACKENDS_2026.md) | 播放语义和平台选择；macOS细节见上表 |
| 存档 | [MANAGED_DES_FALLBACK_2026.md](MANAGED_DES_FALLBACK_2026.md) | Wine缺DES时的可选编译回退 |
| 行为 | [BEHAVIOR_PARITY_FIXES_2026.md](BEHAVIOR_PARITY_FIXES_2026.md) | 行为约定、字体与帧节奏 |
| Windows | [WINDOWS_STARTUP_FIX_2026.md](WINDOWS_STARTUP_FIX_2026.md) | 图集键与启动错误 |
| 测试 | [DEJAGNU_TESTS_2026.md](DEJAGNU_TESTS_2026.md) | 驱动结构与入口 |

`角色设定&ExStory.txt`是原作资料，保留原内容及编码。

## 目录

| 路径 | 内容 |
|---|---|
| `src/` | 游戏与独立配置程序 |
| `game/` | 资源、配置和玩家数据 |
| `reference/` | 原版、反编译与早期尝试 |
| `tests/` | C#及原生API测试程序 |
| `tests/dejagnu/` | 进程验收驱动 |
| `tools/` | 启动、回归与分发脚本 |

## 验证入口

```sh
sh tools/run-game.sh
sh tools/run-shader-regressions.sh
sh tools/run-behavior-regressions.sh
sh tools/run-audio-regressions.sh
sh tools/run-resize-regressions.sh
sh tools/run-mac-regressions.sh
```

Linux无显示环境的窗口测试加 `ALSOFT_DRIVERS=null xvfb-run -a`。mac软件API驱动无需显示服务器，但需要编译器与转换工具。所有测试使用隔离数据，不覆盖玩家存档。
