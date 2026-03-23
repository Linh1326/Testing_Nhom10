using EVCS.Domain.Common;
using EVCS.Domain.Enums;

namespace EVCS.Domain.Entities;

public class Connector : AuditableEntity
{
    public int Id { get; set; }
    public int PoleId { get; set; }
    public int ChargeTypeId { get; set; }
    public string Code { get; set; } = string.Empty;
    public EquipmentStatus Status { get; set; } = EquipmentStatus.Available;
    public DateTime? InstalledAt { get; set; }

    public Pole? Pole { get; set; }
    public ChargeType? ChargeType { get; set; }
    public ICollection<ChargingSession> ChargingSessions { get; set; } = new List<ChargingSession>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
