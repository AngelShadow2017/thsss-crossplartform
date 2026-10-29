# 音频后端与平台构建

## 选择

| 系统 | 默认 | 依赖 |
|---|---|---|
| Linux | OpenAL | OpenAL Soft等支持AL_EXT_FLOAT32的实现 |
| Windows 10/11 | XAudio2 2.9 | 系统xaudio2_9.dll及COM MTA |
| macOS | CoreAudio | 系统AudioToolbox AudioQueue |

`THSSS_AUDIO_BACKEND=auto|openal|xaudio2|coreaudio`，默认auto。显式请求不支持平台的实现、未知名称或设备初始化失败均明确报错，不静默换后端。Windows/macOS可显式选OpenAL，但需提供该库。`THSSS_DIAG_AUDIO=1`输出实际选择。

## 编译边界

`THSSS_WINDOWS`/`THSSS_MACOS`根据目标RID定义：显式RuntimeIdentifier优先，未指定时使用NETCoreSdkRuntimeIdentifier。跨OS发行必须指定RID；普通net10.0加win-x64不会自动定义SDK的WINDOWS常量。

Windows产物包含XAudio2、ole32与Win32窗口导入；macOS产物包含CoreAudio和Cocoa导入。Linux不包含这些原生音频类型。OpenAL仍是可选后端，不能把类型剔除当作所有非默认依赖均已删除。

## 播放契约

公共SlimDX.XAudio2兼容层保留循环、样本游标、逻辑队列、音量和输出矩阵。底层接收已混音的float32立体声块；不为每种平台重写循环点或音量曲线。

XAudio2 vtable：SetVolume12、DestroyVoice18、Start/Stop19/20、SubmitSourceBuffer21、FlushSourceBuffers22、GetState25。GetState返回void并接flags；Stop接flags和operationSet。PCM持有至已消费且公共层退休；UnqueueProcessed只退休一个块。Clear销毁旧voice后重建，Dispose不重建。没有Wine游标假进度补偿。

AudioQueue输出采用交错float32 PCM。回调只表示系统已取得数据，退休还必须满足设备SampleTime已越过该块；暂停冻结游标，Clear重设时间起点，Dispose同步等待回调。原生ABI与验证说明见[MACOS_2026.md](MACOS_2026.md)。

重复设置相同输出矩阵不重置队列；没有待播数据时只更新矩阵。CoreAudio Clear 仅调用同步 AudioQueueStop，使用其自带的 Reset 语义，避免重复重置；已经暂停或尚未启动的队列再次 Pause 时不进入原生 AudioQueuePause。`THSSS_DIAG_INPUT=1` 可记录 Pause/Clear/Start 耗时，供菜单移动卡顿的 Mac 实机复核；本轮仅静态修改，未运行音频或游戏验证。

## 验证

```sh
sh tools/run-audio-regressions.sh
sh tools/run-mac-regressions.sh
```

OpenAL套件保留162项loopback像素外音频数值断言。后端公共契约覆盖有限/无限循环、暂停恢复、EOS、队列顺序、12voice并发、矩阵/音量独立、错误传播与释放。平台类型存在断言防止错误#if让后端消失。

Windows测试使用win-x64自包含EXE和隔离Wine前缀：

```sh
dotnet publish tests/AudioBackendCompatibility/AudioBackendCompatibility.csproj \
  -r win-x64 --self-contained true -o /tmp/thsss-audio-win-test
mkdir -p /tmp/thsss-audio-wine
WINEPREFIX=/tmp/thsss-audio-wine \
THSSS_AUDIO_WINE_EXE=/tmp/thsss-audio-win-test/AudioBackendCompatibility.exe \
sh tools/run-audio-regressions.sh
```

音频DejaGnu默认3项，提供WindowsEXE后4项。CoreAudio的Linux原生API软件驱动是单独的mac验收入口；不能将它称为Apple系统运行测试。Wine不是微软本机驱动；设备热插拔、声卡听感及完整游戏图形/存档不由音频契约通过证明。
