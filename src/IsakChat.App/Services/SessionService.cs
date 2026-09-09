using IsakChat.App.Models;
using IsakChat.Shared.Dtos;

namespace IsakChat.App.Services;

// Drzi podatke o trenutno ulogovanom korisniku u memoriji (dok app radi) i
// sinhronizuje ih sa lokalnom SQLite bazom (da prezive gasenje app-a).
public class SessionService
{
    private readonly LocalDatabase _localDb;

    public SessionService(LocalDatabase localDb)
    {
        _localDb = localDb;
    }

    public string? SessionToken { get; private set; }
    public int UserId { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string DisplayName { get; private set; } = string.Empty;
    public string ServerBaseUrl { get; private set; } = "http://localhost:5203";

    public bool IsLoggedIn => !string.IsNullOrEmpty(SessionToken);

    public async Task<bool> TryRestoreAsync()
    {
        var saved = await _localDb.GetSessionAsync();
        if (saved is null || string.IsNullOrWhiteSpace(saved.SessionToken))
            return false;

        SessionToken = saved.SessionToken;
        UserId = saved.UserId;
        Username = saved.Username;
        DisplayName = saved.DisplayName;
        if (!string.IsNullOrWhiteSpace(saved.ServerBaseUrl))
            ServerBaseUrl = saved.ServerBaseUrl;

        return true;
    }

    public async Task SignInAsync(AuthResponse auth, string serverBaseUrl)
    {
        SessionToken = auth.SessionToken;
        UserId = auth.UserId;
        Username = auth.Username;
        DisplayName = auth.DisplayName;
        ServerBaseUrl = serverBaseUrl;

        await _localDb.SaveSessionAsync(new LocalSession
        {
            SessionToken = auth.SessionToken,
            UserId = auth.UserId,
            Username = auth.Username,
            DisplayName = auth.DisplayName,
            ServerBaseUrl = serverBaseUrl
        });
    }

    public async Task SignOutAsync()
    {
        SessionToken = null;
        UserId = 0;
        Username = string.Empty;
        DisplayName = string.Empty;
        await _localDb.ClearAllDataAsync();
    }

    public void SetServerBaseUrl(string url) => ServerBaseUrl = url;
}
