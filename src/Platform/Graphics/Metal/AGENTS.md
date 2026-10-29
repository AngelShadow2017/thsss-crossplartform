# Native Metal backend

`MetalNative` owns typed Objective-C message dispatch; floating-point/structure arguments must not be passed through integer overloads. `MetalRenderBackend` owns device/queue/layer, native resources track their owner, and external framebuffer depth is borrowed. Present shaders follow the GL-compatible texture row convention; test top/bottom analytically against SPIRV-Cross output before changing flips.

`MetalSourceCompiler` first reads content-addressed `MetalShaders/*.msl` beside the executable. Missing caches may invoke glslangValidator and spirv-cross for development. `tools/export-metal-shaders.sh` exports actual Sprite/3D/FX sources before package checksums.

Verification: `sh tools/run-mac-regressions.sh` uses actual P/Invoke with a Linux x86_64 software native API driver. It checks selectors, ABI, resource storage, command order and Cocoa presentation. The driver does not execute MSL or claim Apple runtime behavior. `tests/testcase/MetalCompatibility` separately runs real shader conversion. CoreAudio has a separate native queue software driver. Keep that evidence distinction explicit.
