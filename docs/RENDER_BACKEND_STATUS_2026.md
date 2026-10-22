# 渲染后端当前状态（2026-10-23）

当前可选择 OpenGL 或 Vulkan：

```sh
sh tools/run-game.sh
THSSS_RENDERER=vk sh tools/run-game.sh
```

Linux 原生 Wayland 和 X11 路径均已运行验证；启动脚本不再修改 WAYLAND_DISPLAY。Vulkan 窗口必须使用 `GraphicsAPI.None`，不能在同一原生窗口先创建 OpenGL 上下文再让 Vulkan 接管。此前“GLFW 的 Wayland surface 无法供 Vulkan 使用、必须重写窗口层”的判断没有依据，已撤回。

## 实际修复

| 问题 | 修复及证据 |
|---|---|
| 主菜单黑屏、启动颜色异常 | `Device.SetRenderState(int)` 原有 `if (GL == null) return`，使 Vulkan 忽略混合/光照等状态。逐次绘制读回显示菜单背景先正常、透明层在混合关闭时覆盖背景。移除早退，混合及深度写入经后端转发；透明精灵保留红色背景测试通过。不是交换链索引必须轮转，也不是必然的纹理跨距错误。 |
| 渲染目标图像用途不正确 | `Texture` 的 RenderTarget 用途现在走 `CreateRenderTexture`，使 Vulkan 图像具有 ColorAttachment 用途。 |
| Vulkan 主菜单约8.4 FPS | 原实现每个 Draw 都提交并等待 GPU。新增 BeginFrame/EndFrame，批量录制、fence 完成后回收描述符和上传 arena。每次 draw 独立快照顶点与 uniform，不在未执行完时覆盖同一份数据。400 次实际 Sprite 绘制只提交一次；不同 transform/tint 的条带测试通过。 |
| 静态字体每帧上传/销毁导致额外 flush | 每个 DX Font 只缓存最近一份字形纹理；缓存键包含文字、尺寸、排版标志、颜色，不包含屏幕位置。变更或 Dispose 释放旧纹理，不无限积累。实际菜单提交次数从4降到1。 |
| MainProcess 提前返回留下未完成帧 | Program 用 finally 调用 EndFrame，支持跳过 Present；空帧、重复 EndFrame 和下一帧继续绘制测试通过。 |
| acquire 失败却等待未提交的 fence | 仅成功/次优获取后等待 acquire fence；获取超时、NotReady、OutOfDate 不等待未关联的 fence。成功获取必须等 fence 完成后再销毁。 |
| Vulkan GPU 资源销毁时存在未提交引用 | 纹理、缓冲、帧缓冲、着色器销毁前 flush 未完成录制；回收只发生在 fence 完成后。 |

## 真实运行证据

2026-10-23，本机 Intel Vulkan 设备；所有运行使用临时配置/Score、独立 Replay/ScreenShot，资源只读链接，不改玩家数据。

- 主菜单经过原黑屏点后，第480/600/840帧中心均保持64/64非黑像素，正常Alt+F4退出。
- 实际桌面使用 grim 截取游戏窗口区域，不是仅对离屏纹理清屏后读回。Wayland日志为 `platform=Wayland ... resolved=yes`；X11日志为 `platform=X11 ... resolved=yes`。两种 Vulkan 菜单截图中心均为 `(166,188,243)`；同场景 GL 中心相同。VK/GL整幅每通道平均绝对差约0.46/0.73/0.82（8位颜色），动画与随机效果非逐帧锁定，不能据此宣称所有场景逐像素等同。
- 修复前菜单实测8.38–8.39 FPS、119ms/帧；批量提交并缓存静态文字后，第480–1080帧约59.9–60.0 FPS、16.6ms/帧，380–430次绘制/帧，1次提交/帧。首次启动、资源加载和冷编译阶段可能低于60；不是全平台性能保证。
- Vulkan实际进入关卡、暂停/恢复并正常退出，关卡稳定阶段约60 FPS；第1500帧截图272790/307200非黑像素。状态脚本与像素证据不是所有关卡人工体验验收。

本次会话的可检查证据（/tmp会被系统清理，不作为永久资产）：

| 内容 | 路径 |
|---|---|
| 原生Wayland实际窗口 | `/tmp/vk-wayland-menu.png`、`/tmp/vk-wayland-verified.log` |
| X11实际窗口 | `/tmp/vk-x11-menu.png`、`/tmp/vk-x11-verified.log` |
| GL对照窗口 | `/tmp/gl-reference-menu.png`、`/tmp/gl-reference-verified.log` |
| 批量提交前/后性能 | `/tmp/vk-fps-before.log`、`/tmp/vk-steady-fps.log` |
| 实际关卡/暂停恢复 | `/tmp/vk-batched-stage.log`、`/tmp/vk-batched-stage.png` |

当前文本模型不能直接查看图像内容；上面的颜色与差异是程序读取PNG的结果，不冒充人工目视审查。先前声称“看到截图中的横带/颜色”的回复不应作为证据。

## 抽象与扩展边界

`IRenderBackend` 包含资源创建、uniform块、绘制、深度/混合/采样状态、frame生命周期与呈现；GL/Vulkan各自拥有资源和同步。未来后端需实现这套契约，并在 BackendKind 与创建/窗口入口注册；不是改游戏弹幕与关卡逻辑。

兼容层仍保留部分 GL 专用优化及诊断分支（例如 Effect 的旧 GL 程序路径），不能声称所有文件已完全不含GL调用。Vulkan使用后端路径而不创建GL上下文，GL路径及已有shader版本未降级。

Vulkan目前使用1.3特性，运行时着色器转换需要可执行的 `glslangValidator`（`SpirVCompiler`）。构建期预编译/无外部工具发布曾讨论但尚未实现，不能宣称发布包已自带SPIR-V或编译器。Windows/macOS、MoltenVK及其他硬件驱动未在本机执行验收。

## 回归入口

```sh
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-shader-regressions.sh
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-behavior-regressions.sh
```

渲染9套（含VulkanBackend），行为6套（List/Audio/Save/TextScreenshot/WindowInput/Config）。此前按 PASS 行数报告“7套行为回归”不准确，以脚本实际suite数为准。Vulkan测试涵盖实际Sprite、菜单图集、透明叠加、批量状态快照、跳帧、帧缓冲往返和深度附件；GL测试继续覆盖FX与110项光照像素断言。不把“运行全部suite”解释成全部suite在两种API上均跑过。

诊断帧率：`THSSS_DIAG_FPS=1 THSSS_RENDERER=vk sh tools/run-game.sh`，输出实际FPS、MainProcess耗时、每帧提交/绘制次数。诊断读回会额外同步并降低帧率，性能测试不要开启THSSS_DIAG_PRESENT或THSSS_DIAG_VK_PIXELS。

## 最终验收补充（2026-10-23）

最终源码重新跑9套渲染、6套行为，均退出0，`git diff --check`通过。日志：`/tmp/vk-20261023-acceptance-shaders.log`、`/tmp/vk-20261023-acceptance-behavior.log`。原生桌面最后一轮Wayland/X11均退出0，第720帧分别60.04/60.14 FPS、1次提交、425/418次draw；日志 `/tmp/vk-final-wayland.log`、`/tmp/vk-final-x11.log`。独立只读复核帧finally、acquire fence、单项字形缓存未发现高置信剩余问题；不是全后端的形式化证明。AFT C#语言服务初始化仍失败，不能作为干净诊断证据；实际dotnet构建与执行回归是验收依据。
