using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Services;

public class PoleService : IPoleService
{
    private readonly IPoleRepository _repo;
    private readonly IUnitOfWork _unitOfWork;

    public PoleService(IPoleRepository repo, IUnitOfWork unitOfWork)
    {
        _repo = repo;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<PoleDetailDto>> GetListAsync(int? stationId, string? keyword, string? status)
    {
        EquipmentStatus? parsedStatus = null;
        if (!string.IsNullOrEmpty(status) && Enum.TryParse<EquipmentStatus>(status, true, out var temp))
            parsedStatus = temp;

        var poles = await _repo.GetListAsync(stationId, keyword, parsedStatus, CancellationToken.None);

        return poles.Select(p => new PoleDetailDto(
            p.Id,
            p.StationId,
            p.Station?.Name ?? "",
            p.Name,
            p.Code,
            p.Model,
            p.Manufacturer,
            p.Status,
            p.InstalledAt,
            p.CreatedAt,
            p.UpdatedAt
        )).ToList();
    }

    public async Task<PoleDetailDto?> GetByIdAsync(int id)
    {
        var pole = await _repo.GetByIdAsync(id, true, CancellationToken.None);
        if (pole == null) return null;

        return new PoleDetailDto(
            pole.Id,
            pole.StationId,
            pole.Station?.Name ?? "",
            pole.Name,
            pole.Code,
            pole.Model,
            pole.Manufacturer,
            pole.Status,
            pole.InstalledAt,
            pole.CreatedAt,
            pole.UpdatedAt
        );
    }

    public async Task<int> CreateAsync(CreatePoleDto input)
    {
        // Kiểm tra trùng Code
        var exists = await _repo.ExistsByCodeAsync(input.Code, null, CancellationToken.None);
        if (exists)
            throw new Exception("Pole code already exists");

        var pole = new Pole
        {
            StationId = input.StationId,
            Name = input.Name,
            Code = input.Code,
            Model = input.Model,
            Manufacturer = input.Manufacturer,
            Status = EquipmentStatus.Available,
            InstalledAt = input.InstalledAt,
            CreatedAt = DateTime.UtcNow
        };

        await _repo.AddAsync(pole, CancellationToken.None);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);

        return pole.Id;
    }

    public async Task UpdateAsync(UpdatePoleRequest input, int id)
    {
        var existing = await _repo.GetByIdAsync(id, false, CancellationToken.None);
        if (existing == null)
            throw new Exception("Pole not found");

        // Kiểm tra trùng Code (trừ chính nó)
        var codeExists = await _repo.ExistsByCodeAsync(input.Code, id, CancellationToken.None);
        if (codeExists)
            throw new Exception("Pole code already exists");

        existing.Name = input.Name;
        existing.Code = input.Code;
        existing.Model = input.Model;
        existing.Manufacturer = input.Manufacturer;
        existing.Status = input.Status;
        existing.StationId = input.StationId;
        existing.InstalledAt = input.InstalledAt;
        existing.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DeleteAsync(int id)
    {
        var pole = await _repo.GetByIdAsync(id, false, CancellationToken.None);
        if (pole == null)
            throw new Exception("Pole not found");

        _repo.Remove(pole);
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);
    }

    public async Task DisableAsync(int id)
    {
        var pole = await _repo.GetByIdAsync(id, false, CancellationToken.None);
        if (pole == null)
            throw new Exception("Pole not found");

        pole.Status = EquipmentStatus.Disabled;
        await _unitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}