# macOS（2026-10-29）

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

M 系列 Mac 的目标 RID 为 `osx-arm64`。构建机需要 .NET 10 SDK、Perl（macOS 系统提供）、完整 Git LFS 资源，以及 `glslangValidator`、`spirv-cross`。一次准备后用单一入口打包：

```sh
git lfs install
git lfs pull
brew install glslang spirv-cross
sh tools/package-macos.command
```

产物为 `dist/thsss-YYYYMMDD-HHMMSS/` 和同名 `.tar.gz`，包含游戏、Config、.NET 10 ARM64 运行时、只读资源、当前 `Setting.INI` 和 `MetalShaders/`。解压后双击 `run-game.command` / `run-config.command`，也可用 `sh run-game.sh` / `sh run-config.sh`。接收测试包的 Mac 无需安装 SDK、运行时或转换工具。打包不启动游戏；缺少工具或 LFS 内容时会明确失败，不把指针文件装进包里。

打包入口调用 `tools/make-dist.pl --rid osx-arm64 --self-contained`，可追加 `--out dist/自定义名称`；默认每次使用新的时间戳目录，避免混入旧的运行产物。`--clean` 仅允许删除仓库 `dist/` 下已解析真实路径的子目录。已有 `Score.dat`、Replay 或 ScreenShot 内容会让复用发布目录失败。

`MetalShaders/*.msl` 文件名由源代码及阶段哈希组成，避免误用过期缓存。运行时先查发布目录缓存，开发环境缺少缓存才调用转换工具；二者皆不可用时给出明确错误。缓存包含实际游戏着色器，Metal窗口呈现的小型MSL随程序内置。

生产导出工具为 `tools/MetalShaderExporter/MetalShaderExporter.csproj`，直接读取游戏实际使用的 Sprite、3D 和 FX 着色器源，不解析 C# 文本或运行测试程序集。导出器按构建机 RID 运行，游戏和 Config 按发布 RID 构建。单独导出仍可运行 `sh tools/export-metal-shaders.sh 发布目录`。

配置程序为独立 Avalonia 工程，按相同 RID 发布。“应用并启动”显式传递正在编辑的数据根，避免配置与游戏使用不同的存档目录。包不会复制开发目录里的 Score/Replay/ScreenShot。

## 精灵提交顺序

`Sprite.Transform` 在每次 `Draw` 写入顶点时生效，保留四维齐次坐标；提交时给普通和 FX 着色器的变换均为恒等矩阵，避免重复变换。同一纹理的不同旋转/缩放可以在一批中提交，纹理比较使用 Texture 对象，不依赖每次可能重新分配的原生句柄包装。

混合/深度状态、目标/深度附件、Clear、3D 绘制、FX 开始/结束及参数变化之前提交待绘批次，保持原先的绘制顺序。文字显式 `Flush` 后才允许更新或释放字形纹理，不再依赖 Transform 自赋值的副作用。Sprite uniform 缓冲复用，普通提交不再创建三个临时数组。

## 3D 输入与异常退出

部分原版背景使用只有 Position/UV 的顶点流，而共享 3D shader 仍声明 Normal/Color/Secondary 输入。Metal 管线创建要求描述其保留的输入，不能依赖 `uHasNormal` 等运行时开关消除描述符要求。后端用槽 29 的常量流 `(0,0,0,1)` 补足缺失属性，`stepFunction=constant`、`stepRate=0`，由 Metal 按 shader 类型截取分量，保持 GL 默认属性值语义；真实顶点流仍在槽 30，uniform 使用槽 0–28。仅当存在属性引用时才配置相应流布局：没有缺失属性的管线不配置槽 29，也不绑定常量数据。是否使用常量流与管线一起缓存，创建和绘制使用同一条件，避免无属性引用的非零 stride 布局触发原生断言。没有修改原版背景顶点数据或关卡逻辑。

Silk.NET 2.23 的渲染回调抛出异常时不会复位内部 render-loop 标记，随后的窗口 Dispose 会再次抛出 Reset 异常。入口在渲染回调边界记录 `ExceptionDispatchInfo` 并结束循环，让框架正常复位标记后再传播原异常，保留原始堆栈和非零退出码。

## 菜单音效耗时

菜单每次播放选择音都会设置输出矩阵，再 Stop/Flush/Submit/Start。公共音频层在矩阵未变化时直接返回；原队列为空时只更新矩阵，不为不存在的数据重建队列。CoreAudio 的 `AudioQueueStop(..., true)` 已包含 Reset，Clear 不再额外调用一次 `AudioQueueReset`。

要记录实际 AudioQueue 操作耗时，在新发布目录运行：

```sh
THSSS_DIAG_INPUT=1 THSSS_DIAG_FPS=1 sh run-game.sh
```

`[AUDIO-TIMING]` 分别输出 Pause/Clear/Start 耗时，默认关闭。耗时包含调用期间的同步等待；不把这些日志当作整个键盘输入路径的采样。

已有 M 系列 Mac 实机反馈确认菜单可进入、菜单移动会卡顿、进关时因缺少 `aNormal` 顶点描述而崩溃；输入源为英文/ABC。本次针对这些反馈仅在 Windows 静态分析和修改，未构建、未打包、未运行测试或游戏，也未修改玩家数据。进关修复和菜单音效优化均需 Mac 复测，不能宣称卡顿已消失或达到目标帧率。Metal 后端原有的每次绘制同步等待仍保留。

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
