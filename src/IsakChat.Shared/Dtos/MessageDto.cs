namespace IsakChat.Shared.Dtos;

// Jedna poruka - koristi se i za opstu sobu (RecipientId == null) i za privatne cetove (RecipientId != null)
public class MessageDto
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public string SenderDisplayName { get; set; } = string.Empty;

    // null = javna poruka u opstoj sobi; inace Id primaoca (privatna poruka)
    public int? RecipientId { get; set; }

    public string? Text { get; set; }

    // relativna putanja koju vraca server (npr. "/uploads/xxxx.jpg"); klijent je spaja sa base adresom servera
    public string? ImageUrl { get; set; }

    // ako je ovo odgovor na drugu poruku
    public int? ReplyToId { get; set; }
    public string? ReplyToSenderDisplayName { get; set; }
    public string? ReplyToPreviewText { get; set; }

    public DateTime SentAtUtc { get; set; }

    // true = posiljalac je obrisao poruku; Text/ImageUrl su tada null, klijent prikazuje placeholder
    public bool IsDeleted { get; set; }
}
