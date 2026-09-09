using IsakChat.Shared.Dtos;

namespace IsakChat.App.ViewModels;

// "Prikazna" verzija poruke - dodaje IsMine (racuna se u odnosu na trenutnog korisnika)
// i spaja relativni ImageUrl sa adresom servera, da ga Image kontrola moze direktno prikazati.
public class MessageItemViewModel
{
    public const string DeletedPlaceholder = "🚫 Poruka je obrisana";

    public int Id { get; init; }
    public string SenderDisplayName { get; init; } = string.Empty;
    public string? Text { get; init; }
    public string? FullImageUrl { get; init; }
    public bool HasImage => !string.IsNullOrEmpty(FullImageUrl);
    public bool HasText => !string.IsNullOrEmpty(Text);
    public bool IsMine { get; init; }
    public bool IsDeleted { get; init; }
    public string TimeText { get; init; } = string.Empty;

    public bool HasReply => !string.IsNullOrEmpty(ReplyPreviewText);
    public string? ReplySenderName { get; init; }
    public string? ReplyPreviewText { get; init; }

    public static MessageItemViewModel From(MessageDto dto, int currentUserId, string serverBaseUrl)
    {
        var local = dto.SentAtUtc.ToLocalTime();
        return new MessageItemViewModel
        {
            Id = dto.Id,
            SenderDisplayName = dto.SenderDisplayName,
            Text = dto.IsDeleted ? DeletedPlaceholder : dto.Text,
            FullImageUrl = dto.IsDeleted || string.IsNullOrEmpty(dto.ImageUrl) ? null : $"{serverBaseUrl.TrimEnd('/')}{dto.ImageUrl}",
            IsMine = dto.SenderId == currentUserId,
            IsDeleted = dto.IsDeleted,
            TimeText = local.ToString("HH:mm"),
            ReplySenderName = dto.ReplyToSenderDisplayName,
            ReplyPreviewText = dto.ReplyToPreviewText
        };
    }

    // Pravi "obrisanu" kopiju postojece stavke - koristi se za trenutno azuriranje UI-ja
    // posle uspesnog brisanja, bez cekanja na sledece osvezavanje sa servera.
    public static MessageItemViewModel AsDeleted(MessageItemViewModel original) => new()
    {
        Id = original.Id,
        SenderDisplayName = original.SenderDisplayName,
        Text = DeletedPlaceholder,
        FullImageUrl = null,
        IsMine = original.IsMine,
        IsDeleted = true,
        TimeText = original.TimeText,
        ReplySenderName = original.ReplySenderName,
        ReplyPreviewText = original.ReplyPreviewText
    };
}
