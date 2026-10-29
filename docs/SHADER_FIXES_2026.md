# Shader 与光照（2026-10-25）

本文件是 shader 的当前状态与验收记录。

## 实现与验收

| 项 | 当前实现 | 验证 |
|---|---|---|
| Blur/Filter/Warp 不执行 | `Effect.cs`、`EffectSource.cs`、`EffectShaders.cs` 保存已声明参数、真实加载源、单technique/pass生命周期、GLSL编译/链接、Mirror/Linear sampler；Sprite实际调用pass程序 | EffectCompatibility：每个案例7680像素全RGBA对照原公式CPU参考；Filter、Blur多强度、Warp半径0/非零、ColorKey -1..6及靠边中心；浮点最大误差约0.000064 |
| 渲染纹理上下方向与Boss中心坐标 | 分离 `uInputFlip`（Sprite坐标）和 `uSourceFlip`（实际纹理方向），在逻辑UV空间计算原FX公式，采样时转换回GL方向 | SpriteIntegrationChecks：真实上传→离屏→Sprite→FX→输出，非对称图像、顶部偏心Boss、六种ColorKey、显式RT源覆盖普通源；8bit误差小于0.002 |
| 参数丢弃、无错误/释放状态 | 未声明参数、错类型、非法pass顺序、释放后使用明确报错；缺文件与不支持源码明确失败；GPU资源丢失后可重建 | API/lifecycle/nested-pass、typo拒绝、Dispose断言 |
| 解绑破坏外部sampler | FX Bind保存非零原sampler，Unbind恢复 | GPU从非零sampler开始，所有效果绘制后均断言恢复 |
| 跨Flush加法丢失 | Flush不重置混合；Begin/End保存/恢复Sprite批次状态；批次内设备设定持续生效 | 蓝底红色加法两个对象跨Transform触发Flush，均为128,0,255,255 |
| 离屏没有深度附件 | `Surface.CreateDepthStencil`创建真实D16 renderbuffer；Device设置深度表面时挂到当前颜色FBO；切换恢复；不匹配尺寸暂不挂载 | 近红后远蓝仍保持红；ZWrite=false允许远蓝覆盖；清深度临时开启DepthMask再还原 |
| 局部viewport忽略 | Device3D使用调用者X/Y/W/H、MinZ/MaxZ；离屏直接映射，窗口逻辑面letterbox后映射 | 64目标中16,16,32,32局部绘制，只改变中心，外部保持原色 |
| ReverseSubtract空实现 | D3DBLENDOP值修正，GL BlendEquation映射Add/Subtract/ReverseSubtract/Min/Max | 蓝目标减红源保留蓝色断言 |
| 第5关Mesh整条路径空实现 | `Mesh.cs`与`XMesh*.cs`解析已交付text .x：Frame变换、凹多边形保绕序三角化、材质索引、独立Normal索引、UV、subset，Clone/ComputeNormals/Dispose；真实VBO绘制 | MeshCompatibility 59基础CPU/GPU断言，并测试游戏Model_Mesh封装加载BMP；Stair378顶点344三角形2材质、祭坛357顶点200三角形10材质 |
| 法线/方向Diffuse/Specular缺失 | 真实法线属性、world/view逆转置、方向光Diffuse、材质Ambient/Emissive、纹理阶段之后加Specular、LocalViewer默认顶点到相机视线 | 法线朝向/反向照明、高光开关、黑纹理不吞掉高光、LocalViewer与无限观察者差异像素断言 |
| 声明/顶点颜色不消费 | 声明offset与实际stride独立；支持Position/UV/Normal/packed D3DCOLOR，末尾padding；FVF属性和SetFVF均清旧声明；修正TexturedColoredVertex声明offset | 绿色packed FVF、红色声明通道、padded stride28声明、同stride布局切换测试 |
| 默认Clamp导致平铺失效 | 3D device sampler默认Wrap，支持Wrap/Clamp/Mirror/Border、Point/Linear、MipFilter；纹理max level与实际层数一致 | UV>1边界、线性混色、BorderColor、单层纹理+mip采样完整性 |
| MSAA状态忽略 | GL Multisample开关接线；窗口读取AntiAlias配置申请4samples，同时申请depth24/stencil8 | GL状态开启/关闭断言；实际驱动可用采样数取决于平台，不承诺每个平台支持4samples |
| Surface创建破坏read/draw绑定、资源泄漏 | 分别保存/恢复read/draw FBO；Surface仅删除自己拥有的texture，深度renderbuffer释放；ScreenTexture不释放借来的旧目标，Reset替换自身资源 | 分离read/draw构造后恢复断言；嵌套FBO测试与深度测试 |
| FX加载异常被吞+忙循环 | 单次明确加载，失败释放已创建FX并带原因上抛；ScreenTexture创建失败不再无间隔重试 | 发布版临时移走Blur.fx后2.77秒内明确失败退出，不挂死；测试finally恢复文件 |
| 发布目录没有FX/模型资源 | csproj复制FX/Model/Image/BGM/Sound/Story/CS/BPic/Setting.INI到publish；首次启动创建Replay/ScreenShot输出目录 | publish逐文件SHA256核对：FX3/Model12/Image445/BGM18/Sound44/Story49/CS943；不设THSSS_DATA_ROOT进入实际关卡并暂停40秒 |

## 测试命令

从 `src/` 执行，使用其中global.json指定SDK8：

```sh
dotnet run --project ../tests/testcase/ShaderAudit/ShaderAudit.csproj
dotnet run --project ../tests/testcase/ShaderGpuAudit/ShaderGpuAudit.csproj
dotnet run --project ../tests/testcase/GraphicsCompatibility/GraphicsCompatibility.csproj
dotnet run --project ../tests/testcase/MatrixConventions/MatrixConventions.csproj
dotnet run --project ../tests/testcase/EffectCompatibility/EffectCompatibility.csproj
dotnet run --project ../tests/testcase/MeshCompatibility/MeshCompatibility.csproj
```

现在 `ShaderAudit`、`ShaderGpuAudit` 是严格回归：不符合预期会抛异常/非零退出，不再是仅打印MISSING的诊断。

发布：`dotnet publish -o /tmp/thsss-shader-fixed-publish`。程序默认以程序集目录为数据根，无需额外环境变量。开发运行仍可用 `THSSS_DATA_ROOT=/home/ictye/Projects/reatest dotnet run`。

## 明确支持边界

- FX实现为三份已交付源码的保真移植，不是通用HLSL编译器；未知/改动算法明确拒绝，不假成功。GBK注释不影响数学代码识别。
- .x解析器支持当前交付的文本模型与相应结构。binary/compressed .x明确报NotSupported，不静默变成空模型。
- 通用Device.DrawIndexedPrimitives没有活动游戏直接调用；当前明确报NotSupported，真实Mesh已展开合法索引为DrawPrimitives，不再靠空调用“成功”。DrawUserPrimitives已上传真实临时VBO。
- 当前活动纹理stage为Texture×Diffuse的Modulate，GLSL对应实现；不声称支持任意D3D多纹理组合。
- 光照实现覆盖游戏活动方向光/环境光/材质/法线；不是任意D3D光照类型的完整驱动替代。
- 所有像素基线是对原FX数学与已知固定管线预期的验证，不是原版可执行程序逐帧截图金标准。完整真人Boss通关与所有驱动视觉对照没有进行；当前模型不能直接查看返回图片，不用“非黑像素”冒充视觉一致性。
- 旧INI包NU1701及旧资源BinaryFormatter警告仍在，属于既有工具链/序列化风险，不是本次shader缺陷修复成果，也没有为了安静而在项目里隐藏这些警告。

## 评审与剩余状态

两次独立静态评审发现并已修复：非零sampler恢复、未知参数静默成功、padded声明/FVF切换、mip完整性、Specular纹理后相加、LocalViewer及分离read/draw恢复；每项都有新增回归。最终是否所有指定测试通过，以实际运行结果为准，不以本文件替代命令输出。

## 最终执行记录

2026-10-20，Intel UHD / Mesa OpenGL 4.6 环境，七套测试按顺序 fail-fast 实际执行，退出码0：ShaderAudit（8条PASS输出，7个API断言）、ShaderGpuAudit（25条PASS）、GraphicsCompatibility（6组PASS）、MatrixConventions（1组）、MathCompatibility（1组）、EffectCompatibility（44条PASS，包括真实Sprite/RT方向）、MeshCompatibility（62条PASS输出，包括59基础断言和两个真实游戏模型封装）。输出：`/tmp/thsss-shader-final-regressions.log`。

运行入口：从仓库根目录执行 `sh tools/run-shader-regressions.sh`（DejaGnu 驱动，9 套）。失败即非零退出，不过滤错误。

## 光照状态（2026-10-25）

法线归一化与顶点材质来源按 D3D9 文档核对实现，不通过改关卡灯光或模型缩放参数掩盖偏差：

- `NormalizeNormals` 默认false；先做world/view逆转置，再由开关决定是否归一化。零法线不执行normalize，避免NaN。基线复现：沿Z缩放2倍应输出红色128，旧版错误输出255；修复后开关切换为128→255→128。
- `ColorVertex` 默认true；Diffuse/Specular/Ambient/Emissive分别从Material/Color1/Color2选择。默认Diffuse=Color1、Specular=Color2、Ambient/Emissive=Material。关闭ColorVertex时全部使用相应材质；缺少指定顶点颜色时同样回退相应材质，不拿另一组顶点颜色顶替。
- 支持声明中Color usageIndex=1以及FVF Specular，两组颜色独立处理packed D3DCOLOR BGRA和Float4；材质来源的Diffuse alpha决定光照结果alpha。高光仍在线性纹理调制之后相加。
- 修正状态数字：SpecularEnable29、LocalViewer142、NormalizeNormals143、AmbientMaterialSource147、EmissiveMaterialSource148。旧AmbientMaterialSource=140会误写FogVertexMode，现已分离。
- `tests/testcase/LightingCompatibility` 实际GPU通过110个像素断言，包括四光照分量×三来源×ColorVertex两状态×四种颜色存在组合，以及默认状态、packed/FVF、法线缩放、旋转相机和CPU逆转置参考。

`sh tools/run-shader-regressions.sh` 当前运行九套，全量退出码 0。

文档依据（D3D9 规范本身是历史 API，引用保持原年份）：
- https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3drenderstatetype
- https://learn.microsoft.com/en-us/windows/win32/direct3d9/d3dmaterialcolorsource
- https://learn.microsoft.com/en-us/windows/win32/direct3d9/diffuse-lighting
- https://learn.microsoft.com/en-us/windows/win32/direct3d9/emissive-lighting

本次光照补充经独立只读评审通过，未发现高置信新增缺陷。真实发布版（`/tmp/thsss-lighting-fixed`）完成关卡、暂停、恢复45秒运行；日志 `/tmp/thsss-lighting-fixed.log`，未发生异常。AFT C#语言服务器初始化失败，不能给出诊断结论；编译与测试结论依据实际dotnet publish和八套回归退出码，而非AFT空诊断。没有进行原版逐帧截图金标准比对，不能据此声称全场景视觉完全一致。
