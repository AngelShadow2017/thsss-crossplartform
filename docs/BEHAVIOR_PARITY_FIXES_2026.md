# 原作行为差异修复与验收（2026-10-20）

修复前证据保留于 [`BEHAVIOR_PARITY_AUDIT_2026.md`](BEHAVIOR_PARITY_AUDIT_2026.md)。本文件是当前实现与验收记录；旧诊断里的 `CONFIRMED` 表示当时复现缺陷，不能当作当前状态或验收通过。

本轮没有改动关卡难度、子弹公式、碰撞半径或随机数算法，也没有覆盖用户原始 `Score.dat`、录像或配置。所有坏档、旧编码及完整游戏集成测试都使用临时独立目录。既有工作区改动不回滚、不顺手提交。

## 逐项修复矩阵

编号与原审计一致。

| 项 | 当前修复 | 验收证据 |
|---|---|---|
| 1 音频区间/循环 | `AudioPlayback.cs` 将PlayBegin/Length与LoopBegin/Length/Count映射为样本时间线；支持有限/无限循环，最多四个原生缓冲区、定时补充，不展开无穷PCM | 真实AL游标及OpenAL Soft loopback，非零起点、精确有限循环2200帧结束、无限循环有界队列 |
| 2 暂停恢复 | SourceVoice.Stop暂停而非SourceStop重置；Start续播，Flush单独清空 | 暂停样本游标3292，恢复5409，不归零；循环段也覆盖 |
| 3 声像/音量 | PCM输出矩阵与独立AL Gain分开；mono左右、stereo交换/交叉/负系数；实时改变矩阵保留已消费位置 | loopback验证左右、静音、独立gain及动态更新，不再把0.2改成1 |
| 4 两输入模式 | 真实窗口事件统一进入WindowInputBridge；UseDirectInput=0转发原Control事件，=1创建快照；两路径使用配置键名 | 两模式完整游戏进程、自定义WASD/J/K与Enter退出；Enter/KeypadEnter别名按转换后键聚合 |
| 5 退出 | Form.Close/事件改为虚方法并桥接唯一真实IWindow；执行保存、关闭录像与资源释放 | 两模式菜单Quit及Alt+F4三个进程退出码均0，不依赖timeout强杀；脚本注入关闭后不再调用已释放MainProcess |
| 6 焦点 | 真实FocusChanged触发原Activated/Deactivate；失焦释放事件与快照、忽略后台新按下 | 假后端验证暂停请求、持键切出/释放/切回无粘键；实际桌面快捷键体验仍需真人验证 |
| 7 列表顺序 | `ListCompat.LegacyItems`实时索引替代错误Snapshot；原ForEach改写的事件、碰撞、背景、粒子、剧情等调用点统一使用；迁移脚本同步 | 五种增删/赋值，真实Execution自删跳过左移项、下一帧推进，真实背景移除/同轮生成；无列表版本异常 |
| 8 字体 | DX Font.MeasureString/DrawText/DrawString使用与Graphics绘字相同的字体解析和单位；临时纹理释放前刷新批次 | 中文测宽1093×18，限宽160×112；真实StoryBox完整文字5行不丢字；直接绘字504个可见像素 |
| 9 手柄 | Silk设备连接/断开、轴与按钮桥接DirectInput兼容枚举；键盘/手柄混合；Linux按capabilities过滤被GLFW误识别的触摸板/鼠标 | 设备热插拔假后端、原轴阈值和配置按钮测试；当前机器GLFW原始枚举16设备，过滤后双模式菜单不再被虚假方向驱动；无实体手柄操作验收 |
| 10 窗口/光标 | Form.ClientSize、初始配置、F8窗口/全屏、CursorState转发真实窗口；不再只改stub | 假后端与真实GLFW；完整游戏四次F8切换后正常退出 |
| 11 P截图 | 按请求JPEG/PNG/BMP/TGA/PPM编码；独立选择源Surface、恢复读FBO/readbuffer/PBO/pack状态；GL行序正确；按物理letterbox读取再回原逻辑尺寸 | 167断言之外另测缩放backbuffer与真实swap后请求：先交换绿帧→P仅排队→画蓝帧→保存蓝帧一次；游戏真实P文件签名FFD8FF |
| 12 帧节拍 | 真实窗口负责等待，保留原MainProcess逻辑帧比与fixed-FPS滞后上限；按配置设置VSync；windowed interval>1仍用软件限速，fullscreen可用swap interval2/3 | 实际GLFW30Hz为13帧/约407ms；假后端验证windowed/fullscreen与interval策略，不宣称多刷新率驱动已全部实测 |
| 13 旧中文存档 | DES Key/IV/hex容器不变；严格UTF8解码失败后尝试CP936，保留载入编码；编码失败发生在写入前 | CP936/UTF8中文玩家名及字节往返；不使用replacement fallback |
| 14 坏档保护 | 全部记录解析到临时PlayerData再提交；失败设置LoadError、保留状态并阻止菜单自动Save；正常写入Flush并原子替换、留.bak | 坏档→载入失败→自动保存原字节不变；中途坏记录回滚；正常备份、写/编码失败不丢原文件 |
| 15 Replay | 固定文化写数字；按旧ToString语法接受单逗号小数，包括195,195；拒绝混合分隔/非有限值；验证关卡标识及初始/最终记录；EOF不再生成-257 | 多locale、旧中文标题、合法4080、空/半键、完整头但缺记录；保留结局LastStage=All。播放意外EOF关闭流、清键、报错并回标题，停止模拟 |
| 16 WAV文件 | 文件名构造在复制后立即释放输入；Stream重载明确由调用方持有 | 独占打开、正常/失败路径释放断言 |
| 17 可见错误 | MessageBox适配输出标题/错误到stderr；配置解析失败明确停止而非保存半套参数 | 隔离错误及坏档测试可观测，不把空弹框当成功 |
| 18 单实例 | 数据根目录文件锁，防同一存档目录多实例并发写 | 独立进程争锁、释放重开；不同数据根可分别运行 |
| 19 加载线程 | Stop等待完成，超时明确抛TimeoutException；关闭超时先取消Close、不保存/释放仍被worker使用的共享资源 | 慢worker超时及完成worker测试；独立审查核对取消分支位于资源释放前；未伪称可以跨平台强杀线程 |

## 回归入口和运行记录

从项目根运行（无桌面显示时用 `xvfb-run -a` 包裹）：

```sh
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-behavior-regressions.sh
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-shader-regressions.sh
```

`ALSOFT_DRIVERS=null`避免测试实时WAV占用实体输出；音频suite另外使用真实OpenAL Soft loopback核对输出样本，不用“声音启动次数”代替正确性。

行为入口运行五套严格回归：

| suite | 已验证 |
|---|---|
| ListCompatibility | 8个索引/事件断言，外加后台线程和实际背景顺序回归 |
| AudioCompatibility | 162断言；实时AL+确定性loopback |
| SaveCompatibility | 84断言；CP936/UTF8、坏档/备份、Replay数字/记录/EOF/正常结局 |
| TextScreenshotCompatibility | 167严格断言；另有缩放backbuffer及交换后P时序回归 |
| WindowInputCompatibility | 109断言；含3个实际游戏进程、GLFW窗口与软件节拍 |

八套shader/lighting回归也全通过，含110个光照像素断言；两套入口不能彼此替代。实际输出记录：

- `/tmp/thsss-behavior-all-fixed.log`：行为suite全退出0；测试坏档产生的 `[Score Error]` 是预期负例，不是验收失败。
- `/tmp/thsss-behavior-final-shaders.log`：八套渲染suite退出0。
- `/tmp/thsss-behavior-publish.log`：Release publish成功。
- `/tmp/thsss-behavior-integration-final.log`：不设THSSS_DATA_ROOT，从独立发布目录进入关卡、暂停、恢复、P截图、Alt+F4有序退出0。生成非空实际AutoSave.rpy（最终记录1506字节）、Score及.bak，真实JPEG文件以FFD8FF开头。因自动测试使用隐藏Xvfb窗口，不将其冒充真人焦点/听感验收。

发布启动同时创建空的BPic目录，保留原包允许无额外弹形遮罩的情况；没有文件的资源目录不会由publish自动复制，此前会出现与实际内容无关的LoadBPic警告。

## 独立评审闭环

- 音频复核未发现高置信新增缺陷。
- 窗口评审指出Enter别名聚合问题：已修，并新增主Enter+小键盘Enter同时按住测试。
- 存档评审指出三位逗号小数及缺少关卡记录：已修；另外保留真正结局 `LastStage=All`，避免格式验证误拒正常结束录像。
- 截图评审指出按P发生在本帧绘制前：改为只排队，Program在绘制后/交换前捕获；用真实SwapBuffers再取新帧测试。
- 最后只读复核通过上述四条关联修改，未发现高置信剩余缺陷；运行结果仍以实际测试日志为准。

## 原列表顺序依据

原版调用List.ForEach；pre-.NET-4.5实现每轮读取实时_size并执行_items[index]。删除当前元素后索引照常递增，跳过左移项；尾部追加同轮访问，不等价于ToArray。2026-10-20核对的历史实现参考：

- https://github.com/microsoft/referencesource/blob/master/mscorlib/system/collections/generic/list.cs
- https://gist.github.com/3288283 （mscorlib4.0反编译记录，直接索引循环）
- https://stackoverflow.com/questions/69601488/how-does-the-listt-foreachactiont-action-method-actually-work （包含4.5兼容开关）

测试用明确实时索引参考与真实游戏EventCtrl/Execution.Update比较，不用快照与自身比较。参考资料和验证日期保持分离，不伪造历史API新版年份。

## 仍需如实保留的边界

1. 原Windows CLR2+SlimDX环境仍不可执行，没有做全关/旧Replay逐帧金标准。此轮关闭的是已审计的具体缺陷，不是证明所有未知行为完全一致。
2. 未标编码、同时合法的UTF8/CP936字节无法无歧义判源；当前UTF8优先，但保留载入编码和备份，不以替代字符吞掉非法字节。不能保证任意第三方ACP编码都可识别。
3. 非有限或混合数字分隔的Replay明确拒绝；原写入不会生成分组符，单逗号包括三位小数按小数处理，不再错误拒绝。未支持开发专用Stage_test回放菜单。
4. 无实体手柄、不同显示器刷新率/驱动VSync和实体扬声器听感测试；自动设备/节拍/样本断言不冒充人工验收。
5. 仍有旧ini-parser的NU1701兼容警告与GTK主题警告；实际编译/测试成功。AFT C#语言服务初始化失败不能当作代码诊断通过。
