namespace IsakChat.Server.Models;

// Predstavlja "ulogovanog" korisnika - kad se korisnik uloguje dobija token koji
// posle salje u Authorization headeru svakog zahteva (Bearer <token>).
public class Session
{
    public int Id { get; set; }
    public string Token { get; set; } = string.Empty;
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
}
