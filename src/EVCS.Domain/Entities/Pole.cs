using EVCS.Domain.Common;
using EVCS.Domain.Enums;

namespace EVCS.Domain.Entities;

public class Pole : AuditableEntity
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Model { get; set; }          // ✅ thêm
    public string? Manufacturer { get; set; }   // ✅ thêm
    public DateTime? InstalledAt { get; set; }  // ✅ thêm

 

    public int StationId { get; set; }

    public Station Station { get; set; } = null!;

    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;

    // 🔥 THÊM 2 CÁI NÀY (FIX LỖI CỦA BẠN)
    public ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}