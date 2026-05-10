# Rvvin Telegram Tool

一个 C# / .NET Windows Forms 桌面工具原型。界面采用白色背景、黑色文字、顶部标题“Rvvin工具开发”、左侧竖向功能分类。

## 已实现

- 启动后自动创建本地工作目录：`accounts`、`sessions`、`imports`、`exports`、`logs`、`settings`。
- 左侧栏目：账号管理、采集群组、自动群发、群组筛选、AI-Token监控、数据报表、全局设置。
- 账号管理：导入 session、删除账号、账号检测、账号登录标记、全部勾选、导出账号、iOS 随机 iPhone 11~17 登录设备参数。
- 群组筛选：导入 txt 群组链接、筛选链接格式、人数/在线人数估算、禁言检测标记、发言频率、相同信息数量、垃圾广告/真人发言启发式判断、导出结果。
- AI-Token监控：维护 Token 总额度和预警阈值，导入 CSV/TSV 历史账单，手动补录输入/输出/缓存 Token 与费用，展示余额、今日/本月/累计用量并导出报表。
- 全局设置：同步 TG 更新、生成 txt 日志、使用本地网络登录账号。

## 合规说明

当前版本不会自动登录 Telegram、不会绕过 Telegram 限制、不会自动群发消息。账号冻结、双向、禁言以及群组真实人数/在线人数需要在获得授权后接入 Telegram 官方 API 或人工复核。AI-Token 监控仅处理用户导入或手动录入的本地账单数据，不会自动调用第三方计费接口。本项目仅提供本地文件管理、界面和合规检测流程框架。

## Windows 打包命令

在安装 .NET 8 SDK 的 Windows 或支持 Windows targeting 的构建环境中执行：

```powershell
dotnet publish src/RvvinTelegramTool/RvvinTelegramTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/RvvinTelegramTool-win-x64
```

生成的 `dist/RvvinTelegramTool-win-x64/RvvinTelegramTool.exe` 可在 Windows 上直接打开。
