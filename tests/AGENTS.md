# Rendering compatibility tests (2026-10-20)

## Resize/DPI acceptance (2026-10-24)

`sh tools/run-resize-regressions.sh` from root runs `WindowResizeCompatibility` on both GL/Vulkan with temporary player data. X11 headless: `ALSOFT_DRIVERS=null xvfb-run -a sh tools/run-resize-regressions.sh`; omit Xvfb for native Wayland desktop. Covers six sizes/non4:3 correction, minimum/restore/fullscreen, stable swapchain generation, non-symmetric corner pixels, fullscreen black borders and GL errors. DPI framebuffer-only updates are also in WindowInputCompatibility/PresentationGeometryChecks. See docs/WINDOW_SCALING_2026.md for compositor override and untested physical multi-monitor limits.

Run all commands from `src/` so `global.json` selects SDK 8. The installed SDK 10 workload manifests are incomplete; do not confuse that environment failure with a code failure.

## Acceptance suites

- `GraphicsCompatibility`: image/RT orientation, nested FBO restore, transformed Sprite, real textured Device3D pixels, dialogue upload, linear/Exp/Exp2 fog, ambient light and blend enable.
- `MatrixConventions` and `MathCompatibility`: stage camera origin/target, nonzero terrain NDC, positive clip W, LH near/far depth, RH handedness, GL upload and 2D pivots.
- `EffectCompatibility`: source/API validation, typed parameters and lifecycle; all RGBA pixels compared to original Blur/Filter/Warp CPU formulas; mirror edges, radius0, all colour keys and off-centre bosses; Sprite+RT source direction and explicit texture override; nonzero sampler restoration.
- `MeshCompatibility`: text .x parser fixtures/concave triangulation/winding/normals/materials/Clone; actual Stair and 祭坛 geometry; GPU subsets/normal strides plus game Model_Mesh with shipped BMPs. It locates repository root itself.
- `LightingCompatibility`: 110 GPU assertions for NormalizeNormals default/off/on, zero normals, nonuniform scale plus camera inverse-transpose, all four material-source terms with Material/Color1/Color2, ColorVertex switches and per-channel fallback, diffuse alpha, default sources, packed/float secondary colours and FVF/declaration layouts.
- `ShaderAudit` and `ShaderGpuAudit`: former defect reproducers are now strict regression assertions. Failure throws/nonzero exit. Coverage includes absent-file/error/lifecycle handling, cross-Flush additive blend, D16 depth/ZWrite, viewport, reverse subtraction, packed vertex declarations and padding, FVF switches, samplers/mip completeness, directional/specular/local-viewer lighting and framebuffer binding restoration.

`dotnet run --project ../tests/<suite>/<suite>.csproj` executes each suite. GPU suites need a working GLFW/OpenGL display. `EffectCompatibility --api-only` and `MeshCompatibility --cpu-only` explicitly skip GPU checks.

## Compatibility invariants

- `Device.Material` must forward to Device3D; retaining an auto-property makes models invisible with zero shader material alpha.
- D3D row-vector matrix fields upload in row element order with transpose=false, then GLSL uses projection*view*world*position. Depth maps by clip.z=2*clip.z-clip.w.
- D3D Blend values include Zero=1, One=2, SourceAlpha=5, InverseSourceAlpha=6. FogEnable=28 and FogTableMode=35 are different states.
- D3D lighting state IDs: SpecularEnable=29, LocalViewer=142, NormalizeNormals=143, DiffuseMaterialSource=145, SpecularMaterialSource=146, AmbientMaterialSource=147, EmissiveMaterialSource=148. NormalizeNormals defaults false; ColorVertex defaults true; default sources are Diffuse=Color1, Specular=Color2, Ambient/Emissive=Material. Missing requested vertex colour falls back to the corresponding material component, not to the other vertex colour.
- FX uses logical top-left UV for its formulas and transforms to actual GL render-target sampling direction independently. Test with an asymmetric image and an off-centre Boss; centre-only images hide Y inversion bugs.
- Sprite Flush must not reset caller blend factors mid-batch. End restores saved state unless DoNotSaveState.
- A surface owns only its own GL objects; switching colour targets must attach the selected compatible depth renderbuffer and restore borrowed targets, not dispose them.

Current repair matrix: `../docs/SHADER_FIXES_2026.md`. `../docs/SHADER_AUDIT_2026.md` is the pre-fix audit snapshot, not current pass/fail state.

Numeric test results do not establish every-stage screenshot parity across GPUs. The current model cannot view returned image attachments; never infer correct visuals from nonblack pixel counts alone.

## Behavior parity diagnostics (2026-10-20)

`BehaviorParityAudit` is NOT an acceptance suite and is intentionally excluded from `run-shader-regressions.sh`. Run from `src` with `dotnet run --project ../tests/BehaviorParityAudit/BehaviorParityAudit.csproj -- --gpu`. It uses temporary isolated score/replay fixtures and silent WAVs; `CONFIRMED` means a current defect/difference was observed, not parity passed. See `../docs/BEHAVIOR_PARITY_AUDIT_2026.md` for original-source comparisons and the missing original Windows CLR2 runtime baseline.

## Behavior repair acceptance (2026-10-20)

- `ListCompatibility` is now a strict regression: pre-4.5 live indexed enumeration (including removals, same-pass appends, actual game events/backgrounds), plus explicit worker-stop timeout. Production former ForEach call sites use `LegacyItems`, not `Snapshot`.
- `SaveCompatibility` uses only temporary score/replay files and asserts legacy CP936/UTF8 byte preservation, failed-load rollback/save blocking, atomic backups and replay number/EOF contracts.
- `WindowInputCompatibility --glfw --game-smoke` includes real hidden GLFW and separate full-game processes for both keyboard modes, remapped controls/F8/Quit, focus and controller snapshots. It filters Linux mouse/touchpad capabilities from controller enumeration. Use Xvfb when there is no display; real physical gamepad and variable-refresh driver tests remain separate manual checks.
- `sh tools/run-behavior-regressions.sh` from root is the acceptance runner for repaired behavior suites. Unlike the historical `BehaviorParityAudit`, its checks must throw/return nonzero on regressions.

Final current behavior matrix: `../docs/BEHAVIOR_PARITY_FIXES_2026.md`. AudioCompatibility: 162 native/loopback assertions. SaveCompatibility: 84 assertions including valid ending LastStage=All and 3-digit legacy comma decimals. TextScreenshotCompatibility: 167 assertions plus logical backbuffer scaling and actual P-request-after-swap sequencing. WindowInputCompatibility: 109 assertions including two keyboard modes and a third complete Alt+F4 process. Historical BehaviorParityAudit is a repair-before snapshot, not a maintained acceptance tool; corrected Replay APIs may explicitly reject its formerly accepted malformed fixtures. Use the strict behavior runner, not the historic diagnostic, to verify fixes.

- 2026-10-21 parity repairs: default System.Drawing.Font constructors now set Point (GDI+ default); FixedFPS deadline (`FinishFrameTiming`) owns live-window pacing with `gameOwnsPacing` disabling GLFW render cap. Tests: TextScreenshotCompatibility 171 incl unit/scale assertions; WindowInputCompatibility 4721 incl 144 deterministic timing scenarios and real-loop stall catch-up. See docs/PARITY_RECHECK_2026.md.

## 双渲染后端验收（2026-10-23）

`VulkanBackend/SpriteChecks.cs` 直接调用游戏实际 Sprite/MySprite、菜单图集与透明叠加，不以常量三角形代替2D链路验证；包括400次draw一次提交、不同变换/颜色的独立快照、跳过Present的EndFrame、静态字形缓存失效。`THSSS_DIAG_FPS=1` 输出实际FPS和每帧提交/draw；性能测试不同时开像素读回诊断。当前根脚本实际为9套渲染+6套行为，以suite入口计数，不能数PASS行。原生Wayland/X11及GL桌面证据见 `../docs/RENDER_BACKEND_STATUS_2026.md`。所有实际游戏测试使用临时配置/Score与独立录像目录，不写live game/玩家数据。
