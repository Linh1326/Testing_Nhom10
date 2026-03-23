using EVCS.Domain.Common;
using EVCS.Domain.Enums;

namespace EVCS.Domain.Entities;

public class Alert : AuditableEntity
{
    public long Id { get; set; }
    public int StationId { get; set; }
    public int? PoleId { get; set; }
    public int? ConnectorId { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.TrungBinh;
    public AlertStatus Status { get; set; } = AlertStatus.Moi;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; set; }
    public string? ResolutionNote { get; set; }

    public Station? Station { get; set; }
    public Pole? Pole { get; set; }
    public Connector? Connector { get; set; }
}
