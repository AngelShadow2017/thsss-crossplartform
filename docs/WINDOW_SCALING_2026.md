# 4:3 连续缩放与 DPI（2026-10-24）

## 使用

运行 `sh tools/run-game.sh` 或 `THSSS_RENDERER=vk sh tools/run-game.sh` 后拖动窗口边缘。客户区请求保持4:3，可用800×600、1000×750等非整数倍尺寸，不限于原F8预设。游戏坐标、碰撞、HUD、对话换行及关卡离屏纹理仍为640×480；字体随整个画面等比缩放，不改变字号，也不做高清文字重新排版。

- GLFW原生 `glfwSetWindowAspectRatio(4,3)` 约束客户区，不包含标题栏。若平台未执行原生提示，在渲染帧边界做尺寸校正，不在Resize回调里递归设置。
- 全屏服从显示器分辨率，内容居中保持4:3，余下区域黑边。
- 窗口坐标与Framebuffer像素尺寸分开。每帧比较真实FramebufferSize，尺寸改变才通知后端；即使窗口坐标未变，DPI变化导致的像素尺寸改变仍会更新。
- 最小化或零像素尺寸跳过渲染及呈现，恢复后使用当前像素尺寸。GL更新输出尺寸，Vulkan在帧开始前等待旧资源安全完成并重建输出与交换链；不重建游戏资源或修改玩家存档。
- 游戏内P截图仍为640×480，统一从实际内容区域读回；Sprite、3D视口、截图与Vulkan合成共用 `PresentationGeometry.Fit`。
- Windows apphost新增PerMonitorV2/PerMonitor DPI清单；该部分仅构建验证，未在Windows实机运行。

## 关键修复

输出变大时，旧640×480深度面不再被当作匹配所有默认目标；按实际目标尺寸匹配，不匹配使用目标自身深度。GL默认Framebuffer0不能修改附件，`AttachDepth`在该情况下保留窗口深度，避免GL_INVALID_OPERATION。

Vulkan合并OutOfDate处理，按surface能力和真实extent判断是否重建，防Suboptimal每帧重建；零surface extent推迟到后续帧。呈现使用线性blit；已经包含黑边的物理backbuffer不重复缩放，逻辑合成纹理按4:3适配。

## 验证

```sh
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-resize-regressions.sh
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-shader-regressions.sh
ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-behavior-regressions.sh
```

有桌面会话时，resize入口可去掉xvfb-run，使用原生Wayland验证。脚本创建临时配置、Score及Replay/ScreenShot，资产只读链接，不覆盖game/玩家文件。

- 两后端实际切换800×600、1000×750、1280×960、640×480，并在1000×650非4:3下验证整幅画面等比缩放、四角完整、留黑边（Wayland由合成器决定外框，X11主动校正客户区）。
- 最小化恢复、全屏往返后仍能绘制；尺寸稳定后Vulkan交换链generation不再增长。
- 非对称四角图案在每个尺寸和1920×1080全屏下验证方向、边界、黑边；GL查询无错误。测试纹理使用64×64色块，避免2×2双线性采样造成的颜色混合误判。
- 几何覆盖竖屏、1920×1080、1001×750舍入、零尺寸；模拟窗口尺寸不变但Framebuffer从800×600变1600×1200，确认后端更新一次、不变尺寸不重复更新、逻辑坐标不变。
- 实际X11及原生Wayland窗口测试均覆盖GL/Vulkan；既有渲染9套、行为6套另行回归。

最终会话日志：`/tmp/dpi-game-toggle-acceptance.log`（X11两后端）、`/tmp/dpi-wayland-toggle-acceptance.log`（Wayland两后端）、`/tmp/dpi-final-shader-acceptance.log`、`/tmp/dpi-final-behavior-acceptance.log`。两种窗口协议、两个后端各完成8项检查，所有入口退出0。/tmp不是永久归档。

全屏测试调用游戏自身 `ToggleFullScreen`，而不是只修改窗口状态导致游戏状态不一致。当前Silk/窗口系统将这条路径报告为`Maximized`，实际Framebuffer为1920×1080；测试验证该实际输出的四角、黑边及退出后画面恢复，不宣称独占全屏。测试有75秒超时与独立阶段，不能在没进入该阶段时提前打印通过。

## 平台边界

Wayland合成器/平铺窗口管理器可以强制窗口外框尺寸并忽略客户端比例提示，例如本机Hyprland对`window.resize(1000,650)`保留1000×650。此时我们**不再尝试改写窗口尺寸**（Wayland下已关闭客户端比例校正，避免与合成器拉锯），而是让整个4:3画面等比缩进实际窗口、上下或左右留黑边，绝不裁切游戏内容。X11下仍主动校正到4:3客户区。原生Wayland的GL/Vulkan在1000×650下四角图案完整、边界为黑；X11在校正后的1000×750下验证。1000×650容器内内容区域为866×650、左右黑边，不是铺满或裁切。不修改用户窗口管理器的全局策略。

`/tmp/wayland-override.png` 是实际合成器强制1000×650后抓取的窗口区域；当前模型无法直接查看图片，完整性结论依据四角和黑边像素断言，不把读图工具成功返回冒充人工目视验收。

真实跨不同 DPI 显示器的拖动、Windows/macOS 运行尚未实测；模拟 DPI 与 Linux 运行结果不冒充这些硬件验收。已实现的是 framebuffer 像素尺寸变化响应与不改变游戏逻辑的缩放，不提高原版贴图分辨率。
