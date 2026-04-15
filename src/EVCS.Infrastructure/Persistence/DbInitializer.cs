using EVCS.Domain.Entities;
using EVCS.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AppDbContext context, CancellationToken cancellationToken = default)
    {
        if (await context.Stations.AnyAsync(cancellationToken))
        {
            return;
        }

        var chargeTypeCcs = new ChargeType
        {
            Code = "TY001",
            Name = "CCS",
            MaxVoltage = 800,
            MaxCurrent = 500,
            SuitableCar = "Xe điện chuẩn CCS",
            Status = EquipmentStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        var chargeTypeType2 = new ChargeType
        {
            Code = "TY002",
            Name = "Type 2",
            MaxVoltage = 400,
            MaxCurrent = 63,
            SuitableCar = "Xe điện chuẩn Type 2",
            Status = EquipmentStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        var station = new Station
        {
            AdminId = 1,
            ManagerId = 1,
            Name = "Trạm sạc Hải Châu",
            Address = "123 Trần Phú, Hải Châu, Đà Nẵng",
            Latitude = 16.054407m,
            Longitude = 108.202167m,
            Status = EquipmentStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        var pole1 = new Pole
        {
            Name = "Pole 1",
            Code = "P0001",
            Model = "Model-A",
            Manufacturer = "NCC1",
            Status = EquipmentStatus.Available,
            InstalledAt = DateTime.UtcNow.AddDays(-45),
            CreatedAt = DateTime.UtcNow,
            Station = station
        };

        var pole2 = new Pole
        {
            Name = "Pole 2",
            Code = "P0002",
            Model = "Model-B",
            Manufacturer = "NCC2",
            Status = EquipmentStatus.Unavailable,
            InstalledAt = DateTime.UtcNow.AddDays(-20),
            CreatedAt = DateTime.UtcNow,
            Station = station
        };

        var session = new ChargingSession
        {
            Station = station,
            Pole = pole1,
            StartedAt = DateTime.UtcNow.AddHours(-6),
            EndedAt = DateTime.UtcNow.AddHours(-5).AddMinutes(-20),
            EnergyKwh = 32.5m,
            Cost = 120000,
            Status = SessionStatus.HoanTat,
            CreatedAt = DateTime.UtcNow.AddHours(-6)
        };

        var alert = new Alert
        {
            Station = station,
            Pole = pole2,
            AlertType = "Connection Lost",
            Message = "Đầu nối không phản hồi trong 5 phút.",
            Severity = AlertSeverity.Medium,
            Status = AlertStatus.Open,
            OccurredAt = DateTime.UtcNow.AddMinutes(-30),
            Note = "Kiểm tra bộ điều khiển và kết nối mạng."
        };

        await context.AddRangeAsync(chargeTypeCcs, chargeTypeType2, station, pole1, pole2, session, alert);
        await context.SaveChangesAsync(cancellationToken);
    }
}
