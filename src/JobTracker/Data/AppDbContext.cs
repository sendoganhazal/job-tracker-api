using JobTracker.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace JobTracker.Data;

// DbContext yerine IdentityDbContext: kullanıcı, rol vb. Identity tablolarını da otomatik ekler.
public class AppDbContext : IdentityDbContext<AppUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<JobApplication> JobApplications => Set<JobApplication>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // ÖNEMLİ: base çağrısı Identity tablolarının ayarlarını yapar, ilk satır olmalı.
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<JobApplication>(entity =>
        {
            // Enum'u yazı olarak sakla
            entity.Property(a => a.Status).HasConversion<string>();

            // Bir kullanıcının çok başvurusu olur. Kullanıcı silinirse başvuruları da silinir (Cascade).
            entity.HasOne(a => a.User)
                  .WithMany()
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            // Her sorguda UserId ile filtreleyeceğiz, index bunu hızlandırır
            entity.HasIndex(a => a.UserId);
        });
    }
}