using banken.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

namespace banken
{
    // Program-klass för att starta Blazor WebAssembly-appen
    public class Program
    {
        public static async Task Main(string[] args)
        {
            // Bygg host för Blazor WebAssembly
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app"); // Rootkomponent som mountas i index.html
            builder.RootComponents.Add<HeadOutlet>("head::after"); // Head outlet för SEO/meta

            // Registrera tjänster i DI-containern
            builder.Services.AddScoped<IAccountService, AccountService>(); // Konto-service

            // Standard HttpClient för att göra HTTP-anrop (om nödvändigt)
            builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
            builder.Services.AddScoped<banken.Services.UserService>(); // Enkel UserService för att läsa/skriva användarnamn i localStorage
            builder.Services.AddScoped<IStorageService, StorageService>(); // Tjänst för localStorage-åtkomst

            await builder.Build().RunAsync(); // Kör appen
        }
    }
}
