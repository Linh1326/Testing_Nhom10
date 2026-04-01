using System.Globalization;
using System.Text;
using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;

namespace EVCS.Application.Services;

public sealed class UsageHistoryService : IUsageHistoryService
{
    private readonly IChargingSessionRepository _chargingSessionRepository;

    public UsageHistoryService(IChargingSessionRepository chargingSessionRepository)
    {
        _chargingSessionRepository = chargingSessionRepository;
    }

    public async Task<IReadOnlyCollection<UsageHistoryItemDto>> GetListAsync(UsageHistoryFilter filter, CancellationToken cancellationToken)
    {
        var sessions = await _chargingSessionRepository.GetListAsync(filter, cancellationToken);

        return sessions
            .Select(s => new UsageHistoryItemDto(
                s.Id,
                s.StationId,
                s.Station?.Name ?? string.Empty,
                s.PoleId,
                s.Pole?.Code,
                s.StartedAt,
                s.EndedAt,
                s.EnergyKwh,
                s.Cost,
                s.Status))
            .ToArray();
    }

    public async Task<ExportFileDto> ExportCsvAsync(UsageHistoryFilter filter, CancellationToken cancellationToken)
    {
        var rows = await GetListAsync(filter, cancellationToken);

        var builder = new StringBuilder();
        builder.AppendLine("SessionId;Station;Pole;StartedAt;EndedAt;EnergyKwh;Cost;Status");

        foreach (var row in rows)
        {
            builder
                .Append(row.Id).Append(';')
                .Append(SafeCsv(row.StationName)).Append(';')
                .Append(SafeCsv(row.PoleCode)).Append(';')
                .Append(row.StartedAt.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.EndedAt?.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture)).Append(';')
                .Append(row.EnergyKwh.ToString(CultureInfo.InvariantCulture)).Append(';')
                .Append(row.Cost.ToString(CultureInfo.InvariantCulture)).Append(';')
                .Append(row.Status)
                .AppendLine();
        }

        var utf8WithBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: true);
        return new ExportFileDto(
            $"lich-su-sac-{DateTime.UtcNow:yyyyMMddHHmmss}.csv",
            "text/csv",
            utf8WithBom.GetBytes(builder.ToString()));
    }

    private static string SafeCsv(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Replace(';', ',').Trim();
    }
}
