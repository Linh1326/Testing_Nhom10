using EVCS.Domain.Enums;

namespace EVCS.Application.DTOs;

public record StationListQuery(string? Keyword, EquipmentStatus? Status);

public record StationSummaryDto(
    int Id,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    EquipmentStatus Status,
    DateTime CreatedAt,
    int PoleCount);

public record StationDetailDto(
    int Id,
    int AdminId,
    int ManagerId,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    EquipmentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyCollection<PoleCompactDto> Poles);

public record PoleCompactDto(int Id, string Name, string Code, EquipmentStatus Status);

public record CreateStationRequest(
    int AdminId,
    int ManagerId,
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    EquipmentStatus? Status,
    IReadOnlyCollection<int>? PoleIds);

public record UpdateStationRequest(
    string Name,
    string Address,
    decimal Latitude,
    decimal Longitude,
    EquipmentStatus Status,
    int ManagerId,
    IReadOnlyCollection<int>? PoleIds);

public record StationDashboardDto(int Total, int Available, int Unavailable, int Disabled);
