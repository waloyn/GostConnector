using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using GostConnector.Models;
using GostConnector.Services;

namespace GostConnector.UI
{
    public class MainForm : Form
    {
        private readonly ProfileManager _profileManager;
        private readonly GostProcessManager _processManager;
        private bool _isUpdatingUi = false;

        // UI 控件
        private ComboBox cboProfiles;
        private Button btnNewProfile;
        private Button btnSaveProfile;
        private Button btnDeleteProfile;

        private TextBox txtCommandLine;
        private Button btnParseCli;
        private Button btnCopyCli;

        private ComboBox cboListenProto;
        private TextBox txtListenAddr;
        private TextBox txtTargetAddr;

        private ComboBox cboForwardProto;
        private TextBox txtForwardServer;
        private TextBox txtAuthParam;
        private TextBox txtExtraArgs;

        private CheckBox chkUseRawCli;
        private CheckBox chkMinimizeToTray;
        private Label lblStatusDot;
        private Label lblStatusText;
        private Button btnToggleConnect;

        private RichTextBox rtbLog;
        private Button btnClearLog;
        private CheckBox chkAutoScroll;

        private NotifyIcon notifyIcon;
        private ContextMenuStrip trayMenu;
        private ToolStripMenuItem trayItemToggle;

        public MainForm()
        {
            _profileManager = new ProfileManager();
            _processManager = new GostProcessManager();

            InitializeComponent();
            SetupEvents();

            LoadProfilesToUi();
            UpdateConnectButtonState(false);
        }

        private void InitializeComponent()
        {
            this.Text = "GOST 转发连接器 (CLI 快捷管理)";
            this.Size = new Size(880, 720);
            this.MinimumSize = new Size(800, 640);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Font = new Font("Microsoft YaHei UI", 9F, FontStyle.Regular, GraphicsUnit.Point);
            this.BackColor = Color.FromArgb(246, 248, 250);

            // 主纵向布局面板
            Panel topPanel = new Panel { Dock = DockStyle.Top, Height = 50, Padding = new Padding(15, 10, 15, 10), BackColor = Color.White };
            Panel cliPanel = new Panel { Dock = DockStyle.Top, Height = 95, Padding = new Padding(15, 8, 15, 5), BackColor = Color.White };
            Panel paramPanel = new Panel { Dock = DockStyle.Top, Height = 175, Padding = new Padding(15, 5, 15, 5), BackColor = Color.White };
            Panel actionPanel = new Panel { Dock = DockStyle.Top, Height = 65, Padding = new Padding(15, 10, 15, 10), BackColor = Color.FromArgb(240, 244, 248) };
            Panel logPanel = new Panel { Dock = DockStyle.Fill, Padding = new Padding(15, 5, 15, 15), BackColor = Color.White };

            // 1. 顶部配置管理栏
            Label lblProfile = new Label { Text = "配置方案:", AutoSize = true, Location = new Point(15, 15), Font = new Font(this.Font, FontStyle.Bold) };
            cboProfiles = new ComboBox { DropDownStyle = ComboBoxStyle.DropDownList, Location = new Point(85, 12), Width = 300, Font = this.Font };
            btnNewProfile = new Button { Text = "➕ 新建", Location = new Point(395, 10), Width = 75, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(235, 240, 245) };
            btnSaveProfile = new Button { Text = "💾 保存", Location = new Point(475, 10), Width = 75, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(235, 240, 245) };
            btnDeleteProfile = new Button { Text = "🗑️ 删除", Location = new Point(555, 10), Width = 75, Height = 28, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(255, 235, 235), ForeColor = Color.DarkRed };

            topPanel.Controls.AddRange(new Control[] { lblProfile, cboProfiles, btnNewProfile, btnSaveProfile, btnDeleteProfile });

            // 2. CLI 命令行输入区
            GroupBox gbCli = new GroupBox { Text = "CLI 命令行 (可直接粘贴完整命令，如 gost -L ... -F ...)", Dock = DockStyle.Fill, Padding = new Padding(10) };
            txtCommandLine = new TextBox { Multiline = true, Location = new Point(12, 22), Size = new Size(580, 52), ScrollBars = ScrollBars.Vertical, Font = new Font("Consolas", 9.5F) };
            btnParseCli = new Button { Text = "⚡ 智能解析到表单", Location = new Point(602, 22), Width = 135, Height = 26, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(230, 243, 255), ForeColor = Color.FromArgb(0, 102, 204) };
            btnCopyCli = new Button { Text = "📋 复制命令行", Location = new Point(602, 50), Width = 135, Height = 24, FlatStyle = FlatStyle.Flat, BackColor = Color.FromArgb(240, 240, 240) };
            
            gbCli.Controls.AddRange(new Control[] { txtCommandLine, btnParseCli, btnCopyCli });
            cliPanel.Controls.Add(gbCli);

            // 3. 参数表单区
            GroupBox gbParam = new GroupBox { Text = "参数配置与拆解", Dock = DockStyle.Fill, Padding = new Padding(10) };
            
            // 监听部分 -L
            Label lblListen = new Label { Text = "【-L 本地监听与目标】", Font = new Font(this.Font, FontStyle.Bold), Location = new Point(12, 22), AutoSize = true, ForeColor = Color.FromArgb(30, 70, 120) };
            Label lblLProto = new Label { Text = "协议:", Location = new Point(12, 48), AutoSize = true };
            cboListenProto = new ComboBox { Location = new Point(52, 45), Width = 90, DropDownStyle = ComboBoxStyle.DropDown };
            cboListenProto.Items.AddRange(new object[] { "tcp", "udp", "http", "socks5", "relay+tcp", "relay+tls" });
            
            Label lblLAddr = new Label { Text = "本地端口/地址:", Location = new Point(155, 48), AutoSize = true };
            txtListenAddr = new TextBox { Location = new Point(255, 45), Width = 110, Text = ":1433" };

            Label lblTarget = new Label { Text = "内网目标(Host:Port):", Location = new Point(380, 48), AutoSize = true };
            txtTargetAddr = new TextBox { Location = new Point(515, 45), Width = 220, Text = "192.168.18.8:1433" };

            // 转发部分 -F
            Label lblForward = new Label { Text = "【-F 中继转发节点】", Font = new Font(this.Font, FontStyle.Bold), Location = new Point(12, 80), AutoSize = true, ForeColor = Color.FromArgb(30, 70, 120) };
            Label lblFProto = new Label { Text = "中继协议:", Location = new Point(12, 106), AutoSize = true };
            cboForwardProto = new ComboBox { Location = new Point(80, 103), Width = 110, DropDownStyle = ComboBoxStyle.DropDown };
            cboForwardProto.Items.AddRange(new object[] { "relay+grpc", "relay+tls", "relay+ws", "tcp", "tls", "http", "socks5" });

            Label lblFServer = new Label { Text = "服务器地址:", Location = new Point(205, 106), AutoSize = true };
            txtForwardServer = new TextBox { Location = new Point(285, 103), Width = 180, Text = "110.0.0.1:10808" };

            Label lblAuth = new Label { Text = "鉴权参数 (auth):", Location = new Point(480, 106), AutoSize = true };
            txtAuthParam = new TextBox { Location = new Point(590, 103), Width = 145 };

            // 附加参数
            Label lblExtra = new Label { Text = "附加参数:", Location = new Point(12, 140), AutoSize = true };
            txtExtraArgs = new TextBox { Location = new Point(80, 137), Width = 655 };

            gbParam.Controls.AddRange(new Control[] {
                lblListen, lblLProto, cboListenProto, lblLAddr, txtListenAddr, lblTarget, txtTargetAddr,
                lblForward, lblFProto, cboForwardProto, lblFServer, txtForwardServer, lblAuth, txtAuthParam,
                lblExtra, txtExtraArgs
            });
            paramPanel.Controls.Add(gbParam);

            // 4. 操作与状态栏
            lblStatusDot = new Label { Text = "●", Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = Color.Gray, Location = new Point(15, 14), AutoSize = true };
            lblStatusText = new Label { Text = "未连接", Location = new Point(42, 22), AutoSize = true, Font = new Font(this.Font, FontStyle.Bold) };

            chkUseRawCli = new CheckBox { Text = "直接使用命令行文本启动", Location = new Point(120, 21), AutoSize = true };
            chkMinimizeToTray = new CheckBox { Text = "关闭时最小化到托盘", Location = new Point(300, 21), AutoSize = true, Checked = true };

            btnToggleConnect = new Button {
                Text = "🚀 启动连接",
                Location = new Point(600, 12),
                Width = 150,
                Height = 40,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(16, 124, 65),
                ForeColor = Color.White,
                Font = new Font(this.Font.FontFamily, 10.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };

            actionPanel.Controls.AddRange(new Control[] { lblStatusDot, lblStatusText, chkUseRawCli, chkMinimizeToTray, btnToggleConnect });

            // 5. 日志监控区
            GroupBox gbLog = new GroupBox { Text = "实时输出日志", Dock = DockStyle.Fill, Padding = new Padding(10) };
            rtbLog = new RichTextBox {
                Dock = DockStyle.Fill,
                BackColor = Color.FromArgb(28, 28, 28),
                ForeColor = Color.FromArgb(220, 220, 220),
                Font = new Font("Consolas", 9.5F),
                ReadOnly = true,
                BorderStyle = BorderStyle.None
            };

            Panel logSubHeader = new Panel { Dock = DockStyle.Top, Height = 30 };
            btnClearLog = new Button { Text = "清空日志", Dock = DockStyle.Right, Width = 80, FlatStyle = FlatStyle.Flat };
            chkAutoScroll = new CheckBox { Text = "自动滚动", Dock = DockStyle.Right, Width = 90, Checked = true };

            logSubHeader.Controls.AddRange(new Control[] { chkAutoScroll, btnClearLog });
            gbLog.Controls.Add(rtbLog);
            gbLog.Controls.Add(logSubHeader);
            logPanel.Controls.Add(gbLog);

            // 组装窗口
            this.Controls.Add(logPanel);
            this.Controls.Add(actionPanel);
            this.Controls.Add(paramPanel);
            this.Controls.Add(cliPanel);
            this.Controls.Add(topPanel);

            // 6. 系统托盘
            trayMenu = new ContextMenuStrip();
            trayItemToggle = new ToolStripMenuItem("启动连接", null, (s, e) => ToggleConnection());
            var trayItemShow = new ToolStripMenuItem("显示主窗口", null, (s, e) => RestoreFromTray());
            var trayItemExit = new ToolStripMenuItem("退出", null, (s, e) => ExitApplication());
            trayMenu.Items.AddRange(new ToolStripItem[] { trayItemToggle, new ToolStripSeparator(), trayItemShow, trayItemExit });

            notifyIcon = new NotifyIcon {
                Text = "GOST 转发连接器",
                Icon = SystemIcons.Application,
                ContextMenuStrip = trayMenu,
                Visible = true
            };
            notifyIcon.DoubleClick += (s, e) => RestoreFromTray();

            // 自动调整宽度适配
            this.Resize += (s, e) =>
            {
                int w = this.ClientSize.Width;
                txtCommandLine.Width = Math.Max(200, w - 200);
                btnParseCli.Left = txtCommandLine.Right + 10;
                btnCopyCli.Left = txtCommandLine.Right + 10;

                txtTargetAddr.Width = Math.Max(120, w - 570);
                txtForwardServer.Width = Math.Max(120, w - 610);
                txtAuthParam.Left = txtForwardServer.Right + 120;
                lblAuth.Left = txtForwardServer.Right + 10;
                txtExtraArgs.Width = Math.Max(200, w - 120);

                btnToggleConnect.Left = w - 180;
            };
        }

        private void SetupEvents()
        {
            // 配置切换
            cboProfiles.SelectedIndexChanged += (s, e) =>
            {
                if (_isUpdatingUi) return;
                var selected = cboProfiles.SelectedItem as GostProfile;
                if (selected != null)
                {
                    _profileManager.Data.ActiveProfileId = selected.Id;
                    BindProfileToForm(selected);
                }
            };

            // 新建配置
            btnNewProfile.Click += (s, e) =>
            {
                string name = PromptDialog.Show("请输入新配置名称:", "新建配置", "新转发配置");
                if (!string.IsNullOrWhiteSpace(name))
                {
                    var p = new GostProfile { Name = name.Trim() };
                    p.CommandLine = GostCliParser.BuildFullCommandLine(p);
                    _profileManager.Data.Profiles.Add(p);
                    _profileManager.Data.ActiveProfileId = p.Id;
                    _profileManager.Save();
                    LoadProfilesToUi();
                }
            };

            // 保存配置
            btnSaveProfile.Click += (s, e) =>
            {
                SaveCurrentFormToProfile();
                _profileManager.Save();
                AppendLog("已成功保存当前配置。", Color.LightGreen);
            };

            // 删除配置
            btnDeleteProfile.Click += (s, e) =>
            {
                if (_profileManager.Data.Profiles.Count <= 1)
                {
                    MessageBox.Show("至少保留一个配置方案。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var current = cboProfiles.SelectedItem as GostProfile;
                if (current != null)
                {
                    var dr = MessageBox.Show(string.Format("确认删除配置 [{0}] 吗？", current.Name), "确认删除", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (dr == DialogResult.Yes)
                    {
                        _profileManager.Data.Profiles.Remove(current);
                        _profileManager.Data.ActiveProfileId = _profileManager.Data.Profiles[0].Id;
                        _profileManager.Save();
                        LoadProfilesToUi();
                    }
                }
            };

            // 解析 CLI 按钮
            btnParseCli.Click += (s, e) =>
            {
                ParseCommandLineInput();
            };

            // 复制 CLI 按钮
            btnCopyCli.Click += (s, e) =>
            {
                string text = txtCommandLine.Text.Trim();
                if (!string.IsNullOrEmpty(text))
                {
                    Clipboard.SetText(text);
                    AppendLog("已复制命令行到剪贴板。", Color.LightSkyBlue);
                }
            };

            // 参数表单改变时同步组装命令行
            EventHandler formFieldChanged = (s, e) =>
            {
                if (_isUpdatingUi) return;
                SyncFormToCommandLineText();
            };

            cboListenProto.TextChanged += formFieldChanged;
            txtListenAddr.TextChanged += formFieldChanged;
            txtTargetAddr.TextChanged += formFieldChanged;
            cboForwardProto.TextChanged += formFieldChanged;
            txtForwardServer.TextChanged += formFieldChanged;
            txtAuthParam.TextChanged += formFieldChanged;
            txtExtraArgs.TextChanged += formFieldChanged;

            // 启动 / 停止连接
            btnToggleConnect.Click += (s, e) => ToggleConnection();

            // 清空日志
            btnClearLog.Click += (s, e) => rtbLog.Clear();

            // 进程输出事件
            _processManager.OnOutputReceived += (line, isError) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    AppendLog(line, isError ? Color.LightCoral : Color.FromArgb(220, 220, 220));
                }));
            };

            _processManager.OnStatusChanged += (isRunning) =>
            {
                this.BeginInvoke(new Action(() =>
                {
                    UpdateConnectButtonState(isRunning);
                }));
            };

            // 最小化到托盘处理
            this.FormClosing += (s, e) =>
            {
                if (chkMinimizeToTray.Checked && e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                    notifyIcon.ShowBalloonTip(2000, "GOST 转发连接器", "程序已最小化到系统托盘，后台持续转发中。", ToolTipIcon.Info);
                }
                else
                {
                    ExitApplication();
                }
            };
        }

        private void LoadProfilesToUi()
        {
            _isUpdatingUi = true;
            try
            {
                cboProfiles.Items.Clear();
                foreach (var p in _profileManager.Data.Profiles)
                {
                    cboProfiles.Items.Add(p);
                }

                var active = _profileManager.GetActiveProfile();
                if (active != null)
                {
                    cboProfiles.SelectedItem = active;
                    BindProfileToForm(active);
                }
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void BindProfileToForm(GostProfile p)
        {
            _isUpdatingUi = true;
            try
            {
                cboListenProto.Text = p.ListenProtocol ?? "tcp";
                txtListenAddr.Text = p.ListenAddress ?? ":1433";
                txtTargetAddr.Text = p.TargetAddress ?? "";

                cboForwardProto.Text = p.ForwardProtocol ?? "relay+grpc";
                txtForwardServer.Text = p.ForwardServer ?? "";
                txtAuthParam.Text = p.AuthParam ?? "";
                txtExtraArgs.Text = p.ExtraArgs ?? "";

                if (!string.IsNullOrWhiteSpace(p.CommandLine))
                {
                    txtCommandLine.Text = p.CommandLine;
                }
                else
                {
                    txtCommandLine.Text = GostCliParser.BuildFullCommandLine(p);
                }
            }
            finally
            {
                _isUpdatingUi = false;
            }
        }

        private void SyncFormToCommandLineText()
        {
            var temp = new GostProfile
            {
                ListenProtocol = cboListenProto.Text.Trim(),
                ListenAddress = txtListenAddr.Text.Trim(),
                TargetAddress = txtTargetAddr.Text.Trim(),
                ForwardProtocol = cboForwardProto.Text.Trim(),
                ForwardServer = txtForwardServer.Text.Trim(),
                AuthParam = txtAuthParam.Text.Trim(),
                ExtraArgs = txtExtraArgs.Text.Trim()
            };

            string cmd = GostCliParser.BuildFullCommandLine(temp);
            txtCommandLine.Text = cmd;
        }

        private void ParseCommandLineInput()
        {
            string raw = txtCommandLine.Text.Trim();
            if (string.IsNullOrEmpty(raw))
            {
                MessageBox.Show("请先在文本框中输入或粘贴 gost 命令行。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var parsed = GostCliParser.Parse(raw);
            _isUpdatingUi = true;
            try
            {
                cboListenProto.Text = parsed.ListenProtocol;
                txtListenAddr.Text = parsed.ListenAddress;
                txtTargetAddr.Text = parsed.TargetAddress;
                cboForwardProto.Text = parsed.ForwardProtocol;
                txtForwardServer.Text = parsed.ForwardServer;
                txtAuthParam.Text = parsed.AuthParam;
                txtExtraArgs.Text = parsed.ExtraArgs;
            }
            finally
            {
                _isUpdatingUi = false;
            }

            AppendLog(string.Format("命令行解析成功: 监听 {0}://{1}/{2} -> 转发 {3}://{4}",
                parsed.ListenProtocol, parsed.ListenAddress, parsed.TargetAddress,
                parsed.ForwardProtocol, parsed.ForwardServer), Color.LightCyan);
        }

        private void SaveCurrentFormToProfile()
        {
            var cur = cboProfiles.SelectedItem as GostProfile;
            if (cur == null) return;

            cur.ListenProtocol = cboListenProto.Text.Trim();
            cur.ListenAddress = txtListenAddr.Text.Trim();
            cur.TargetAddress = txtTargetAddr.Text.Trim();
            cur.ForwardProtocol = cboForwardProto.Text.Trim();
            cur.ForwardServer = txtForwardServer.Text.Trim();
            cur.AuthParam = txtAuthParam.Text.Trim();
            cur.ExtraArgs = txtExtraArgs.Text.Trim();
            cur.CommandLine = txtCommandLine.Text.Trim();
        }

        private void ToggleConnection()
        {
            if (_processManager.IsRunning)
            {
                AppendLog("正在断开 gost 连接...", Color.Orange);
                _processManager.Stop();
            }
            else
            {
                SaveCurrentFormToProfile();
                _profileManager.Save();

                string argsToRun = "";
                if (chkUseRawCli.Checked)
                {
                    string raw = txtCommandLine.Text.Trim();
                    var tokens = GostCliParser.Tokenize(raw);
                    int start = 0;
                    if (tokens.Count > 0 && (tokens[0].Equals("gost", StringComparison.OrdinalIgnoreCase) || tokens[0].EndsWith("gost.exe", StringComparison.OrdinalIgnoreCase)))
                    {
                        start = 1;
                    }
                    if (tokens.Count > start)
                    {
                        argsToRun = string.Join(" ", tokens.GetRange(start, tokens.Count - start).ToArray());
                    }
                }
                else
                {
                    var cur = cboProfiles.SelectedItem as GostProfile;
                    argsToRun = GostCliParser.BuildArguments(cur);
                }

                if (string.IsNullOrWhiteSpace(argsToRun))
                {
                    MessageBox.Show("参数不能为空，请配置或输入命令行后重试。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                AppendLog("==================================================", Color.Gray);
                AppendLog("正在启动: gost " + argsToRun, Color.Yellow);

                string error;
                if (!_processManager.Start(argsToRun, out error))
                {
                    AppendLog("启动失败: " + error, Color.Red);
                    MessageBox.Show("启动失败: " + error, "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else
                {
                    AppendLog("gost 进程已在后台成功启动。", Color.LightGreen);
                }
            }
        }

        private void UpdateConnectButtonState(bool isRunning)
        {
            if (isRunning)
            {
                btnToggleConnect.Text = "🛑 断开连接";
                btnToggleConnect.BackColor = Color.FromArgb(196, 43, 28); // 红色
                lblStatusDot.ForeColor = Color.FromArgb(16, 124, 65); // 绿色圆点
                lblStatusText.Text = "运行中";
                lblStatusText.ForeColor = Color.FromArgb(16, 124, 65);
                trayItemToggle.Text = "断开连接";
                notifyIcon.Text = "GOST 转发连接器 - 运行中";
            }
            else
            {
                btnToggleConnect.Text = "🚀 启动连接";
                btnToggleConnect.BackColor = Color.FromArgb(16, 124, 65); // 绿色
                lblStatusDot.ForeColor = Color.Gray;
                lblStatusText.Text = "未连接";
                lblStatusText.ForeColor = Color.Black;
                trayItemToggle.Text = "启动连接";
                notifyIcon.Text = "GOST 转发连接器 - 未连接";
            }
        }

        private void AppendLog(string message, Color color)
        {
            if (rtbLog.IsDisposed) return;

            string time = DateTime.Now.ToString("HH:mm:ss");
            string line = string.Format("[{0}] {1}\r\n", time, message);

            rtbLog.SelectionStart = rtbLog.TextLength;
            rtbLog.SelectionLength = 0;
            rtbLog.SelectionColor = color;
            rtbLog.AppendText(line);
            rtbLog.SelectionColor = rtbLog.ForeColor;

            if (chkAutoScroll.Checked)
            {
                rtbLog.ScrollToCaret();
            }
        }

        private void RestoreFromTray()
        {
            this.Show();
            this.WindowState = FormWindowState.Normal;
            this.BringToFront();
            this.Activate();
        }

        private void ExitApplication()
        {
            notifyIcon.Visible = false;
            _processManager.Dispose();
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (_processManager != null) _processManager.Dispose();
                if (notifyIcon != null) notifyIcon.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// 轻量输入提示框
    /// </summary>
    public static class PromptDialog
    {
        public static string Show(string text, string caption, string defaultValue = "")
        {
            Form prompt = new Form()
            {
                Width = 360,
                Height = 160,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                Text = caption,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false
            };
            Label textLabel = new Label() { Left = 20, Top = 15, Text = text, AutoSize = true };
            TextBox textBox = new TextBox() { Left = 20, Top = 40, Width = 300, Text = defaultValue };
            Button confirmation = new Button() { Text = "确定", Left = 160, Width = 75, Top = 75, DialogResult = DialogResult.OK };
            Button cancel = new Button() { Text = "取消", Left = 245, Width = 75, Top = 75, DialogResult = DialogResult.Cancel };

            confirmation.Click += (sender, e) => { prompt.Close(); };
            cancel.Click += (sender, e) => { prompt.Close(); };

            prompt.Controls.Add(textBox);
            prompt.Controls.Add(confirmation);
            prompt.Controls.Add(cancel);
            prompt.Controls.Add(textLabel);
            prompt.AcceptButton = confirmation;
            prompt.CancelButton = cancel;

            return prompt.ShowDialog() == DialogResult.OK ? textBox.Text : "";
        }
    }
}
