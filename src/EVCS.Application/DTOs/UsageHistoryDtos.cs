using EVCS.Domain.Enums;

namespace EVCS.Application.DTOs;

public record UsageHistoryFilter(
    DateTime? FromDate,
    DateTime? ToDate,
    int? StationId,
    int? PoleId,
    SessionStatus? Status);

public record UsageHistoryItemDto(
    long Id,
    int StationId,
    string StationName,
    int? PoleId,
    string? PoleCode,
    DateTime StartedAt,
    DateTime? EndedAt,
    decimal EnergyKwh,
    decimal Cost,
    SessionStatus Status);
