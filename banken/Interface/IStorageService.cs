namespace banken.Interface
{
    // Abstraktion för lagring (t.ex. localStorage i Blazor WASM)
    public interface IStorageService
    {
        // Spara ett objekt under angiven nyckel
        Task SetItemAsync<T>(string key, T value);

        // Hämta ett objekt som tidigare sparats under angiven nyckel
        Task<T> GetItemAsync<T>(string key);
    }
}
