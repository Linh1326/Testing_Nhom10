using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.Common;
using EVCS.Application.DTOs;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Services;

public sealed class ConnectorService : IConnectorService
{
    private readonly IConnectorRepository _connectorRepository;
    private readonly IPoleRepository _poleRepository;
    private readonly IChargeTypeRepository _chargeTypeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ConnectorService(
        IConnectorRepository connectorRepository,
        IPoleRepository poleRepository,
        IChargeTypeRepository chargeTypeRepository,
        IUnitOfWork unitOfWork)
    {
        _connectorRepository = connectorRepository;
        _poleRepository = poleRepository;
        _chargeTypeRepository = chargeTypeRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<ConnectorSummaryDto>> GetListAsync(ConnectorListQuery query, CancellationToken cancellationToken)
    {
        var connectors = await _connectorRepository.GetListAsync(query.PoleId, query.ChargeTypeId, query.Status, cancellationToken);

        return connectors
            .Select(c => new ConnectorSummaryDto(
                c.Id,
                c.Code,
                c.PoleId,
                c.Pole?.Name ?? string.Empty,
                c.ChargeTypeId,
                c.ChargeType?.Name ?? string.Empty,
                c.ChargeType?.MaxVoltage ?? 0,
                c.ChargeType?.MaxCurrent ?? 0,
                c.InstalledAt,
                c.Status))
            .ToArray();
    }

    public async Task<ConnectorDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var connector = await _connectorRepository.GetByIdAsync(id, includeChildren: true, cancellationToken)
            ?? throw new AppException("Không tìm thấy đầu nối.", 404);

        return MapDetail(connector);
    }

    public async Task<ConnectorDetailDto> CreateAsync(CreateConnectorRequest request, CancellationToken cancellationToken)
    {
        var pole = await _poleRepository.GetByIdAsync(request.PoleId, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

        var chargeType = await _chargeTypeRepository.GetByIdAsync(request.ChargeTypeId, cancellationToken)
            ?? throw new AppException("Không tìm thấy loại sạc.", 404);

        var code = string.IsNullOrWhiteSpace(request.Code)
            ? $"CC{DateTime.UtcNow:yyyyMMddHHmmssfff}"
            : request.Code.Trim();

        var existed = await _connectorRepository.ExistsByCodeAsync(code, null, cancellationToken);
        ValidationGuard.Against(existed, "Mã đầu nối đã tồn tại.");

        var connector = new Connector
        {
            PoleId = pole.Id,
            ChargeTypeId = chargeType.Id,
            Code = code,
            Status = request.Status ?? EquipmentStatus.Available,
            InstalledAt = request.InstalledAt,
            CreatedAt = DateTime.UtcNow
        };

        await _connectorRepository.AddAsync(connector, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(connector.Id, cancellationToken);
    }

    public async Task<ConnectorDetailDto> UpdateAsync(int id, UpdateConnectorRequest request, CancellationToken cancellationToken)
    {
        var connector = await _connectorRepository.GetByIdAsync(id, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy đầu nối.", 404);

        var pole = await _poleRepository.GetByIdAsync(request.PoleId, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

        var chargeType = await _chargeTypeRepository.GetByIdAsync(request.ChargeTypeId, cancellationToken)
            ?? throw new AppException("Không tìm thấy loại sạc.", 404);

        connector.PoleId = pole.Id;
        connector.ChargeTypeId = chargeType.Id;
        connector.Status = request.Status;
        connector.InstalledAt = request.InstalledAt;
        connector.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var connector = await _connectorRepository.GetByIdAsync(id, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy đầu nối.", 404);

        _connectorRepository.Remove(connector);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static ConnectorDetailDto MapDetail(Connector connector)
        => new(
            connector.Id,
            connector.Code,
            connector.PoleId,
            connector.Pole?.Name ?? string.Empty,
            connector.ChargeTypeId,
            connector.ChargeType?.Name ?? string.Empty,
            connector.ChargeType?.MaxVoltage ?? 0,
            connector.ChargeType?.MaxCurrent ?? 0,
            connector.ChargeType?.SuitableCar,
            connector.InstalledAt,
            connector.Status,
            connector.CreatedAt,
            connector.UpdatedAt);
}
