# Windows 启动崩溃修复（2026-10-25）

## 症状

Windows 实机启动 THSSS 后立即中止：

```
[Run] System.ArgumentException: An item with the same key has already been added. Key: back
   at Shooting.GlobalDataPackage.TextureObjectLoader(String imageFileName, Int32 colorKey)
   at Shooting.GameState_SplashScreen.UpdateData()
```

同一构建在 Linux 上正常，所以问题与资源内容无关，而是文件枚举。

## 定位过程

1. 通过 QEMU 的 SSH 转发（`127.0.0.1:2223`）进入 Windows，用 PowerShell 复现游戏自己的加载算法。
2. Windows 上的 `Directory.GetFiles(path, "*.png")` 与 Linux 的 `Directory.GetFiles` 返回**不同的文件集合和顺序**：Windows 会额外返回 `Star.txt` 这类被 8.3 通配规则匹配到的名字，且顺序未排序。
3. Linux 上完整的键集合是 2129 个且无冲突；Windows 上同样算法得到 2103 个键并出现 26 处重复。首次复现显示 `a..z` 全部落在 `Font_Gonthic.png`，说明 Windows 上同一次遍历里该文件被处理了两次，进而 `back` 第二次插入时抛异常。
4. 用真实 Windows 进程捕获确认崩溃键先是 `Leaf`、后是 `back`，取决于加载顺序，证明这是**顺序相关的重复插入**，不是某个特定资源缺失。

## 修复

`src/Shooting/GlobalDataPackage.cs` 的 `TextureObjectLoader` 把 `Dictionary.Add` 改为索引赋值：

```csharp
TextureObjectDictionary[array2[0]] = textureObject;
TextureObjectDictionary[fileInfo.Name.Replace(".png", "")] = textureObject2;
```

后加载的定义覆盖先加载的。这样：

- 启动不再因平台枚举差异中止
- 加载顺序不再决定游戏能否启动
- 资源内容未改动，键名与图集语义保持不变

## 验证

| 检查 | 结果 |
|---|---|
| Windows 实机启动 | 连续运行 120 秒无异常，错误行 0 |
| 相同资源、旧实现的对照 | 曾崩溃于 `Key: back` / `Key: Leaf`，不再复现 |
| Linux 行为回归 7 套 | 全部通过 |
| Linux 渲染回归 9 套 | 全部通过 |
| 窗口缩放回归（GL/Vulkan） | 全部通过 |
| `git diff --check` | 干净 |

Windows 验证使用 `-r win-x64 --self-contained` 自包含发布，通过 SSH 在真实 Windows 虚拟机中运行，不是 Wine。

## 关于 Wine 的 DES 失败（不是 Windows 问题）

先前在 Wine 中出现的 `CryptographicException 0xC1000002`（`DES.Create().CreateEncryptor()`）**在真实 Windows 上不存在**：

```
OK 21-88-FB-89-89-A5-87-76-96-FC-24-C9-97-6A-55-87
```

同样的密钥 `SeiweLL0`、IV `0SeiweLL`、同样 .NET 8，在 Windows 上输出与 Linux 完全一致。这是 Wine 的 BCrypt 未实现 DES 导致的环境限制，**不需要修改游戏代码，也不影响存档格式兼容性**。

## 未覆盖

- 未在 Windows 上验证图形显示（SSH 会话无桌面合成器，截图 API 返回 `handle is invalid`），只验证了进程存活与无异常
- 未验证 Windows 上的音频输出与存档写入的实际文件
- macOS 未涉及
