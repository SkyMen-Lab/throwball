using System;
using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using ThrowBall.Models;

namespace ThrowBall.TCP
{
    public class Connection
    {
        private readonly object _statusLock = new();
        
        public Guid Id { get; set; }
        
        public TcpClient Client { get; set; }
        public bool IsOpen { get; set; } = true;
        
        public ConcurrentQueue<Packet> PendingQueue { get; set; }

        public void SetConnectionStatus(bool isOpen)
        {
            lock(_statusLock) {
                IsOpen = isOpen;
            }
        }
    }
}