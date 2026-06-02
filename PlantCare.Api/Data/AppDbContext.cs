using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Models;
using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Diagnoses;
using PlantCare.Api.Models.Schedules;

namespace PlantCare.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Plant> Plants => Set<Plant>();
    public DbSet<PlantPhoto> PlantPhotos => Set<PlantPhoto>();
    public DbSet<Diagnosis> Diagnoses => Set<Diagnosis>();
    public DbSet<DiagnosisProblem> DiagnosisProblems => Set<DiagnosisProblem>();
    public DbSet<Schedule> Schedules => Set<Schedule>();
    public DbSet<ScheduleTask> ScheduleTasks => Set<ScheduleTask>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User 1 -> Many Plants
        modelBuilder.Entity<User>()
            .HasMany(u => u.Plants)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plant 1 -> Many Photos
        modelBuilder.Entity<Plant>()
            .HasMany(p => p.Photos)
            .WithOne(ph => ph.Plant)
            .HasForeignKey(ph => ph.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlantPhoto 1 -> 0..1 Diagnosis
        modelBuilder.Entity<PlantPhoto>()
            .HasOne(ph => ph.Diagnosis)
            .WithOne(d => d.PlantPhoto)
            .HasForeignKey<Diagnosis>(d => d.PlantPhotoId)
            .OnDelete(DeleteBehavior.Cascade);

        // Diagnosis 1 -> Many Problems
        modelBuilder.Entity<Diagnosis>()
            .HasMany(d => d.Problems)
            .WithOne(p => p.Diagnosis)
            .HasForeignKey(p => p.DiagnosisId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plant 1 -> Many Schedules
        modelBuilder.Entity<Plant>()
            .HasMany(p => p.Schedules)
            .WithOne(s => s.Plant)
            .HasForeignKey(s => s.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Diagnosis 1 -> 0..1 Schedule
        modelBuilder.Entity<Diagnosis>()
            .HasOne(d => d.Schedule)
            .WithOne(s => s.Diagnosis)
            .HasForeignKey<Schedule>(s => s.DiagnosisId)
            .OnDelete(DeleteBehavior.Cascade);

        // Schedule 1 -> Many Tasks
        modelBuilder.Entity<Schedule>()
            .HasMany(s => s.Tasks)
            .WithOne(t => t.Schedule)
            .HasForeignKey(t => t.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();
    }
}