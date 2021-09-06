using System;
using System.Text.Json;

namespace ThrowBall.Models
{

    /// <summary>
    /// General implementation of all messages that are going between TCP client and server
    /// </summary>
    public class TcpPacket : BasePacket
    {
        public Guid Id { get; set; }

        public TcpPacket(Guid guid, Meta meta, byte[] message) : base(meta, message)
        {
            Id = guid;
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static TcpPacket FromJson(string message)
        {
            return JsonSerializer.Deserialize<TcpPacket>(message);
        }
    }
}