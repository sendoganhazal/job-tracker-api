using JobTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Data;

// DbContext = veritabanıyla konuştuğumuz köprü. Tüm sorgular ve kayıtlar buradan geçer.
public class AppDbContext : DbContext
{
    // Ayarlar (hangi veritabanı, hangi bağlantı) dışarıdan, Program.cs'ten gelir.
    // Buna Dependency Injection denir. React'te Context Provider'dan değer almaya benzer.
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    // DbSet = tablo. "JobApplications" tablosuna sorgu atmak için bunu kullanırız.
    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    // Model ayarlarını burada inceleriz
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Enum'u veritabanında 0,1,2 yerine "Applied","Interview" gibi yazı olarak sakla.
        // DB'yi açıp baktığında okunabilir olur.
        modelBuilder.Entity<JobApplication>()
            .Property(a => a.Status)
            .HasConversion<string>();
    }
}