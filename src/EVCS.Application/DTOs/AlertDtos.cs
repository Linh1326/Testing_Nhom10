using EVCS.Domain.Enums;

namespace EVCS.Application.DTOs;

public record AlertFilter(int? StationId, AlertStatus? Status, AlertSeverity? Severity, string? Keyword);

public record AlertLogDto(DateTime Time, string Message);

public record AlertItemDto(
    long InternalId,
    string Id,
    int StationId,
    string StationName,
    int? PoleId,
    string? PoleCode,
    string Type,
    string Severity,
    string Status,
    DateTime OccurredAt,
    string Description,
    string Suggestion,
    IReadOnlyCollection<AlertLogDto> Logs);

public record CreateAlertRequest(
    int StationId,
    int? PoleId,
    string? AlertType,
    string? Type,
    string? Message,
    string? Description,
    string? Note,
    string? Suggestion,
    string Severity,
    DateTime? OccurredAt,
    string? Status);

public record ProcessAlertRequest(string Status, string? Note, string? Suggestion);
