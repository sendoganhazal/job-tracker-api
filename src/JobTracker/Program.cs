using System.Text.Json.Serialization;
using JobTracker.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controller'ları aktif et. JSON ayarı: enum'lar frontend'e 0/1/2 yerine "Interview" gibi yazı olarak gitsin
builder.Services.AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

// DbContext'i DI sistemine kaydet. Artık controller'lar bunu "isteyebilir".
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS: tarayıcı, farklı porttaki API'ye istek atmana varsayılan olarak izin vermez.
// Senin React/Next.js dev sunucun (3000 ya da 5173) için izin veriyoruz.
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173")
              .AllowAnyHeader()
              .AllowAnyMethod()));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();
app.UseCors("Frontend");   // Sıra önemli: MapControllers'dan önce
app.UseAuthorization();
app.MapControllers();      // [HttpGet] vb. ile işaretli endpoint'leri route tablosuna ekler

app.Run();