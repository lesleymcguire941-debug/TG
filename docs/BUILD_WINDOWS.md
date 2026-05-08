# Windows 打包说明

1. 安装 .NET 8 SDK。
2. 在仓库根目录运行：

```powershell
dotnet publish src/RvvinTelegramTool/RvvinTelegramTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/RvvinTelegramTool-win-x64
```

3. 打开 `dist/RvvinTelegramTool-win-x64/RvvinTelegramTool.exe`。

> 注意：本仓库当前实现的是本地管理和筛选框架。真实 Telegram 账号登录、@SpamBot 查询、群组人数/在线人数读取需要合法授权并接入 Telegram 官方 API。
