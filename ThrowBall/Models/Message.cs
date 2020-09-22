using Newtonsoft.Json;

namespace ThrowBall.Models
{
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
    }
    
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