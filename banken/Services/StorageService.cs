using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace banken.Services
{
    // Enkel wrapper runt IJSRuntime för att spara/hämta JSON i localStorage
    public class StorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime; // JS-runtime för att anropa browser-API:er

        // Inställningar för Json-serialisering (enum som sträng och camelCase)
        JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        // Konstruktor med injicerad IJSRuntime
        public StorageService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

        // Spara ett objekt i localStorage som JSON
        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonSerializerOptions); // serialisera objektet
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json); // spara i browser storage
        }

        // Hämta ett objekt från localStorage och deserialisera det
        public async Task<T> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key); // läs JSON-strängen
            if (string.IsNullOrWhiteSpace(json))
                return default; // inget sparat
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)!; // deserialisera
        }

        
    }
}
