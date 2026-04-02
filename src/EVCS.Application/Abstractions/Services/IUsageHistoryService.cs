using EVCS.Application.DTOs;

namespace EVCS.Application.Abstractions.Services;

public interface IUsageHistoryService
{
    Task<IReadOnlyCollection<UsageHistoryItemDto>> GetListAsync(UsageHistoryFilter filter, CancellationToken cancellationToken);
    Task<ExportFileDto> ExportCsvAsync(UsageHistoryFilter filter, CancellationToken cancellationToken);
}
