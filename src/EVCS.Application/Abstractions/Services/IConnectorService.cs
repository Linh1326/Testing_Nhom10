using EVCS.Application.DTOs;

namespace EVCS.Application.Abstractions.Services;

public interface IConnectorService
{
    Task<IReadOnlyCollection<ConnectorSummaryDto>> GetListAsync(ConnectorListQuery query, CancellationToken cancellationToken);
    Task<ConnectorDetailDto> GetByIdAsync(int id, CancellationToken cancellationToken);
    Task<ConnectorDetailDto> CreateAsync(CreateConnectorRequest request, CancellationToken cancellationToken);
    Task<ConnectorDetailDto> UpdateAsync(int id, UpdateConnectorRequest request, CancellationToken cancellationToken);
    Task DeleteAsync(int id, CancellationToken cancellationToken);
}
