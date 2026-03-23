using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Abstractions.Persistence;

public interface IConnectorRepository
{
    Task<List<Connector>> GetListAsync(int? poleId, int? chargeTypeId, EquipmentStatus? status, CancellationToken cancellationToken);
    Task<Connector?> GetByIdAsync(int id, bool includeChildren, CancellationToken cancellationToken);
    Task<bool> ExistsByCodeAsync(string code, int? excludeId, CancellationToken cancellationToken);
    Task<bool> ExistsActiveByPoleIdAsync(int poleId, CancellationToken cancellationToken);
    Task AddAsync(Connector connector, CancellationToken cancellationToken);
    void Remove(Connector connector);
}
