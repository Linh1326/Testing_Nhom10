using EVCS.Application.Abstractions.Persistence;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;
using EVCS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Infrastructure.Repositories;

public sealed class PoleRepository : IPoleRepository
{
    private readonly AppDbContext _context;

    public PoleRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Pole>> GetListAsync(int? stationId, string? keyword, EquipmentStatus? status, CancellationToken cancellationToken)
    {
        var query = _context.Poles
            .Include(x => x.Station)
            .AsQueryable();

        if (stationId.HasValue)
        {
            query = query.Where(x => x.StationId == stationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalized = keyword.Trim();
            query = query.Where(x => x.Name.Contains(normalized) || x.Code.Contains(normalized));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Pole?> GetByIdAsync(int id, bool includeChildren, CancellationToken cancellationToken)
    {
        IQueryable<Pole> query = _context.Poles;

        if (includeChildren)
        {
            query = query
                .Include(x => x.Station);
        }

        return query.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public Task<List<Pole>> GetByIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var idSet = ids.ToHashSet();
        return _context.Poles.Where(x => idSet.Contains(x.Id)).ToListAsync(cancellationToken);
    }

    public Task<bool> ExistsByCodeAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToLower();
        return _context.Poles.AnyAsync(
            x => x.Code.ToLower() == normalized && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsActiveByStationIdAsync(int stationId, CancellationToken cancellationToken)
        => _context.Poles.AnyAsync(
            x => x.StationId == stationId && x.Status != EquipmentStatus.Disabled,
            cancellationToken);

    public Task AddAsync(Pole pole, CancellationToken cancellationToken)
        => _context.Poles.AddAsync(pole, cancellationToken).AsTask();

    public void Remove(Pole pole)
        => _context.Poles.Remove(pole);
}
