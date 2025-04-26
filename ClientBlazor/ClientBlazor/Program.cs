using ClientBlazor.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.JSInterop;

namespace ClientBlazor
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");
            builder.Services.AddLocalStorageServices();
            var backendUrl = builder.Configuration["BackendUrl"] ?? "http://127.0.0.1:5140/api/";
            builder.Services.AddScoped(sp => new HttpClient
            {
                BaseAddress = new Uri(backendUrl)
            });

            builder.Services.AddScoped<ProductService>();

            
            builder.Services.AddScoped<SessionService>();

            builder.Services.AddScoped(sp =>
                new QueueNotificationService(
                    sp.GetRequiredService<SessionService>(),
                    backendUrl));

            builder.Services.AddScoped(sp =>
                new CartService(
                    sp.GetRequiredService<ILocalStorageService>(),
                    sp.GetRequiredService<HttpClient>(),
                    sp.GetRequiredService<SessionService>(),
                    sp.GetRequiredService<ProductService>(),
                    sp.GetRequiredService<QueueNotificationService>()));

            await builder.Build().RunAsync();
        }
    }
}
