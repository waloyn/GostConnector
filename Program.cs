using System;
using System.Threading;
using System.Windows.Forms;
using GostConnector.UI;

namespace GostConnector
{
    static class Program
    {
        private static Mutex _mutex = null;

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // 单实例防重复运行
            const string appName = "Global\\GostConnector_Singleton_Mutex";
            bool createdNew;
            _mutex = new Mutex(true, appName, out createdNew);

            if (!createdNew)
            {
                MessageBox.Show("GOST 转发连接器已在运行中，请检查系统托盘或任务管理器。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 全局异常捕获
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);
            Application.ThreadException += (s, e) =>
            {
                MessageBox.Show("发生未捕获错误:\n" + e.Exception.Message, "程序异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                var ex = e.ExceptionObject as Exception;
                string msg = ex != null ? ex.Message : "未知错误";
                MessageBox.Show("严重错误:\n" + msg, "致命异常", MessageBoxButtons.OK, MessageBoxIcon.Error);
            };

            try
            {
                Application.Run(new MainForm());
            }
            finally
            {
                if (_mutex != null)
                {
                    _mutex.ReleaseMutex();
                    _mutex.Close();
                }
            }
        }
    }
}
