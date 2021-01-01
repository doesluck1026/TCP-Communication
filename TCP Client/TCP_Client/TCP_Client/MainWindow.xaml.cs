using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TCP_Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Main main;
        private Timer UIUpdateTimer;
        private int UiUpdateFrequency = 30; /// Hz
        private int UiUpdatePeriod;             // milliseconds
        public MainWindow()
        {
            InitializeComponent();
        }

        private void btn_Connect_Click(object sender, RoutedEventArgs e)
        {
            main = new Main(txt_ServerIP.Text);
            main.StartCommunicationThread();
            StartUpdateTimer();
        }
        private void UpdateUI()
        {
            if (main == null)
                return;
            Dispatcher.Invoke(() =>
            {
                progressBar.Value = main.TargetPosition;
                txt_ServerIP.Text = main.ServerIP;
                main.LedColor = Color.FromRgb((byte)sld_Red.Value, (byte)sld_Green.Value, (byte)sld_Blue.Value);
                main.ClientMessage = txt_ClientMessage.Text;
                if (main.IsConnectedToServer)
                    lbl_ConnectionStatus.Background = Brushes.Lime;
                else
                    lbl_ConnectionStatus.Background = Brushes.Red;
            });
        }
        private void StartUpdateTimer()
        {
            UiUpdatePeriod = (int)(1000.0 / UiUpdateFrequency);
            UIUpdateTimer = new Timer(Timer_Tick, null, 0, UiUpdatePeriod);
        }
        private void StopUpdateTimer()
        {
            if (UIUpdateTimer != null)
            {
                UIUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);
                UIUpdateTimer.Dispose();
                UIUpdateTimer = null;
            }
        }
        private void Timer_Tick(object state)
        {
            if (UIUpdateTimer == null)
                return;
            UIUpdateTimer.Change(Timeout.Infinite, Timeout.Infinite);       /// to avoid multiple events
            var watch = Stopwatch.StartNew();
            UpdateUI();
            if (UIUpdateTimer == null)
                return;
            UIUpdateTimer.Change((int)Math.Max(0, UiUpdatePeriod - watch.ElapsedMilliseconds), UiUpdatePeriod);
        }
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            StopUpdateTimer();
            if (main != null)
                main.StopCommunication();
            Environment.Exit(0);
        }
    }
}
