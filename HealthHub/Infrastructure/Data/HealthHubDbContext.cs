using HealthHub.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HealthHub.Infrastructure.Data;

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
            
            // Configure owned types for value objects
            entity.OwnsOne(p => p.Name, name =>
            {
                name.Property(n => n.FirstName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("FirstName");
                
                name.Property(n => n.LastName)
                    .IsRequired()
                    .HasMaxLength(100)
                    .HasColumnName("LastName");
            });

            entity.Property(p => p.DateOfBirth)
                .IsRequired();

            entity.Property(p => p.CreatedAt)
                .IsRequired();

            entity.Property(p => p.UpdatedAt);

            entity.HasMany(p => p.DiagnosticResults)
                .WithOne()
                .HasForeignKey(d => d.PatientId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<DiagnosticResult>(entity =>
        {
            entity.HasKey(d => d.Id);
            
            entity.OwnsOne(d => d.Diagnosis, diagnosis =>
            {
                diagnosis.Property(d => d.Value)
                    .IsRequired()
                    .HasMaxLength(500)
                    .HasColumnName("Diagnosis");
            });

            entity.HasOne(d => d.Patient)
                .WithMany(p => p.DiagnosticResults)
                .HasForeignKey(d => d.PatientId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(d => d.Notes)
                .HasMaxLength(2000);

            entity.Property(d => d.TimestampUtc)
                .IsRequired()
                .HasConversion(
                    v => v,
                    v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            entity.Property(d => d.CreatedAt)
                .IsRequired();

            entity.Property(d => d.IsActive)
                .IsRequired()
                .HasDefaultValue(true);
        });
    }
}