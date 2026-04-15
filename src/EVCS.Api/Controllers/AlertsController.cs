using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
using EVCS.Application.Services;
using EVCS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Api.Controllers;

[ApiController]
[Route("api/alerts")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _alertService;

    public AlertsController(IAlertService alertService)
    {
        _alertService = alertService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<AlertItemDto>>>> GetList(
        [FromQuery] int? stationId,
        [FromQuery] string? status,
        [FromQuery] string? severity,
        [FromQuery] string? keyword,
        CancellationToken cancellationToken)
    {
        var filter = new AlertFilter(
            stationId,
            TryParseStatus(status),
            TryParseSeverity(severity),
            keyword);

        var data = await _alertService.GetListAsync(filter, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AlertItemDto>>.Ok(data));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> GetById(long id, CancellationToken cancellationToken)
    {
        var data = await _alertService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data));
    }

    [HttpGet("by-display-id/{displayId}")]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> GetByDisplayId(string displayId, CancellationToken cancellationToken)
    {
        if (!AlertService.TryParseDisplayId(displayId, out var id))
        {
            return BadRequest(ApiResponse<object>.Fail("Mã cảnh báo không hợp lệ."));
        }

        var data = await _alertService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> Create(
        [FromBody] CreateAlertRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _alertService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.InternalId }, ApiResponse<AlertItemDto>.Ok(data, "Ghi nhận cảnh báo thành công."));
    }

    [HttpPatch("{id:long}")]
    [HttpPatch("{id:long}/process")]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> Process(
        long id,
        [FromBody] ProcessAlertRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _alertService.ProcessAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data, "Cập nhật trạng thái cảnh báo thành công."));
    }

    [HttpPatch("{id:long}/notify-maintenance")]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> NotifyMaintenance(
        long id,
        [FromBody] NotifyMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _alertService.NotifyMaintenanceAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data, "Đã ghi nhận thao tác thông báo cho nhân viên bảo trì."));
    }

    [HttpPatch("by-display-id/{displayId}")]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> ProcessByDisplayId(
        string displayId,
        [FromBody] ProcessAlertRequest request,
        CancellationToken cancellationToken)
    {
        if (!AlertService.TryParseDisplayId(displayId, out var id))
        {
            return BadRequest(ApiResponse<object>.Fail("Mã cảnh báo không hợp lệ."));
        }

        var data = await _alertService.ProcessAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data, "Cập nhật trạng thái cảnh báo thành công."));
    }

    [HttpPatch("by-display-id/{displayId}/notify-maintenance")]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> NotifyMaintenanceByDisplayId(
        string displayId,
        [FromBody] NotifyMaintenanceRequest request,
        CancellationToken cancellationToken)
    {
        if (!AlertService.TryParseDisplayId(displayId, out var id))
        {
            return BadRequest(ApiResponse<object>.Fail("Mã cảnh báo không hợp lệ."));
        }

        var data = await _alertService.NotifyMaintenanceAsync(id, request, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data, "Đã ghi nhận thao tác thông báo cho nhân viên bảo trì."));
    }

    private static AlertStatus? TryParseStatus(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "open" => AlertStatus.Open,
            "resolved" => AlertStatus.Resolved,
            _ => throw new Application.Common.AppException("Giá trị status không hợp lệ. Chỉ chấp nhận open hoặc resolved.")
        };
    }

    private static AlertSeverity? TryParseSeverity(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("all", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant() switch
        {
            "low" => AlertSeverity.Low,
            "medium" => AlertSeverity.Medium,
            "critical" => AlertSeverity.Critical,
            _ => throw new Application.Common.AppException("Giá trị severity không hợp lệ. Chỉ chấp nhận low, medium hoặc critical.")
        };
    }
}
