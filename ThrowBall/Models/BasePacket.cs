using System;
using System.Text.Json;

namespace ThrowBall.Models
{

    public class BasePacket
    {
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

        public BasePacket(Meta meta, byte[] message)
        {
            Message = message;
            MetaData = meta;
            if (message != default)
            {
                Size = message.Length;
            }
        }

        public BasePacket() { }


    }

    ///Meta label which would allow mark a message and help server to decide whether to decode the message or not
    public enum Meta
    {
        Connect = 0,
        Disconnect = 1,
        Message = 2
    }
}