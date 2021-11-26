using System;
using System.Windows;
using System.Windows.Forms;
using System.Drawing;
using System.Windows.Threading;
using System.IO;
using NetFwTypeLib;

namespace Ring
{
    public partial class MainWindow : Window
    {
        private NotifyIcon notifyIcon = null;

        /// <summary>
        /// 定时返回首页
        /// </summary>
        private DispatcherTimer tmClock = new DispatcherTimer();
        private DispatcherTimer tmRing = new DispatcherTimer();
        private int timer = 1800;
        private int intervalTimer = 120;
        private string txtPath;

        public MainWindow()
        {
            #region firewall
            ////创建firewall管理类的实例
            //INetFwMgr netFwMgr = (INetFwMgr)Activator.CreateInstance(Type.GetTypeFromProgID("HNetCfg.FwMgr"));

            ////创建一个认证程序类的实例
            //INetFwAuthorizedApplication app = (INetFwAuthorizedApplication)Activator.CreateInstance(
            //    Type.GetTypeFromProgID("HNetCfg.FwAuthorizedApplication"));

            ////在例外列表里，程序显示的名称
            //app.Name = "自定义";

            ////程序的决定路径，这里使用程序本身
            //app.ProcessImageFileName = System.Windows.Forms.Application.ExecutablePath;

            ////是否启用该规则
            //app.Enabled = true;

            ////加入到防火墙的管理策略
            //netFwMgr.LocalPolicy.CurrentProfile.AuthorizedApplications.Add(app);
            #endregion

            txtPath = AppDomain.CurrentDomain.BaseDirectory + "setting.txt";
            InitializeComponent();
            
            InitialWindow();

            #region click event
            btnOK.Click += BtnOK_Click;
            btnClock.Click += BtnClock_Click;
            btnReset.Click += BtnReset_Click;
            btnWait.Click += BtnWait_Click;
            btnConfirm1.Click += BtnConfirm1_Click;
            btnConfirm2.Click += BtnConfirm2_Click;
            #endregion
        }

        #region 初始化
        private void InitialWindow()
        {
            System.Windows.Size OutTaskBarSize = new System.Windows.Size(SystemInformation.WorkingArea.Width, SystemInformation.WorkingArea.Height);

            System.Windows.Size ScreenSize = new System.Windows.Size(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
            Left = 1920 - this.Width;
            Top = 1080 - (ScreenSize.Height - OutTaskBarSize.Height) - this.Height;
            InitialTray();
        }

        private void InitialTray()
        {
            //隐藏主窗体
            this.Visibility = Visibility.Hidden;

            //设置托盘的各个属性
            notifyIcon = new NotifyIcon();
            notifyIcon.BalloonTipText = "Ring...";
            notifyIcon.Text = "Ring";
            notifyIcon.Icon = new Icon(@"E:\data\Mine\Ring\Ring\clock.ico");
            notifyIcon.Visible = true;
            notifyIcon.ShowBalloonTip(1000);

            //设置菜单项
            MenuItem[] children;

            //退出菜单项
            MenuItem exit = new MenuItem("exit");
            exit.Click += new EventHandler(exit_Click);

            if (File.Exists(txtPath))
            {
                #region menu
                MenuItem setting1 = new MenuItem("time");
                MenuItem setting2 = new MenuItem("sentence");
                //MenuItem setting3 = new MenuItem("waiting...");
                MenuItem setting = new MenuItem("setting", new MenuItem[] { setting1, setting2/*, setting3*/ });
                setting1.Click += new EventHandler(setting1_click);
                setting2.Click += new EventHandler(setting2_click);

                //帮助选项
                //MenuItem help = new MenuItem("help");
                //help.Click += new EventHandler(Help_Click);

                //关于选项
                //MenuItem about = new MenuItem("about");

                //关联托盘控件
                children = new MenuItem[] { setting, exit };
                #endregion

                string[] s = File.ReadAllLines(txtPath);
                string[] s0 = s[0].Split(',');
                timer = int.Parse(s0[0]);
                intervalTimer = int.Parse(s0[1]);
                labR.Content = s[1];
            }
            else
            {
                //关联托盘控件
                children = new MenuItem[] { exit };
            }

            notifyIcon.ContextMenu = new ContextMenu(children);

            notifyIcon.Click += new EventHandler(NotifyIcon_Click);

            InitialTimer(timer);
        }

        private void SwitchGrid(System.Windows.Controls.Grid show, System.Windows.Controls.Grid hide1, System.Windows.Controls.Grid hide2, System.Windows.Controls.Grid hide3)
        {
            this.Visibility = Visibility.Visible;
            this.Activate();
            show.Visibility = Visibility.Visible;
            hide1.Visibility = Visibility.Collapsed;
            hide2.Visibility = Visibility.Collapsed;
            hide3.Visibility = Visibility.Collapsed;
        }

        private void InitialTimer(int timer)
        {
            if (tmRing.IsEnabled)
            {
                tmRing.Stop();
            }
            tmRing.Interval = new TimeSpan(0, 0, timer);
            tmRing.Tick += (object obj, EventArgs ea) =>
            {
                this.Visibility = Visibility.Visible;
                this.Activate();
                ringing.Visibility = Visibility.Visible;
            };
            tmRing.Start();
            InitialClock(timer);
        }

        private void InitialClock(int timer)
        {
            if (tmClock.IsEnabled)
            {
                tmClock.Stop();
            }
            int clock = timer;
            int min, sec;
            tmClock.Interval = new TimeSpan(0, 0, 1);
            tmClock.Tick += (object obj, EventArgs ea) =>
            {
                clock--;
                min = clock / 60;
                sec = clock % 60;
                tbClock.Text = min.ToString("00") + ":" + sec.ToString("00");
            };
            tmClock.Start();
        }
        #endregion

        #region 按钮
        private void BtnOK_Click(object sender, RoutedEventArgs e)
        {
            InitialTimer(timer);
            this.Visibility = Visibility.Hidden;
        }

        private void BtnClock_Click(object sender, RoutedEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
        }

        private void BtnReset_Click(object sender, RoutedEventArgs e)
        {
            if (System.Windows.MessageBox.Show("sure to reset?",
                                                  "Ring...",
                                                   MessageBoxButton.YesNo,
                                                   MessageBoxImage.Question,
                                                   MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                InitialTimer(timer);
                this.Visibility = Visibility.Hidden;
            }
            else
            {
                this.Visibility = Visibility.Hidden;
            }
        }

        private void BtnWait_Click(object sender, RoutedEventArgs e)
        {
            InitialTimer(intervalTimer);
            this.Visibility = Visibility.Hidden;
        }

        private void BtnConfirm1_Click(object sender, RoutedEventArgs e)
        {
            timer = int.Parse(tb1.Text);
            File.WriteAllText(txtPath, tb1.Text + "\r\n" + labR.Content);
            InitialTimer(timer);
            this.Visibility = Visibility.Hidden;
        }

        private void BtnConfirm2_Click(object sender, RoutedEventArgs e)
        {
            labR.Content = tb2.Text;
            File.WriteAllText(txtPath, timer.ToString() + "\r\n" + labR.Content);
            this.Visibility = Visibility.Hidden;
        }
        #endregion

        #region 菜单栏点击
        //左键
        private void NotifyIcon_Click(object sender, EventArgs e)
        {
            SwitchGrid(Clock, setting1, setting2, ringing);
        }

        //右键
        private void setting1_click(object sender, EventArgs e)
        {
            SwitchGrid(setting1, ringing, setting2, Clock);
        }

        private void setting2_click(object sender, EventArgs e)
        {
            SwitchGrid(setting2, ringing, setting1, Clock);

        }

        /// <summary>
        /// 退出选项
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void exit_Click(object sender, EventArgs e)
        {
            if (System.Windows.MessageBox.Show("sure to exit?",
                                               "Ring...",
                                                MessageBoxButton.YesNo,
                                                MessageBoxImage.Question,
                                                MessageBoxResult.No) == MessageBoxResult.Yes)
            {
                notifyIcon.Visible = false;
                System.Windows.Application.Current.Shutdown();
            }
        }
        #endregion

    }
}