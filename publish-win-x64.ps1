$ErrorActionPreference = "Stop"

dotnet publish .\TGTool.csproj `
  -c Release `
  -r win-x64 `
  --self-contained true `
  -p:PublishSingleFile=true `
  -p:EnableCompressionInSingleFile=true

Write-Host "EXE: bin\Release\net8.0-windows\win-x64\publish\RvvinTelegramTool.exe"
