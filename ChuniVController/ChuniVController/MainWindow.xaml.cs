using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChuniVController
{
    public partial class MainWindow : Window
    {
        private ChuniIO cio;

        private void handleRecv(ChuniIoMessage message)
        {
            if (message.Source != (byte)ChuniMessageSources.Game || message.Type != (byte)ChuniMessageTypes.LedSet || message.Target >= 16) return;
            touchpads[message.Target].LedStrip.Dispatcher.BeginInvoke(
                (Action) (() => touchpads[15 - message.Target].LedStrip.Fill =
                    new SolidColorBrush(Color.FromRgb(message.LedColorRed, message.LedColorGreen,
                        message.LedColorBlue))));
        }

        public MainWindow()
        {
            InitializeComponent();
            cio = new ChuniIO("127.0.0.1", 24864, handleRecv);
            cio.Start();
        }

        private bool drag_locked = false;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int GWL_EXSTYLE = -20;

        private TouchPad[] touchpads;
        private TouchPad[] irs;

        [DllImport("user32", SetLastError = true)]
        private extern static int GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32", SetLastError = true)]
        private extern static int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewValue);

        private void DenyFocus ()
        {
            WindowInteropHelper wih = new WindowInteropHelper(this);
            int exstyle = GetWindowLong(wih.Handle, GWL_EXSTYLE);
            exstyle |= WS_EX_NOACTIVATE;
            SetWindowLong(wih.Handle, GWL_EXSTYLE, exstyle);
        }

        private void AllowFocus()
        {
            // make the window unforcusable
            WindowInteropHelper wih = new WindowInteropHelper(this);
            int exstyle = GetWindowLong(wih.Handle, GWL_EXSTYLE);
            exstyle &= ~WS_EX_NOACTIVATE;
            SetWindowLong(wih.Handle, GWL_EXSTYLE, exstyle);
        }

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            touchpads = new TouchPad[] { Key0, Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11, Key12, Key13, Key14, Key15 };
            irs = new TouchPad[] { Ir0, Ir1, Ir2, Ir3, Ir4, Ir5 };
            foreach (TouchPad t in touchpads) t.io = cio;
            foreach (TouchPad t in irs) t.io = cio;

            Render();

            // make the window unforcusable
            DenyFocus();
        }

        protected override void OnClosed(EventArgs e)
        {
            cio.Stop();
            base.OnClosed(e);
        }

        private void Render()
        {
            double keyw = 80;
            double keyh = 200;
            double airh = 30;
            double.TryParse(KeyWidth.Text, out keyw);
            double.TryParse(KeyHeight.Text, out keyh);
            double.TryParse(AirHeight.Text, out airh);

            foreach (TouchPad t in irs)
            {
                t.Height = airh;
            }

            foreach (TouchPad t in touchpads)
            {
                t.Height = keyh;
                t.Width = keyw;
            }
        }

        protected override void OnPreviewKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Space) e.Handled = true; // so that checkboxes won't be trigger by the IR sensor simulation
            base.OnPreviewKeyDown(e);
        }

        private void NumericValidation(object sender, TextCompositionEventArgs e)
        {
            int _;
            e.Handled = !int.TryParse(e.Text, out _);
            if (e.Handled && e.Text.Equals(".") && ((TextBox) sender).Text.IndexOf('.') == -1)
            {
                e.Handled = false;
            }
        }

        private void SetAllowFocus(object sender, RoutedEventArgs e)
        {
            AllowFocus();
        }

        private void SetDenyFocus(object sender, RoutedEventArgs e)
        {
            DenyFocus();
        }

        private void DoExit(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }

        private void DoApply(object sender, RoutedEventArgs e)
        {
            Render();
        }

        private void DoCoin(object sender, RoutedEventArgs e)
        {
            ChuniIoMessage msg = new ChuniIoMessage();
            msg.Source = (byte)ChuniMessageSources.Controller;
            msg.Type = (byte)ChuniMessageTypes.CoinInsert;
            cio.Send(msg);
        }

        private void DoTest(object sender, RoutedEventArgs e)
        {
            ChuniIoMessage msg = new ChuniIoMessage();
            msg.Source = (byte)ChuniMessageSources.Controller;
            msg.Type = (byte)ChuniMessageTypes.CabinetTest;
            cio.Send(msg);
        }

        private void DoServiceTest(object sender, RoutedEventArgs e)
        {
            ChuniIoMessage msg = new ChuniIoMessage();
            msg.Source = (byte)ChuniMessageSources.Controller;
            msg.Type = (byte)ChuniMessageTypes.CabinetTest;
            cio.Send(msg);
            msg.Type = (byte)ChuniMessageTypes.CabinetService;
            cio.Send(msg);
        }

        private void DoService(object sender, RoutedEventArgs e)
        {
            ChuniIoMessage msg = new ChuniIoMessage();
            msg.Source = (byte)ChuniMessageSources.Controller;
            msg.Type = (byte)ChuniMessageTypes.CabinetService;
            cio.Send(msg);
        }

        private void SetAllowMouse(object sender, RoutedEventArgs e)
        {
            foreach (TouchPad t in irs) t.allowMouse = true;
            foreach (TouchPad t in touchpads) t.allowMouse = true;
        }

        private void UnsetAllowMouse(object sender, RoutedEventArgs e)
        {
            foreach (TouchPad t in irs) t.allowMouse = false;
            foreach (TouchPad t in touchpads) t.allowMouse = false;
        }

        private void SetLockWindow(object sender, RoutedEventArgs e)
        {
            drag_locked = true;
        }

        private void UnsetLockWindow(object sender, RoutedEventArgs e)
        {
            drag_locked = false;
        }

        private void DoMove(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed && !drag_locked) DragMove();
        }
    }
}
