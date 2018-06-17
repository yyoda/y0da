using Net;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace Net
{
    public class TcpTransporter : IListener, IConnection, IMessageTransporter
    {
        private Socket _listener;

        public bool IsRunning { get; private set; }

        public bool Start(int port, int count)
        {
            _listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _listener.Bind(new IPEndPoint(IPAddress.Any, port));
            _listener.Listen(count);

            return IsRunning = true;
        }

        public void Stop()
        {
            _listener.Close();
            _listener = null;
            IsRunning= false;
        }

        private Socket _socket;
        public bool IsConnected { get; private set; }

        public bool Connect(string host, int port)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _socket.NoDelay = true;
            _socket.Connect(host, port);
            _socket.SendBufferSize = 0;

            return IsConnected = true;
        }

        public bool Disconnect()
        {
            _socket.Shutdown(SocketShutdown.Both);
            _socket.Close();
            _socket = null;

            return IsConnected = false;
        }

        private PacketQueue _sendQueue = new PacketQueue();
        private PacketQueue _recvQueue = new PacketQueue();

        public bool Send(byte[] data) => _sendQueue.Enqueue(data);

        public bool Receive(ref byte[] buffer) => _recvQueue.Dequeue(ref buffer);

        private EventHandler _handler;

        public void RegisterEventHandler(EventHandler handler) => _handler += handler;
        public void UnregisterEventHandler(EventHandler handler) => _handler -= handler;

        public void AcceptClient()
        {
            if (_listener != null && _listener.Poll(0, SelectMode.SelectRead))
            {
                _socket = _listener.Accept();
                IsConnected = true;
                _handler?.Invoke(new NetEventState(NetEventType.Connect, NetEventResult.Success));
            }
        }

        private Thread _thread;
        private bool _isStarted;
        private bool _threadLoop;

        public bool LaunchThread()
        {
            try
            {
                _thread = new Thread(new ThreadStart(Dispatch));
                _thread.Start();
            }
            catch
            {
                return false;
            }

            _isStarted = true;

            return true;
        }

        public void Dispatch()
        {
            while (_threadLoop)
            {
                AcceptClient();

                if (_socket != null && IsConnected)
                {
                    DispatchSend();

                }
            }
        }

        private const int MaximumTransmissionUnit = 1500;

        public void DispatchSend()
        {
            if (_socket.Poll(0, SelectMode.SelectWrite))
            {
                var buffer = new byte[MaximumTransmissionUnit];
                _sendQueue.Dequeue(ref buffer);
                var sendSize = buffer.Length;

                while (sendSize > 0)
                {
                    _socket.Send(buffer, buffer.Length, SocketFlags.None);
                    _sendQueue.Dequeue(ref buffer);
                    sendSize = buffer.Length;
                }
            }
        }

        public void DispatchReceive()
        {
            while (_socket.Poll(0, SelectMode.SelectRead))
            {
                var buffer = new byte[MaximumTransmissionUnit];
                _socket.Receive(buffer, buffer.Length, SocketFlags.None);
                var recvSize = buffer.Length;

                if (recvSize == 0)
                {
                    Disconnect();
                }
                else if (recvSize > 0)
                {
                    _recvQueue.Enqueue(buffer);
                }
            }
        }
    }
}
