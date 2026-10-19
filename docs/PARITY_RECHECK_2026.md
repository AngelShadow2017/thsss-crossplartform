# 原版与移植版再次对比（2026-10-21）

> **2026-10-21 已修复**：下面两项差异均已修补并通过回归，结论见文末「修补结果」；本篇其余内容是修复前证据。

## 范围与结论

本轮只读比较 `reference/decompiled/` 与 `src/`，复核修复后的实际代码，未修改生产逻辑或玩家数据。不是重复列出修复前的19组问题，也不是原Windows全关逐帧验收。

新确认两个有活动调用链的剩余差异：默认字体单位、FixedFPS卡顿后追帧。两项都是源码契约确认，未在原Windows CLR2下双版本实测。其余抽查未发现达到高置信度门槛的新问题，不代表不存在其他差异。

## 1. 默认字号把point当成pixel

**影响：** 对白字号、宽度、换行位置与原作不一致。优先级高。

- 原版 `reference/decompiled/Shooting/StoryBox.cs:88` 使用 System.Drawing.Font(name,16f,style)，默认单位是Point。
- 当前同一调用保留，但 `src/Platform/Compat/AdditionalTypes.cs:168–173` 两个默认构造器没有初始化Unit；`:183` 的零值是Pixel。
- `src/Platform/Compat/GraphicsText.cs:123–130` 只有非Pixel才乘96/72。因此在端口当前96DPI模型中，16pt本应约21.33px，实际上按16px绘制。
- 真实路径是剧情BaseStory_SSS → StoryBox测量/生成文字纹理，不是闲置API。
- `tests/TextScreenshotCompatibility/Program.cs` 的默认字体与显式Pixel等宽预期反而固化了错误。现有167断言通过不证明此项一致。

建议后续修复默认Unit=Point，显式Pixel保持不变；验收默认16pt等价显式21.33px，并重新核对真实对白行数与裁切。字体替代（楷体/宋体→可用CJK字体）本身是平台差异，不能与单位错误混为一谈。

## 2. FixedFPS累计时钟不再驱动真实窗口等待

**影响：** VSync关闭的软件限速路径，短暂卡顿后的追帧行为与原作不同。优先级高；具体差值尚未运行量化。

- 原版 `reference/decompiled/Shooting/Game_Main.cs:363–372,514–527` 推进累计计时基准，并用它决定Sleep/等待；在容忍范围内落后时可通过后续不等待赶回进度。
- 当前 `src/Shooting/Game_Main.cs:548–562` 保留计时修正，但只有 `Form_Main.Window == null` 才使用该基准等待。
- 实际窗口 `src/Platform/Compat/RenderForm.cs:75–79` 独立固定为60/interval帧率，不接收fixedFPS或累计欠帧。修正计时基准不能直接改变这个外部调度器。
- 当前GLFW测试只证明无卡顿情况下30Hz平均节拍，不证明FixedFPS=0/1在一次卡顿后的行为相同。

建议后续统一下一帧期限与累计时钟，避免双重限速。验收关闭VSync，分别FixedFPS=0/1，注入低于DelayTolerance上限的卡顿，比较恢复期间逻辑帧数与墙钟时间。不能以恒定帧率的冒烟测试替代。

## 已修复项与保持一致部分

重新运行的基线：`/tmp/thsss-parity-recheck-behavior.log`（5套）、`/tmp/thsss-parity-recheck-shader.log`（8套），组合命令退出0。Audio162、Save84、Text167、Window109及光照110等现有断言仍通过。这些是端口自身回归，不是原版二进制对照。

本轮抽查确认旧的音频循环/暂停/声像、坏档保护、输入桥接、P截图时序等修复仍在。MyRandom与种子调用在所查范围内保持原逻辑；失焦清键及回放意外EOF退出已实现，不再作为旧缺陷重复报告。

原版523个C#文件按路径映射后：311个字节一致、206个有改动、6个原路径没有对应文件。统计只表示源码范围，不表示206个玩法差异。对应清单 `/tmp/thsss-parity-source-inventory.json`。没有同路径对应的文件包括入口、生成资源/设置、AssemblyInfo与旧MCI模块；当前有新入口与资源适配，不能按缺文件直接认定功能丢失。原APIClass只被旧MP3_Player引用，未找到该播放器在游戏其他源码的调用。

## 有意差异与验证边界

- 坏档禁止自动覆盖、原子备份、截断录像明确拒绝，是安全改进，不建议为了字面一致撤销。
- OpenGL/OpenAL、系统字体替代及跨平台窗口桥接与原DirectX/WinForms不是同一后端；已有公式/样本测试，但没有全场景像素/听感金标准。
- `Matrix.Invert`仍返回Identity，音频FilterON相关支持也需谨慎，但本轮没有找到游戏活动调用，列为潜在兼容缺口，不冒充实际关卡故障。
- 机器存在Wine和Mono不代表可执行原CLR2+SlimDX；此前原版隔离运行失败，当前仍未建立原Windows运行基线。没有安装/修改Wine环境，也没有用原版运行触碰玩家数据。
- 实体手柄、多刷新率显示器、全关旧Replay逐帧一致性仍未覆盖。

## 下一步优先级

先修默认字体单位并调整测试，再补FixedFPS卡顿注入测试和统一调度。其后才做原版可运行环境的逐帧/旧录像金标准，不以13套回归通过宣称完全复刻。

## 修补结果（2026-10-21）

### 1. 默认字号恢复 Point 语义

`src/Platform/Compat/AdditionalTypes.cs:169–173` 中 `Font(name,size)` 与 `Font(name,size,style)` 现在显式设置 `Unit = GraphicsUnit.Point`，与 GDI+ 默认一致；带 `GraphicsUnit` 参数的构造器语义不变，DX Font 的原生“按像素高度”构造器仍显式传 Pixel。

测试不再固化旧错误：`tests/TextScreenshotCompatibility/Program.cs` 新增断言默认单位是 Point、16pt 等价 21.33px、且 16pt 明显大于 16px。修复前该套件在第一条新断言即失败（`/tmp/thsss-font-unit-before.log`），修复后 171 条断言全通过（`/tmp/thsss-font-unit-after.log`）。副作用也符合预期：同一段中文测宽由旧值变为 1457×23，真实 StoryBox 由 5 行变为 7 行——这表明字号确实变大，而非测试放宽。

受影响的游戏调用点（主菜单、ED、关卡提示、StoryBox、DescriptionBox）都是原版使用 point 的位置，没有逐个改字体字号。

CPU Graphics 与 GPU DXFont 的墨迹面积容差由15像素改为“两者相差不超过较大者的10%”，因为两套栅格器抗锯齿不同；Point 单位与 21.33px 等价这两项仍为严格相等断言，没有被放宽。

### 2. FixedFPS 累计时钟重新驱动真实窗口

- `src/Shooting/Game_Main.cs:548–573` 恢复原版等待逻辑：`FinishFrameTiming` 由生产计时器提供时长/Sleep/推帧，先按累计 `num` 等待到 deadline，再做原 16.62ms 采样循环，最后按 `DelayTolerance` 补偿累积基准。`verticalSync && (presentInterval == 1 || fullWindow)` 仍提前返回，与原版一致。
- `Form_Main.ConfigurePresentation(..., gameOwnsPacing: true)` 在初始化和 F8 切换两处启用；`src/Platform/Compat/RenderForm.cs:79` 此时不再叠加 GLFW `FramesPerSecond`，独立使用 RenderForm 的其它场景保持原行为。

验证分两层：

- 确定性：`tests/WindowInputCompatibility/FrameTiming.cs` 以生产 `FinishFrameTiming` 对比原版公式，144 种场景×30 帧（FixedFPS 0/1、100/500ms 卡顿、PresentInterval 1–3、DelayTolerance 0/2/10、全屏/同步/启动画面组合）一致。
- 真实循环：隐藏 GLFW + 真实时钟，卡顿后 FixedFPS=0 恢复约 16.7ms/帧，FixedFPS=1 出现 5 个近乎立即的追帧，然后回到 16.49ms/帧。合并到套件后 `WindowInputCompatibility` 为 4721 断言，包含两种输入模式和完整游戏进程。

### 最终回归

- `sh tools/run-behavior-regressions.sh`：List/Audio162/Save84/Text171/WindowInput4721 全通过，日志 `/tmp/thsss-parity-fix-behavior.log`。
- `sh tools/run-shader-regressions.sh`：8 套通过，含 59 条 Mesh、110 条光照像素断言，日志 `/tmp/thsss-parity-fix-shader.log`。
- `dotnet build src/THSSS.csproj`：0 错误。

**仍未证明的边界**：卡顿测试使用生产计时函数和真实 GLFW 循环，不是完整游戏负载；没有测量硬件 VSync 节拍、实体手柄、多刷新率显示器或原 Windows CLR2 逐帧金标准。字体修复未做原版截图逐像素比对，字号方向由 GDI+ 文档与单位断言支撑。
