using System.Globalization;
using System.Text.RegularExpressions;
using EVCS.Application.Abstractions.Persistence;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.Common;
using EVCS.Application.DTOs;
using EVCS.Domain.Entities;
using EVCS.Domain.Enums;

namespace EVCS.Application.Services;

public sealed class AlertService : IAlertService
{
    private const string LogPrefix = "[LOG]|";

    private readonly IAlertRepository _alertRepository;
    private readonly IStationRepository _stationRepository;
    private readonly IPoleRepository _poleRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AlertService(
        IAlertRepository alertRepository,
        IStationRepository stationRepository,
        IPoleRepository poleRepository,
        IUnitOfWork unitOfWork)
    {
        _alertRepository = alertRepository;
        _stationRepository = stationRepository;
        _poleRepository = poleRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyCollection<AlertItemDto>> GetListAsync(AlertFilter filter, CancellationToken cancellationToken)
    {
        var alerts = await _alertRepository.GetListAsync(filter, cancellationToken);
        return alerts.Select(Map).ToArray();
    }

    public async Task<AlertItemDto> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không tìm thấy cảnh báo.", 404);

        return Map(alert);
    }

    public async Task<AlertItemDto> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        var alertType = FirstFilled(request.AlertType, request.Type);
        var message = FirstFilled(request.Message, request.Description);
        var suggestion = FirstFilled(request.Suggestion, request.Note);
        var severity = ParseSeverity(request.Severity);
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? AlertStatus.Open
            : ParseStatus(request.Status);

        ValidationGuard.AgainstNullOrWhiteSpace(alertType, "Loại cảnh báo không được để trống.");
        ValidationGuard.AgainstNullOrWhiteSpace(message, "Nội dung cảnh báo không được để trống.");

        var station = await _stationRepository.GetByIdAsync(request.StationId, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm thấy trạm sạc.", 404);

        if (request.PoleId.HasValue)
        {
            var pole = await _poleRepository.GetByIdAsync(request.PoleId.Value, includeChildren: false, cancellationToken)
                ?? throw new AppException("Không tìm thấy trụ sạc.", 404);

            ValidationGuard.Against(pole.StationId != station.Id, "Trụ sạc không thuộc trạm đã chọn.");
        }

        var alert = new Alert
        {
            StationId = request.StationId,
            PoleId = request.PoleId,
            AlertType = alertType!.Trim(),
            Message = message!.Trim(),
            Severity = severity,
            Status = status,
            OccurredAt = request.OccurredAt ?? DateTime.UtcNow,
            Note = BuildStoredNote(NormalizeOptionalText(suggestion), Array.Empty<AlertLogDto>())
        };

        await _alertRepository.AddAsync(alert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _alertRepository.GetByIdAsync(alert.Id, cancellationToken)
            ?? throw new AppException("Không thể lấy dữ liệu cảnh báo vừa tạo.", 500);

        return Map(saved);
    }

    public async Task<AlertItemDto> ProcessAsync(long id, ProcessAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không tìm thấy cảnh báo.", 404);

        var previousStatus = alert.Status;
        var currentSuggestion = ExtractSuggestion(alert.Note);
        var nextSuggestion = NormalizeOptionalText(FirstFilled(request.Suggestion, currentSuggestion));
        var logs = ExtractStoredLogs(alert.Note).ToList();

        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            logs.Add(new AlertLogDto(DateTime.UtcNow, request.Note.Trim()));
        }

        alert.Status = ParseStatus(request.Status);

        if (previousStatus != alert.Status)
        {
            logs.Add(new AlertLogDto(
                DateTime.UtcNow,
                alert.Status == AlertStatus.Resolved
                    ? "Cảnh báo đã được đánh dấu là resolved."
                    : $"Trạng thái cảnh báo đã được chuyển sang {ToStatusValue(alert.Status)}."));
        }

        alert.Note = BuildStoredNote(nextSuggestion, logs);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không thể cập nhật cảnh báo.", 500);

        return Map(updated);
    }

    public async Task<AlertItemDto> NotifyMaintenanceAsync(long id, NotifyMaintenanceRequest request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không tìm thấy cảnh báo.", 404);

        ValidationGuard.Against(
            alert.Status == AlertStatus.Resolved,
            "Không thể thông báo bảo trì cho cảnh báo đã được resolved.");

        var suggestion = ExtractSuggestion(alert.Note);
        var logs = ExtractStoredLogs(alert.Note).ToList();
        var recipient = NormalizeOptionalText(request.RecipientName);
        var extraNote = NormalizeOptionalText(request.Note);

        var notificationMessage = string.IsNullOrWhiteSpace(recipient)
            ? "Đã thông báo cho nhân viên bảo trì."
            : $"Đã thông báo cho nhân viên bảo trì: {recipient}.";

        logs.Add(new AlertLogDto(DateTime.UtcNow, notificationMessage));

        if (!string.IsNullOrWhiteSpace(extraNote))
        {
            logs.Add(new AlertLogDto(DateTime.UtcNow, extraNote));
        }

        alert.Note = BuildStoredNote(suggestion, logs);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không thể cập nhật cảnh báo sau khi thông báo bảo trì.", 500);

        return Map(updated);
    }

    public static bool TryParseDisplayId(string displayId, out long id)
    {
        id = 0;
        if (string.IsNullOrWhiteSpace(displayId))
        {
            return false;
        }

        var match = Regex.Match(displayId.Trim(), "(\\d+)$");
        return match.Success && long.TryParse(match.Groups[1].Value, NumberStyles.None, CultureInfo.InvariantCulture, out id);
    }

    private static AlertItemDto Map(Alert alert)
    {
        var displayId = $"ALT-{alert.Id:D4}";
        var suggestion = ExtractSuggestion(alert.Note)
            ?? "Kiểm tra trạm, xác minh nguyên nhân và cập nhật hướng xử lý trong nhật ký vận hành.";
        var logs = BuildLogs(alert);

        return new AlertItemDto(
            alert.Id,
            displayId,
            alert.StationId,
            alert.Station?.Name ?? string.Empty,
            alert.PoleId,
            alert.Pole?.Code,
            alert.AlertType,
            ToSeverityValue(alert.Severity),
            ToStatusValue(alert.Status),
            alert.OccurredAt,
            alert.Message,
            suggestion,
            logs);
    }

    private static IReadOnlyCollection<AlertLogDto> BuildLogs(Alert alert)
    {
        var logs = new List<AlertLogDto>
        {
            new(alert.OccurredAt, $"Hệ thống ghi nhận cảnh báo '{alert.AlertType}'."),
            new(alert.OccurredAt, alert.Message)
        };

        logs.AddRange(ExtractStoredLogs(alert.Note));
        return logs.OrderByDescending(x => x.Time).ToArray();
    }

    private static string? ExtractSuggestion(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return null;
        }

        var lines = note
            .Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries)
            .Where(line => !line.StartsWith(LogPrefix, StringComparison.Ordinal))
            .ToArray();

        if (lines.Length == 0)
        {
            return null;
        }

        return string.Join(Environment.NewLine, lines).Trim();
    }

    private static IReadOnlyCollection<AlertLogDto> ExtractStoredLogs(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return Array.Empty<AlertLogDto>();
        }

        var logs = new List<AlertLogDto>();
        var lines = note.Split(new[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries);

        foreach (var rawLine in lines)
        {
            if (!rawLine.StartsWith(LogPrefix, StringComparison.Ordinal))
            {
                continue;
            }

            var payload = rawLine[LogPrefix.Length..];
            var parts = payload.Split('|', 2);
            if (parts.Length != 2)
            {
                continue;
            }

            if (!DateTime.TryParse(parts[0], CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind, out var time))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(parts[1]))
            {
                continue;
            }

            logs.Add(new AlertLogDto(time, parts[1].Trim()));
        }

        return logs;
    }

    private static string? BuildStoredNote(string? suggestion, IEnumerable<AlertLogDto> logs)
    {
        var lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(suggestion))
        {
            lines.Add(suggestion.Trim());
        }

        lines.AddRange(logs.Select(log => $"{LogPrefix}{log.Time:O}|{log.Message}"));

        return lines.Count == 0 ? null : string.Join(Environment.NewLine, lines);
    }

    private static string? FirstFilled(params string?[] values)
        => values.FirstOrDefault(value => !string.IsNullOrWhiteSpace(value));

    private static string? NormalizeOptionalText(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static AlertSeverity ParseSeverity(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            "low" => AlertSeverity.Low,
            "medium" => AlertSeverity.Medium,
            "critical" => AlertSeverity.Critical,
            _ => throw new AppException("Mức độ cảnh báo không hợp lệ. Chỉ chấp nhận low, medium hoặc critical.")
        };

    private static AlertStatus ParseStatus(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            "open" => AlertStatus.Open,
            "resolved" => AlertStatus.Resolved,
            _ => throw new AppException("Trạng thái cảnh báo không hợp lệ. Chỉ chấp nhận open hoặc resolved.")
        };

    private static string ToSeverityValue(AlertSeverity severity)
        => severity switch
        {
            AlertSeverity.Low => "low",
            AlertSeverity.Medium => "medium",
            AlertSeverity.Critical => "critical",
            _ => "medium"
        };

    private static string ToStatusValue(AlertStatus status)
        => status switch
        {
            AlertStatus.Open => "open",
            AlertStatus.Resolved => "resolved",
            _ => "open"
        };
}
