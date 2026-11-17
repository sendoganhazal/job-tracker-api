using System.Linq.Expressions;
using System.Security.Claims;
using JobTracker.Data;
using JobTracker.Dtos;
using JobTracker.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Controllers;

// [Authorize]: bu controller'daki TÜM endpoint'ler için geçerli token şart.
// Token yoksa ya da geçersizse istek controller'a hiç girmeden 401 döner.
[Authorize]
[ApiController]
[Route("api/[controller]")]
public class JobApplicationsController : ControllerBase
{
    private readonly AppDbContext _db;

    public JobApplicationsController(AppDbContext db)
    {
        _db = db;
    }

    // Token'daki kullanıcı Id'si. Claim adı ayara göre "sub" ya da NameIdentifier olarak gelebilir,
    // ikisini de deniyoruz ki hangisi gelirse çalışsın.
    private string CurrentUserId =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? User.FindFirstValue("sub")
        ?? throw new UnauthorizedAccessException("Token içinde kullanıcı bilgisi yok.");

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

    private static readonly Func<JobApplication, JobApplicationResponse> ToResponse = ToResponseExpr.Compile();

    // Sadece giriş yapan kullanıcının kayıtları. Her endpoint bu başlangıç sorgusunu kullanıyor,
    // böylece "UserId filtresini unutma" riski tek yerde çözülüyor.
    private IQueryable<JobApplication> MyApplications =>
        _db.JobApplications.Where(a => a.UserId == CurrentUserId);

    [HttpGet]
    public async Task<ActionResult<List<JobApplicationResponse>>> GetAll(
        [FromQuery] ApplicationStatus? status,
        [FromQuery] string? search)
    {
        IQueryable<JobApplication> query = MyApplications.AsNoTracking();

        if (status.HasValue)
        {
            query = query.Where(a => a.Status == status.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim();
            query = query.Where(a =>
                EF.Functions.Like(a.CompanyName, $"%{term}%") ||
                EF.Functions.Like(a.Position, $"%{term}%"));
        }

        var items = await query
            .OrderByDescending(a => a.AppliedDate)
            .Select(ToResponseExpr)
            .ToListAsync();

        return Ok(items);
    }

    [HttpGet("summary")]
    public async Task<ActionResult<Dictionary<string, int>>> GetSummary()
    {
        var counts = await MyApplications
            .AsNoTracking()
            .GroupBy(a => a.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToListAsync();

        var result = Enum.GetValues<ApplicationStatus>()
            .ToDictionary(
                s => s.ToString(),
                s => counts.FirstOrDefault(c => c.Status == s)?.Count ?? 0);

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<JobApplicationResponse>> GetById(int id)
    {
        var item = await MyApplications
            .AsNoTracking()
            .Where(a => a.Id == id)
            .Select(ToResponseExpr)
            .FirstOrDefaultAsync();

        // Başkasının kaydı için de 404 dönüyoruz (403 değil): kaydın var olduğunu bile sızdırmıyoruz
        if (item is null) return NotFound();

        return Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<JobApplicationResponse>> Create(CreateJobApplicationDto dto)
    {
        var entity = new JobApplication
        {
            CompanyName = dto.CompanyName.Trim(),
            Position = dto.Position.Trim(),
            Status = dto.Status,
            AppliedDate = dto.AppliedDate ?? DateTime.UtcNow,
            Notes = dto.Notes,
            UserId = CurrentUserId // sahibi her zaman token'daki kullanıcı, istemci belirleyemez
        };

        _db.JobApplications.Add(entity);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = entity.Id }, ToResponse(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateJobApplicationDto dto)
    {
        var entity = await MyApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (entity is null) return NotFound();

        entity.CompanyName = dto.CompanyName.Trim();
        entity.Position = dto.Position.Trim();
        entity.Status = dto.Status;
        entity.AppliedDate = dto.AppliedDate;
        entity.Notes = dto.Notes;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, UpdateStatusDto dto)
    {
        var entity = await MyApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (entity is null) return NotFound();

        entity.Status = dto.Status!.Value;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var entity = await MyApplications.FirstOrDefaultAsync(a => a.Id == id);
        if (entity is null) return NotFound();

        _db.JobApplications.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}