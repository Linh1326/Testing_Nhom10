using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.Common;
using EVCS.Application.DTOs;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Services;

public sealed class PoleService : IPoleService
{
    private readonly IPoleRepository _poleRepository;
    private readonly IStationRepository _stationRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PoleService(
        IPoleRepository poleRepository,
        IStationRepository stationRepository,
        IUnitOfWork unitOfWork)
    {
        _poleRepository = poleRepository;
        _stationRepository = stationRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<PoleSummaryDto>> GetListAsync(PoleListQuery query, CancellationToken cancellationToken)
    {
        var poles = await _poleRepository.GetListAsync(query.StationId, query.Keyword, query.Status, cancellationToken);

        return poles
            .Select(p => new PoleSummaryDto(
                p.Id,
                p.StationId,
                p.Station?.Name ?? string.Empty,
                p.Name,
                p.Code,
                p.Model,
                p.Manufacturer,
                p.Status,
                p.InstalledAt))
            .ToArray();
    }

    public async Task<PoleDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken)
    {
        var pole = await _poleRepository.GetByIdAsync(id, includeChildren: true, cancellationToken)
            ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

        return MapDetail(pole);
    }

    public async Task<PoleDetailDto> CreateAsync(CreatePoleRequest request, CancellationToken cancellationToken)
    {
        ValidationGuard.AgainstNullOrWhiteSpace(request.Name, "Tên trụ sạc không được để trống.");
        ValidationGuard.AgainstNullOrWhiteSpace(request.Code, "Mã trụ không được để trống.");

        var station = await _stationRepository.GetByIdAsync(request.StationId, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc để gắn trụ.", 404);

        var existed = await _poleRepository.ExistsByCodeAsync(request.Code.Trim(), null, cancellationToken);
        ValidationGuard.Against(existed, "Mã trụ đã tồn tại.");

        var pole = new Pole
        {
            StationId = station.Id,
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Model = request.Model?.Trim(),
            Manufacturer = request.Manufacturer?.Trim(),
            Status = request.Status ?? EquipmentStatus.Available,
            InstalledAt = request.InstalledAt,
            CreatedAt = DateTime.UtcNow
        };

        await _poleRepository.AddAsync(pole, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(pole.Id, cancellationToken);
    }

    public async Task<PoleDetailDto> UpdateAsync(int id, UpdatePoleRequest request, CancellationToken cancellationToken)
    {
        var pole = await _poleRepository.GetByIdAsync(id, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

        ValidationGuard.AgainstNullOrWhiteSpace(request.Name, "Tên trụ sạc không được để trống.");
        ValidationGuard.AgainstNullOrWhiteSpace(request.Code, "Mã trụ không được để trống.");

        var station = await _stationRepository.GetByIdAsync(request.StationId, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc để gắn trụ.", 404);

        var existed = await _poleRepository.ExistsByCodeAsync(request.Code.Trim(), id, cancellationToken);
        ValidationGuard.Against(existed, "Thông tin trụ đã tồn tại hoặc không hợp lệ.");

        pole.Name = request.Name.Trim();
        pole.Code = request.Code.Trim();
        pole.Model = request.Model?.Trim();
        pole.Manufacturer = request.Manufacturer?.Trim();
        pole.StationId = station.Id;
        pole.Status = request.Status;
        pole.InstalledAt = request.InstalledAt;
        pole.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    public async Task DeleteAsync(int id, CancellationToken cancellationToken)
    {
        var pole = await _poleRepository.GetByIdAsync(id, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

        _poleRepository.Remove(pole);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<PoleDetailDto> DeactivateAsync(int id, CancellationToken cancellationToken)
    {
        var pole = await _poleRepository.GetByIdAsync(id, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

        pole.Status = EquipmentStatus.Disabled;
        pole.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return await GetByIdAsync(id, cancellationToken);
    }

    private static PoleDetailDto MapDetail(Pole pole)
        => new(
            pole.Id,
            pole.StationId,
            pole.Station?.Name ?? string.Empty,
            pole.Name,
            pole.Code,
            pole.Model,
            pole.Manufacturer,
            pole.Status,
            pole.InstalledAt,
            pole.CreatedAt,
            pole.UpdatedAt);
}
