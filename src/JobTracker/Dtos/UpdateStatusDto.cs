using System.ComponentModel.DataAnnotations;
using JobTracker.Models;

namespace JobTracker.Dtos;

// Sadece durumu değiştirmek için küçük bir DTO (frontend'de kanban sürükle-bırak gibi yerlerde işine yarar)
public class UpdateStatusDto
{
    // Nullable yapıp [Required] ekledik: enum'lar normalde null olamaz, alan hiç gönderilmezse
    // sessizce ilk değere (Applied) düşerdi. Böylece eksik alan 400 döner.
    [Required]
    public ApplicationStatus? Status { get; set; }
}