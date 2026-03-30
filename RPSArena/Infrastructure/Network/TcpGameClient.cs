using System;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RPSArena.Core.Networking;

namespace RPSArena.Infrastructure.Network
{
    public class TcpGameClient : IGameClient
    {
        private TcpClient _tcpClient;
        private NetworkStream _stream;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _disposed = false;
        
        public bool IsConnected => _tcpClient?.Connected == true;
        public string ClientId { get; private set; }
        
        public event Action<string> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        public event Action<Exception> OnError;
        
        public async Task<bool> ConnectAsync(string host, int port, string clientId = null)
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync(host, port);
                _stream = _tcpClient.GetStream();
                
                ClientId = clientId ?? Guid.NewGuid().ToString().Substring(0, 8);
                
                _cancellationTokenSource = new CancellationTokenSource();
                _ = Task.Run(() => ReceiveMessagesAsync(_cancellationTokenSource.Token));
                
                OnConnected?.Invoke();
                return true;
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                return false;
            }
        }
        
        private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
        {
            byte[] buffer = new byte[4096];
            
            while (!cancellationToken.IsCancellationRequested && IsConnected)
            {
                try
                {
                    int bytesRead = await _stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
                    if (bytesRead > 0)
                    {
                        string message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        OnMessageReceived?.Invoke(message);
                    }
                }
                catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    if (!cancellationToken.IsCancellationRequested)
                        OnError?.Invoke(ex);
                    break;
                }
            }
        }
        
        public async Task SendMessageAsync(string message)
        {
            if (!IsConnected)
                throw new InvalidOperationException("Not connected to server");
            
            try
            {
                byte[] data = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                OnError?.Invoke(ex);
                throw;
            }
        }
        
        public async Task DisconnectAsync()
        {
            _cancellationTokenSource?.Cancel();
            
            try
            {
                if (IsConnected && ClientId != null)
                {
                    var disconnectMsg = new GameMessage(MessageType.Disconnect, ClientId);
                    await SendMessageAsync(disconnectMsg.ToJson());
                }
            }
            catch { }
            finally
            {
                _stream?.Close();
                _tcpClient?.Close();
                OnDisconnected?.Invoke();
            }
        }
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;
            
            if (disposing)
            {
                _cancellationTokenSource?.Cancel();
                _stream?.Dispose();
                _tcpClient?.Dispose();
                _cancellationTokenSource?.Dispose();
            }
            
            _disposed = true;
        }
        
        ~TcpGameClient()
        {
            Dispose(false);
        }
    }
}