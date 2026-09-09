using IsakChat.Server.Data;
using IsakChat.Server.Models;
using IsakChat.Server.Services;
using IsakChat.Shared.Dtos;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;

namespace IsakChat.Server.Endpoints;

public static class MessagesEndpoints
{
    private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

    public static void MapMessagesEndpoints(this WebApplication app)
    {
        // Nove javne poruke (opsta soba) posle poruke sa datim Id-jem
        app.MapGet("/api/messages/public", async (HttpRequest request, int afterId, AppDbContext db, SessionAuth auth) =>
        {
            var me = await auth.GetCurrentUserAsync(request);
            if (me is null) return Results.Unauthorized();

            var messages = await db.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender)
                .Where(m => m.RecipientId == null && m.Id > afterId)
                .OrderBy(m => m.Id)
                .Take(200)
                .ToListAsync();

            return Results.Ok(messages.Select(ToDto));
        });

        // Nove privatne poruke izmedju mene i otherUserId, posle poruke sa datim Id-jem
        app.MapGet("/api/messages/private/{otherUserId:int}", async (int otherUserId, HttpRequest request, int afterId, AppDbContext db, SessionAuth auth) =>
        {
            var me = await auth.GetCurrentUserAsync(request);
            if (me is null) return Results.Unauthorized();

            var messages = await db.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender)
                .Where(m => m.Id > afterId &&
                    ((m.SenderId == me.Id && m.RecipientId == otherUserId) ||
                     (m.SenderId == otherUserId && m.RecipientId == me.Id)))
                .OrderBy(m => m.Id)
                .Take(200)
                .ToListAsync();

            return Results.Ok(messages.Select(ToDto));
        });

        // Slanje poruke - multipart/form-data jer moze da nosi i sliku.
        // Polja: text (opciono ako ima slike), image (opciono), replyToId (opciono), recipientId (opciono - null/izostavljeno = javna poruka)
        app.MapPost("/api/messages", async (HttpRequest request, AppDbContext db, SessionAuth auth, IWebHostEnvironment env) =>
        {
            var me = await auth.GetCurrentUserAsync(request);
            if (me is null) return Results.Unauthorized();

            if (!request.HasFormContentType)
                return Results.BadRequest("Ocekuje se multipart/form-data.");

            var form = await request.ReadFormAsync();
            var text = form["text"].ToString();
            var replyToRaw = form["replyToId"].ToString();
            var recipientRaw = form["recipientId"].ToString();

            int? replyToId = int.TryParse(replyToRaw, out var rId) ? rId : null;
            int? recipientId = int.TryParse(recipientRaw, out var recId) ? recId : null;

            var file = form.Files["image"];
            string? imagePath = null;

            if (string.IsNullOrWhiteSpace(text) && file is null)
                return Results.BadRequest("Poruka mora imati tekst ili sliku.");

            if (recipientId is not null)
            {
                var recipientExists = await db.Users.AnyAsync(u => u.Id == recipientId);
                if (!recipientExists) return Results.BadRequest("Primalac ne postoji.");
            }

            if (file is not null)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!AllowedImageExtensions.Contains(ext))
                    return Results.BadRequest("Nepodrzan format slike.");

                var uploadsDir = Path.Combine(env.WebRootPath, "uploads");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{Guid.NewGuid():N}{ext}";
                var fullPath = Path.Combine(uploadsDir, fileName);
                await using (var stream = File.Create(fullPath))
                {
                    await file.CopyToAsync(stream);
                }
                imagePath = $"uploads/{fileName}";
            }

            var message = new Message
            {
                SenderId = me.Id,
                RecipientId = recipientId,
                Text = string.IsNullOrWhiteSpace(text) ? null : text.Trim(),
                ImagePath = imagePath,
                ReplyToId = replyToId,
                SentAtUtc = DateTime.UtcNow
            };

            db.Messages.Add(message);
            await db.SaveChangesAsync();

            // ucitaj ponovo sa Include-ovima da bismo popunili reply/sender info u odgovoru
            var saved = await db.Messages
                .Include(m => m.Sender)
                .Include(m => m.ReplyTo).ThenInclude(r => r!.Sender)
                .FirstAsync(m => m.Id == message.Id);

            return Results.Ok(ToDto(saved));
        });

        // Brisanje (opozivanje) poruke - samo posiljalac sme da obrise SVOJU poruku.
        // Ne brisemo red iz baze (pokvarilo bi reply lance) - samo sakrijemo sadrzaj.
        app.MapDelete("/api/messages/{id:int}", async (int id, HttpRequest request, AppDbContext db, SessionAuth auth) =>
        {
            var me = await auth.GetCurrentUserAsync(request);
            if (me is null) return Results.Unauthorized();

            var message = await db.Messages.FirstOrDefaultAsync(m => m.Id == id);
            if (message is null) return Results.NotFound();

            if (message.SenderId != me.Id)
                return Results.StatusCode(StatusCodes.Status403Forbidden);

            message.IsDeleted = true;
            message.Text = null;
            message.ImagePath = null;
            await db.SaveChangesAsync();

            return Results.Ok();
        });
    }

    private static MessageDto ToDto(Message m) => new()
    {
        Id = m.Id,
        SenderId = m.SenderId,
        SenderDisplayName = m.Sender?.DisplayName ?? "?",
        RecipientId = m.RecipientId,
        Text = m.IsDeleted ? null : m.Text,
        ImageUrl = m.IsDeleted || m.ImagePath is null ? null : $"/{m.ImagePath}",
        ReplyToId = m.ReplyToId,
        ReplyToSenderDisplayName = m.ReplyTo?.Sender?.DisplayName,
        ReplyToPreviewText = ReplyPreview(m.ReplyTo),
        SentAtUtc = m.SentAtUtc,
        IsDeleted = m.IsDeleted
    };

    private static string? ReplyPreview(Message? replyTo)
    {
        if (replyTo is null) return null;
        if (replyTo.IsDeleted) return "🚫 Poruka je obrisana";
        return replyTo.ImagePath is not null
            ? (string.IsNullOrWhiteSpace(replyTo.Text) ? "🖼 Slika" : $"🖼 {replyTo.Text}")
            : replyTo.Text;
    }
}
