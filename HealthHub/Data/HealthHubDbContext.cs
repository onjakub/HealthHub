using HealthHub.Models;
using Microsoft.EntityFrameworkCore;

namespace HealthHub.Data;

public class HealthHubDbContext(DbContextOptions<HealthHubDbContext> options) : DbContext(options)
{
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<DiagnosticResult> DiagnosticResults => Set<DiagnosticResult>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Patient>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            entity.Property(p => p.LastName).IsRequired().HasMaxLength(100);

            entity.HasMany(p => p.DiagnosticResults)
                .WithOne(d => d.Patient!)
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiagnosticResult>(entity =>
        {
            entity.HasKey(d => d.Id);
            entity.Property(d => d.Diagnosis).IsRequired().HasMaxLength(500);
            entity.Property(d => d.Notes).HasMaxLength(2000);
            entity.Property(d => d.TimestampUtc).HasConversion(
                v => v,
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
        });
    }
}
