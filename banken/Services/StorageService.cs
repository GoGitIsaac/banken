using Microsoft.JSInterop;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace banken.Services
{
    public class StorageService : IStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters = { new JsonStringEnumConverter() }
        };

        public StorageService(IJSRuntime jsRuntime) => _jsRuntime = jsRuntime;

        public async Task SetItemAsync<T>(string key, T value)
        {
            var json = JsonSerializer.Serialize(value, _jsonSerializerOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, json);
        }
        public async Task<T> GetItemAsync<T>(string key)
        {
            var json = await _jsRuntime.InvokeAsync<string>("localStorage.getItem", key);
            if (string.IsNullOrWhiteSpace(json))
                return default;
            return JsonSerializer.Deserialize<T>(json, _jsonSerializerOptions)!;
        }

        
    }
}
