using IsakChat.Server.Data;
using IsakChat.Server.Services;
using IsakChat.Shared.Dtos;
using Microsoft.EntityFrameworkCore;

namespace IsakChat.Server.Endpoints;

public static class UsersEndpoints
{
    public static void MapUsersEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users");

        // Svi ostali korisnici - za pocinjanje novog privatnog ceta
        group.MapGet("/", async (HttpRequest request, AppDbContext db, SessionAuth auth) =>
        {
            var me = await auth.GetCurrentUserAsync(request);
            if (me is null) return Results.Unauthorized();

            var users = await db.Users
                .Where(u => u.Id != me.Id)
                .OrderBy(u => u.DisplayName)
                .Select(u => new UserDto { Id = u.Id, Username = u.Username, DisplayName = u.DisplayName })
                .ToListAsync();

            return Results.Ok(users);
        });

        // Lista "poruke" - korisnici sa kojima vec postoji privatna prepiska, sortirano po poslednjoj aktivnosti
        app.MapGet("/api/conversations", async (HttpRequest request, AppDbContext db, SessionAuth auth) =>
        {
            var me = await auth.GetCurrentUserAsync(request);
            if (me is null) return Results.Unauthorized();

            var privateMessages = await db.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipient)
                .Where(m => m.RecipientId != null && (m.SenderId == me.Id || m.RecipientId == me.Id))
                .OrderByDescending(m => m.SentAtUtc)
                .ToListAsync();

            var conversations = privateMessages
                .GroupBy(m => m.SenderId == me.Id ? m.RecipientId!.Value : m.SenderId)
                .Select(g =>
                {
                    var last = g.First(); // vec sortirano opadajuce po vremenu
                    var other = last.SenderId == me.Id ? last.Recipient : last.Sender;
                    return new ConversationDto
                    {
                        UserId = other!.Id,
                        Username = other.Username,
                        DisplayName = other.DisplayName,
                        LastMessagePreview = last.IsDeleted
                            ? "🚫 Poruka je obrisana"
                            : (last.ImagePath is not null
                                ? (string.IsNullOrWhiteSpace(last.Text) ? "🖼 Slika" : $"🖼 {last.Text}")
                                : last.Text),
                        LastMessageAtUtc = last.SentAtUtc
                    };
                })
                .OrderByDescending(c => c.LastMessageAtUtc)
                .ToList();

            return Results.Ok(conversations);
        });
    }
}
