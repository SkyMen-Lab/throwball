using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;

namespace ThrowBall.Models{
    class Client {
        public ConcurrentQueue<byte[]> SendQueue = new ConcurrentQueue<byte[]>();
        public ManualResetEvent Sync { get; set; }

        public TcpClient TcpClient { get; set; }

        public Thread SendThread { get; set; }
        public Thread ReceiveThread { get; set; }

        public Client(TcpClient tcpClient, Thread sendThread, Thread receiveThread) {
            TcpClient = tcpClient;
            SendThread = sendThread;
            ReceiveThread = receiveThread;
        }
    }
}