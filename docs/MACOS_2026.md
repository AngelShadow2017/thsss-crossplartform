# macOS（2026-10-28）

## 运行方式

macOS 默认渲染后端为原生 Metal，音频为 AudioToolbox AudioQueue（`coreaudio`）。不经 MoltenVK，也不把 OpenAL 作为默认音频依赖。

```sh
sh tools/run-game.sh
# 显式选择：
THSSS_RENDERER=metal THSSS_AUDIO_BACKEND=coreaudio sh tools/run-game.sh
```

Intel 对应 `osx-x64`，Apple Silicon 对应 `osx-arm64`。Metal 需要系统提供 Metal device；设备不可用会明确报错，不静默切换渲染器。原生调用使用系统 Metal、QuartzCore、Objective-C runtime 与 AudioToolbox framework，不要求 .NET macOS workload。

## 实现契约

- `IRenderBackend` 下的 Metal 后端拥有纹理、共享缓冲、深度附件、采样器、管线及命令队列。Cocoa NSWindow 由现有 GLFW 窗口取得，附加 CAMetalLayer；不另建不兼容的窗口/输入循环。
- 图像逻辑保持640×480；drawable按实际Framebuffer尺寸更新。离屏纹理使用与既有后端一致的行约定，呈现shader只进行一次坐标映射，保留完整画面和黑边。
- 各次绘制使用真实Metal selector与专用结构体/浮点P/Invoke签名；提交错误转换为包含NSError的异常。每帧释放autorelease对象，显式资源销毁时解除追踪，外部深度附件不转移所有权。
- Metal使用共享GLSL源经glslangValidator→SPIR-V→SPIRV-Cross MSL转换。游戏实际Sprite、3D、Blur/Filter/Warp均覆盖；Metal系统编译生成的MSL，不复制另一套效果公式。
- CoreAudio接受公共层混音后的交错float32立体声块。AudioQueue回调表示系统已获取buffer，不表示声音已播放；游标与退休判断采用设备SampleTime。暂停冻结游标，Clear清队列并重新建立时间起点，Dispose同步等待回调后释放。
- `AudioStreamBasicDescription`为40字节，AudioTimeStamp为64字节，AudioQueueBuffer数据指针偏移为8字节（64位ABI）。

## 发布与着色器

构建机需要 `dotnet`、`glslangValidator`、`spirv-cross`。将MSL预先导出到发布目录，用户运行时无需这些转换工具：

```sh
dotnet publish src/THSSS.csproj -r osx-arm64 --self-contained true -o dist/mac-arm64
sh tools/export-metal-shaders.sh dist/mac-arm64
```

`MetalShaders/*.msl` 文件名由源代码及阶段哈希组成，避免误用过期缓存。运行时先查发布目录缓存，开发环境缺少缓存才调用转换工具；二者皆不可用时给出明确错误。缓存包含实际游戏着色器，Metal窗口呈现的小型MSL随程序内置。

配置程序为独立Avalonia工程，按相同RID发布。正式分发应使用 `tools/make-dist.pl`，保留其不复制玩家Score/Replay/ScreenShot的约束。

## 自动验证

```sh
sh tools/run-mac-regressions.sh
```

Linux x86_64验收环境需要C编译器、DejaGnu、glslangValidator与spirv-cross。三条DejaGnu用例：

1. AudioQueue原生ABI与软件队列驱动：预取回调、游标、暂停恢复、退休与重置。
2. 实际Metal后端P/Invoke的软件Objective-C/Metal驱动：selector和结构体参数、上传行距、清屏/读回、命令顺序、错误传播、所有权以及Cocoa drawable呈现。
3. 实际游戏GLSL经过真实编译器生成MSL；另测试无转换工具PATH时仍可载入全部发布缓存。

软件驱动不会执行MSL光栅化。清屏/存储像素和调用顺序是真实跨原生边界的测试；呈现方向另由实际转换后的MSL及非对称坐标断言核对，不冒称Apple驱动截图或扬声器听感测试。`osx-x64`、`osx-arm64`交叉发布验证编译与资源；Linux既有GL/Vulkan回归独立运行。

## 边界

没有把缺少GPU加速当成测试不可开展的理由。上述软件API驱动可验证调用契约，但不能替代Apple系统实现；本环境尚未完成Apple运行时的完整游戏/音频输出测试，Apple Silicon的调用约定仅交叉编译和结构布局核对。macOS签名、公证、安装器与实体设备性能不在当前自动验收范围。

默认Metal附件为单采样；MSAA开关没有制造虚假多采样能力。采样器的任意自定义BorderColor、MipLodBias及非当前游戏使用的拓展功能不应被宣称完整支持。

OpenGL保留既有实现且未因macOS而降级。Sampler objects在OpenGL 3.3已进入核心，`RGB8`也是合法内部格式；不能以“它们需要4.2/在core非法”为依据删除已验证的路径。macOS可选GL路径的最终系统运行验证独立于Metal测试。
