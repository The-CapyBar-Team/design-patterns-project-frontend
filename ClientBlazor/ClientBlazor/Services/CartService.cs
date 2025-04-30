using ClientBlazor.Models;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static System.Net.WebRequestMethods;
using System.Net.Http.Json;
using ClientBlazor.Pages;

namespace ClientBlazor.Services
{
    public class CartService
    {
        private List<CartItem> _cartItems = new List<CartItem>();
        private const string CART_STORAGE_KEY = "blazor_cart_items";

        public event Action OnChange;

        public IReadOnlyList<CartItem> CartItems => _cartItems;

        private readonly HttpClient _http;
        private readonly SessionService _sessionService;
        private readonly ProductService _productService;

        public CartService(ILocalStorageService localStorage, HttpClient http, SessionService sessionService, ProductService productService, QueueNotificationService queueNotificationService)
        {
            _http = http;
            _sessionService = sessionService;
            _productService = productService;

            queueNotificationService.OnQueuePositionUpdated += UpdateQueuePosition;
            queueNotificationService.OnLostProduct += RemoveFromCart;
            queueNotificationService.OnLoadAllCart += UpdateAllCart;

            AskForCartData();

            _ = queueNotificationService.InitializeConnectionAsync();
        }


        private async void AskForCartData()
        {
            if (_cartItems.Count == 0)
            {
                var userId = _sessionService.GetOrCreateSessionId();

                var request = new StatusRequest
                {
                    UserId = userId,
                };

                await _http.PostAsJsonAsync("basket/status", request);
            }
        }
        private void UpdateAllCart(QueuePositionUpdateMessage[] items)
        {
            foreach (var item in items) 
            {
                var product = _productService.CachedProducts?.FirstOrDefault(prod => prod.Id == item.ProductId);
                CartItem cartItem = new CartItem(product, !item.HasQueuePosition, item.QueuePosition, item.AcquisitionTime);
                _cartItems.Add(cartItem);
            }
            NotifyStateChanged();
        }
        private void RemoveFromCart(int productId)
        {
            var item = _cartItems.FirstOrDefault(item => item.Product.Id == productId);
            if (item != null)
            {
                _cartItems.Remove(item);
                NotifyStateChanged();
            }
        }

        private void UpdateQueuePosition(QueuePositionUpdateMessage message)
        {
            var item = _cartItems.FirstOrDefault(item => item.Product.Id == message.ProductId);
            if (item == null)
            {
                var product = _productService.CachedProducts?.FirstOrDefault(item => item.Id == message.ProductId);
                if (product != null)
                {
                    CartItem cartItem = new CartItem(product, !message.HasQueuePosition, (message.QueuePosition + 1), message.AcquisitionTime);
                    _cartItems.Add(cartItem);
                }
            }
            else
            {
                item.InStock = !message.HasQueuePosition;
                item.QueuePosition = message.QueuePosition;
            }
            NotifyStateChanged();
        }
        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task RequestRemoveFromCartAsync(int productId)
        {
            var userId = _sessionService.GetOrCreateSessionId();

            var request = new ProductRequest
            {
                UserId = userId,
                ProductId = productId
            };

            var response = await _http.PostAsJsonAsync("basket/remove", request);

            if (response.IsSuccessStatusCode)
            {
                RemoveFromCart(productId);
            }
            else
            {
                Console.WriteLine("Failed to remove item from server.");
            }
        }
        public async Task RequestAddToCartAsync(Product product)
        {
            var userId = _sessionService.GetOrCreateSessionId();

            var request = new ProductRequest
            {
                UserId = userId,
                ProductId = product.Id
            };

            _ = await _http.PostAsJsonAsync("basket/add", request);
            
            
        }
        public async Task RequestBuyFromCartAsync(int productId)
        {
            var sessionId = _sessionService.GetOrCreateSessionId();

            var request = new ProductRequest
            {
                UserId = sessionId,
                ProductId = productId
            };

            var response = await _http.PostAsJsonAsync("basket/buy", request);

            if (response.IsSuccessStatusCode)
            {
                RemoveFromCart(productId);
            }
            else
            {
                Console.WriteLine("Failed to buy item from server.");
            }
        }
    }
}
