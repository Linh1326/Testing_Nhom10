using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
using EVCS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Api.Controllers;

[ApiController]
[Route("api/stations")]
public class StationsController : ControllerBase
{
    private readonly IStationService _stationService;

    public StationsController(IStationService stationService)
    {
        _stationService = stationService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<StationSummaryDto>>>> GetList(
        [FromQuery] string? keyword,
        [FromQuery] EquipmentStatus? status,
        CancellationToken cancellationToken)
    {
        var data = await _stationService.GetListAsync(new StationListQuery(keyword, status), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<StationSummaryDto>>.Ok(data));
    }

    [HttpGet("dashboard")]
    public async Task<ActionResult<ApiResponse<StationDashboardDto>>> GetDashboard(CancellationToken cancellationToken)
    {
        var data = await _stationService.GetDashboardAsync(cancellationToken);
        return Ok(ApiResponse<StationDashboardDto>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<StationDetailDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var data = await _stationService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<StationDetailDto>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<StationDetailDto>>> Create(
        [FromBody] CreateStationRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _stationService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<StationDetailDto>.Ok(data, "Tạo trạm sạc thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<StationDetailDto>>> Update(
        int id,
        [FromBody] UpdateStationRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _stationService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<StationDetailDto>.Ok(data, "Cập nhật trạm sạc thành công."));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponse<StationDetailDto>>> Deactivate(int id, CancellationToken cancellationToken)
    {
        var data = await _stationService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse<StationDetailDto>.Ok(data, "Ngừng hoạt động trạm sạc thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _stationService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Xóa trạm sạc thành công."));
    }
}
