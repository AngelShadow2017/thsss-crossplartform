# GitHub Actions

`.github/workflows/ci.yml` 有四个作业：Linux、Windows、macOS、Wine。

- 平台无关的行为套件由三个系统的 `tools/run-behavior-regressions.sh` 共同执行，改套件时不能只验一个系统。
- Linux 额外运行GL/Vulkan渲染、OpenAL音频、Metal与AudioQueue软件API驱动；Windows使用真实XAudio2和Win32；macOS使用真实Metal与CoreAudio。三处`NativeApiCompatibility`失败即作业失败，`THSSS_REQUIRE_NATIVE_API=1`让无法创建设备的环境返回非零，不能以跳过冒充通过。
- macOS的Metal API契约和着色器转换在macOS原生运行；Linux上同名软件驱动只证明调用契约。
- Wine作业依赖Windows产出的自包含测试EXE，用于比较Wine下XAudio2契约，不替代Windows实机。
- 修改工作流后本地先跑 `actionlint -no-color .github/workflows/ci.yml`，并尽量在本地复现被改的入口。不要把玩家数据带进CI。
