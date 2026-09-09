using IsakChat.Server.Data;
using IsakChat.Server.Models;
using IsakChat.Server.Services;
using IsakChat.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IsakChat.Server.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/auth");

        group.MapPost("/register", async (RegisterRequest req, AppDbContext db) =>
        {
            if (string.IsNullOrWhiteSpace(req.Username) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest("Korisnicko ime i lozinka su obavezni.");

            var usernameTaken = await db.Users.AnyAsync(u => u.Username == req.Username);
            if (usernameTaken)
                return Results.Conflict("Korisnicko ime je vec zauzeto.");

            var (hash, salt) = PasswordHasher.Hash(req.Password);
            var user = new User
            {
                Username = req.Username.Trim(),
                DisplayName = string.IsNullOrWhiteSpace(req.DisplayName) ? req.Username.Trim() : req.DisplayName.Trim(),
                PasswordHash = hash,
                PasswordSalt = salt
            };

            db.Users.Add(user);
            await db.SaveChangesAsync();

            var session = await CreateSessionAsync(db, user.Id);
            return Results.Ok(ToAuthResponse(user, session));
        });

        group.MapPost("/login", async (LoginRequest req, AppDbContext db) =>
        {
            var user = await db.Users.FirstOrDefaultAsync(u => u.Username == req.Username);
            if (user is null || !PasswordHasher.Verify(req.Password, user.PasswordHash, user.PasswordSalt))
                return Results.Unauthorized();

            var session = await CreateSessionAsync(db, user.Id);
            return Results.Ok(ToAuthResponse(user, session));
        });

        group.MapPost("/logout", async (HttpRequest request, AppDbContext db, SessionAuth auth) =>
        {
            var user = await auth.GetCurrentUserAsync(request);
            if (user is null) return Results.Unauthorized();

            var header = request.Headers.Authorization.ToString();
            var token = header["Bearer ".Length..].Trim();
            var session = await db.Sessions.FirstOrDefaultAsync(s => s.Token == token);
            if (session is not null)
            {
                db.Sessions.Remove(session);
                await db.SaveChangesAsync();
            }

            return Results.Ok();
        });
    }

    private static async Task<Session> CreateSessionAsync(AppDbContext db, int userId)
    {
        var session = new Session
        {
            Token = Guid.NewGuid().ToString("N"),
            UserId = userId,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(30)
        };
        db.Sessions.Add(session);
        await db.SaveChangesAsync();
        return session;
    }

    private static AuthResponse ToAuthResponse(User user, Session session) => new()
    {
        SessionToken = session.Token,
        UserId = user.Id,
        Username = user.Username,
        DisplayName = user.DisplayName
    };
}
