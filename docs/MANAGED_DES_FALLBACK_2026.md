# 托管 DES 回退（2026-10-25）

## 为什么需要

Wine 和 Proton 的 `bcrypt` 没有 DES 提供程序：

```
trace:bcrypt:BCryptOpenAlgorithmProvider ..., L"DES", (null), 0
fixme:bcrypt:BCryptOpenAlgorithmProvider algorithm L"DES" not supported
```

`.NET` 在 Windows 上通过 CNG 实现 `System.Security.Cryptography.DES`，所以存档加解密在 Wine 下直接抛 `CryptographicException 0xC1000002`，启动中止。真实 Windows 没有这个问题（实测同密钥同 IV 输出与 Linux 一致）。

启用原生 `bcrypt.dll` 不可行：把真实 Windows 的 `bcrypt.dll` 放进 Wine 前缀后能加载，但 `DllMain(PROCESS_ATTACH)` 初始化失败，它依赖真实 Windows 内核与 BCryptPrimitives 实现。

## 构建开关（默认关闭）

回退代码由编译常量 `THSSS_MANAGED_DES_FALLBACK` 控制，**默认构建完全不包含它**——`src/Shooting/ManagedDes.cs` 整个文件在 `#if` 之外没有代码，程序集里搜不到 `ManagedDes`。

```sh
# 生产构建：不含回退
dotnet publish src/THSSS.csproj -r win-x64 --self-contained

# Wine/Proton 构建：包含回退
dotnet publish src/THSSS.csproj -r win-x64 --self-contained -p:THSSSManagedDesFallback=true
```

运行时只有一条分支：先尝试平台 DES，抛 `CryptographicException` 时才用回退。未启用回退的构建在这种情况下抛出明确信息，提示用上述参数重新构建，而不是静默返回错误结果。

## 实现

`src/Shooting/Encryption.cs` 的 `Cipher()` 返回 `SaveCipher`：

- `PlatformCipher`：原样使用 `DES.Create()` 的加解密器
- `ManagedCipher`（仅启用开关时编译）：调用 `ManagedDes` 的 CBC + PKCS#7

密钥、IV、CBC、填充都与原实现一致，存档格式不变。

`ManagedDes` 是标准 FIPS 46-3 DES 实现（初始置换、16 轮 Feistel、S 盒、最终置换、PC-1/PC-2 子密钥）。

## 验证

**算法正确性**：用 FIPS 公布的标准测试向量

```
key 133457799BBCDFF1, plaintext 0123456789ABCDEF
got      85-E8-13-54-0F-0A-B4-05
expected 85-E8-13-54-0F-0A-B4-05
```

**与平台 DES 逐字节等价**（`tests/ManagedDesCheck`，已并入行为回归）：

- 5 个样本（空串、单字符、ASCII、中文、多行）加密结果与平台 DES 完全一致
- 双向互解：平台密文由托管解出原文，托管密文由平台解出原文（5000 字节 + 中文）

**Wine 实跑**：

| 检查 | 结果 |
|---|---|
| 未启用回退的构建在 Wine 启动 | `CryptographicException 0xC1000002` |
| 启用回退的构建在 Wine 启动 | 越过存档加密，写出 `Score.dat`（76512 字节）与 `.bak` |
| 该存档由 Linux 平台 DES 解密 | 成功，38248 字节、1565 行，格式互通 |
| Linux 行为回归 7 套 | 通过（含新增 `ManagedDesCheck`） |
| 默认构建不含回退 | `strings THSSS.dll | grep ManagedDes` = 0 |

## 未覆盖

- Wine 启动后仍会因环境缺少中文字体报 `No usable system font was found`，这是独立的字体问题，不属于本回退
- 未在真实 Windows 上用启用回退的构建跑完整游戏（真机走平台 DES，本来就不会用到回退）
- 未验证 Proton 实机
