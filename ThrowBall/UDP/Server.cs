using System;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using ThrowBall.Models;

namespace ThrowBall.UDP
{
    public class Server : ProtocolBase
    {
        private int _port;

        private ConcurrentQueue<UdpPacket> _incomingQueue = new ConcurrentQueue<UdpPacket>();
        private ConcurrentQueue<UdpPacket> _pendingQueue = new ConcurrentQueue<UdpPacket>();
        private UdpClient _udpServer;

        private Thread sendThread;

        private bool _isConnected;

        private object lockObj = new object();

        public bool IsConnected
        {
            get
            {
                return IsConnected;
            }
            set
            {
                lock (lockObj)
                {
                    _isConnected = value;
                }
            }
        }

        public Action OnConnected;
        public Action<UdpPacket> OnMessageReceived;
        public Action OnDisconnected;

        public Server(int port)
        {
            _port = port;
            _udpServer = new UdpClient(_port);
        }


        public bool Start()
        {
            try
            {
                receiveThread = new Thread(() =>
                {
                    UdpHelpers.ReceiveFrom(_udpServer, _incomingQueue, IsConnected);
                });
                receiveThread.IsBackground = true;
                receiveThread.Priority = ThreadPriority.AboveNormal;
                receiveThread.Start();

                sendThread = new Thread(() =>
                {
                    UdpHelpers.SendTo(_udpServer, _pendingQueue, IsConnected);
                });
                sendThread.Priority = ThreadPriority.AboveNormal;
                sendThread.IsBackground = true;
                sendThread.Start();

                return true;
            }
            catch (SocketException se)
            {
                return false;
            }
        }

        public bool Stop()
        {
            IsConnected = false;
            _incomingQueue.Enqueue(new UdpPacket(Meta.Disconnect, default));
            return true;
        }


        public void Broadcast(UdpPacket udpPacket)
        {
            throw new NotImplementedException();
        }

        public void Send(byte[] data, IPEndPoint iPEndPoint)
        {
            try {
                _udpServer?.Send(data, data.Length, iPEndPoint);
            } catch (SocketException se) {
                
            }
        }
        public override bool ProcessNextMessage()
        {
            if (!_incomingQueue.TryDequeue(out UdpPacket udpPacket))
            {
                return false;
            }

            switch (udpPacket.MetaData)
            {
                case Meta.Connect:
                    OnConnected?.Invoke();
                    break;
                case Meta.Message:
                    OnMessageReceived?.Invoke(udpPacket);
                    break;
                case Meta.Disconnect:
                    OnDisconnected?.Invoke();
                    break;
            }
            return true;

        }
    }
}