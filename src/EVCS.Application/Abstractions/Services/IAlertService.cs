using EVCS.Application.DTOs;

namespace EVCS.Application.Abstractions.Services;

public interface IAlertService
{
    Task<IReadOnlyCollection<AlertItemDto>> GetListAsync(AlertFilter filter, CancellationToken cancellationToken);
    Task<AlertItemDto> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<AlertItemDto> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken);
    Task<AlertItemDto> ProcessAsync(long id, ProcessAlertRequest request, CancellationToken cancellationToken);
    Task<AlertItemDto> NotifyMaintenanceAsync(long id, NotifyMaintenanceRequest request, CancellationToken cancellationToken);
}
