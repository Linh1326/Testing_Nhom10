using EVCS.Application.Abstractions.Persistence;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Pole> Poles => Set<Pole>();
    public DbSet<ChargeType> ChargeTypes => Set<ChargeType>();
    public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureStation(modelBuilder);
        ConfigurePole(modelBuilder);
        ConfigureChargeType(modelBuilder);
        ConfigureChargingSession(modelBuilder);
        ConfigureAlert(modelBuilder);
    }

    private static void ConfigureStation(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Station>();

        builder.ToTable("stations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("station_id");
        builder.Ignore(x => x.AdminId);
        builder.Ignore(x => x.ManagerId);
        builder.Property(x => x.Name).HasColumnName("station_name").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Address).HasColumnName("address").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Latitude).HasColumnName("latitude").HasPrecision(10, 6).IsRequired();
        builder.Property(x => x.Longitude).HasColumnName("longitude").HasPrecision(10, 6).IsRequired();
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Name).IsUnique();
    }

    private static void ConfigurePole(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Pole>();

        builder.ToTable("poles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("pole_id");
        builder.Property(x => x.StationId).HasColumnName("station_id");
        builder.Property(x => x.Name).HasColumnName("pole_name").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Code).HasColumnName("pole_code").HasMaxLength(50).IsRequired();
        builder.Property(x => x.Model).HasColumnName("model").HasMaxLength(255);
        builder.Property(x => x.Manufacturer).HasColumnName("manufacturer").HasMaxLength(255);
        builder.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.InstalledAt).HasColumnName("install_date");
        builder.Property(x => x.CreatedAt).HasColumnName("created_at");
        builder.Property(x => x.UpdatedAt).HasColumnName("updated_at");

        builder.HasIndex(x => x.Code).IsUnique();

        builder
            .HasOne(x => x.Station)
            .WithMany(x => x.Poles)
            .HasForeignKey(x => x.StationId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureChargeType(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ChargeType>();

        builder.ToTable("ChargeTypes");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.MaxVoltage).HasPrecision(18, 2);
        builder.Property(x => x.MaxCurrent).HasPrecision(18, 2);
        builder.Property(x => x.SuitableCar).HasMaxLength(255);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();
        builder.HasIndex(x => x.Name).IsUnique();
    }

    private static void ConfigureChargingSession(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<ChargingSession>();

        builder.ToTable("ChargingSessions");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.EnergyKwh).HasPrecision(18, 2);
        builder.Property(x => x.Cost).HasPrecision(18, 2);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder
            .HasOne(x => x.Station)
            .WithMany(x => x.ChargingSessions)
            .HasForeignKey(x => x.StationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Pole)
            .WithMany(x => x.ChargingSessions)
            .HasForeignKey(x => x.PoleId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureAlert(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Alert>();

        builder.ToTable("alerts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Id).HasColumnName("alert_id");
        builder.Property(x => x.StationId).HasColumnName("station_id").IsRequired();
        builder.Property(x => x.PoleId).HasColumnName("pole_id");
        builder.Property(x => x.AlertType).HasColumnName("alert_type").HasMaxLength(255).IsRequired();
        builder.Property(x => x.Message).HasColumnName("message").HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Severity).HasColumnName("severity").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasColumnName("alert_status").HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.OccurredAt).HasColumnName("occurred_at").IsRequired();
        builder.Property(x => x.Note).HasColumnName("note").HasMaxLength(1000);

        builder
            .HasOne(x => x.Station)
            .WithMany(x => x.Alerts)
            .HasForeignKey(x => x.StationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.Pole)
            .WithMany(x => x.Alerts)
            .HasForeignKey(x => x.PoleId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
