using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
using EVCS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Api.Controllers;

[ApiController]
[Route("api/poles")]
public class PolesController : ControllerBase
{
    private readonly IPoleService _poleService;

    public PolesController(IPoleService poleService)
    {
        _poleService = poleService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<PoleSummaryDto>>>> GetList(
        [FromQuery] int? stationId,
        [FromQuery] string? keyword,
        [FromQuery] EquipmentStatus? status,
        CancellationToken cancellationToken)
    {
        var data = await _poleService.GetListAsync(new PoleListQuery(stationId, keyword, status), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<PoleSummaryDto>>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<PoleDetailDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var data = await _poleService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<PoleDetailDto>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PoleDetailDto>>> Create(
        [FromBody] CreatePoleRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _poleService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<PoleDetailDto>.Ok(data, "Tạo trụ sạc thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<PoleDetailDto>>> Update(
        int id,
        [FromBody] UpdatePoleRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _poleService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<PoleDetailDto>.Ok(data, "Cập nhật trụ sạc thành công."));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<ActionResult<ApiResponse<PoleDetailDto>>> Deactivate(int id, CancellationToken cancellationToken)
    {
        var data = await _poleService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse<PoleDetailDto>.Ok(data, "Ngừng hoạt động trụ sạc thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _poleService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Xóa trụ sạc thành công."));
    }
}
