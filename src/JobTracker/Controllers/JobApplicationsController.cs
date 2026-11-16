using System.Linq.Expressions;
using JobTracker.Data;
using JobTracker.Dtos;
using JobTracker.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobApplicationsController(AppDbContext db)
    {
        _db = db;
    }

    // --- Entity -> DTO dönüşümü, tek bir yerde ---
    // Expression<Func<...>>: "kod olarak değil, veri olarak" tutulan fonksiyon.
    // EF Core bunu okuyup SQL'e çevirebilir (sadece gereken sütunları SELECT eder).
    private static readonly Expression<Func<JobApplication, JobApplicationResponse>> ToResponseExpr =
        a => new JobApplicationResponse
        {
            Id = a.Id,
            CompanyName = a.CompanyName,
            Position = a.Position,
            Status = a.Status,
            AppliedDate = a.AppliedDate,
            Notes = a.Notes
        };

    // Aynı dönüşümü bellekteki tek bir nesne için de lazım olacak (Create'te).
    // Compile() ile expression'ı normal bir fonksiyona çeviriyoruz, böylece mapping'i iki kere yazmıyoruz.
    private static readonly Func<JobApplication, JobApplicationResponse> ToResponse = ToResponseExpr.Compile();

    // GET /api/jobapplications?status=Interview&search=teknoloji
    // [FromQuery]: değerler URL'deki ?anahtar=değer kısmından gelir. İkisi de opsiyonel (? ile nullable).
    [HttpGet]
    public async Task<ActionResult<List<JobApplicationResponse>>> GetAll(
        [FromQuery] ApplicationStatus? status,
        [FromQuery] string? search)
    {
        // IQueryable = "henüz çalışmamış sorgu". Where/OrderBy eklemek veritabanına gitmez,
        // sadece sorguyu tarif eder. SQL, ToListAsync() anında tek seferde çalışır.
        IQueryable<JobApplication> query = _db.JobApplications.AsNoTracking();

        // Filtreleri sadece gönderildiyse ekliyoruz (koşullu sorgu kurma)
        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            // EF.Functions.Like -> SQL'deki LIKE '%...%'. Şirket adı ya da pozisyonda arar.
            query = query.Where(a =>
                EF.Functions.Like(a.CompanyName, $"%{term}%") ||
                EF.Functions.Like(a.Position, $"%{term}%"));
        }

        var items = await query
            .OrderByDescending(a => a.AppliedDate)
            .Select(ToResponseExpr) // SELECT'i DTO'nun alanlarına daraltır
            .ToListAsync();

        return Ok(items);
    }

    // GET /api/jobapplications/summary -> { "Applied": 5, "Interview": 2, "Rejected": 1, "Offer": 0 }
    // Frontend'de dashboard kartları için kullanabilirsin.
    // Not: Bu rota, {id:int} rotasından ÖNCE tanımlı olmak zorunda değil ama "summary" sabit bir segment
    // olduğu için ASP.NET onu parametreli rotaya göre önceliklendirir.
    [HttpGet("summary")]
    public async Task<ActionResult<Dictionary<string, int>>> GetSummary()
    {
        // GroupBy -> SQL'de GROUP BY Status + COUNT(*). Veriyi belleğe çekmeden veritabanı sayar.
        var counts = await _db.JobApplications
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        // Hiç kaydı olmayan durumlar için 0 yazalım ki frontend "undefined" ile uğraşmasın
        var result = Enum.GetValues<ApplicationStatus>()
            .ToDictionary(
                s => s.ToString(),
                s => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0);

        return Ok(result);
    }

    // {id:int} -> "id sadece tam sayı olabilir" kısıtı. /api/jobapplications/abc artık 404 döner.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobApplicationResponse>> GetById(int id)
    {
        var item = await _db.JobApplications
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(ToResponseExpr)
            .FirstOrDefaultAsync(); // yoksa null döner

        if (item is null) return NotFound();

        return Ok(item);
    }

    // POST /api/jobapplications
    [HttpPost]
    public async Task<ActionResult<JobApplicationResponse>> Create(CreateJobApplicationDto dto)
    {
        // DTO'dan entity'ye elle map ediyoruz. Neyin içeri girdiği tamamen bizim kontrolümüzde.
        var entity = new JobApplication
        {
            CompanyName = dto.CompanyName.Trim(),
            Position = dto.Position.Trim(),
            Status = dto.Status,
            AppliedDate = dto.AppliedDate ?? DateTime.UtcNow, // ?? = "null ise sağdakini kullan" (JS'deki ?? ile aynı)
            Notes = dto.Notes
        };

        _db.JobApplications.Add(entity);
        await _db.SaveChangesAsync(); // INSERT çalışır, entity.Id dolar

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToResponse(entity));
    }

    // PUT /api/jobapplications/5
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateJobApplicationDto dto)
    {
        var entity = await _db.JobApplications.FindAsync(id);
        if (entity is null) return NotFound();

        entity.CompanyName = dto.CompanyName.Trim();
        entity.Position = dto.Position.Trim();
        entity.Status = dto.Status;
        entity.AppliedDate = dto.AppliedDate;
        entity.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PATCH /api/jobapplications/5/status  body: { "status": "Offer" }
    // PUT = tüm kaydı değiştir, PATCH = sadece bir parçayı değiştir
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var entity = await _db.JobApplications.FindAsync(id);
        if (entity is null) return NotFound();

        entity.Status = dto.Status!.Value; // [Required] sayesinde buraya null gelmez, "!" bunu derleyiciye söyler

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // DELETE /api/jobapplications/5
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await _db.JobApplications.FindAsync(id);
        if (entity is null) return NotFound();

        _db.JobApplications.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}