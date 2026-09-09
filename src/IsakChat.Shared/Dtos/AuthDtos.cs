namespace IsakChat.Shared.Dtos;

// Sta klijent salje serveru da bi se registrovao
public class RegisterRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}

// Sta klijent salje serveru da bi se ulogovao
public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

// Sta server vraca posle uspesnog login-a/registracije: token sesije + osnovni podaci o korisniku
public class AuthResponse
{
    public string SessionToken { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
}
