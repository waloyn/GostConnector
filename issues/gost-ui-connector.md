# 任务计划：gost-ui-connector

## 1. 上下文与背景
用户日常频繁在 Windows 下通过命令行启动 gost 进行端口转发与穿透，典型命令行如下：
`gost -L tcp://:1433/192.168.18.8:1433 -F relay+grpc://110.0.0.1:10808?auth=xxxxxx`
为了简化日常 CLI 操作，需要一个 C# 开发的桌面 UI，具备：
- 能够直接粘贴并解析 CLI 命令行，支持表单可视化修改并双向生成命令行。
- 一键点击“连接”启动 `bin/gost.exe`，并支持优雅断开。
- 绑定 Windows Job Object，确保主进程退出/崩溃时子进程 gost.exe 彻底终结，不留孤儿进程占用端口。
- 实时日志输出查看、多配置保存切换、系统托盘常驻。

## 2. 执行计划
- **步骤 1**：创建工程配置文件 `GostConnector.csproj` 和 `App.config`（启用 PerMonitorV2 高 DPI 支持）。
- **步骤 2**：编写 Win32 `JobObject` 封装类，实现 `JOB_OBJECT_LIMIT_KILL_ON_JOB_CLOSE`。
- **步骤 3**：实现 `GostProfile` 模型与 `GostCliParser` CLI 解析与反向拼装引擎。
- **步骤 4**：实现 `GostProcessManager` 负责进程无黑框调度与 stdout/stderr 异步捕获。
- **步骤 5**：实现 `ProfileManager` 配置存储与 JSON 持久化管理。
- **步骤 6**：构建 `MainForm` 现代化扁平 WinForms 窗体与托盘菜单，编写 `Program.cs`。
- **步骤 7**：使用 MSBuild 4.8 编译并执行自动化测试与验证。
