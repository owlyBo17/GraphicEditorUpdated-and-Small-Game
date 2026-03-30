using System;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RPSArena.Core.Networking
{
    public enum MessageType
    {
        Connect,
        Disconnect,
        JoinGame,
        LeaveGame,
        MakeChoice,
        ChatMessage,
        GameState,
        Error
    }
    
    public class GameMessage
    {
        [JsonPropertyName("type")]
        public MessageType Type { get; set; }
        
        [JsonPropertyName("senderId")]
        public string SenderId { get; set; }
        
        [JsonPropertyName("sessionId")]
        public string SessionId { get; set; }
        
        [JsonPropertyName("data")]
        public string Data { get; set; }
        
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }
        
        public GameMessage()
        {
            Timestamp = DateTime.UtcNow;
        }
        
        public GameMessage(MessageType type, string senderId, string data = null)
        {
            Type = type;
            SenderId = senderId;
            Data = data;
            Timestamp = DateTime.UtcNow;
        }
        
        public string ToJson()
        {
            return JsonSerializer.Serialize(this, new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
        }
        
        public static GameMessage FromJson(string json)
        {
            return JsonSerializer.Deserialize<GameMessage>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        
        public static GameMessage CreateChatMessage(string senderId, string text)
        {
            return new GameMessage(MessageType.ChatMessage, senderId, text);
        }
        
        public static GameMessage CreateChoiceMessage(string senderId, string choice)
        {
            return new GameMessage(MessageType.MakeChoice, senderId, choice);
        }
        
        public static GameMessage CreateJoinMessage(string senderId, string sessionId = null)
        {
            return new GameMessage(MessageType.JoinGame, senderId, sessionId);
        }
    }
}