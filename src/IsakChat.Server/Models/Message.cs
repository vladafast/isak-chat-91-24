namespace IsakChat.Server.Models;

public class Message
{
    public int Id { get; set; }

    public int SenderId { get; set; }
    public User? Sender { get; set; }

    // null = poruka u opstoj (javnoj) sobi; inace privatna poruka ka tom korisniku
    public int? RecipientId { get; set; }
    public User? Recipient { get; set; }

    public string? Text { get; set; }

    // relativna putanja fajla na disku servera, npr. "uploads/3f2a....jpg"
    public string? ImagePath { get; set; }

    public int? ReplyToId { get; set; }
    public Message? ReplyTo { get; set; }

    public DateTime SentAtUtc { get; set; } = DateTime.UtcNow;

    // Poruka nije fizicki obrisana iz baze (da ne pokvarimo reply lance i istoriju) -
    // samo se sakrije sadrzaj i markira kao obrisana; klijent prikazuje "Poruka je obrisana".
    public bool IsDeleted { get; set; }
}
