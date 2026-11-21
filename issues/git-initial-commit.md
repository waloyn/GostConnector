# 任务计划：git-initial-commit

## 1. 上下文与目标
为 `gost-ui-connector` 项目进行标准的 Git 仓库初始化与首次提交：
- 排除近 70MB 的外部二进制文件 `bin/gost.exe` 与包含敏感 Key 的 `profiles.json`；
- 保留 `bin/` 目录结构（`bin/.gitkeep`）；
- 将默认分支设立为 `main` 并创建首次提交。

## 2. 执行计划
- **步骤 1**：完善 `.gitignore` 规则并创建 `bin/.gitkeep`。
- **步骤 2**：执行 `git init -b main` 初始化仓库。
- **步骤 3**：暂存全部安全源码与资产并执行审计。
- **步骤 4**：执行首次提交 `git commit` 并验证。
