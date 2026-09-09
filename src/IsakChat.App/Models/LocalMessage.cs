using SQLite;

namespace IsakChat.App.Models;

// Lokalna (kes) kopija poruke, cuvana u SQLite bazi na uredjaju - zbog brzeg prikaza pri
// otvaranju ceta (ne cekamo mrezu) i osnovnog rada kad nema konekcije.
// ConversationKey: "public" za opstu sobu, ili "dm-{userId}" za privatni cet sa tim korisnikom.
[Table("local_message")]
public class LocalMessage
{
    [PrimaryKey]
    public int ServerId { get; set; }

    [Indexed]
    public string ConversationKey { get; set; } = string.Empty;

    public int SenderId { get; set; }
    public string SenderDisplayName { get; set; } = string.Empty;
    public int? RecipientId { get; set; }
    public string? Text { get; set; }
    public string? ImageUrl { get; set; }
    public int? ReplyToId { get; set; }
    public string? ReplyToSenderDisplayName { get; set; }
    public string? ReplyToPreviewText { get; set; }
    public DateTime SentAtUtc { get; set; }
    public bool IsDeleted { get; set; }
}
