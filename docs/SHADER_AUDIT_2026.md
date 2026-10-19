# THSSS Shader 效果审计

> **2026-10-20 修复更新**：本文以下“缺失/偏差/MISSING”为修复前的审计快照，不再代表当前实现。逐项修复、可复现验收与剩余支持边界见 [`SHADER_FIXES_2026.md`](SHADER_FIXES_2026.md)。原先两个审计程序现已改成失败即抛异常的回归测试；不能继续按旧输出说明解释。

日期：2026-10-20。范围：原版 FX 源码、游戏调用路径、当前跨平台 GLSL 与固定管线兼容状态，以及可运行的 API/GPU 诊断。

## 结论

**当前不能认定 shader 效果与原版一致。** 原版 `FX/` 有 3 个效果，全部有游戏调用；当前 `Effect` 完全未执行这些 FX。另有 2 套实际 GLSL 程序：Sprite 和 Device3D，只覆盖基础纹理、颜色、矩阵、部分雾效与环境光。

本轮是检查，不是修复所有缺陷。新增的是审计程序与报告，没有把缺失效果静默替换为“近似正确”，也没有更改本轮之前的渲染实现。

## 1. 原始 FX：完整清单

三个文件都存在，全部有调用，无闲置 FX。均只有一个 `technique1/Pass0`，仅提供像素着色器，无 FX 顶点着色器。共同采样状态为 MIN/MAG/MIP LINEAR、U/V MIRROR。

| FX | 原版效果与参数 | 实际场景/调用 | 当前结论 |
|---|---|---|---|
| `FX/Blur.fx`，ps_2_0 | `(sample(uv)+sample(uv+b)+sample(uv-b))/3`；RGB 减 `50*b`；参数 `BlurLevel`。这是对角线三抽头模糊并变暗，不是高斯模糊 | `Shooting/GameState_PauseMenu.cs:39–53`。前50帧将 b 增至0.0025，最终RGB减0.125；菜单在模糊背景之后独立绘制 | **未执行**：Effect 空实现，所以暂停背景的模糊/变暗不是原版 FX 输出 |
| `FX/Filter.fx`，ps_2_0 | 原样返回 `xColoredTexture` 采样值，无额外滤色 | `Shooting/BaseStage.cs:680–687` 与 `GameState_ED.cs:250–257`，关卡/结局最终离屏呈现 | **FX pass未执行**。普通 Sprite 也会采样原图，因此可能看起来仍有画面，但不证明原版 MIRROR/MIP 采样状态被保留 |
| `FX/Warp.fx`，ps_2_b | `X/Y/Time/Radius/ColorKey`；两个移动中心和一个Boss固定中心叠加径向UV扭曲，使用acos曲线，Y距离乘3/4；中心区域按ColorKey减色 | `Shooting/BackgroundManager3D.cs:162–179`。Boss存在时处理背景合成纹理，不是处理整张最终游戏画面 | **未执行**：Boss背景扭曲及减色缺失 |

加载入口：`GlobalDataPackage.cs:129–131`。`StageDataPackage` 只是代理全局 Effect；原版反编译目录中的加载/执行逻辑与当前游戏侧调用一致。当前三个 FX 的数学表达式均已逐个阅读；Warp 的 GBK 注释经转码检查。

Warp参数：`X=Boss.X/640`、`Y=Boss.Y/480`、`Time=TimeMain/20`、`Radius=WarpRadius/640`。ColorKey分支：0减R、1减G、2减B、3减RG、4减RB、5减GB。游戏搜索发现主动赋值0和2；其他分支存在于FX中，但未找到选择它们的游戏调用。

注意：半径为0仍会执行Warp pass，着色半径又额外增加0.02，不能把半径0简单当作无效果。

### Effect 兼容层的运行时证据

`src/Platform/Compat/Effect.cs`：

- `FromFile` 不读取文件，文件不存在也返回成功。
- `FromString` 不校验/编译代码，无效源码也成功。
- `SetValue/SetTexture/SetMatrix` 等没有保存和传递参数。
- `GetTechnique` 返回空句柄；`Begin()` 返回0。
- `BeginPass/EndPass/CommitChanges` 不做GPU操作。
- `Dispose` 不更新 `IsDisposed`。

`tests/ShaderAudit` 实测输出6项 `MISSING`：缺文件未拒绝、非法源码未拒绝、写0.75读0、technique名为null、passes=0、Dispose后IsDisposed=false。

即使游戏无条件调用 `BeginPass(0)`，也不会出现原版效果；Sprite.Flush 始终绑定自己的普通纹理程序。

## 2. 当前实际 GLSL：完整清单

### Sprite.cs（顶点 + 片元）

实现：`uProjection * uTransform * position`，采样纹理后乘顶点RGBA。支持图集UV、普通图像与离屏纹理方向差异、变换、基础alpha混合。

**已运行通过的测试**：图像行方向、FBO创建/恢复、嵌套离屏合成方向、Sprite变换与指定像素颜色、对话Bitmap到GPU纹理上传。

**确认偏差：跨Flush混合状态丢失。** `Flush()` 结束调用 `Device.ResetBlendState()`，将调用者要求的加法混合重置为普通Alpha。`MySprite`每个对象恢复Transform就能触发Flush；`ParticleManager3D`等实际路径要求连续加法叠加。

GPU复现：蓝底上相同半透明红色，两个对象之间触发一次Flush，第一处为 `(128,0,255,255)`，第二处变为 `(128,0,127,191)`。同样的加法效果依对象顺序而变，不符合持续设备状态。

`SpriteFlags` 并未完整实现；Begin无条件启用混合/关闭深度和剔除，尚无全部flag语义的测试。

### Device3D.cs（顶点 + 片元）

实现：W/V/P、D3D 0..W到GL -W..W深度转换、Position+UV纹理×材质、线性/Exp/Exp2雾分支、材质Ambient/Emissive与灯光Ambient、基础混合。

**已运行通过的测试**：真实纹理3D三角形红色中心像素、非轴向相机、正clip W、相机原点/目标、LH近远深度、RH手性、小数线性雾红蓝等比混色、环境光减半亮度、AlphaBlendEnable开/关、D3D混合枚举常量。

这只证明上述断言，不代表所有关卡模型/光照正确。Exp/Exp2分支存在，但没有对应GPU断言；游戏活动路径只发现Linear。

## 3. 活动路径的其他确认缺陷

| 项目 | 证据与影响 | 验证等级 |
|---|---|---|
| 离屏深度缓冲缺失 | `ScreenTexture.cs:38–55` 创建D16并切换；`Surface.CreateDepthStencil` 只返回普通Surface，FBO没有深度附件，`Device.DepthStencilSurface`仅保存属性。第5关启用Z测试/写入、第6关动态启用Z测试 | **GPU复现**：先画近红再画远蓝，开启Z测试后仍得到蓝色 `(0,0,255,255)`，正确结果应保留红色 |
| 局部3D viewport被忽略 | `BackgroundManager3D.cs:119–127`、`ParticleManager3D.cs:77–83` 设置局部viewport；Device3D却按整张目标/4:3重新计算，未用调用者的X/Y/宽高/深度范围 | **静态确认**；尚无场景像素基线 |
| 第5关Mesh路径空实现 | `GlobalDataPackage`加载Stair/祭坛，`EnvironmentSSS05`使用；`Model_Mesh`要求FromFile、材质、ComputeNormals、DrawSubset，兼容Mesh均为空、材质数组恒空 | **静态确认，活动调用**：缺失的是模型绘制，不仅光照 |
| 完整光照缺失 | Device3D仅实现环境光与Emissive，未消费法线、方向光Diffuse和Specular；已有环境光测试仅适用于无Normal的billboard特例 | **静态确认**；Mesh/法线效果不可宣称还原 |
| ZWriteEnable未处理 | 第5关有调用，但Device没有DepthMask实现；ZEnable不等于ZWriteEnable | **静态确认** |
| MSAA状态未处理 | Game_Main请求MultisampleAntialias，兼容层忽略 | **静态确认**；窗口实际采样数未测，不能据此断言所有抗锯齿均关闭 |

## 4. 存在代码但未证实活动可达的缺口

这些不应与已经复现的当前游戏缺陷混为一谈：

- `BackgroundUUZtree` 请求ReverseSubtract，但 `SetRenderState(BlendOperation)` 不做任何事。未找到该类实例化调用。
- `StageTitle3D`/`TexturedColoredVertex`需要顶点颜色；当前3D输入只有Position+UV。该结构字段顺序与声明偏移也不一致。未找到StageTitle3D实例化调用。
- `BackgroundStage1_3D` 使用V=4的平铺；Texture固定Linear/ClampToEdge，D3D默认Wrap不完整。未找到该类实例化调用。
- `DrawIndexedPrimitives/DrawUserPrimitives`、通用纹理stage、SamplerState setter仍为空。游戏没有直接调用这些绘制API或Sampler setter的证据。活动texture-stage只用Texture×Diffuse Alpha Modulate，硬编码乘法覆盖这一个特例，不能泛称完整实现。
- 未发现活动AlphaTestEnable/AlphaFunc/AlphaRef/ZFunc调用，不能把未实现的比较函数重载直接判成当前场景故障。
- 游戏使用 `CullMode=true` 时数值为1，对应D3D None；现有关闭剔除对此一致，不应把它误报为确定的剔除故障。

## 5. 加载与发布风险

`GlobalDataPackage`吞掉FX加载异常，并在初始化中无间隔重试。此问题也存在于原反编译代码，不是此次移植新增。当前Effect是假成功，因此尚未暴露；真实实现FX后可能变成启动忙循环。项目未声明复制根目录FX资产到发布目录，发布完整性未验证。

## 6. 可复现命令与验收边界

从 `src/` 执行，使用其中 `global.json` 指定的SDK8：

```sh
dotnet run --project ../tests/ShaderAudit/ShaderAudit.csproj
dotnet run --project ../tests/ShaderGpuAudit/ShaderGpuAudit.csproj
dotnet run --project ../tests/GraphicsCompatibility/GraphicsCompatibility.csproj
dotnet run --project ../tests/MatrixConventions/MatrixConventions.csproj
```

前两个是**审计诊断**，输出MISSING并正常结束，只表示成功复现缺陷，不是验收通过。后两个带预期值断言，通过表示其限定的基础渲染行为满足断言。GPU程序需要可用的GLFW/OpenGL显示环境。

本轮结果：Effect六项缺失复现；跨Flush混合和深度两个GPU缺陷复现；现有五组GPU基础断言与相机矩阵断言通过。构建仍有旧INI包兼容警告；C#语言服务初始化失败，未把AFT诊断当成编译通过证据。

未做原版与移植版逐帧视觉对比，未走遍全部关卡Boss和符卡，未验证所有驱动/平台。当前模型不能视觉查看截图，因此没有根据图片路径或非黑像素数量宣称效果正确。

## 建议修复顺序（不属于本轮已完成项）

1. 实现Blur/Filter/Warp及pass/参数/镜像采样，加入原公式CPU参考与GPU像素对比。
2. 修正持续混合状态、离屏深度附件/ZWrite和局部viewport，各保留现有复现程序作为回归起点。
3. 补齐活动Mesh读取/材质/subset绘制与法线光照，再验证第5关阶梯、祭坛和第6关深度遮挡。
4. 用暂停、Boss扭曲、关卡远景和上述模型场景做原版/移植版对应截图或视频比较，最后再扩大到未证实活动的代码。
