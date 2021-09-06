using System;
using System.Net;

namespace ThrowBall.Models
{
    public class UdpPacket : BasePacket
    {
        public IPEndPoint IPEndPoint { get; set; }
        public UdpPacket() {

        }
        public UdpPacket(Meta meta, byte[] data, IPEndPoint iPEndPoint = null) : base(meta, data) {
            IPEndPoint = iPEndPoint;
        }
    }
}