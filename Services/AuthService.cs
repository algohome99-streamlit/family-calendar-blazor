using Blazored.LocalStorage;

namespace FamilyCalendar.Blazor.Services;

public class AuthService
{
    private readonly ILocalStorageService _localStorage;
    private readonly IDataService _dataService;
    private const string AuthKey = "family_calendar_auth";

    public bool IsAuthenticated { get; private set; }
    public event Action? OnAuthStateChanged;

    public AuthService(ILocalStorageService localStorage, IDataService dataService)
    {
        _localStorage = localStorage;
        _dataService = dataService;
    }

    public async Task InitializeAsync()
    {
        IsAuthenticated = await _localStorage.GetItemAsync<bool>(AuthKey);
    }

    public async Task<bool> LoginAsync(string password)
    {
        var settings = await _dataService.GetSettingsAsync();

        if (password == settings.Password)
        {
            IsAuthenticated = true;
            await _localStorage.SetItemAsync(AuthKey, true);
            OnAuthStateChanged?.Invoke();
            return true;
        }

        return false;
    }

    public async Task LogoutAsync()
    {
        IsAuthenticated = false;
        await _localStorage.RemoveItemAsync(AuthKey);
        OnAuthStateChanged?.Invoke();
    }
}
