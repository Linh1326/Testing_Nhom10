using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
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
        [FromQuery] AlertStatus? status,
        [FromQuery] AlertSeverity? severity,
        CancellationToken cancellationToken)
    {
        var data = await _alertService.GetListAsync(new AlertFilter(stationId, status, severity), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<AlertItemDto>>.Ok(data));
    }

    [HttpGet("{id:long}")]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> GetById(
        long id,
        CancellationToken cancellationToken)
    {
        var data = await _alertService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<AlertItemDto>>> Create(
        [FromBody] CreateAlertRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _alertService.CreateAsync(request, cancellationToken);
        return Ok(ApiResponse<AlertItemDto>.Ok(data, "Ghi nhận cảnh báo thành công."));
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
}
