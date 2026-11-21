# 任务计划：retarget-to-dotnet48

## 1. 上下文与问题
GitHub Actions 构建报错：
`error MSB3644: The reference assemblies for .NETFramework,Version=v4.0 were not found.`
原因是 GitHub Actions `windows-latest` 虚拟机中已移除淘汰的 .NET 4.0 Developer Pack，而自带 .NET 4.8 / 4.8.1。

## 2. 执行计划
- **步骤 1**：将 `GostConnector.csproj` 的 TargetFrameworkVersion 升级为 `v4.8`。
- **步骤 2**：更新 `App.config` 支持 .NET 4.8 及启用 PerMonitorV2 高 DPI。
- **步骤 3**：本地编译验证并提交 Git。
