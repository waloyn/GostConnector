using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace GostConnector.Services
{
    public class GostProcessManager : IDisposable
    {
        private Process _process;
        private JobObject _jobObject;
        private readonly object _lock = new object();

        public event Action<string, bool> OnOutputReceived;
        public event Action<bool> OnStatusChanged;

        public bool IsRunning
        {
            get
            {
                lock (_lock)
                {
                    return _process != null && !_process.HasExited;
                }
            }
        }

        public GostProcessManager()
        {
            try
            {
                _jobObject = new JobObject();
            }
            catch
            {
                _jobObject = null;
            }
        }

        /// <summary>
        /// 获取 gost.exe 路径
        /// </summary>
        public string FindGostExecutable()
        {
            string appDir = AppDomain.CurrentDomain.BaseDirectory;

            // 1. 尝试当前目录下的 bin\gost.exe
            string path1 = Path.Combine(appDir, "bin", "gost.exe");
            if (File.Exists(path1)) return Path.GetFullPath(path1);

            // 2. 尝试工作区根目录的 bin\gost.exe（开发调试模式下如果在 bin\Debug 或 bin\Release 里运行）
            string path2 = Path.GetFullPath(Path.Combine(appDir, "..", "..", "bin", "gost.exe"));
            if (File.Exists(path2)) return path2;

            // 3. 尝试当前目录的 gost.exe
            string path3 = Path.Combine(appDir, "gost.exe");
            if (File.Exists(path3)) return Path.GetFullPath(path3);

            // 4. 尝试环境 PATH
            string pathEnv = Environment.GetEnvironmentVariable("PATH") ?? "";
            foreach (var dir in pathEnv.Split(';'))
            {
                if (string.IsNullOrWhiteSpace(dir)) continue;
                try
                {
                    string candidate = Path.Combine(dir.Trim(), "gost.exe");
                    if (File.Exists(candidate)) return candidate;
                }
                catch { }
            }

            return path1; // 默认返回 bin\gost.exe
        }

        /// <summary>
        /// 启动 gost
        /// </summary>
        public bool Start(string arguments, out string error)
        {
            error = string.Empty;
            lock (_lock)
            {
                if (IsRunning)
                {
                    error = "gost 进程已在运行中，请先断开。";
                    return false;
                }

                string exePath = FindGostExecutable();
                if (!File.Exists(exePath))
                {
                    error = string.Format("未找到 gost.exe，查找路径为: {0}", exePath);
                    return false;
                }

                try
                {
                    ProcessStartInfo psi = new ProcessStartInfo
                    {
                        FileName = exePath,
                        Arguments = arguments,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        StandardOutputEncoding = Encoding.UTF8,
                        StandardErrorEncoding = Encoding.UTF8,
                        WorkingDirectory = Path.GetDirectoryName(exePath)
                    };

                    _process = new Process
                    {
                        StartInfo = psi,
                        EnableRaisingEvents = true
                    };

                    _process.OutputDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            var h = OnOutputReceived;
                            if (h != null) h(e.Data, false);
                        }
                    };

                    _process.ErrorDataReceived += (s, e) =>
                    {
                        if (e.Data != null)
                        {
                            var h = OnOutputReceived;
                            if (h != null) h(e.Data, true);
                        }
                    };

                    _process.Exited += (s, e) =>
                    {
                        var hOut = OnOutputReceived;
                        if (hOut != null) hOut("gost 进程已退出。", false);

                        var hStat = OnStatusChanged;
                        if (hStat != null) hStat(false);
                    };

                    bool started = _process.Start();
                    if (!started)
                    {
                        error = "无法启动 gost 进程。";
                        return false;
                    }

                    // 绑定到 JobObject 确保无孤儿进程
                    if (_jobObject != null)
                    {
                        _jobObject.AddProcess(_process);
                    }

                    _process.BeginOutputReadLine();
                    _process.BeginErrorReadLine();

                    var handler = OnStatusChanged;
                    if (handler != null) handler(true);
                    return true;
                }
                catch (Exception ex)
                {
                    error = ex.Message;
                    Stop();
                    return false;
                }
            }
        }

        /// <summary>
        /// 停止 gost 进程
        /// </summary>
        public void Stop()
        {
            lock (_lock)
            {
                if (_process != null)
                {
                    try
                    {
                        if (!_process.HasExited)
                        {
                            _process.Kill();
                            _process.WaitForExit(1000);
                        }
                    }
                    catch { }
                    finally
                    {
                        try { _process.Dispose(); } catch { }
                        _process = null;
                    }
                }
                var h = OnStatusChanged;
                if (h != null) h(false);
            }
        }

        public void Dispose()
        {
            Stop();
            if (_jobObject != null)
            {
                try { _jobObject.Dispose(); } catch { }
                _jobObject = null;
            }
        }
    }
}
