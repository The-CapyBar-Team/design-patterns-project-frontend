using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ClientBlazor.Models;
using ClientBlazor.Services;
namespace ClientBlazor.Services
{
    public class QueueNotificationService : IAsyncDisposable
    {
        private readonly SessionService _sessionService;
        private HubConnection _hubConnection;
        private readonly string _hubUrl;
        public event Action<QueuePositionUpdateMessage> OnQueuePositionUpdated;
        public event Action<int> OnLostProduct;
        public event Action<QueuePositionUpdateMessage[]> OnLoadAllCart; 

        public QueueNotificationService(SessionService sessionService, string backendUrl)
        {
            _sessionService = sessionService;
            _hubUrl = $"{backendUrl}notifications";
        }

        public async Task InitializeConnectionAsync()
        {
            if (_hubConnection == null || _hubConnection.State == HubConnectionState.Disconnected)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(_hubUrl, options =>
                    {
                        options.Headers.Add("X-Session-ID", _sessionService.GetOrCreateSessionId());
                    })
                    .WithAutomaticReconnect()
                    .Build();
                await _hubConnection.StartAsync();
                await _hubConnection.InvokeAsync("SubscribeToNotifications", _sessionService.GetOrCreateSessionId());

                _hubConnection.On<QueuePositionUpdateMessage>("QueuePositionUpdate", (message) =>
                {
                    OnQueuePositionUpdated?.Invoke(message);
                });
                _hubConnection.On<int>("LostProduct", (productId) =>
                {
                    OnLostProduct?.Invoke(productId);
                });
                _hubConnection.On<QueuePositionUpdateMessage[]>("ProductStatusUpdates", (array) =>
                {
                    OnLoadAllCart?.Invoke(array);
                });

                Console.WriteLine("SignalR connection established");
            }
        }

        public bool IsConnected => _hubConnection?.State == HubConnectionState.Connected;

        public async ValueTask DisposeAsync()
        {
            if (_hubConnection != null)
            {
                await _hubConnection.DisposeAsync();
            }
        }
    }

}
