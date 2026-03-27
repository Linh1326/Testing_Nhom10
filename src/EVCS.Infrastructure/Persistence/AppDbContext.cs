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
    public DbSet<Connector> Connectors => Set<Connector>();
    public DbSet<ChargeType> ChargeTypes => Set<ChargeType>();
    public DbSet<ChargingSession> ChargingSessions => Set<ChargingSession>();
    public DbSet<Alert> Alerts => Set<Alert>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ConfigureStation(modelBuilder);
        ConfigurePole(modelBuilder);
        ConfigureChargeType(modelBuilder);
        ConfigureConnector(modelBuilder);
        ConfigureChargingSession(modelBuilder);
        ConfigureAlert(modelBuilder);
    }

    private static void ConfigureStation(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Station>();

        builder.ToTable("Stations");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Address).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Latitude).HasPrecision(10, 6).IsRequired();
        builder.Property(x => x.Longitude).HasPrecision(10, 6).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(x => x.Name).IsUnique();
    }

    private static void ConfigurePole(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Pole>();

        builder.ToTable("Poles");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Model).HasMaxLength(255);
        builder.Property(x => x.Manufacturer).HasMaxLength(255);
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

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

    private static void ConfigureConnector(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Connector>();

        builder.ToTable("Connectors");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Code).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        builder.HasIndex(x => x.Code).IsUnique();

        builder
            .HasOne(x => x.Pole)
            .WithMany(x => x.Connectors)
            .HasForeignKey(x => x.PoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.ChargeType)
            .WithMany(x => x.Connectors)
            .HasForeignKey(x => x.ChargeTypeId)
            .OnDelete(DeleteBehavior.Restrict);
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

        builder
            .HasOne(x => x.Connector)
            .WithMany(x => x.ChargingSessions)
            .HasForeignKey(x => x.ConnectorId)
            .OnDelete(DeleteBehavior.NoAction);
    }

    private static void ConfigureAlert(ModelBuilder modelBuilder)
    {
        var builder = modelBuilder.Entity<Alert>();

        builder.ToTable("Alerts");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ErrorType).HasMaxLength(255).IsRequired();
        builder.Property(x => x.Message).HasMaxLength(1000).IsRequired();
        builder.Property(x => x.Severity).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).IsRequired();
        builder.Property(x => x.ResolutionNote).HasMaxLength(1000);

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

        builder
            .HasOne(x => x.Connector)
            .WithMany(x => x.Alerts)
            .HasForeignKey(x => x.ConnectorId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
