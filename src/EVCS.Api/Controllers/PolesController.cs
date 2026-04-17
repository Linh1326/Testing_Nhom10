using EVCS.Api.Contracts;
using EVCS.Application.Abstractions.Persistence;
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
    private readonly IStationRepository _stationRepo;

    public PolesController(IPoleService poleService, IStationRepository stationRepo)
    {
        _poleService = poleService;
        _stationRepo = stationRepo;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int? stationId,
        [FromQuery] string? keyword,
        [FromQuery] string? status,
        CancellationToken cancellationToken)
    {
        PoleStatus? poleStatus = status?.ToLower() switch
        {
            "active" or "available" => PoleStatus.Available,
            "inactive" => PoleStatus.Inactive,
            "fault" => PoleStatus.Fault,
            "in_use" or "inuse" => PoleStatus.InUse,
            _ => null
        };

        var data = await _poleService.GetListAsync(new PoleListQuery(stationId, keyword, poleStatus), cancellationToken);
        return Ok(ApiResponse<object>.Ok(data.Select(ToFrontend).ToArray()));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var data = await _poleService.GetByIdAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ToFrontend(data)));
    }

    // Frontend sends stationId as string code "ST001" or numeric string
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FrontendPoleRequest request, CancellationToken cancellationToken)
    {
        var stationNumericId = await ResolveStationId(request.StationId, cancellationToken);
        if (stationNumericId is null)
            return BadRequest(ApiResponse<object>.Fail("Không tìm thấy trạm sạc."));

        var poleStatus = request.Status?.ToLower() is "inactive" ? PoleStatus.Inactive : PoleStatus.Available;
        var req = new CreatePoleRequest(
            stationNumericId.Value,
            request.Name ?? "",
            request.ActiveCode ?? request.Id ?? "",
            request.Model,
            request.Manufacturer,
            request.Connectors?.Length ?? 1,
            poleStatus,
            string.IsNullOrEmpty(request.InstalledAt) ? null : DateTime.TryParse(request.InstalledAt, out var dt) ? dt : null);

        var data = await _poleService.CreateAsync(req, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ToFrontend(data), "Tạo trụ sạc thành công."));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] FrontendPoleRequest request, CancellationToken cancellationToken)
    {
        var stationNumericId = await ResolveStationId(request.StationId, cancellationToken);
        if (stationNumericId is null)
            return BadRequest(ApiResponse<object>.Fail("Không tìm thấy trạm sạc."));

        var poleStatus = request.Status?.ToLower() is "inactive" ? PoleStatus.Inactive : PoleStatus.Available;
        var req = new UpdatePoleRequest(
            stationNumericId.Value,
            request.Name ?? "",
            request.ActiveCode ?? request.Id ?? "",
            request.Model,
            request.Manufacturer,
            request.Connectors?.Length ?? 1,
            poleStatus,
            string.IsNullOrEmpty(request.InstalledAt) ? null : DateTime.TryParse(request.InstalledAt, out var dt) ? dt : null);

        var data = await _poleService.UpdateAsync(id, req, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ToFrontend(data), "Cập nhật trụ sạc thành công."));
    }

    [HttpPatch("{id:int}/deactivate")]
    public async Task<IActionResult> Deactivate(int id, CancellationToken cancellationToken)
    {
        var data = await _poleService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ToFrontend(data), "Ngừng hoạt động trụ sạc thành công."));
    }

    [HttpPatch("{id:int}/activate")]
    public async Task<IActionResult> Activate(int id, CancellationToken cancellationToken)
    {
        var data = await _poleService.ActivateAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ToFrontend(data), "Kích hoạt trụ sạc thành công."));
    }

    // Frontend calls PATCH /poles/{id}/status with { status: "Active"|"Inactive" }
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> SetStatus(int id, [FromBody] SetStatusRequest request, CancellationToken cancellationToken)
    {
        var isActive = request.Status?.ToLower() is "active";
        var data = isActive
            ? await _poleService.ActivateAsync(id, cancellationToken)
            : await _poleService.DeactivateAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(ToFrontend(data), isActive ? "Kích hoạt thành công." : "Ngừng hoạt động thành công."));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        await _poleService.DeleteAsync(id, cancellationToken);
        return Ok(ApiResponse<object>.Ok(new { }, "Xóa trụ sạc thành công."));
    }

    // Resolve stationId: accepts "ST001" (code) or "1" (numeric string)
    private async Task<int?> ResolveStationId(string? stationId, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(stationId)) return null;
        if (int.TryParse(stationId, out var numId)) return numId;
        var station = await _stationRepo.GetByCodeAsync(stationId.Trim(), ct);
        return station?.Id;
    }

    private static object ToFrontend(PoleSummaryDto p) => new
    {
        id = p.Code,
        numericId = p.Id,
        name = p.Name,
        activeCode = p.Code,
        manufacturer = p.Manufacturer ?? "",
        model = p.Model ?? "",
        station = p.StationName,
        stationId = p.StationCode,
        stationNumericId = p.StationId,
        status = p.Status is PoleStatus.Available or PoleStatus.InUse ? "Active" : "Inactive",
        installedAt = p.InstalledAt?.ToString("o") ?? DateTime.Now.ToString("o"),
        connectors = BuildConnectors(p.NumberOfPorts, p.Status),
        createdAt = p.CreatedAt.ToString("o"),
        updatedAt = p.UpdatedAt?.ToString("o")
    };

    private static string[] BuildConnectors(int count, PoleStatus status)
    {
        var s = status is PoleStatus.Inactive or PoleStatus.Fault ? "Inactive" : "Available";
        return Enumerable.Range(1, Math.Max(1, count)).Select(i => $"Connector {i} / {s}").ToArray();
    }

    public record FrontendPoleRequest(
        string? Id, string? Name, string? ActiveCode, string? Manufacturer,
        string? Model, string? StationId, string? InstalledAt, string? Status,
        string[]? Connectors);

    public record SetStatusRequest(string? Status);
}
