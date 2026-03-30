using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;
using EVCS.Domain.Enums;
using Microsoft.AspNetCore.Mvc;

namespace EVCS.Api.Controllers;

[ApiController]
[Route("api/usage-history")]
public class UsageHistoryController : ControllerBase
{
    private readonly IUsageHistoryService _usageHistoryService;

    public UsageHistoryController(IUsageHistoryService usageHistoryService)
    {
        _usageHistoryService = usageHistoryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyCollection<UsageHistoryItemDto>>>> GetList(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? stationId,
        [FromQuery] int? poleId,
        [FromQuery] SessionStatus? status,
        CancellationToken cancellationToken)
    {
        var filter = new UsageHistoryFilter(fromDate, toDate, stationId, poleId, status);
        var data = await _usageHistoryService.GetListAsync(filter, cancellationToken);
        return Ok(ApiResponse<IReadOnlyCollection<UsageHistoryItemDto>>.Ok(data));
    }

    [HttpGet("export-csv")]
    public async Task<IActionResult> ExportCsv(
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? stationId,
        [FromQuery] int? poleId,
        [FromQuery] SessionStatus? status,
        CancellationToken cancellationToken)
    {
        var filter = new UsageHistoryFilter(fromDate, toDate, stationId, poleId, status);
        var file = await _usageHistoryService.ExportCsvAsync(filter, cancellationToken);
        return File(file.Content, file.ContentType, file.FileName);
    }
}
