using ClientBlazor.Models;
using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using static System.Net.WebRequestMethods;
using System.Net.Http.Json;

namespace ClientBlazor.Services
{
    public class CartService
    {
        private readonly ILocalStorageService _localStorage;
        private List<CartItem> _cartItems = new List<CartItem>();
        private const string CART_STORAGE_KEY = "blazor_cart_items";

        public event Action? OnChange;

        public IReadOnlyList<CartItem> CartItems => _cartItems;

        private readonly HttpClient _http;
        private readonly SessionService _sessionService;
        private readonly ProductService _productService;

        public CartService(ILocalStorageService localStorage, HttpClient http, SessionService sessionService, ProductService productService, QueueNotificationService queueNotificationService)
        {
            _localStorage = localStorage;
            _http = http;
            _sessionService = sessionService;
            _productService = productService;

            queueNotificationService.OnQueuePositionUpdated += UpdateQueuePosition;
            queueNotificationService.OnLostProduct += RemoveFromCart;

            InitializeCart();

            _ = queueNotificationService.InitializeConnectionAsync();
        }


        private void InitializeCart()
        {
            var storedItems = _localStorage.GetItem<List<CartItem>>(CART_STORAGE_KEY);
            if (storedItems != null)
            {
                _cartItems = storedItems;
                NotifyStateChanged();
            }
        }

        private void RemoveFromCart(int productId)
        {
            var item = _cartItems.FirstOrDefault(item => item.Product.Id == productId);
            if (item != null)
            {
                _cartItems.Remove(item);
                SaveCartToLocalStorage();
                NotifyStateChanged();
            }
        }

        private void SaveCartToLocalStorage()
        {
            _localStorage.SetItem(CART_STORAGE_KEY, _cartItems);
        }

        private void UpdateQueuePosition(QueuePositionUpdateMessage message)
        {
            var item = _cartItems.FirstOrDefault(item => item.Product.Id == message.ProductId);
            if (item == null)
            {
                var product = _productService.CachedProducts?.FirstOrDefault(item => item.Id == message.ProductId);
                if (product != null)
                {
                    CartItem cartItem = new CartItem(product, !message.HasQueuePosition, message.QueuePosition);
                    _cartItems.Add(cartItem);
                    SaveCartToLocalStorage();
                    NotifyStateChanged();
                }
            }
            else
            {
                item.InStock = !message.HasQueuePosition;
                item.QueuePosition = message.QueuePosition;
            }
        }
        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task RequestRemoveFromCartAsync(int productId)
        {
            var sessionId = _sessionService.GetOrCreateSessionId();

            var request = new ProductRequest
            {
                UserId = sessionId,
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
            var sessionId = _sessionService.GetOrCreateSessionId();

            var request = new ProductRequest
            {
                UserId = sessionId,
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
