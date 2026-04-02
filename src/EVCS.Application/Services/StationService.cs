using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.Common;
using EVCS.Application.DTOs;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Services;

public sealed class StationService : IStationService
{
    private readonly IStationRepository _stationRepository;
    private readonly IPoleRepository _poleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public StationService(
        IStationRepository stationRepository,
        IPoleRepository poleRepository,
        IUnitOfWork unitOfWork)
    {
        _stationRepository = stationRepository;
        _poleRepository = poleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<StationSummaryDto>> GetListAsync(StationListQuery query, CancellationToken cancellationToken)
    {
        var stations = await _stationRepository.GetListAsync(query.Keyword, query.Status, cancellationToken);
        return stations.Select(MapSummary).ToArray();
    }

    public async Task<StationDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var station = await _stationRepository.GetByIdAsync(id, includeChildren: true, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc.", 404);

        return MapDetail(station);
    }

    public async Task<StationDetailDto> CreateAsync(CreateStationRequest request, CancellationToken cancellationToken)
    {
        ValidationGuard.AgainstNullOrWhiteSpace(request.Name, "Tên trạm sạc không được để trống.");
        ValidationGuard.AgainstNullOrWhiteSpace(request.Address, "Địa chỉ không được để trống.");
        ValidationGuard.AgainstOutOfRange(request.Latitude, -90, 90, "Vĩ độ không hợp lệ.");
        ValidationGuard.AgainstOutOfRange(request.Longitude, -180, 180, "Kinh độ không hợp lệ.");

        var existed = await _stationRepository.ExistsByNameAsync(request.Name.Trim(), null, cancellationToken);
        ValidationGuard.Against(existed, "Trạm sạc đã tồn tại. Vui lòng nhập tên khác.");

        var poles = new List<Pole>();
        if (request.PoleIds is { Count: > 0 })
        {
            poles = await _poleRepository.GetByIdsAsync(request.PoleIds, cancellationToken);
            ValidationGuard.Against(
                poles.Count != request.PoleIds.Count,
                "Danh sách trụ sạc liên kết không hợp lệ.");
        }

        var station = new Station
        {
            AdminId = request.AdminId,
            ManagerId = request.ManagerId,
            Name = request.Name.Trim(),
            Address = request.Address.Trim(),
            Latitude = request.Latitude,
            Longitude = request.Longitude,
            Status = request.Status ?? EquipmentStatus.Available,
            CreatedAt = DateTime.UtcNow
        };

        await _stationRepository.AddAsync(station, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        if (poles.Count > 0)
        {
            foreach (var pole in poles)
            {
                pole.StationId = station.Id;
                pole.UpdatedAt = DateTime.UtcNow;
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }

        return await GetByIdAsync(station.Id, cancellationToken);
    }

    public async Task<StationDetailDto> UpdateAsync(int id, UpdateStationRequest request, CancellationToken cancellationToken)
    {
        var station = await _stationRepository.GetByIdAsync(id, includeChildren: true, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc.", 404);

        ValidationGuard.AgainstNullOrWhiteSpace(request.Name, "Tên trạm sạc không được để trống.");
        ValidationGuard.AgainstNullOrWhiteSpace(request.Address, "Địa chỉ không được để trống.");
        ValidationGuard.AgainstOutOfRange(request.Latitude, -90, 90, "Vĩ độ không hợp lệ.");
        ValidationGuard.AgainstOutOfRange(request.Longitude, -180, 180, "Kinh độ không hợp lệ.");

        var existed = await _stationRepository.ExistsByNameAsync(request.Name.Trim(), id, cancellationToken);
        ValidationGuard.Against(existed, "Thông tin trạm đã tồn tại. Vui lòng thử lại.");

        station.Name = request.Name.Trim();
        station.Address = request.Address.Trim();
        station.Latitude = request.Latitude;
        station.Longitude = request.Longitude;
        station.Status = request.Status;
        station.ManagerId = request.ManagerId;
        station.UpdatedAt = DateTime.UtcNow;

        if (request.PoleIds is not null)
        {
            var poles = await _poleRepository.GetByIdsAsync(request.PoleIds, cancellationToken);
            ValidationGuard.Against(
                poles.Count != request.PoleIds.Count,
                "Danh sách trụ sạc liên kết không hợp lệ.");

            foreach (var pole in poles)
            {
                pole.StationId = station.Id;
                pole.UpdatedAt = DateTime.UtcNow;
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var station = await _stationRepository.GetByIdAsync(id, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc.", 404);

        var hasActivePole = await _poleRepository.ExistsActiveByStationIdAsync(id, cancellationToken);
        ValidationGuard.Against(hasActivePole, "Không thể xóa trạm đang có trụ hoạt động.");

        _stationRepository.Remove(station);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<StationDetailDto> DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        var station = await _stationRepository.GetByIdAsync(id, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc.", 404);

        station.Status = EquipmentStatus.Disabled;
        station.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task<StationDashboardDto> GetDashboardAsync(CancellationToken cancellationToken)
    {
        var total = await _stationRepository.CountAsync(cancellationToken);
        var available = await _stationRepository.CountByStatusAsync(EquipmentStatus.Available, cancellationToken);
        var unavailable = await _stationRepository.CountByStatusAsync(EquipmentStatus.Unavailable, cancellationToken);
        var disabled = await _stationRepository.CountByStatusAsync(EquipmentStatus.Disabled, cancellationToken);

        return new StationDashboardDto(total, available, unavailable, disabled);
    }

    private static StationSummaryDto MapSummary(Station station)
        => new(
            station.Id,
            station.Name,
            station.Address,
            station.Latitude,
            station.Longitude,
            station.Status,
            station.CreatedAt,
            station.Poles.Count);

    private static StationDetailDto MapDetail(Station station)
        => new(
            station.Id,
            station.AdminId,
            station.ManagerId,
            station.Name,
            station.Address,
            station.Latitude,
            station.Longitude,
            station.Status,
            station.CreatedAt,
            station.UpdatedAt,
            station.Poles
                .OrderBy(p => p.Id)
                .Select(p => new PoleCompactDto(p.Id, p.Name, p.Code, p.Status))
                .ToArray());
}
