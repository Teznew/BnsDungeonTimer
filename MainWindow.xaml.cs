using System.IO;
using Newtonsoft.Json.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;
using Newtonsoft.Json;

namespace BnsDungeonTimer
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isRunning = false;
        private string jsonString = File.ReadAllText("timeInfo.json");
        private List<Stage> stages;

        /// <summary>
        /// 全局热键
        /// </summary>
        /// <param name="e"></param>
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            OutputTextBox.Text = "按 Shift+F1 开始\n按 Shift+F2 重置";
            var helper = new WindowInteropHelper(this);
            var source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);

            // 注册Shift + F1为全局快捷键
            if (!App.RegisterHotKey(helper.Handle, 0, App.MOD_SHIFT, (uint)KeyInterop.VirtualKeyFromKey(Key.F1)))
            {
                MessageBox.Show("无法注册快捷键！");
            }

            // 注册Shift + F2为全局快捷键
            if (!App.RegisterHotKey(helper.Handle, 1, App.MOD_SHIFT, (uint)KeyInterop.VirtualKeyFromKey(Key.F2)))
            {
                MessageBox.Show("无法注册快捷键！");
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            this.Topmost = true;  // 设置窗口置顶
            CommandBinding startTaskBinding = new(CustomCommands.StartCommand, StartTask);
            CommandBinding resetTaskBinding = new(CustomCommands.ResetCommand, ResetTask);
            CommandBindings.Add(startTaskBinding);
            CommandBindings.Add(resetTaskBinding);

            stages = JsonConvert.DeserializeObject<List<Stage>>(jsonString);
        }


        public void StartTask(object sender, EventArgs e)
        {
            // 防止启动多个后台线程
            if (isRunning) return;
            isRunning = true;

            Thread workerThread = new Thread(new ThreadStart(WorkerMethod))
            {
                IsBackground = true
            };
            this.Dispatcher.Invoke(() =>
            {
                OutputTextBox.Text = "";
            });
            workerThread.Start();
        }

        private void ResetTask(object sender, RoutedEventArgs e)
        {
            // 停止后台线程
            isRunning = false;
            this.Dispatcher.Invoke(() =>
            {
                OutputTextBox.Text = "";
            });
        }

        #region
        //private void WorkerMethod()
        //{
        //    DateTime startTime = DateTime.Now;

        //    while (isRunning)
        //    {
        //        foreach (JObject obj in jsonArray)
        //        {
        //            string? stage_name = obj["stage_name"]?.ToString();
        //            string? details = obj["details"]?.ToString();
        //            if (!string.IsNullOrEmpty(stage_name) && !string.IsNullOrEmpty(details))
        //            {
        //                UpdateOutputTextBox(stage_name);
        //                JArray detailsArray = JArray.Parse(details);
        //                foreach (JObject detail in detailsArray)
        //                {
        //                    string? titleStr = detail["title"]?.ToString();
        //                    string? timeStr = detail["time"]?.ToString();
        //                    if (!string.IsNullOrEmpty(titleStr) && !string.IsNullOrEmpty(timeStr))
        //                    {
        //                        //TimeSpan timeKnot = TimeSpan.ParseExact(timeStr, "mm:ss", CultureInfo.InvariantCulture);
        //                        string[] parts = timeStr.Split(':');
        //                        int minutes = int.Parse(parts[0]);
        //                        int seconds = int.Parse(parts[1]);
        //                        TimeSpan timeKnot = new TimeSpan(0, minutes, seconds);
        //                        while (true)
        //                        {
        //                            if (!isRunning) break; // 检查是否停止
        //                            TimeSpan diff = DateTime.Now - startTime;
        //                            if (timeKnot > diff)
        //                            {
        //                                UpdateOutputTextBox($"倒数: {(int)timeKnot.TotalSeconds - (int)diff.TotalSeconds}s {titleStr}");
        //                                Thread.Sleep(1000);
        //                            }
        //                            else { break; }
        //                        }
        //                    }
        //                    if (!isRunning) break; // 检查是否停止
        //                }
        //                if (!isRunning) break; // 检查是否停止
        //            }
        //            if (!isRunning) break; // 检查是否停止
        //        }
        //    }
        //}
        #endregion

        private async void WorkerMethod()
        {
            DateTime startTime = DateTime.Now;

            foreach (var stage in stages)
            {
                if (!isRunning) { break; }
                if (!string.IsNullOrEmpty(stage.StageName) && stage.Details != null)
                {
                    if (!isRunning) { break; }

                    foreach (var detail in stage.Details)
                    {
                        if (!isRunning) { break; }
                        TimeSpan timestamp = TimeSpan.Parse("00:" + detail.TimestampStr); // 将detail中的时间点转换为TimeSpan
                        TimeSpan timeSpan;
                        do
                        {
                            if (!isRunning) { break; }
                            // 计算当前时间与程序开始运行时间的差值
                            timeSpan = DateTime.Now - startTime;

                            // 如果时间差小于detail中的timestamp，则等待
                            if (timeSpan < timestamp)
                            {
                                await Task.Delay(1000); // 每次等待1秒再检查
                                UpdateOutputTextBox($"{stage.StageName}\n倒数: {(int)(timestamp.TotalSeconds - timeSpan.TotalSeconds)}s {detail.Title}");
                            }
                        }
                        while (timeSpan < timestamp);
                    }
                }
            }
            this.Dispatcher.Invoke(() =>
            {
                OutputTextBox.Text = "按 Shift+F1 开始\n按 Shift+F2 重置";
            });
        }

        private void UpdateOutputTextBox(string text)
        {
            this.Dispatcher.Invoke(() =>
            {
                //OutputTextBox.AppendText($"{text}\n");
                //OutputTextBox.ScrollToEnd();
                OutputTextBox.Text = text;
                //OutputTextBox.ScrollToEnd();
            });
        }

        private void MainWindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            // 调用 DragMove 方法来移动窗口
            this.DragMove();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == App.WM_HOTKEY)
            {
                // 判断触发的是哪个热键
                if ((int)wParam == 0) // 0是我们为Shift + F1设置的ID
                {
                    StartTask(this, new EventArgs());
                    handled = true; // 表明该消息已处理
                }
                else if ((int)wParam == 1) // 1是我们为Shift + F2设置的ID
                {
                    ResetTask(this, new RoutedEventArgs());
                    handled = true; // 表明该消息已处理
                }
            }

            return IntPtr.Zero;
        }

        protected override void OnClosed(EventArgs e)
        {
            var helper = new WindowInteropHelper(this);
            App.UnregisterHotKey(helper.Handle, 0); // 使用相同的ID注销
            App.UnregisterHotKey(helper.Handle, 1); // 使用相同的ID注销
            base.OnClosed(e);
        }
    }

    public static class CustomCommands
    {
        public static readonly RoutedCommand StartCommand = new RoutedCommand();
        public static readonly RoutedCommand ResetCommand = new RoutedCommand();
    }
}