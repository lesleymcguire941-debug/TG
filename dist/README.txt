此目录用于放置 Windows 发布产物。

构建命令：
dotnet publish src/RvvinTelegramTool/RvvinTelegramTool.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o dist/RvvinTelegramTool-win-x64
