using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Threading;

namespace ChuniVController
{
    public enum ChuniMessageSources
    {
        Game = 0,
        Controller = 1
    }

    public enum ChuniMessageTypes
    {
        CoinInsert = 0,
        SliderPress = 1,
        SliderRelease = 2,
        LedSet = 3,
        CabinetTest = 4,
        CabinetService = 5,
        IrBlocked = 6,
        IrUnblocked = 7
    }

    public struct ChuniIoMessage
    {
        public byte Source;
        public byte Type;
        public byte Target;
        public byte LedColorRed;
        public byte LedColorGreen;
        public byte LedColorBlue;
    }

    public class ChuniIO
    {
        private readonly UdpClient _client;
        private readonly string _ioServerAddress;
        private readonly int _port;
        private bool _running;
        private byte[] _sendBuffer;
        private readonly IntPtr _sendBufferUnmanaged;
        private readonly RecvCallback _recvCallback;
        private Thread _recvThread;

        private struct RecvContext
        {
            public UdpClient client;
            public RecvCallback callback;
        }

        public delegate void RecvCallback(ChuniIoMessage message);

        public ChuniIO(string ioServerAddress, int port, RecvCallback recvCallback)
        {
            _ioServerAddress = ioServerAddress;
            _port = port;
            _running = false;
            _client = new UdpClient();
            _sendBuffer = new byte[32];
            _sendBufferUnmanaged = Marshal.AllocHGlobal(32);
            
            _recvCallback = recvCallback;
        }

        ~ChuniIO()
        {
            Marshal.FreeHGlobal(_sendBufferUnmanaged);
        }

        private static void RecvThread(object c)
        {
            ChuniIoMessage message = new ChuniIoMessage();
            int sz = Marshal.SizeOf(message);
            IPEndPoint endpoint = new IPEndPoint(IPAddress.Any, 0);
            IntPtr recvBufferPtr;
            recvBufferPtr = Marshal.AllocHGlobal(32);

            RecvContext context = (RecvContext) c;

            try
            {
                while (true)
                {
                    byte[] recvBuffer = context.client.Receive(ref endpoint);
                    if (recvBuffer.Length == 0) break;
                    if (recvBuffer.Length != sz) continue;
                    Marshal.Copy(recvBuffer, 0, recvBufferPtr, sz);
                    message = (ChuniIoMessage)Marshal.PtrToStructure(recvBufferPtr, message.GetType());
                    context.callback(message);
                }
            }
            catch 
            {
                // noting, just exit.
            }
            

            Marshal.FreeHGlobal(recvBufferPtr);
        }

        public bool Start()
        {
            if (_running) return false;
            try
            {
                _client.Connect(_ioServerAddress, _port);
            }
            catch
            {
                return false;
            }

            RecvContext c = new RecvContext();
            c.client = _client;
            c.callback = _recvCallback;

            _recvThread = new Thread(RecvThread);
            _recvThread.Start(c);

            _running = true;
            return true;
        }

        public bool Stop()
        {
            if (!_running) return false;
            try
            {
                _client.Close();
            }
            catch 
            {
                return false;
            }
            return true;
        }

        public void Join()
        {
            _recvThread.Join();
        }

        private void SendCallback(IAsyncResult ar)
        {
            // don't care
        }

        public void Send(ChuniIoMessage message)
        {
            int sz = Marshal.SizeOf(message);
            Marshal.StructureToPtr(message, _sendBufferUnmanaged, false);
            Marshal.Copy(_sendBufferUnmanaged, _sendBuffer, 0, sz);
            _client.BeginSend(_sendBuffer, sz, SendCallback, null);
        }

         

    }
}
