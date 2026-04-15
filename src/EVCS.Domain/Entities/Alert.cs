using EVCS.Domain.Enums;

namespace EVCS.Domain.Entities;

public class Alert
{
    public long Id { get; set; }
    public int StationId { get; set; }
    public int? PoleId { get; set; }
    public string AlertType { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public AlertSeverity Severity { get; set; } = AlertSeverity.Medium;
    public AlertStatus Status { get; set; } = AlertStatus.Open;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }

    public Station? Station { get; set; }
    public Pole? Pole { get; set; }
}
