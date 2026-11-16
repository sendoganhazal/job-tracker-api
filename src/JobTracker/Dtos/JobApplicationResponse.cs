using JobTracker.Models;

namespace JobTracker.Dtos;

// API'nin istemciye döndüğü şekil. Frontend'deki TypeScript interface'inin karşılığı budur.
public class JobApplicationResponse
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public ApplicationStatus Status { get; set; }
    public DateTime AppliedDate { get; set; }
    public string? Notes { get; set; }
}