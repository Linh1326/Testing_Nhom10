using EVCS.Domain.Enums;

namespace EVCS.Application.DTOs;

public record PoleListQuery(int? StationId, string? Keyword, EquipmentStatus? Status);



public record PoleDetailDto(
    int Id,
    int StationId,
    string StationName,
    string Name,
    string Code,
    string? Model,
    string? Manufacturer,
    EquipmentStatus Status,
    DateTime? InstalledAt,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreatePoleDto(
    int StationId,
    string Name,
    string Code,
    string? Model,
    string? Manufacturer,
    DateTime? InstalledAt
);

public record UpdatePoleRequest(
    string Name,
    string Code,
    string? Model,
    string? Manufacturer,
    EquipmentStatus Status,
    int StationId,
    DateTime? InstalledAt
);
