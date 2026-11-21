# Gost Connector

用于便捷管理 `bin/gost.exe` 端口转发的 Windows 桌面工具。

## 功能特点

1. **CLI 命令解析与组装**：
   - 支持粘贴完整命令行（例如：`gost -L tcp://:1433/192.168.18.8:1433 -F relay+grpc://110.0.0.1:10808?auth=xxxxxx`）。
   - 自动解析提取 `-L`（协议、监听端口、内网目标）与 `-F`（转发协议、节点地址、认证参数）。
   - 修改界面表单字段时，实时逆向生成标准命令行，支持一键复制。

2. **进程安全守护 (Job Object)**：
   - 后台静默运行 `gost.exe`，重定向输出与错误日志。
   - 使用 Windows 作业对象（Job Object）绑定子进程生命周期，主界面退出或异常终止时，内核级自动终结 `gost.exe`，避免端口占用。

3. **多配置管理**：
   - 支持新增、保存、删除多个转发方案，配置保存在本地 `profiles.json`。

4. **实时日志与系统托盘**：
   - 界面内置终端风格日志窗口，实时打印进程输出。
   - 支持最小化到 Windows 系统托盘，后台静默转发。

## 文件结构

```text
connector/
├── GostConnector.exe          # 主程序
├── GostConnector.exe.config   # 配置文件
├── profiles.json              # 转发配置数据
└── bin/
    └── gost.exe               # GOST 核心程序
```

## 编译方法

在当前目录下使用系统自带的 MSBuild 编译：

```powershell
msbuild GostConnector.csproj /p:Configuration=Release /p:Platform=x64 /t:Rebuild
```

编译输出位于 `bin\Release\GostConnector.exe`。

## 持续集成

仓库配置了 GitHub Actions 工作流（`.github/workflows/release.yml`）：
- 推送以 `v` 开头的 Tag（例如 `v1.0.0`）时，自动编译并发布至 GitHub Release。
- 支持在 Actions 界面通过 `workflow_dispatch` 手动触发构建与发布。
