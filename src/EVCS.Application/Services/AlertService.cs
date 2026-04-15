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
            ?? throw new AppException("Không tìm th?y c?nh báo.", 404);

        return Map(alert);
    }

    public async Task<AlertItemDto> CreateAsync(CreateAlertRequest request, CancellationToken cancellationToken)
    {
        var alertType = FirstFilled(request.AlertType, request.Type);
        var message = FirstFilled(request.Message, request.Description);
        var note = FirstFilled(request.Note, request.Suggestion);
        var severity = ParseSeverity(request.Severity);
        var status = string.IsNullOrWhiteSpace(request.Status)
            ? AlertStatus.Open
            : ParseStatus(request.Status);

        ValidationGuard.AgainstNullOrWhiteSpace(alertType, "Lo?i c?nh báo không du?c d? tr?ng.");
        ValidationGuard.AgainstNullOrWhiteSpace(message, "N?i dung c?nh báo không du?c d? tr?ng.");

        var station = await _stationRepository.GetByIdAsync(request.StationId, includeChildren: false, cancellationToken)
            ?? throw new AppException("Không tìm th?y tr?m s?c.", 404);

        if (request.PoleId.HasValue)
        {
            var pole = await _poleRepository.GetByIdAsync(request.PoleId.Value, includeChildren: false, cancellationToken)
                ?? throw new AppException("Không tìm th?y tr? s?c.", 404);

            ValidationGuard.Against(pole.StationId != station.Id, "Tr? s?c không thu?c tr?m dã ch?n.");
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
            Note = NormalizeOptionalText(note)
        };

        await _alertRepository.AddAsync(alert, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var saved = await _alertRepository.GetByIdAsync(alert.Id, cancellationToken)
            ?? throw new AppException("Không th? l?y d? li?u c?nh báo v?a t?o.", 500);

        return Map(saved);
    }

    public async Task<AlertItemDto> ProcessAsync(long id, ProcessAlertRequest request, CancellationToken cancellationToken)
    {
        var alert = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không tìm th?y c?nh báo.", 404);

        alert.Status = ParseStatus(request.Status);
        alert.Note = NormalizeOptionalText(FirstFilled(request.Note, request.Suggestion));

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var updated = await _alertRepository.GetByIdAsync(id, cancellationToken)
            ?? throw new AppException("Không th? c?p nh?t c?nh báo.", 500);

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
        var note = string.IsNullOrWhiteSpace(alert.Note)
            ? "Ki?m tra tr?m, xác minh nguyên nhân và c?p nh?t hu?ng x? lý trong nh?t ký v?n hành."
            : alert.Note.Trim();

        var logs = BuildLogs(alert, note);

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
            note,
            logs);
    }

    private static IReadOnlyCollection<AlertLogDto> BuildLogs(Alert alert, string note)
    {
        var logs = new List<AlertLogDto>
        {
            new(alert.OccurredAt, $"H? th?ng ghi nh?n c?nh báo '{alert.AlertType}'."),
            new(alert.OccurredAt, alert.Message)
        };

        if (!string.IsNullOrWhiteSpace(note))
        {
            logs.Add(new AlertLogDto(alert.OccurredAt, note));
        }

        if (alert.Status == AlertStatus.Resolved)
        {
            logs.Insert(0, new AlertLogDto(DateTime.UtcNow, "C?nh báo dã du?c dánh d?u là resolved."));
        }

        return logs;
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
            _ => throw new AppException("M?c d? c?nh báo không h?p l?. Ch? ch?p nh?n low, medium ho?c critical.")
        };

    private static AlertStatus ParseStatus(string value)
        => value.Trim().ToLowerInvariant() switch
        {
            "open" => AlertStatus.Open,
            "resolved" => AlertStatus.Resolved,
            _ => throw new AppException("Tr?ng thái c?nh báo không h?p l?. Ch? ch?p nh?n open ho?c resolved.")
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
