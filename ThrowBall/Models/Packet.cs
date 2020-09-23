using System;
using Newtonsoft.Json;

namespace ThrowBall.Models
{
    public class Packet
    {
        public Guid Id { get; set; }
        public Meta MetaData { get; set; }
        public byte[] Message { get; set; }

        public Packet(Guid id, Meta meta, byte[] message)
        {
            Id = id;
            Message = message;
            MetaData = meta;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Packet FromJson(string message)
        {
            return JsonConvert.DeserializeObject<Packet>(message);
        } 
    }

    public enum Meta
    {
        Connect,
        Disconnect,
        Message,
    }
}