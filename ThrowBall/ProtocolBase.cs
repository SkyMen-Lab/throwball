using System;
using System.Threading;
using System.Net.Sockets;
using System.Collections.Concurrent;
using ThrowBall.Models;

namespace ThrowBall
{
    public abstract class ProtocolBase
    {

        protected ConcurrentQueue<TcpPacket> incomingQueue;
        protected int maxQueueSize = 5000;
        protected int maxMessageSize = 512;
        protected Thread receiveThread { get; set; }

        public abstract bool ProcessNextMessage();
    }
}
