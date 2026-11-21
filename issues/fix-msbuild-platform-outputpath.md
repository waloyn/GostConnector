# 任务计划：fix-msbuild-platform-outputpath

## 1. 上下文与问题
GitHub Actions 在执行发布工作流时报错：
`error : The BaseOutputPath/OutputPath property is not set for project 'GostConnector.csproj'. Configuration='Release' Platform='x64'`
原因是 `GostConnector.csproj` 缺乏对 `Release|x64` 的显式 PropertyGroup 定义，导致没有配置项匹配该组合，OutputPath 未能设置。

## 2. 执行计划
- **步骤 1**：在 `GostConnector.csproj` 中补齐 `Release|x64` 和 `Debug|x64` 配置，并在顶级 PropertyGroup 增加 OutputPath 兜底。
- **步骤 2**：在本地执行与 CI 相同的构建命令进行验证。
- **步骤 3**：提交 Git 变更。
