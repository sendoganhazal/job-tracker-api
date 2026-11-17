using System.Text;
using System.Text.Json.Serialization;
using JobTracker.Data;
using JobTracker.Models;
using JobTracker.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// --- Identity: kullanıcı/şifre yönetimi ---
// AddIdentityCore: sadece API için gereken kısım (cookie/arayüz yok)
builder.Services.AddIdentityCore<AppUser>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.User.RequireUniqueEmail = true; // aynı e-posta ile ikinci kayıt olmasın
})
    .AddEntityFrameworkStores<AppDbContext>(); // kullanıcıları bizim veritabanında sakla

builder.Services.AddScoped<TokenService>();

// --- JWT doğrulama: gelen token'ı kontrol eden kısım ---
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key bulunamadı. 'dotnet user-secrets set \"Jwt:Key\" \"...\"' komutunu çalıştır.");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ValidateIssuerSigningKey = true, // imza kontrolü: sahte token'ı yakalar
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateLifetime = true,         // süresi dolan token'ı reddet
            ClockSkew = TimeSpan.Zero        // varsayılan 5 dk tolerans yerine tam süre
        };
    });

builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()   // Authorization header'ına da izin verir
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors("Frontend");
app.UseAuthentication(); // "Sen kimsin?" -> token'ı okur. UseAuthorization'dan ÖNCE olmak zorunda
app.UseAuthorization();  // "Buna iznin var mı?" -> [Authorize] kontrolü
app.MapControllers();

app.Run();