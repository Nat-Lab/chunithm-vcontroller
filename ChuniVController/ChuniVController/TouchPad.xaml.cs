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
        public enum IoType
        {
            IR = 0,
            Slider = 1
        }

        public bool allowMouse { get; set; } = false;
        public bool localColoring { get; set; } = false;
        public Brush activeColor { get; set; }
        public Brush idleColor { get; set; }

        public ChuniIO io { get; set; }
        public IoType ioType { get; set; }
        public byte ioTarget { get; set; }

        private ChuniIoMessage message;
        private int fingers = 0;
        public TouchPad()
        {
            InitializeComponent();
            message = new ChuniIoMessage();
            message.Source = (byte)ChuniMessageSources.Controller;
        }

        private void Press()
        {
            fingers++;
            if (fingers == 1) // send only the first finger
            {
                if (localColoring) LedStrip.Fill = activeColor;
                message.Target = ioTarget;
                message.Type = (byte)(ioType == IoType.Slider ? ChuniMessageTypes.SliderPress : ChuniMessageTypes.IrBlocked);
                io.Send(message);
            }
        }

        private void Release()
        {
            fingers--;
            if (fingers == 0) // send only if no more fingers touched
            {
                if (localColoring) LedStrip.Fill = idleColor;
                message.Target = ioTarget;
                message.Type = (byte)(ioType == IoType.Slider ? ChuniMessageTypes.SliderRelease : ChuniMessageTypes.IrUnblocked);
                io.Send(message);
            }
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
