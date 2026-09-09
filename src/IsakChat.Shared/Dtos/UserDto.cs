namespace IsakChat.Shared.Dtos;

public class UserDto
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

// Jedan red u listi "Poruke" (DM lista) - korisnik + preview poslednje poruke izmedju nas dvoje
public class ConversationDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? LastMessagePreview { get; set; }
    public DateTime? LastMessageAtUtc { get; set; }
}
