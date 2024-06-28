using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

            var clickThroughItem = trayIcon.ContextMenuStrip.Items.Add("鼠标穿透");
            clickThroughItem.Click += (sender, args) =>
            {
                isClickThroughEnabled = !isClickThroughEnabled;
                clickThroughItem.Text = isClickThroughEnabled ? "取消鼠标穿透" : "鼠标穿透";
                ToggleClickThrough(isClickThroughEnabled);
            };

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
            IntPtr hwnd = new WindowInteropHelper(MainWindow).Handle;
            uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);

            if (enable)
            {
                SetTranMouseWind(hwnd);
            }
            else
            {
                ReSetNormalWind(hwnd);
            }
            // 强制更新窗口
            MainWindow.Hide();
            MainWindow.Show();
        }

        #region 设置窗口属性使得鼠标事件穿透到下层窗体
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int GWL_EXSTYLE = -20;

        private const int WS_EX_LAYERED = 0x00080000;//取消鼠标穿透的

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        private static extern uint SetWindowLong(IntPtr hwnd, int nIndex, uint dwNewLong);

        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        private static extern uint GetWindowLong(IntPtr hwnd, int nIndex);
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

        public static void SetTranMouseWind(IntPtr hwnd)
        {
            try
            {
                uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
                SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT);
            }
            catch { }

        }
        public static void ReSetNormalWind(IntPtr hwnd)
        {
            try
            {
                uint extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE | WS_EX_LAYERED);
                SetWindowLong(hwnd, GWL_EXSTYLE, WS_EX_LAYERED);
            }
            catch { }

        }
    }
}
