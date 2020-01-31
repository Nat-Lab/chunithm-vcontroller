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
        public MainWindow()
        {
            InitializeComponent();
        }

        private bool drag_locked = false;
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int GWL_EXSTYLE = -20;

        private TouchPad[] touchpads;

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
            touchpads = new TouchPad[] { Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11, Key12, Key13, Key14, Key15, Key16 };
            Render();

            // make the window unforcusable
            DenyFocus();
        }

        

        private void Render()
        {
            double keyw = 120;
            double keyh = 250;
            double airh = 300;
            double.TryParse(KeyWidth.Text, out keyw);
            double.TryParse(KeyHeight.Text, out keyh);
            double.TryParse(AirHeight.Text, out airh);

            Air.Margin = new Thickness(0, 0, 0, keyh + 35);

            double left_offset = 0;

            foreach (TouchPad t in touchpads)
            {
                t.Height = keyh;
                t.Width = keyw;
                t.Margin = new Thickness(left_offset, airh, 0, 35);
                left_offset += keyw;
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
            System.Windows.Application.Current.Shutdown();
        }

        private void DoApply(object sender, RoutedEventArgs e)
        {
            Render();
        }

        private void SetAllowMouse(object sender, RoutedEventArgs e)
        {
            Air.allowMouse = true;
            foreach (TouchPad t in touchpads) t.allowMouse = true;
        }

        private void UnsetAllowMouse(object sender, RoutedEventArgs e)
        {
            Air.allowMouse = false;
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
