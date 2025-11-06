using Microsoft.JSInterop;

namespace banken.Services
{
    // Enkel tjänst för att spara/ läsa användarnamn i localStorage via JS interop
    public class UserService
    {
        private readonly IJSRuntime _js; // JS runtime för att anropa localStorage
        private string? _userName; // cache för användarnamnet

        public UserService(IJSRuntime js)
        {
            _js = js; // injicera IJSRuntime
        }

        // Försök returnera cachat användarnamn eller läs från localStorage
        public async Task<string?> GetUserNameAsync()
        {
            if (!string.IsNullOrEmpty(_userName))
                return _userName; // returnera cachet värde om det finns

            try
            {
                // Läs från browser localStorage
                _userName = await _js.InvokeAsync<string>("localStorage.getItem", "userName");
            }
            catch
            {
                _userName = null; // vid fel, sätt null
            }

            return _userName;
        }

        // Sätt användarnamn både i cache och i localStorage
        public async Task SetUserNameAsync(string name)
        {
            _userName = name; // spara i cache
            await _js.InvokeVoidAsync("localStorage.setItem", "userName", name); // skriv till localStorage
        }

        // Rensa användarnamn från cache och localStorage
        public async Task ClearUserNameAsync()
        {
            _userName = null; // töm cache
            await _js.InvokeVoidAsync("localStorage.removeItem", "userName"); // ta bort från localStorage
        }
    }
}
