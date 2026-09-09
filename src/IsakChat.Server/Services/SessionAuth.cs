using IsakChat.Server.Data;
using IsakChat.Server.Models;
using Microsoft.EntityFrameworkCore;

namespace IsakChat.Server.Services;

// Cita "Authorization: Bearer <token>" header, nalazi sesiju u bazi i vraca ulogovanog korisnika.
// Ovo je nasa "rucna" verzija logovanja i sesije korisnika (bez JWT-a) - jednostavnije za odbranu:
// server pamti token u tabeli Sessions, klijent ga salje uz svaki zahtev.
public class SessionAuth
{
    private readonly AppDbContext _db;

    public SessionAuth(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetCurrentUserAsync(HttpRequest request)
    {
        var header = request.Headers.Authorization.ToString();
        if (string.IsNullOrWhiteSpace(header) || !header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return null;

        var token = header["Bearer ".Length..].Trim();
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var session = await _db.Sessions
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.Token == token);

        if (session is null || session.ExpiresAtUtc < DateTime.UtcNow)
            return null;

        return session.User;
    }
}
