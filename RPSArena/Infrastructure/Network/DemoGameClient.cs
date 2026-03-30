using System;
using System.Threading.Tasks;
using RPSArena.Core.Networking;

namespace RPSArena.Infrastructure.Network
{
    public class DemoGameClient : IGameClient
    {
        private bool _isConnected = true;
        private readonly Random _random = new Random();
        
        public bool IsConnected => _isConnected;
        public string ClientId { get; private set; }
        
        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<Exception> OnError;
        
        public DemoGameClient(string clientId = null)
        {
            ClientId = clientId ?? "DEMO_" + _random.Next(1000, 9999);
            
            // Simular conexión exitosa después de un delay
            Task.Delay(500).ContinueWith(_ => 
            {
                OnConnected?.Invoke();
                
                // Simular mensaje de bienvenida
                Task.Delay(1000).ContinueWith(__ =>
                {
                    var welcomeMsg = new GameMessage(
                        MessageType.ChatMessage, 
                        "SYSTEM", 
                        "¡Bienvenido al modo demo! Juega contra la CPU.");
                    
                    OnMessageReceived?.Invoke(welcomeMsg.ToJson());
                });
            });
        }
        
        public Task<bool> ConnectAsync(string host, int port, string clientId = null)
        {
            // En demo, siempre "conecta" exitosamente
            ClientId = clientId ?? ClientId;
            _isConnected = true;
            
            OnConnected?.Invoke();
            return Task.FromResult(true);
        }
        
        public Task SendMessageAsync(string message)
        {
            if (!_isConnected)
                throw new InvalidOperationException("Not connected");
            
            // Simular respuesta de la CPU
            Task.Delay(800).ContinueWith(_ =>
            {
                try
                {
                    var receivedMsg = GameMessage.FromJson(message);
                    
                    if (receivedMsg.Type == MessageType.MakeChoice)
                    {
                        // La CPU elige aleatoriamente
                        var choices = new[] { "rock", "paper", "scissors" };
                        var cpuChoice = choices[_random.Next(3)];
                        
                        var cpuMsg = new GameMessage(
                            MessageType.MakeChoice,
                            "CPU_Player",
                            cpuChoice);
                        
                        OnMessageReceived?.Invoke(cpuMsg.ToJson());
                        
                        // Enviar mensaje de chat de la CPU
                        Task.Delay(500).ContinueWith(__ =>
                        {
                            var chatMsg = new GameMessage(
                                MessageType.ChatMessage,
                                "CPU",
                                "¡Buena jugada! 😎");
                            
                            OnMessageReceived?.Invoke(chatMsg.ToJson());
                        });
                    }
                    else if (receivedMsg.Type == MessageType.ChatMessage)
                    {
                        // Responder a mensajes de chat
                        Task.Delay(1000).ContinueWith(__ =>
                        {
                            var responses = new[]
                            {
                                "¡Hola! Soy la CPU, listo para jugar.",
                                "¿Listo para perder? 😜",
                                "Piedra, papel o tijera... ¡Elige!",
                                "Buena suerte... vas a necesitarla 😄"
                            };
                            
                            var response = responses[_random.Next(responses.Length)];
                            var cpuMsg = new GameMessage(
                                MessageType.ChatMessage,
                                "CPU",
                                response);
                            
                            OnMessageReceived?.Invoke(cpuMsg.ToJson());
                        });
                    }
                }
                catch { }
            });
            
            return Task.CompletedTask;
        }
        
        public Task DisconnectAsync()
        {
            _isConnected = false;
            OnDisconnected?.Invoke();
            return Task.CompletedTask;
        }
        
        public void Dispose()
        {
            _isConnected = false;
            // No hay recursos que liberar
        }
    }
}