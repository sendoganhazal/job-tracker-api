using JobTracker.Dtos;
using JobTracker.Models;
using JobTracker.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<AppUser> _userManager; // Identity'nin kullanıcı yönetim sınıfı
    private readonly TokenService _tokenService;

    public AuthController(UserManager<AppUser> userManager, TokenService tokenService)
    {
        _userManager = userManager;
        _tokenService = tokenService;
    }

    // POST /api/auth/register
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterDto dto)
    {
        var user = new AppUser { UserName = dto.Email, Email = dto.Email };

        // CreateAsync şifreyi hash'leyip kaydeder, düz şifre hiçbir yerde tutulmaz
        var result = await _userManager.CreateAsync(user, dto.Password);

        if (!result.Succeeded)
        {
            // Identity'nin hata mesajlarını (şifre çok zayıf, e-posta kayıtlı vb.) 400 olarak döndür
            foreach (var error in result.Errors)
                ModelState.AddModelError(error.Code, error.Description);

            return ValidationProblem(ModelState);
        }

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt, Email = user.Email! });
    }

    // POST /api/auth/login
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);

        // Kullanıcı yok mu, şifre mi yanlış? İkisinde de AYNI mesajı veriyoruz.
        // Aksi halde saldırgan hangi e-postaların kayıtlı olduğunu öğrenebilir.
        if (user is null || !await _userManager.CheckPasswordAsync(user, dto.Password))
            return Unauthorized(new { message = "E-posta veya şifre hatalı." });

        var (token, expiresAt) = _tokenService.CreateToken(user);
        return Ok(new AuthResponse { Token = token, ExpiresAt = expiresAt, Email = user.Email! });
    }
}