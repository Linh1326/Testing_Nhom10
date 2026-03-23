using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Abstractions.Persistence;

public interface IStationRepository
{
    Task<List<Station>> GetListAsync(string? keyword, EquipmentStatus? status, CancellationToken cancellationToken);
    Task<Station?> GetByIdAsync(int id, bool includeChildren, CancellationToken cancellationToken);
    Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken);
    Task AddAsync(Station station, CancellationToken cancellationToken);
    void Remove(Station station);
    Task<int> CountAsync(CancellationToken cancellationToken);
    Task<int> CountByStatusAsync(EquipmentStatus status, CancellationToken cancellationToken);
}
