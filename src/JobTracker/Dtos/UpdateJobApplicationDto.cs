using System.ComponentModel.DataAnnotations;
using JobTracker.Models;

namespace JobTracker.Dtos;

// Güncellemede tarih zorunlu: PUT "kaydın tamamını değiştir" demektir
public class UpdateJobApplicationDto
{
    [Required, MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; }

    public DateTime AppliedDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}