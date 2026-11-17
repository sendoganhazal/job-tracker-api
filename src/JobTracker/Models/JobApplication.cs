using System.ComponentModel.DataAnnotations;

namespace JobTracker.Models;

// Bu sınıf = veritabanındaki tablonun şeması. EF Core her property'yi bir sütuna çevirir.
// TypeScript'teki interface'e benzer, ama burada tablo da oluşturur.
public class JobApplication
{
    // "Id" adı sayesinde EF Core bunu otomatik primary key + auto-increment yapar
    public int Id { get; set; }

    // [Required] -> boş gelirse hata verir (zod/yup validasyonu gibi)
    // [MaxLength] -> hem validasyon yapar hem veritabanı sütun uzunluğunu sınırlar
    [Required, MaxLength(100)]
    public string CompanyName { get; set; } = string.Empty; // = string.Empty: null uyarısını engeller

    [Required, MaxLength(100)]
    public string Position { get; set; } = string.Empty;

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;

    // UTC kullanıyoruz: saat dilimi karmaşasından kaçınmak için sunucuda hep UTC, gösterimde frontend çevirir
    public DateTime AppliedDate { get; set; } = DateTime.UtcNow;

    // string? -> "null olabilir" demek (TypeScript'teki notes?: string)
    [MaxLength(1000)]
    public string? Notes { get; set; }
    // Bu başvuru kime ait? IdentityUser'ın Id'si string (GUID) olduğu için string.
    public string UserId { get; set; } = string.Empty;

    // Navigation property: EF Core ilişkiyi buradan anlar (JOIN yapabilmesi için)
    public AppUser? User { get; set; }
}