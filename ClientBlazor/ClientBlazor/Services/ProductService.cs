using ClientBlazor.Models;
using System.Net.Http.Json;

namespace ClientBlazor.Services
{
    public class ProductService
    {
        private readonly HttpClient _http;
        private List<Product> _cachedProducts;

        public ProductService(HttpClient http)
        {
            _http = http;
        }

        public List<Product> CachedProducts { get => _cachedProducts;}

        public async Task<List<Product>> GetProductsAsync()
        {
            _cachedProducts = await _http.GetFromJsonAsync<List<Product>>("allproducts") ?? new List<Product>();

            return _cachedProducts;
        }

        
    }
}
