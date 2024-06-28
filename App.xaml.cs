using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;

namespace BnsDungeonTimer
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private NotifyIcon trayIcon;
        private bool isClickThroughEnabled = false;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetupTrayIcon();
        }

        private void SetupTrayIcon()
        {
            trayIcon = new NotifyIcon
            {
                Icon = new System.Drawing.Icon("app.ico"),
                Visible = true,
                ContextMenuStrip = new ContextMenuStrip()
            };

            //var clickThroughItem = trayIcon.ContextMenuStrip.Items.Add("鼠标穿透");
            //clickThroughItem.Click += (sender, args) =>
            //{
            //    isClickThroughEnabled = !isClickThroughEnabled;
            //    clickThroughItem.Text = isClickThroughEnabled ? "取消鼠标穿透" : "鼠标穿透";
            //    ToggleClickThrough(isClickThroughEnabled);
            //};

            var exitItem = trayIcon.ContextMenuStrip.Items.Add("退出");
            exitItem.Click += (sender, args) => Current.Shutdown();

            Current.Exit += (sender, args) => trayIcon.Visible = false;
        }

        /// <summary>
        /// 实现窗口的鼠标穿透功能或取消
        /// </summary>
        /// <param name="enable"></param>
        private void ToggleClickThrough(bool enable)
        {
            var mainWindowPtr = new System.Windows.Interop.WindowInteropHelper(MainWindow).Handle;
            int style = GetWindowLong(mainWindowPtr, GWL_EXSTYLE);

            if (enable)
            {
                // 设置 WS_EX_TRANSPARENT 和 WS_EX_LAYERED 实现鼠标穿透
                SetWindowLong(mainWindowPtr, GWL_EXSTYLE, style | WS_EX_TRANSPARENT | WS_EX_LAYERED);
            }
            else
            {
                // 移除 WS_EX_TRANSPARENT 和 WS_EX_LAYERED 取消鼠标穿透
                SetWindowLong(mainWindowPtr, GWL_EXSTYLE, style & ~(WS_EX_TRANSPARENT | WS_EX_LAYERED));
            }
            //// 强制更新窗口
            //MainWindow.Hide();
            //MainWindow.Show();
            // 强制更新窗口以应用新样式
            SetWindowPos(mainWindowPtr, IntPtr.Zero, 0, 0, 0, 0,
                         SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_FRAMECHANGED);
        }

        #region P/Invoke
        const int GWL_EXSTYLE = -20;
        const int WS_EX_TRANSPARENT = 0x20;
        const int WS_EX_LAYERED = 0x80000;

        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOZORDER = 0x0004;
        const uint SWP_FRAMECHANGED = 0x0020;


        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter,
                                int X, int Y,
                                int cx, int cy,
                                uint uFlags);
        #endregion

        #region 全局快捷键
        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public const uint MOD_ALT = 0x0001;
        public const uint MOD_CONTROL = 0x0002;
        public const uint MOD_SHIFT = 0x0004;
        public const uint WM_HOTKEY = 0x0312; // 热键消息ID
        #endregion
    }
}
