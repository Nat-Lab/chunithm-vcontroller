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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChuniVController
{
    public partial class TouchPad : UserControl
    {

        // use old "keybd_event" so we can controll keyup/keydown ourself
        [DllImport("user32.dll", SetLastError = true)]
        private static extern void keybd_event(byte key, byte scan, int flags, int info);

        private const int KEYEVENTF_EXTENDEDKEY = 0x0001; // keydown
        private const int KEYEVENTF_KEYUP = 0x0002; // keyup

        public string idleColor { get; set; }
        public string activeColor { get; set; }
        public byte keyCode { get; set; }

        public bool allowMouse { get; set; } = false;
        public TouchPad()
        {
            InitializeComponent();
        }

        protected void Press()
        {
            keybd_event(keyCode, 0, KEYEVENTF_EXTENDEDKEY, 0);
            LedStrip.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(activeColor));
        }

        protected void Release()
        {
            keybd_event(keyCode, 0, KEYEVENTF_KEYUP, 0);
            LedStrip.Fill = (SolidColorBrush)(new BrushConverter().ConvertFrom(idleColor));
        }

        protected override void OnMouseEnter(MouseEventArgs e)
        {
            if (allowMouse) Press();
            base.OnMouseEnter(e);
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            if (allowMouse) Release();
            base.OnMouseLeave(e);
        }

        // TouchEnter: key pressed
        protected override void OnTouchEnter(TouchEventArgs e)
        {
            Press();
            base.OnTouchEnter(e);
        }

        // TouchLeave: released
        protected override void OnTouchLeave(TouchEventArgs e)
        {
            Release();
            base.OnTouchLeave(e);
        }
    }
}
