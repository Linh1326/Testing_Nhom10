using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Abstractions.Persistence;

public interface IPoleRepository
{
    Task<List<Pole>> GetListAsync(int? stationId, string? keyword, EquipmentStatus? status, CancellationToken cancellationToken);
    Task<Pole?> GetByIdAsync(int id, bool includeChildren, CancellationToken cancellationToken);
    Task<List<Pole>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId, CancellationToken cancellationToken);
    Task<bool> ExistsActiveByStationIdAsync(int stationId, CancellationToken cancellationToken);
    Task AddAsync(Pole pole, CancellationToken cancellationToken);
    void Remove(Pole pole);
}
