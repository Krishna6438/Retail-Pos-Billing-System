using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BillingService.Controllers;

[Authorize]
[ApiController]
[Route("bills/shifts")]
public class ShiftController : ControllerBase
{
    private readonly ShiftService _service;

    public ShiftController(ShiftService service)
    {
        _service = service;
    }

    private int GetCurrentUserId()
    {
        var claimValue = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (!int.TryParse(claimValue, out var userId))
            throw new UnauthorizedAccessException("Invalid user context");

        return userId;
    }

    [HttpGet("current")]
    public IActionResult GetCurrentShift()
    {
        var shift = _service.GetCurrentShift(GetCurrentUserId());
        return Ok(shift);
    }

    [HttpPost("open")]
    public IActionResult OpenShift([FromBody] OpenShiftRequest request)
    {
        var shiftId = _service.OpenShift(GetCurrentUserId(), request.StartingFloat);
        return Ok(new { message = "Shift opened successfully", shiftId });
    }

    [HttpPost("{shiftId}/close")]
    public IActionResult CloseShift(int shiftId, [FromBody] CloseShiftRequest request)
    {
        var result = _service.CloseShift(GetCurrentUserId(), shiftId, request.ActualCash);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("all")]
    public IActionResult GetAllShifts()
    {
        return Ok(_service.GetAllShifts());
    }
}

public class OpenShiftRequest
{
    public decimal StartingFloat { get; set; }
}

public class CloseShiftRequest
{
    public decimal ActualCash { get; set; }
}
