# 评审报告：github-release-workflow

## 1. 计划对照评估
| 计划步骤 | 目标 | 执行情况 | 评估结论 |
| :--- | :--- | :--- | :--- |
| **步骤 1** | 创建 `.gitignore` | 编写并纳入临时编译输出过滤、用户配置文件隔离规则 | ✅ 达成 |
| **步骤 2** | 编写 `.github/workflows/release.yml` | 支持 Tag 触发 (`v*`) 与网页端手动触发，配置 MSBuild 编译、打包、自愈下载 gost 核心及 SHA256 校验生成 | ✅ 达成 |
| **步骤 3** | 工作流规范性与安全性审查 | 显式声明 `permissions: contents: write`，使用 GitHub Actions 官方推荐的安全 token 访问 | ✅ 达成 |

## 2. 交付产物
* **工作流文件**：`.github/workflows/release.yml`
* **Git 忽略规则**：`.gitignore`
* **任务记录档案**：
  * 计划：`issues/github-release-workflow.md`
  * 评审：`review/github-release-workflow.md`

## 3. 使用建议
- **常规发布**：在本地完成开发并打标签推送到 GitHub：
  ```bash
  git tag v1.0.0
  git push origin v1.0.0
  ```
- **手动发布**：随时可在 GitHub 仓库的 **Actions** 标签页中选择 **Release** 工作流，点击 **Run workflow** 手动触发。
