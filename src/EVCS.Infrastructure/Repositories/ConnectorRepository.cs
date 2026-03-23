using EVCS.Application.Abstractions.Persistence;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;
using EVCS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Infrastructure.Repositories;

public sealed class ConnectorRepository : IConnectorRepository
{
    private readonly AppDbContext _context;

    public ConnectorRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Connector>> GetListAsync(int? poleId, int? chargeTypeId, EquipmentStatus? status, CancellationToken cancellationToken)
    {
        var query = _context.Connectors
            .Include(x => x.Pole)
            .Include(x => x.ChargeType)
            .AsQueryable();

        if (poleId.HasValue)
        {
            query = query.Where(x => x.PoleId == poleId.Value);
        }

        if (chargeTypeId.HasValue)
        {
            query = query.Where(x => x.ChargeTypeId == chargeTypeId.Value);
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Connector?> GetByIdAsync(int id, bool includeChildren, CancellationToken cancellationToken)
    {
        IQueryable<Connector> query = _context.Connectors;

        if (includeChildren)
        {
            query = query
                .Include(x => x.Pole)
                .Include(x => x.ChargeType);
        }

        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToLower();
        return _context.Connectors.AnyAsync(
            x => x.Code.ToLower() == normalized && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveByPoleIdAsync(int poleId, CancellationToken cancellationToken)
        => _context.Connectors.AnyAsync(
            x => x.PoleId == poleId && x.Status != EquipmentStatus.Disabled,
            cancellationToken);

    public Task AddAsync(Connector connector, CancellationToken cancellationToken)
        => _context.Connectors.AddAsync(connector, cancellationToken).AsTask();

    public void Remove(Connector connector)
        => _context.Connectors.Remove(connector);
}
