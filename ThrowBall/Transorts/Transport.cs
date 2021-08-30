

using System.Collections.Concurrent;
using ThrowBall.Models;

namespace ThrowBall.Transports
{
    public abstract class Transport
    {
        public ConcurrentQueue<Packet> InQueue {get; set;}
        public ConcurrentQueue<Packet> OutQueue { get; set; }
    }
}