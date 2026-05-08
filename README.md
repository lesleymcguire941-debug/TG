# Rvvin Telegram Tool

这是一个 C# / .NET Windows 桌面工具原型，启动时会自动创建账号、session、导入、导出、日志、群组筛选报告和设置目录。

## 已实现页面

- 顶部标题：`Rvvin工具开发`
- 白色背景、黑色字体
- 左侧竖向功能分类：账号管理、采集群组、自动群发、群组筛选、预留栏目、全局设置
- 账号管理：导入 session、导入手机号、删除账号、账号检测、账号登录、全部勾选、取消勾选、导出账号、iOS 随机 iPhone 11~17 设备参数
- 群组筛选：导入 Telegram 群组链接 txt、访问公开 t.me 页面读取公开人数/在线人数、生成禁言/发言频率/重复信息/垃圾广告判断、导出 txt 报告
- 全局设置：同步 TG 更新、生成 txt 日志、使用本地网络登录账号

## 运行与发布

开发机需要安装 .NET SDK 8 或更高版本。

```bash
dotnet publish TGTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true
```

发布后的 exe 位于：

```text
bin/Release/net8.0-windows/win-x64/publish/RvvinTelegramTool.exe
```

> 说明：本版本未内置 Telegram API ID/API Hash，也不会绕过 Telegram 限制。@SpamBot 检测与禁言检测位置已预留为可替换服务，当前以中文本地结果模拟，便于先验收界面和文件流转。
