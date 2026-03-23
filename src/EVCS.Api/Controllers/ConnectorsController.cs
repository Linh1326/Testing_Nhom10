using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
using EVCS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Api.Controllers;

[ApiController]
[Route("api/connectors")]
public class ConnectorsController : ControllerBase
{
    private readonly IConnectorService _connectorService;

    public ConnectorsController(IConnectorService connectorService)
    {
        _connectorService = connectorService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<ConnectorSummaryDto>>>> GetList(
        [FromQuery] int? poleId,
        [FromQuery] int? chargeTypeId,
        [FromQuery] EquipmentStatus? status,
        CancellationToken cancellationToken)
    {
        var data = await _connectorService.GetListAsync(new ConnectorListQuery(poleId, chargeTypeId, status), cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<ConnectorSummaryDto>>.Ok(data));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApiResponse<ConnectorDetailDto>>> GetById(int id, CancellationToken cancellationToken)
    {
        var data = await _connectorService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<ConnectorDetailDto>.Ok(data));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ConnectorDetailDto>>> Create(
        [FromBody] CreateConnectorRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _connectorService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = data.Id }, ApiResponse<ConnectorDetailDto>.Ok(data, "Tạo đầu nối thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApiResponse<ConnectorDetailDto>>> Update(
        int id,
        [FromBody] UpdateConnectorRequest request,
        CancellationToken cancellationToken)
    {
        var data = await _connectorService.UpdateAsync(id, request, cancellationToken);
        return Ok(ApiResponse<ConnectorDetailDto>.Ok(data, "Cập nhật đầu nối thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult<ApiResponse<object>>> Delete(int id, CancellationToken cancellationToken)
    {
        await _connectorService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Xóa đầu nối thành công."));
    }
}
