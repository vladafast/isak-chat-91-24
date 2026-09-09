using SQLite;

namespace IsakChat.App.Models;

// Cuvamo TACNO jedan red ovde - podatke o trenutno ulogovanom korisniku,
// da app posle gasenja/paljenja ne trazi login ponovo (auto-login).
[Table("local_session")]
public class LocalSession
{
    [PrimaryKey]
    public int Id { get; set; } = 1;

    public string SessionToken { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string ServerBaseUrl { get; set; } = string.Empty;
}
