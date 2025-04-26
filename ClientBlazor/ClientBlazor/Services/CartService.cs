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

        public CartService(ILocalStorageService localStorage, HttpClient http, SessionService sessionService, QueueNotificationService queueNotificationService)
        {
            _localStorage = localStorage;
            _http = http;
            _sessionService = sessionService;

            queueNotificationService.OnQueuePositionUpdated += UpdateQueuePosition;

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

        public void AddToCartAsync(Product product)
        {
            var existingItem = _cartItems.FirstOrDefault(item => item.Product.Id == product.Id);
            if (existingItem == null)
            {
                var newItem = new CartItem
                {
                    Product = product,
                    InStock = product.Stock > 0,
                    QueuePosition = product.Stock > 0 ? 0 : GetNextQueuePosition() //TODO get correct place from server
                };
                _cartItems.Add(newItem);

                SaveCartToLocalStorage();

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

        private int GetNextQueuePosition()
        {
            var maxPosition = _cartItems
                .Where(item => !item.InStock)
                .Select(item => item.QueuePosition)
                .DefaultIfEmpty(0)
                .Max();
            return maxPosition + 1;
        }
        private void UpdateQueuePosition(int productId, int newPosition)
        {
            var item = _cartItems.FirstOrDefault(i => i.Product.Id == productId);
            if (item != null)
            {
                item.QueuePosition = newPosition;
                if (newPosition == 0)
                {
                    item.InStock = true;
                }
                SaveCartToLocalStorage();
                NotifyStateChanged();
            }
        }
        private void NotifyStateChanged() => OnChange?.Invoke();

        public async Task RequestRemoveFromCartAsync(int productId)
        {
            var sessionId = _sessionService.GetOrCreateSessionId();

            var request = new ProductRequest
            {
                SessionId = sessionId,
                ProductId = productId
            };

            var response = await _http.PostAsJsonAsync("/api/cart/remove", request);

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
                SessionId = sessionId,
                ProductId = product.Id
            };

            var response = await _http.PostAsJsonAsync("cart/add", request);
            //TODO uncomment and complete
            //if (response.IsSuccessStatusCode)
            //{
            //    AddToCartAsync(product); // локально добавим
            //}
            //else
            //{
            //    Console.WriteLine("Failed to add item to server cart.");
            //}
        }
    }
}
