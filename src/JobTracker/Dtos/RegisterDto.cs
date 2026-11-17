using System.ComponentModel.DataAnnotations;

namespace JobTracker.Dtos;

public class RegisterDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    // Asıl şifre kuralları (büyük harf, rakam vb.) Program.cs'teki Identity ayarlarından gelir
    [Required, MinLength(8)]
    public string Password { get; set; } = string.Empty;
}