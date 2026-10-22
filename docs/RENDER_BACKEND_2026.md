# 渲染后端抽象（2026-10-22）

> **状态更新**：本文中「尚未完成 / Vulkan 只能创建不能渲染」的结论已过时。当前进度与边界见 [`RENDER_BACKEND_STATUS_2026.md`](RENDER_BACKEND_STATUS_2026.md)。

## 现状

渲染已从“兼容层直接调 OpenGL”改为“兼容层只调 `IRenderBackend`”。GL 行为未降级：仍是 330 core、同样的 D3D9 混合/采样值表、同样的离屏与截屏语义。

- 接口：`src/Platform/Graphics/IRenderBackend.cs`、`IRenderResources.cs`
- GL 实现：`src/Platform/Graphics/OpenGl/`（backend + texture/buffer/shader/framebuffer）
- 兼容层：`Texture`、`VertexBuffer`、`Surface`、`Sprite`、`Device3D`、`Effect`、`Device` 只依赖接口
- 入口：`Device.RenderBackend`（当前构造 GL 后端）

## 为多后端留的缝

| 缝 | 作用 |
|---|---|
| `IRenderHandle` | 句柄不再是 `uint` 的 GL 名字 |
| `BackendFeatures` | 能力查询（sampler object、RGB8、原点方向、着色器语言），调用方按能力分支而不是猜后端 |
| `BlendFactor`/`BlendEquation` | D3D9 数值枚举，映射集中在 GL 后端 |
| `SamplerStateBlock` | 采样状态按 D3D9 语义传递 |
| `VertexLayout` | 顶点布局描述，后端自行绑定 |
| `ShaderSource` | GLSL 源码；Vulkan 后端需要在此做 GLSL→SPIR-V |

## 迁移中修掉的真实缺陷

这些都曾让 8 套渲染回归失败，已逐项修复并留作断言：

1. **后端必须持有 VAO**：core profile 下跨 VAO 的 `DrawArrays` 返回 `GL_INVALID_OPERATION`，3D 全黑。
2. **渲染目标自带深度**：`DepthFormat.None` 使 `depthBits=0`，深度遮挡完全失效。
3. **不得清空目标自带深度**：`AttachDepthSurface` 在未设 D3D9 深度面时把附件覆盖为 0。
4. **未设纹理的白色回退**：D3D9 未绑纹理阶段采样为白色不透明，之前依赖上一位调用者遗留的绑定。
5. **`ReadPixels` 规范化 pixel-pack 状态**：外部遗留的 PBO/对齐会让截屏读出全 0。
6. **截屏恢复 split FBO 绑定**：`BindFramebuffer(null)` 会重置读/写绑定对，调用方的状态必须原样还原。
7. **回读统一翻转**：自建 FBO、临时包装 FBO、默认帧缓冲三种来源都是自下而上。

## 验证

- `sh tools/run-shader-regressions.sh`：8 套全通过
- `sh tools/run-behavior-regressions.sh`：5 套全通过
- 真实游戏经新后端运行：0 异常

## 尚未完成

- **后端选择入口**（`THSSS_RENDERER=gl|vk`）和 **Vulkan 后端实现**尚未开始。接口、句柄抽象与能力查询已就位，但没有任何 Vulkan 代码。
- GL 仍是唯一可用后端；选择器缺位意味着“同时支持 vk 和 gl”这一条还没兑现。

## 后端选择（2026-10-22 追加）

`THSSS_RENDERER` 环境变量选择后端，默认 `gl`：

| 值 | 行为 |
|---|---|
| 未设置 / `gl` / `opengl` | OpenGL 后端（现有实现） |
| `vk` / `vulkan` | `VulkanRenderBackend` |
| 其他 | `ArgumentException`，列出支持的值 |

实测：`THSSS_RENDERER=vk` 与 `THSSS_RENDERER=bogus` 都会立即报错并给出明确原因，不会静默回退到 GL。

## Vulkan 进展

已完成（`tests/VulkanBackend`，5 项断言在真实设备上通过）：

- 实例、物理设备选择（找带 graphics 队列的设备）、逻辑设备、命令池
- `VulkanRenderBackend` 实现 `IRenderBackend`，能力声明为 `spirv`、`OriginBottomLeft=false`

**未完成**（Vulkan 后端目前只能创建，不能渲染）：

- 纹理、缓冲、着色器、帧缓冲的全部方法仍是 `PlatformNotSupportedException`
- 没有交换链，因此不能直接接管现有 GL 窗口；接入需要重建窗口层或引入离屏合成
- 没有 GLSL→SPIR-V 的构建期管线

也就是说：“同时支持 vk 和 gl”**尚未达成**。选择器与后端骨架就位，GL 是唯一能实际渲染的后端。

## 后续路线（已定，未执行）

1. 构建期用 `glslangValidator` 把现有 GLSL 预编译成 SPIR-V 资源，发布包不依赖编译器
2. 实现离屏渲染路径（texture/buffer/shader/framebuffer/clear/draw/readback），用 GPU 像素断言验证与 GL 一致
3. 最后处理合成：离屏 Vulkan 结果交给窗口，或把窗口层改成双后端
