using System.Net.Http.Json;
using IsakChat.Shared.Dtos;

namespace IsakChat.App.Services;

// Sav klijent-server (asinhroni) razgovor sa web servisom ide kroz ovu klasu.
public class ApiClient
{
    private readonly HttpClient _http;
    private readonly SessionService _session;

    public ApiClient(HttpClient http, SessionService session)
    {
        _http = http;
        _session = session;
    }

    private string BaseUrl => _session.ServerBaseUrl.TrimEnd('/');

    private HttpRequestMessage NewRequest(HttpMethod method, string path)
    {
        var request = new HttpRequestMessage(method, $"{BaseUrl}{path}");
        if (!string.IsNullOrEmpty(_session.SessionToken))
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _session.SessionToken);
        return request;
    }

    public async Task<AuthResponse> RegisterAsync(string username, string password, string displayName)
    {
        var request = NewRequest(HttpMethod.Post, "/api/auth/register");
        request.Content = JsonContent.Create(new RegisterRequest { Username = username, Password = password, DisplayName = displayName });
        var response = await SendAsync(request);
        return await ReadOrThrowAsync<AuthResponse>(response, "Registracija nije uspela.");
    }

    public async Task<AuthResponse> LoginAsync(string username, string password)
    {
        var request = NewRequest(HttpMethod.Post, "/api/auth/login");
        request.Content = JsonContent.Create(new LoginRequest { Username = username, Password = password });
        var response = await SendAsync(request);
        return await ReadOrThrowAsync<AuthResponse>(response, "Pogresno korisnicko ime ili lozinka.");
    }

    public async Task LogoutAsync()
    {
        try
        {
            var request = NewRequest(HttpMethod.Post, "/api/auth/logout");
            await _http.SendAsync(request);
        }
        catch
        {
            // ako server nije dostupan pri logout-u, ionako brisemo lokalnu sesiju - nije bitno
        }
    }

    public async Task<List<UserDto>> GetUsersAsync()
    {
        var request = NewRequest(HttpMethod.Get, "/api/users");
        var response = await SendAsync(request);
        return await ReadOrThrowAsync<List<UserDto>>(response, "Ne mogu da ucitam korisnike.");
    }

    public async Task<List<ConversationDto>> GetConversationsAsync()
    {
        var request = NewRequest(HttpMethod.Get, "/api/conversations");
        var response = await SendAsync(request);
        return await ReadOrThrowAsync<List<ConversationDto>>(response, "Ne mogu da ucitam poruke.");
    }

    public async Task<List<MessageDto>> GetPublicMessagesAsync(int afterId)
    {
        var request = NewRequest(HttpMethod.Get, $"/api/messages/public?afterId={afterId}");
        var response = await SendAsync(request);
        return await ReadOrThrowAsync<List<MessageDto>>(response, "Ne mogu da ucitam poruke.");
    }

    public async Task<List<MessageDto>> GetPrivateMessagesAsync(int otherUserId, int afterId)
    {
        var request = NewRequest(HttpMethod.Get, $"/api/messages/private/{otherUserId}?afterId={afterId}");
        var response = await SendAsync(request);
        return await ReadOrThrowAsync<List<MessageDto>>(response, "Ne mogu da ucitam poruke.");
    }

    public async Task<MessageDto> SendMessageAsync(string? text, FileResult? image, int? replyToId, int? recipientId)
    {
        using var content = new MultipartFormDataContent();

        if (!string.IsNullOrWhiteSpace(text))
            content.Add(new StringContent(text), "text");
        if (replyToId is not null)
            content.Add(new StringContent(replyToId.Value.ToString()), "replyToId");
        if (recipientId is not null)
            content.Add(new StringContent(recipientId.Value.ToString()), "recipientId");

        if (image is not null)
        {
            var stream = await image.OpenReadAsync();
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(
                image.ContentType is { Length: > 0 } ? image.ContentType : "image/jpeg");
            content.Add(streamContent, "image", image.FileName);
        }

        var request = NewRequest(HttpMethod.Post, "/api/messages");
        request.Content = content;
        var response = await SendAsync(request);
        return await ReadOrThrowAsync<MessageDto>(response, "Slanje poruke nije uspelo.");
    }

    public async Task DeleteMessageAsync(int messageId)
    {
        var request = NewRequest(HttpMethod.Delete, $"/api/messages/{messageId}");
        var response = await SendAsync(request);

        if (response.IsSuccessStatusCode) return;

        if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            throw new ApiException("Mozes da obrises samo svoju poruku.");

        var body = await SafeReadStringAsync(response);
        throw new ApiException(string.IsNullOrWhiteSpace(body) ? "Brisanje poruke nije uspelo." : body);
    }

    // Svi zahtevi prolaze kroz ovo mesto - hvata mrezne greske (server ugasen, nema konekcije,
    // pogresna adresa...) i pretvara ih u ApiException. Bez ovoga bi svaka mrezna greska
    // (HttpRequestException) prosla neuhvacena kroz ViewModel-e i srusila celu aplikaciju.
    private async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
    {
        try
        {
            return await _http.SendAsync(request);
        }
        catch (ApiException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new ApiException($"Ne mogu da se povezem sa serverom ({BaseUrl}). Proveri da li server radi i da li je adresa tacna.\n({ex.GetType().Name}: {ex.Message})");
        }
    }

    private static async Task<T> ReadOrThrowAsync<T>(HttpResponseMessage response, string fallbackMessage)
    {
        if (!response.IsSuccessStatusCode)
        {
            var body = await SafeReadStringAsync(response);
            throw new ApiException(string.IsNullOrWhiteSpace(body) ? fallbackMessage : body);
        }

        var result = await response.Content.ReadFromJsonAsync<T>();
        if (result is null) throw new ApiException(fallbackMessage);
        return result;
    }

    private static async Task<string?> SafeReadStringAsync(HttpResponseMessage response)
    {
        try { return await response.Content.ReadAsStringAsync(); }
        catch { return null; }
    }
}
