using EVCS.Application.DTOs;

namespace EVCS.Application.Abstractions.Services;

public interface IPoleService
{
    Task<List<PoleDetailDto>> GetListAsync(int? stationId, string? keyword, string? status);
    Task<PoleDetailDto?> GetByIdAsync(int id);
    Task<int> CreateAsync(CreatePoleDto input);
    Task UpdateAsync(UpdatePoleRequest input, int id );
    Task DeleteAsync(int id);
    Task DisableAsync(int id);
}