using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using ClientBlazor.Models;
using ClientBlazor.Services;
public class QueueNotificationService : IAsyncDisposable
    {
        private readonly SessionService _sessionService;
        private HubConnection _hubConnection;
        private readonly string _hubUrl;
        public event Action<int> OnQueuePositionUpdated;

        public QueueNotificationService(SessionService sessionService, string backendUrl)
        {
            _sessionService = sessionService;
            _hubUrl = $"{backendUrl}/notifications";
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
            await _hubConnection.InvokeAsync("SubscribeToNotifications", _sessionService.GetOrCreateSessionId());

            _hubConnection.On<int>("UpdateQueuePosition", (productId) =>
                {
                    Console.WriteLine($"Received queue update for product {productId}");
                    OnQueuePositionUpdated?.Invoke(productId);
                });

                await _hubConnection.StartAsync();
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
