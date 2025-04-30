using ClientBlazor.Models;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;

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
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                Formatting = Formatting.Indented
            };
           
            var response = await _http.GetAsync("allproducts");
            string json = await response.Content.ReadAsStringAsync();
            _cachedProducts = JsonConvert.DeserializeObject<List<Product>>(json, settings);
            //_cachedProducts = new List<Product>();
            return _cachedProducts;
        }

        
    }
}
