using System;
using System.Text.Json;

namespace ThrowBall.Models
{
    
    /// <summary>
    /// General implementation of all messages that are going between client and server
    /// </summary>
    public class Packet
    {
        public Guid Id { get; set; }
        public Meta MetaData { get; set; }
        public byte[] Message { get; set; }

        private int _size = 0;
        public int Size
        {
            get
            {
                return _size;
            }

            set
            {
                if (value >= 0)
                {
                    _size = value;
                }
                else
                {
                    //TODO: log
                }
            }
        }

        public Packet(Guid id, Meta meta, byte[] message)
        {
            Id = id;
            Message = message;
            MetaData = meta;
            if (message != default)
            {
                Size = message.Length;
            }
        }

        public string ToJson()
        {
            return JsonSerializer.Serialize(this);
        }

        public static Packet FromJson(string message)
        {
            return JsonSerializer.Deserialize<Packet>(message);
        } 
    }

    ///Meta label which would allow mark a message and help server to decide whether to decode the message or not
    public enum Meta
    {
        Connect = 0,
        Disconnect = 1,
        Message = 2
    }
}