using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.Common;
using EVCS.Application.DTOs;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Services;

public sealed class AlertService : IAlertService
{
    private readonly IAlertRepository _alertRepository;
    private readonly IStationRepository _stationRepository;
    private readonly IPoleRepository _poleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlertService(
        IAlertRepository alertRepository,
        IStationRepository stationRepository,
        IPoleRepository poleRepository,
        IUnitOfWork unitOfWork)
    {
        _alertRepository = alertRepository;
        _stationRepository = stationRepository;
        _poleRepository = poleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<AlertItemDto>> GetListAsync(AlertFilter filter, CancellationToken cancellationToken)
    {
        var alerts = await _alertRepository.GetListAsync(filter, cancellationToken);
        return alerts.Select(Map).ToArray();
    }

    public async Task<AlertItemDto> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        ValidationGuard.AgainstNullOrWhiteSpace(request.ErrorType, "Loại lỗi không được để trống.");
        ValidationGuard.AgainstNullOrWhiteSpace(request.Message, "Nội dung cảnh báo không được để trống.");

        var station = await _stationRepository.GetByIdAsync(request.StationId, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc.", 404);

        if (request.PoleId.HasValue)
        {
            var pole = await _poleRepository.GetByIdAsync(request.PoleId.Value, includeChildren: false, cancellationToken)
                ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

            ValidationGuard.Against(pole.StationId != station.Id, "Trụ sạc không thuộc trạm đã chọn.");
        }

        var alert = new Alert
        {
            StationId = request.StationId,
            PoleId = request.PoleId,
            ErrorType = request.ErrorType.Trim(),
            Message = request.Message.Trim(),
            Severity = request.Severity,
            Status = AlertStatus.Moi,
            OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        await _alertRepository.AddAsync(alert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _alertRepository.GetByIdAsync(alert.Id, cancellationToken)
            ?? throw new AppException("Không thể lấy dữ liệu cảnh báo vừa tạo.", 500);

        return Map(saved);
    }

    public async Task<AlertItemDto> ProcessAsync(long id, ProcessAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không tìm thấy cảnh báo.", 404);

        alert.Status = request.Status;
        alert.ResolutionNote = request.ResolutionNote?.Trim();
        alert.ProcessedAt = DateTime.UtcNow;
        alert.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không thể cập nhật cảnh báo.", 500);

        return Map(updated);
    }

    private static AlertItemDto Map(Alert alert)
        => new(
            alert.Id,
            alert.StationId,
            alert.Station?.Name ?? string.Empty,
            alert.PoleId,
            alert.Pole?.Code,
            alert.ErrorType,
            alert.Message,
            alert.Severity,
            alert.Status,
            alert.OccurredAt,
            alert.ProcessedAt,
            alert.ResolutionNote);
}
