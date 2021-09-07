using System;
using System.Text;
using System.Collections.Concurrent;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using ThrowBall.Models;


namespace ThrowBall.UDP
{
    public class Client : ProtocolBase
    {
        private ConcurrentQueue<UdpPacket> _incomingQueue = new ConcurrentQueue<UdpPacket>();
        private ConcurrentQueue<UdpPacket> _pendingQueue = new ConcurrentQueue<UdpPacket>();
        private Thread sendThread;
        
        private UdpClient _udpClient;
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

        public Client() {
            _udpClient = new UdpClient();
        }

        public Action OnConnected;
        public Action<UdpPacket> OnMessageReceived;
        public Action OnDisconnected;


        public bool Connect(string ip, int port) {
            try {
                _udpClient?.Connect(ip, port);
                StartThreads();
                return true;
            } catch (SocketException se) {
                return false;
            }
        }
        

        private void StartThreads() {
            receiveThread = new Thread(() => {
                UdpHelpers.ReceiveFrom(_udpClient, _incomingQueue, IsConnected);
            });
            receiveThread.IsBackground = true;
            receiveThread.Priority = ThreadPriority.BelowNormal;
            receiveThread.Start();

            sendThread = new Thread(() => {
                UdpHelpers.SendTo(_udpClient, _pendingQueue, IsConnected);
            });
            sendThread.IsBackground = true;
            sendThread.Priority = ThreadPriority.AboveNormal;
            sendThread.Start();
        }


        public bool SendMessage(byte[] data) {
            var newSize = _pendingQueue.Count + 1;
            if (newSize > maxQueueSize) {
                return false;
            }
            _pendingQueue.Enqueue(new UdpPacket(Meta.Message, data));
            return false;
        }


        public void Disconnect() {
            _udpClient.Close();
            _incomingQueue.Enqueue(new UdpPacket(Meta.Disconnect, default));
        }


        public override bool ProcessNextMessage()
        {
            if (!_incomingQueue.TryDequeue(out UdpPacket udpPacket)) {
                return false;
            }

            switch(udpPacket.MetaData) {
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