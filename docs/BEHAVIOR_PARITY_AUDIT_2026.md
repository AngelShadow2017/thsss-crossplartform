# 原作行为差异与剩余问题审计（2026-10-20）

> 本文是修复前审计快照。后续修复状态与验收证据见 [`BEHAVIOR_PARITY_FIXES_2026.md`](BEHAVIOR_PARITY_FIXES_2026.md)；不要把历史 `CONFIRMED` 输出继续当作当前实现状态。

## 范围与证据等级

目标：比较原版 THSSS 与当前 .NET 8/OpenGL/OpenAL 移植版，找出行为差异和仍有问题的路径。此次只审查、添加隔离诊断和报告，**未改动生产游戏代码、用户Score/Replay或配置，也未将缺陷诊断误报为修复验收**。

- `O/` = `reference/decompiled/Shooting/`（原版反编译源码）；`C/` = `src/Shooting/`；`P/` = `src/Platform/`。
- **运行复现**：当前端口真实方法/GL/OpenAL或已发布入口测到异常。
- **源码确认**：完整调用链及空实现证明行为缺失，但没有进行桌面/手柄硬件操作。
- **兼容风险**：差异已测，但无法直接测原Windows CLR2，因此不宣称原版逐帧后果已量化。
- **原作既存缺陷**：原反编译代码同样存在，明确不算新移植回归。

原版程序集 `THSSS.exe` 标识：1.0.0.1。尝试在临时隔离目录运行原版+SlimDX，使用已安装Mono；环境报告CLR2不可用并退到v4，且SlimDX C++/CLI原生方法不能执行。12秒超时终止，没有有效原版运行基线。日志 `/tmp/thsss-original-runtime-attempt.log`。Wine存在不等于原版.NET2/SlimDX环境就绪，本次没有安装依赖或改Wine配置。因此以下比较主要是原源码契约与移植版实测，**不是Windows原版全关逐帧截图/Replay金标准**。

## 必须优先处理

### 1. 音乐不按原作循环点或起播位置播放（运行复现，移植回归）

- 当前 `P/Compat/XAudio2Engine.cs:122–142,245–255` 保存PlayBegin/PlayLength/LoopBegin/LoopLength/LoopCount字段，却只将完整PCM绑定成一个非循环静态OpenAL buffer。
- 原 `O/XAudio2_PlayerV2.cs:130–144` 将这些区间交给真实XAudio2。活动场景：`C/GameState_MainMenu.cs:181` 的无限循环、`C/MenuGroup_MusicRoom.cs:131–143` 的指定起点。
- 实测：0.15秒静音WAV要求LoopCount=255，0.45秒后AL状态4116（Stopped）。不是“音乐有启动voice所以循环正确”。
- 影响：曲子播完变无声、Music Room不能从选定位置起播、关卡循环段不对。
- 最小修复方向：按样本帧落实播放/循环区间，支持无限和有限次数；测试非零起点、非全曲循环和自然结束队列状态。

### 2. 音乐暂停后恢复变成从头播放（运行复现，移植回归）

- 当前 `P/Compat/XAudio2Engine.cs:146–159` 用SourceStop/SourcePlay模拟XAudio2 Stop/Start。
- 原 `O/Wave_Player.cs:100–109`、`O/XAudio2_PlayerV2.cs:92–108` 依赖Stop/Start保留播放游标；`C/GlobalDataPackage.cs:376–385` 仍使用同一调用协议。
- 实测：2秒静音WAV先播放约0.2秒，SampleOffset=4468；Stop/Start后offset=0。
- 影响：暂停、返回菜单或切换状态后音乐反复重新开始。
- 修复方向：把暂停/继续与真正停止/清空分开，使用可保持offset的后端操作。

### 3. 声像设置覆盖音量，左右定位消失（运行复现，移植回归）

- 当前 `P/Compat/XAudio2Engine.cs:183–191` 把输出矩阵系数求和后写入Volume，不做左右通道混合。
- 原 `O/XAudio2_Player.cs:67–85` 独立设置声像与Volume；活动调用 `C/StageDataPackage.cs:238–254`。
- 实测：Volume=0.2后SetOutputMatrix(1,2,[1,0])变成1.0；这不只是左右声像不准，也破坏设置的音量。
- 修复方向：保持音量独立，真实实现单声道左右增益/立体声矩阵；测试静音/小音量不能被Play覆盖。

### 4. 非DirectInput模式完全没有键盘输入（源码确认，移植回归）

- 原 `O/Game_Main.cs:158–167` 在UseDirectInput=0时订阅真实Control事件。当前 `C/Game_Main.cs:167–175` 同样订阅适配Form，但真实窗口在 `src/Program.cs:46–63`，事件只写KeyboardInput。
- 当前 `C/Game_Main.cs:446` 此时跳过轮询，适配Form也不收到真实事件。
- 影响：UseDirectInput=0时方向、确认、ESC和快捷键不响应；此模式的自定义键位也不能工作。
- 修复方向：统一真实窗口事件源，并分别测试UseDirectInput=0/1及自定义键位，不只直接注入KeyboardInput。

### 5. 游戏内退出没有关闭真实窗口（运行复现，移植回归）

- `C/MenuGroup_Main.cs:115–119` 请求GameExit，`C/Game_Main.cs:410–413` 调用((Form)Form_Main).Close()；实际落到 `P/Compat/WinFormsStubs.cs:266` 的空方法。
- 原 `O/Game_Main.cs:388–391` 调用真实窗体Close。显式Alt+F4分支也走当前同一空Close；操作系统可能另行处理组合键，不代表游戏分支有效。
- 实测发布版输入600:Up、700:Z（主菜单选Quit），到frame1000仍继续截图、窗口未退出，最终由24秒timeout终止；日志 `/tmp/thsss-audit-quit.log`。
- 修复方向：将统一退出请求接到真实IWindow.Close及有序保存/释放。

### 6. 失焦不自动暂停，存在粘键风险（源码确认；粘键需桌面复测）

- 原 `O/Game_Main.cs:101–103,580–595,467–469` 的Deactivate/Activated将失焦转成暂停。
- 当前 `C/Game_Main.cs:110–112,609–623,496–498` 保留处理器，但Program没有转发真实FocusChanged；KeyboardInput没有失焦清空/重新同步。
- 影响：切出游戏后战斗继续；若后端不补KeyUp，后台释放的方向/射击可能留在按下状态。未实际操作焦点，不能把粘键风险写成已复现。
- 修复方向：转发焦点生命周期，并明确清理按键状态，验证按住键→切出→释放→切回。

### 7. 快照遍历不是原列表行为，可能改变弹幕/事件帧序（运行复现差异，原CLR后果待对照）

- `P/Compat/ListCompat.cs:10–17` 声称快照等价，并称删除项会跳过；实际`:24,33`无条件遍历ToArray，**注释与实现本身就不一致**。
- 当前 `C/BaseObject_CS.cs:125–138` 和各CS/Event管线大量用Snapshot；原对应位置用List.ForEach。`C/Execution.cs:248–251` 会在Update期间删除自己。
- 实测：列表[1,2]回调1删除2，快照仍执行2；追加3则当轮不执行3。真实BaseObject_CS中第1执行项自删后，第2项本帧仍递减ctime。若原CLR是活索引遍历，第2项会因索引左移跳过，与快照不同。
- **限制**：没有在原Windows CLR2执行此样本，不能仅凭历史注释认定原行为；现代微软文档也明确不支持回调中修改列表。须提取/复核原运行时语义再重建对应顺序。
- 影响：可能影响子弹属性变化时刻、事件同帧推进和旧Replay稳定性，而不仅是性能。它不是“崩溃没了就等价”。
- 修复方向：先以原运行时/明确顺序样本建立基线，再集中实现真正需要的遍历策略，不直接再次全仓机械替换。

## 仍需处理

### 8. 对话有字但测宽为0，自动换行失效（运行复现，移植回归）

- `P/Compat/MissingTypes.cs:195–213` 的D3D Font仍是空桩，MeasureString恒返回Size.Empty、DrawString也为空。
- 原/当前 `StoryBox.cs:102,115,135,152` 依赖MeasureString.Width判断换行；当前非空中文/英文句子测宽=0，阈值永远不触发。
- 影响：长对话越过框边或被固定纹理截断；主菜单 `C/GameState_MainMenu.cs:137` 的直接DXFont版本文字也没有输出。
- 字体已用ImageSharp绘出不代表字体度量已实现；Linux的Noto替代楷体/Arial也没有原字体逐字度量金标准。
- 修复方向：真实实现Font度量/直接绘制，使用同一个字体选择和尺寸系统；测试超过框宽的真实剧情句子。

### 9. 手柄设备永远枚举为空（源码确认，移植回归）

- `P/Compat/DirectInputCompat.cs:246–249` GetDevices恒返回空；Program仅连接键盘。
- 原/当前JoystickCapture仍按原轴阈值与配置按钮读取，原 `O/JoystickCapture.cs:18–63` 有真实DirectInput设备。
- 影响：轴、慢速/射击/Bomb/跳过/暂停全无效。未做硬件测试。
- 修复方向：接入Silk.NET手柄，测试热插拔、配置按钮索引与键盘混合。

### 10. 窗口、全屏与光标设置只改stub字段（源码确认，移植回归）

- `C/Game_Main.cs:153–159,560–605` 写强转Form.ClientSize；该属性是自动属性，Device.Reset为空，Windowed参数没有映射到真实窗口。
- 真实Program固定创建640×480；原 `O/Game_Main.cs:144–150,185–221,531–577` 直接影响真实窗体/设备。
- MouseCtrl写CursorState但没有窗口层消费者；FullWindow/WindowSize和F8尺寸循环不生效，光标隐藏同样不生效。
- 修复方向：把所有窗口状态收敛到真实IWindow，不能仅验证字段变化。

### 11. 游戏内P截图格式、方向不对（运行复现，移植回归）

- 原 `O/Game_Main.cs:725–732` 请求JPEG；当前仍请求.jpg，但 `P/Compat/Surface.cs:102–116` 恒写PNG并未翻转ReadPixels行序。
- GPU双色样本实测：.jpg开头是PNG签名；屏幕上蓝下红，保存图片上红下蓝。
- 环境变量THSSS_SCREENSHOT走的是Program另一条已翻行PNG路径，之前那些截图通过不能证明游戏内P路径正确。
- 修复方向：按请求编码器处理；同一截图实现统一GL行序与read FBO来源。

### 12. 固定VSync/60回调与PresentInterval形成双重节拍（源码确认配置差异，速率后果未量化）

- 原 `O/Game_Main.cs:195–219` 按VerticalSync和presentInterval选择呈现间隔，主循环`:471–484` 每次推进presentInterval次，再`:510–526` 按16.62ms×interval限速。
- 当前逻辑基本保留 `C/Game_Main.cs:500–513,539–550`，但真实Program强制VSync=true且FramesPerSecond=60，不读取VerticalSync/PresentInterval；Device.Present空，交换由GLFW负责。
- 影响：VerticalSync=0实际仍开同步；低刷新率显示器或PresentInterval=2/3可能有不同卡顿、输入延迟、节拍。**尚未做原版/多刷新率实测速率，不声称必然慢一半或快两倍。**
- 修复方向：统一一个节拍所有者，按配置落实呈现与逻辑帧比，采集真实时间/逻辑帧/输入帧数据再调整。

### 13. 旧中文Score存档解密后乱码（运行复现兼容情形）

- 原/当前 `Encryption.cs:15,49` 使用Encoding.Default；Windows .NET Framework取系统ANSI代码页，现代.NET固定UTF-8。注册CodePages不改变Default。
- 合成原格式CP936明文“博丽灵梦”，用相同DES Key/IV加密，端口Decrypt结果为替代字符而非原名；DES/hex容器本身并未改版。
- 影响：来自CP936 Windows原版的中文玩家名/分数名字会损坏并可能在下次保存后固定下来。没有冒充已读取用户真实存档。
- 修复方向：明确原版编码检测与迁移策略，保留原文件备份，不能无条件重新编码已有数据。

### 14. 损坏Score加载失败后会被覆盖（运行复现，原作既存）

- 原 `O/PlayerData.cs:57–73,202–243` 同样有此逻辑；当前 `C/PlayerData.cs:220–237` 逐条修改历史、失败后不回滚，构造catch只调用空MessageBox，主菜单 `C/GameState_MainMenu.cs:48–51` 自动SaveData先截断文件。
- 临时目录放入损坏fixture→new PlayerData→同主菜单SaveData，原字节被默认状态覆盖。没有触碰仓库Score.dat。
- 影响：恢复机会丢失；这是继承问题但在移植中仍需处理。
- 修复方向：先完整解析到临时状态，失败不覆盖、保留备份，成功后原子写入。

### 15. Replay跨区域设置读取错误，截断输入不报错（运行复现，原作既存可移植性缺陷）

- `C/Replay.cs:58–65,145–160` 使用CurrentCulture读写位置/倍率，原代码相同。相同192.5在en-US读192.5，de-DE读1925。
- `C/Replay.cs:88–91` 不检查ReadByte=-1；空文件返回-257，不是约定末尾4080；原也相同。
- 影响：跨locale回放错位、截断文件被当作键输入继续跑。Replay的二进制键结构/尾标记/metadata次序保持原样，并不代表所有回放正确。
- 修复方向：明确兼容旧locale、固定未来文件文化；EOF与元数据错误显式拒绝，勿悄悄生成空ReplayInfo。

### 16. WAV文件名构造没有关闭输入FileStream（源码确认，移植回归）

- `P/Compat/MultimediaCompat.cs:84–88,125,136` 文件名构造打开FileStream后只复制数据，Dispose仅关闭内存流。
- 活动 `C/XAudio2_PlayerV2.cs:59` 在BGM切换时走此构造；原`:171–178` 释放原WaveStream。
- 影响：反复切曲的文件句柄由GC最终化回收，不随播放器及时释放。
- 修复方向：构造时using打开输入，或明确流所有权；增加重复切曲句柄测试。

## 其他确认但较低优先级的差异

17. **错误提示被吞**：WinFormsStubs MessageBox.Show为空，配置非法数字/分数载入失败时用户看不到原因；某些配置字段已部分应用后仍继续运行。原版会显示对话框。建议统一可观察错误通道。
18. **取消原版单实例限制**：O/Program.cs:16–29命名Mutex防双开；当前入口没有。除窗口重复外，多个实例会并发写Score.dat与Replay/AutoSave.rpy。需决定是否保持单实例并处理跨平台锁，不能默默更改。
19. **停止加载线程只等待最多5秒**：ThreadCompat.Stop超时仅打印日志，线程仍可继续，不等同原Thread.Abort；未复现慢盘/挂起场景，不将其归为已发生数据竞争。

## 未算成活动缺陷的边界

- Matrix.Invert仍返回Identity、部分Graphics形状API/高级D3D状态/滤波字段为空；此次未证明活动游戏调用导致具体偏差，所以不靠“发现空方法”凑结论。
- xWMA、音频filter、实时SamplesPlayed/队列状态存在实现差异，但当前普通BGM使用WAV，尚未证明对应活动调用的影响。
- MyRandom.cs、Execution.cs、KeyClass.cs、BaseBullet_Touhou.cs与反编译版本逐字节相同；不能由此推导包含它们的整体帧序或旧Replay完全一致。
- 之前八套shader/lighting测试验证数学和局部像素，不覆盖真实窗口生命周期、音频循环/暂停、输入模式与旧存档；本报告的发现不与那些局部测试通过相矛盾。

## 可重复诊断

从 `src/`：

```sh
dotnet run --project ../tests/BehaviorParityAudit/BehaviorParityAudit.csproj -- --gpu
```

程序只创建并删除独立临时fixture，音频使用静音WAV；不启动正式游戏、不写用户Score/Replay。输出 **CONFIRMED** 表示复现当前缺陷，不代表正确行为。此次输出13条异常观察，其中3条属于同一快照问题、2条属于截图问题，因此不是13个互不相关缺陷。日志 `/tmp/thsss-behavior-parity-audit.log`。

真实Quit入口测试使用已有独立发布目录 `/tmp/thsss-lighting-fixed`，不是用户游戏目录。实际记录 `/tmp/thsss-audit-quit.log`；这些绝对临时路径不是永久测试资产。

## 建议处理顺序与验收

1. 先保护Score/Replay，恢复真实窗口退出/失焦与UseDirectInput=0输入：避免用户操作无反馈或记录丢失。
2. 恢复音频循环点、暂停游标及音量/声像独立性，使用静音/采样游标或loopback量化，不用voice计数替代播放语义。
3. 建立原CLR列表变更基线，解决事件帧序和旧Replay一致性；同时补对话测宽与长文本换行。
4. 完成窗口模式/手柄/截图与节拍配置，再由真人验证手感、Boss/符卡/掉落/继续流程。

本轮没有改变这些行为。报告不是“剩下只有19项”的穷尽证明：未覆盖每关每符卡、所有机器、GPU/刷新率、全套用户旧Replay；当前结论已经明确区分代码确认、运行复现与未验证。
