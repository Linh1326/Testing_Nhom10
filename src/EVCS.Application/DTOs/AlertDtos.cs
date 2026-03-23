using EVCS.Domain.Enums;

namespace EVCS.Application.DTOs;

public record AlertFilter(int? StationId, AlertStatus? Status, AlertSeverity? Severity);

public record AlertItemDto(
    long Id,
    int StationId,
    string StationName,
    int? PoleId,
    string? PoleCode,
    int? ConnectorId,
    string? ConnectorCode,
    string ErrorType,
    string Message,
    AlertSeverity Severity,
    AlertStatus Status,
    DateTime OccurredAt,
    DateTime? ProcessedAt,
    string? ResolutionNote);

public record CreateAlertRequest(
    int StationId,
    int? PoleId,
    int? ConnectorId,
    string ErrorType,
    string Message,
    AlertSeverity Severity,
    DateTime? OccurredAt);

public record ProcessAlertRequest(AlertStatus Status, string? ResolutionNote);
