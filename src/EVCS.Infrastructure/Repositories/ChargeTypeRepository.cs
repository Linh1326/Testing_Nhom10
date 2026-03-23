using EVCS.Application.Abstractions.Persistence;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;
using EVCS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Infrastructure.Repositories;

public sealed class ChargeTypeRepository : IChargeTypeRepository
{
    private readonly AppDbContext _context;

    public ChargeTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<ChargeType>> GetListAsync(string? keyword, EquipmentStatus? status, CancellationToken cancellationToken)
    {
        var query = _context.ChargeTypes.AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var normalized = keyword.Trim();
            query = query.Where(x => x.Code.Contains(normalized) || x.Name.Contains(normalized));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        return query
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public Task<ChargeType?> GetByIdAsync(int id, CancellationToken cancellationToken)
        => _context.ChargeTypes.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task<bool> ExistsByCodeAsync(string code, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = code.Trim().ToLower();
        return _context.ChargeTypes.AnyAsync(
            x => x.Code.ToLower() == normalized && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> ExistsByNameAsync(string name, int? excludeId, CancellationToken cancellationToken)
    {
        var normalized = name.Trim().ToLower();
        return _context.ChargeTypes.AnyAsync(
            x => x.Name.ToLower() == normalized && (!excludeId.HasValue || x.Id != excludeId.Value),
            cancellationToken);
    }

    public Task<bool> IsInUseAsync(int chargeTypeId, CancellationToken cancellationToken)
        => _context.Connectors.AnyAsync(x => x.ChargeTypeId == chargeTypeId, cancellationToken);

    public Task AddAsync(ChargeType chargeType, CancellationToken cancellationToken)
        => _context.ChargeTypes.AddAsync(chargeType, cancellationToken).AsTask();

    public void Remove(ChargeType chargeType)
        => _context.ChargeTypes.Remove(chargeType);
}
