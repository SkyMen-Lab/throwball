using System;
using System.Text;
using System.Threading;
using System.Collections.Concurrent;
using ThrowBall.Models;

namespace ThrowBall.UDP
{
    public class Server : ProtocolBase
    {
        private int _port;

        private ConcurrentQueue<UdpPacket> _pendingQueue = new ConcurrentQueue<UdpPacket>();

        public Server(int port) {
            _port = port;
        }

        public override bool ProcessNextMessage()
        {
            throw new NotImplementedException();
        }
    }
}