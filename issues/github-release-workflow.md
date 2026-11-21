# 任务计划：github-release-workflow

## 1. 上下文与目标
为项目编写 GitHub Actions 自动发布工作流，在推送 Git Tag（如 `v1.0.0`）或在 GitHub 网页上手动触发时，自动完成：
- 在 `windows-latest` 运行器上配置 MSBuild 环境；
- 编译 Release 版本的 64 位 `GostConnector.exe`；
- 自动检测并打包 `bin/gost.exe` 核心（具备自愈下载机制）；
- 生成开箱即用的 ZIP 压缩包与 SHA256 校验和文件；
- 通过 `softprops/action-gh-release@v2` 创建 GitHub Release 并上传附件。

## 2. 执行计划
- **步骤 1**：创建 `.gitignore` 规则，过滤开发临时产物与日志，保护仓库纯净。
- **步骤 2**：编写 `.github/workflows/release.yml` 完整发布工作流。
- **步骤 3**：验证 YAML 语法格式与逻辑完整性。
