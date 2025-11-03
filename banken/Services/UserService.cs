using Microsoft.JSInterop;

namespace banken.Services
{
    public class UserService
    {
        private readonly IJSRuntime _js;
        private string? _userName;

        public UserService(IJSRuntime js)
        {
            _js = js;
        }

        public async Task<string?> GetUserNameAsync()
        {
            if (!string.IsNullOrEmpty(_userName))
                return _userName;

            try
            {
                _userName = await _js.InvokeAsync<string>("localStorage.getItem", "userName");
            }
            catch
            {
                _userName = null;
            }

            return _userName;
        }

        public async Task SetUserNameAsync(string name)
        {
            _userName = name;
            await _js.InvokeVoidAsync("localStorage.setItem", "userName", name);
        }

        public async Task ClearUserNameAsync()
        {
            _userName = null;
            await _js.InvokeVoidAsync("localStorage.removeItem", "userName");
        }
    }
}
