using Microsoft.EntityFrameworkCore;
using PlantCare.Api.Models;
using PlantCare.Api.Models.Plants;
using PlantCare.Api.Models.Diagnoses;
using PlantCare.Api.Models.Schedules;
using PlantCare.Api.Models.Devices;

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

    public DbSet<Device> Devices => Set<Device>();
    public DbSet<PlantDevice> PlantDevices => Set<PlantDevice>();
    public DbSet<DeviceCommand> DeviceCommands => Set<DeviceCommand>();
    public DbSet<DeviceEvent> DeviceEvents => Set<DeviceEvent>();
    public DbSet<DeviceReading> DeviceReadings => Set<DeviceReading>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .IsRequired();

        // User 1 -> Many Plants
        modelBuilder.Entity<User>()
            .HasMany(u => u.Plants)
            .WithOne(p => p.User)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // User 1 -> Many Devices
        modelBuilder.Entity<User>()
            .HasMany<Device>()
            .WithOne()
            .HasForeignKey(d => d.UserId)
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

        modelBuilder.Entity<Diagnosis>()
            .HasIndex(d => d.PlantPhotoId)
            .IsUnique();

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
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);

        // Schedule 1 -> Many Tasks
        modelBuilder.Entity<Schedule>()
            .HasMany(s => s.Tasks)
            .WithOne(t => t.Schedule)
            .HasForeignKey(t => t.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        // Device 1 -> Many PlantDevices
        modelBuilder.Entity<Device>()
            .HasMany(d => d.PlantDevices)
            .WithOne(pd => pd.Device)
            .HasForeignKey(pd => pd.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // Plant 1 -> Many PlantDevices
        modelBuilder.Entity<Plant>()
            .HasMany<PlantDevice>()
            .WithOne()
            .HasForeignKey(pd => pd.PlantId)
            .OnDelete(DeleteBehavior.Cascade);

        // Avoid duplicate device-plant links
        modelBuilder.Entity<PlantDevice>()
            .HasIndex(pd => new { pd.DeviceId, pd.PlantId })
            .IsUnique();

        // PlantDevice 1 -> Many Commands
        modelBuilder.Entity<PlantDevice>()
            .HasMany<DeviceCommand>()
            .WithOne()
            .HasForeignKey(dc => dc.PlantDeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlantDevice 1 -> Many Events
        modelBuilder.Entity<PlantDevice>()
            .HasMany<DeviceEvent>()
            .WithOne()
            .HasForeignKey(de => de.PlantDeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // PlantDevice 1 -> Many Readings
        modelBuilder.Entity<PlantDevice>()
            .HasMany<DeviceReading>()
            .WithOne()
            .HasForeignKey(dr => dr.PlantDeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // ApiKeyHash required
        modelBuilder.Entity<Device>()
            .Property(d => d.ApiKeyHash)
            .IsRequired();

        // Battery level default
        modelBuilder.Entity<Device>()
            .Property(d => d.BatteryLevel)
            .HasDefaultValue(100);
    }
}