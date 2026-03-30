using System;
using System.Threading.Tasks;

namespace RPSArena.Core.Networking
{
    public interface IGameClient : IDisposable
    {
        bool IsConnected { get; }
        string ClientId { get; }
        
        event Action<string> OnMessageReceived;
        event Action OnConnected;
        event Action OnDisconnected;
        event Action<Exception> OnError;
        
        Task<bool> ConnectAsync(string host, int port, string clientId = null);
        Task SendMessageAsync(string message);
        Task DisconnectAsync();
    }
}