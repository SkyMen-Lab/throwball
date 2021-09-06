using System;
using System.Text;
using Newtonsoft.Json;

namespace ThrowBall.Models
{
    /// <summary>
    /// thep0ng.io specific message model which is contained within Packet
    /// </summary>
    public class Message
    {
        public GameEvent ContentType { get; set; }
        public string Content { get; set; }

        public Message(GameEvent @event, string content)
        {
            ContentType = @event;
            Content = content;
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this);
        }

        public static Message FromJson(string message)
        {
            return JsonConvert.DeserializeObject<Message>(message);
        }

        public static string JsonFromBytes(byte[] load)
        {
            return ASCIIEncoding.ASCII.GetString(load);
        }
    }

    ///Contextual labels for game messages    
    public enum GameEvent
    {
        StartGame,
        InitGame,
        UpdateNumberOfPlayers,
        Movement,
        Score,
        FinishGame,
        Error
    }
}