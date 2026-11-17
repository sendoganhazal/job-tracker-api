namespace JobTracker.Dtos;

// Giriş/kayıt başarılı olunca frontend'e dönen cevap
public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public string Email { get; set; } = string.Empty;
}