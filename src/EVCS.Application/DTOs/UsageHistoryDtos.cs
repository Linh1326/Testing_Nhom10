using EVCS.Domain.Enums;

namespace EVCS.Application.DTOs;

public record UsageHistoryFilter(
    DateTime? FromDate,
    DateTime? ToDate,
    int? StationId,
    int? PoleId,
    int? ConnectorId,
    SessionStatus? Status);

public record UsageHistoryItemDto(
    long Id,
    int StationId,
    string StationName,
    int? PoleId,
    string? PoleCode,
    int? ConnectorId,
    string? ConnectorCode,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal EnergyKwh,
    decimal Cost,
    SessionStatus Status);
