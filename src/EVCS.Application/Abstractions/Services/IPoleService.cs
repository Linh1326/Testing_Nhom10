using EVCS.Application.DTOs;

namespace EVCS.Application.Abstractions.Services;

public interface IPoleService
{
    Task<IReadOnlyCollection<PoleSummaryDto>> GetListAsync(PoleListQuery query, CancellationToken cancellationToken);
    Task<PoleDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<PoleDetailDto> CreateAsync(CreatePoleRequest request, CancellationToken cancellationToken);
    Task<PoleDetailDto> UpdateAsync(int id, UpdatePoleRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
    Task<PoleDetailDto> DeactivateAsync(int id, CancellationToken cancellationToken);
}
