# DejaGnu 测试驱动（2026-10-29）

## 结构

```
tests/
├── testcase/<Suite>/         C# 夹具，引用 ../../../src/THSSS.csproj
└── dejagnu/<tool>/           .exp 驱动，每个 toolkit 独立 srcdir
```

一个 toolkit 一个源目录是硬性要求：`runtest --srcdir` 会执行目录内所有 `.exp`，共用会让某个 runner 顺带跑到别人的用例。

| Toolkit | Runner | 用例 |
|---|---|---|
| `shader` | `tools/run-shader-regressions.sh` | 9 套渲染 |
| `behavior` | `tools/run-behavior-regressions.sh` | 8 套行为 + GL/VK 各一次窗口缩放 |
| `audio` | `tools/run-audio-regressions.sh` | OpenAL 契约、非法后端、可选 Wine 原生 XAudio2 |
| `mac` | `tools/run-mac-regressions.sh` | Metal/AudioQueue 软件 API 驱动、真实着色器转译 |
| 缩放 | `tools/run-resize-regressions.sh` | GL/VK 各 8 项 |

## 判定规则

驱动只管进程、超时和退出码，数值断言留在 C# 里。三条规则：

1. **每个用例匹配自己的 PASS 标记**。只看退出码会放过“打印前就死掉”的夹具。
2. **用例显式声明超时**。默认 45 秒；真实游戏循环类（窗口缩放）给 180 秒。
3. **数据隔离**。需要玩家数据的夹具拷贝到临时目录，或传入独立数据根；绝不写 `game/`。

## 运行目录

渲染类夹具使用相对路径读取 `game/FX`、`game/Model`，因此 `shader.exp` 在运行前切到 `src/`。移动夹具目录时，`AppContext.BaseDirectory` 向上遍历的层数会变——`MetalCompatibility` 是唯一靠固定层数找仓库根的，移动时最容易漏。

## 入口

```sh
sh tools/run-shader-regressions.sh
sh tools/run-behavior-regressions.sh
sh tools/run-audio-regressions.sh
sh tools/run-mac-regressions.sh
sh tools/run-resize-regressions.sh
```

Linux 无显示环境加 `ALSOFT_DRIVERS=null xvfb-run -a`。mac 驱动额外需要 C 编译器、`glslangValidator`、`spirv-cross`；缺工具会明确报错，不会静默跳过。
