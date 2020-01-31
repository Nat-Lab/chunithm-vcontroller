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

        
        private const int WS_EX_NOACTIVATE = 0x08000000;
        private const int GWL_EXSTYLE = -20;

        private TouchPad[] touchpads;

        [DllImport("user32", SetLastError = true)]
        private extern static int GetWindowLong(IntPtr hwnd, int nIndex);

        [DllImport("user32", SetLastError = true)]
        private extern static int SetWindowLong(IntPtr hwnd, int nIndex, int dwNewValue);

        private void OnLoad(object sender, RoutedEventArgs e)
        {
            // make the window unforcusable
            /*WindowInteropHelper wih = new WindowInteropHelper(this);
            int exstyle = GetWindowLong(wih.Handle, GWL_EXSTYLE);
            exstyle |= WS_EX_NOACTIVATE;
            SetWindowLong(wih.Handle, GWL_EXSTYLE, exstyle);*/

            touchpads = new TouchPad[] { Key1, Key2, Key3, Key4, Key5, Key6, Key7, Key8, Key9, Key10, Key11, Key12, Key13, Key14, Key15, Key16 };
            Render();
        }

        private void Render()
        {
            double keyw = 130;
            double keyh = 250;
            double airh = 300;
            double.TryParse(KeyWidth.Text, out keyw);
            double.TryParse(KeyHeight.Text, out keyh);
            double.TryParse(AirHeight.Text, out airh);

            double toth = keyh + airh;
            double totw = keyw * 16 + 20;

            this.Height = toth + 75;
            this.Width = totw;

            Air.Height = airh;
            Air.Width = totw;

            double left_offset = 0;

            foreach (TouchPad t in touchpads)
            {
                t.Height = keyh;
                t.Width = keyw;
                t.Margin = new Thickness(left_offset, 0, 0, 35);
                left_offset += keyw;
            }
        }

        private void DoApply(object sender, RoutedEventArgs e)
        {
            Render();
        }
    }
}
