using System.ComponentModel.DataAnnotations;
using JobTracker.Models;

namespace JobTracker.Dtos;

// Yeni başvuru oluştururken istemciden beklediğimiz alanlar.
// Dikkat: Id yok. Id'yi veritabanı verir, istemci karışmaz.
public class CreateJobApplicationDto
{
    [Required, MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty;

    [Required, MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    // Gönderilmezse "Applied" olur
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    // DateTime? = opsiyonel. Gönderilmezse controller'da "şimdi" atayacağız
    public DateTime? AppliedDate { get; set; }

    [MaxLength(1000)]
    public string? Notes { get; set; }
}