using Microsoft.AspNetCore.Mvc;
using EVCS.Application.Abstractions.Services;
using EVCS.Application.DTOs;

namespace EVCS.Api.Controllers;

[ApiController]
[Route("api/poles")]
public class PolesController : ControllerBase
{
    private readonly IPoleService _service;

    public PolesController(IPoleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetList(
        [FromQuery] int? stationId,
        [FromQuery] string? keyword,
        [FromQuery] string? status)
    {
        var result = await _service.GetListAsync(stationId, keyword, status);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var pole = await _service.GetByIdAsync(id);
        if (pole == null) return NotFound();
        return Ok(pole);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePoleDto input)
    {
        try
        {
            var id = await _service.CreateAsync(input);
            return Ok(new { id });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdatePoleRequest input)
    {
        try
        {
            

            await _service.UpdateAsync(input, id);
            return Ok(new { message = "Updated" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(new { message = "Deleted" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPatch("{id}/disable")]
    public async Task<IActionResult> Disable(int id)
    {
        try
        {
            await _service.DisableAsync(id);
            return Ok(new { message = "Disabled" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}