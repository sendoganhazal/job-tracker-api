using System.Security.Claims;
using System.Text;
using JobTracker.Models;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace JobTracker.Services;

// JWT üreten sınıf. JWT = "bu kullanıcı kim" bilgisini içeren, sunucunun imzaladığı bir metin.
// Frontend bunu saklar ve her istekte Authorization header'ında geri yollar.
public class TokenService
{
    private readonly IConfiguration _config;

    public TokenService(IConfiguration config)
    {
        _config = config;
    }

    public (string Token, DateTime ExpiresAt) CreateToken(AppUser user)
    {
        var key = _config["Jwt:Key"]
            ?? throw new InvalidOperationException("Jwt:Key ayarlanmamış (user-secrets).");
        var expiresAt = DateTime.UtcNow.AddMinutes(_config.GetValue<int>("Jwt:ExpiresMinutes"));

        var descriptor = new SecurityTokenDescriptor
        {
            // Claim = token'ın içine yazılan bilgi parçası. "sub" kullanıcının Id'si.
            // Şifreyi ASLA token'a koyma: token'ın içeriği herkes tarafından okunabilir (sadece imzası sahteye karşı korur).
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", user.Id),
                new Claim("email", user.Email ?? string.Empty)
            }),
            Expires = expiresAt,
            Issuer = _config["Jwt:Issuer"],
            Audience = _config["Jwt:Audience"],
            // Token'ı gizli anahtarla imzalıyoruz. Biri içeriği değiştirirse imza tutmaz, token reddedilir.
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                SecurityAlgorithms.HmacSha256)
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return (token, expiresAt);
    }
}