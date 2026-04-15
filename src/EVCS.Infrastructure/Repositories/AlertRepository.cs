using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.DTOs;
using EVCS.Domain.Entities;
using EVCS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EVCS.Infrastructure.Repositories;

public sealed class AlertRepository : IAlertRepository
{
    private readonly AppDbContext _context;

    public AlertRepository(AppDbContext context)
    {
        _context = context;
    }

    public Task<List<Alert>> GetListAsync(AlertFilter filter, CancellationToken cancellationToken)
    {
        var query = _context.Alerts
            .Include(x => x.Station)
            .Include(x => x.Pole)
            .AsQueryable();

        if (filter.StationId.HasValue)
        {
            query = query.Where(x => x.StationId == filter.StationId.Value);
        }

        if (filter.Status.HasValue)
        {
            query = query.Where(x => x.Status == filter.Status.Value);
        }

        if (filter.Severity.HasValue)
        {
            query = query.Where(x => x.Severity == filter.Severity.Value);
        }

        if (!string.IsNullOrWhiteSpace(filter.Keyword))
        {
            var keyword = filter.Keyword.Trim();
            query = query.Where(x =>
                x.AlertType.Contains(keyword) ||
                x.Message.Contains(keyword) ||
                (x.Note != null && x.Note.Contains(keyword)) ||
                (x.Station != null && x.Station.Name.Contains(keyword)));
        }

        return query
            .OrderByDescending(x => x.OccurredAt)
            .ToListAsync(cancellationToken);
    }

    public Task<Alert?> GetByIdAsync(long id, CancellationToken cancellationToken)
        => _context.Alerts
            .Include(x => x.Station)
            .Include(x => x.Pole)
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

    public Task AddAsync(Alert alert, CancellationToken cancellationToken)
        => _context.Alerts.AddAsync(alert, cancellationToken).AsTask();
}
