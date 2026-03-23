using EVCS.Domain.Common;
using EVCS.Domain.Enums;

namespace EVCS.Domain.Entities;

public class ChargingSession : AuditableEntity
{
    public long Id { get; set; }
    public int StationId { get; set; }
    public int? PoleId { get; set; }
    public int? ConnectorId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public decimal EnergyKwh { get; set; }
    public decimal Cost { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.DangDienRa;

    public Station? Station { get; set; }
    public Pole? Pole { get; set; }
    public Connector? Connector { get; set; }
}
