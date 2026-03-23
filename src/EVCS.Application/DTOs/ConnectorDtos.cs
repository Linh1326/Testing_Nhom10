using EVCS.Domain.Enums;

namespace EVCS.Application.DTOs;

public record ConnectorListQuery(int? PoleId, int? ChargeTypeId, EquipmentStatus? Status);

public record ConnectorSummaryDto(
    int Id,
    string Code,
    int PoleId,
    string PoleName,
    int ChargeTypeId,
    string ChargeTypeName,
    decimal MaxVoltage,
    decimal MaxCurrent,
    DateTime? InstalledAt,
    EquipmentStatus Status);

public record ConnectorDetailDto(
    int Id,
    string Code,
    int PoleId,
    string PoleName,
    int ChargeTypeId,
    string ChargeTypeName,
    decimal MaxVoltage,
    decimal MaxCurrent,
    string? SuitableCar,
    DateTime? InstalledAt,
    EquipmentStatus Status,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public record CreateConnectorRequest(
    int PoleId,
    int ChargeTypeId,
    string? Code,
    EquipmentStatus? Status,
    DateTime? InstalledAt);

public record UpdateConnectorRequest(
    int PoleId,
    int ChargeTypeId,
    EquipmentStatus Status,
    DateTime? InstalledAt);
