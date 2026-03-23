using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
using EVCS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Api.Controllers;

[ApiController]
[Route("api/charge-types")]
public class ChargeTypesController : ControllerBase
{
    private readonly IChargeTypeService _chargeTypeService;

    public ChargeTypesController(IChargeTypeService chargeTypeService)
    {
        _chargeTypeService = chargeTypeService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ChargeTypeSummaryDto>>>> GetList(
        [FromQuery] string? keyword,
        [FromQuery] EquipmentStatus? status,
        CancellationToken cancellationToken)
    {
        var data = await _chargeTypeService.GetListAsync(new ChargeTypeListQuery(keyword, status), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ChargeTypeSummaryDto>>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ChargeTypeDetailDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var data = await _chargeTypeService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ChargeTypeDetailDto>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ChargeTypeDetailDto>>> Create(
        [FromBody] CreateChargeTypeRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _chargeTypeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<ChargeTypeDetailDto>.Ok(data, "Tạo loại sạc thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ChargeTypeDetailDto>>> Update(
        int id,
        [FromBody] UpdateChargeTypeRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _chargeTypeService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ChargeTypeDetailDto>.Ok(data, "Cập nhật loại sạc thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _chargeTypeService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Xóa loại sạc thành công."));
    }
}
