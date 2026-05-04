using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AdminService.Services;

namespace AdminService.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("admin")]
public class AdminController : ControllerBase
{
    private readonly AdminService.Services.AdminService _service;

    public AdminController(AdminService.Services.AdminService service)
    {
        _service = service;
    }

    [HttpGet("dashboard")]
    public async Task<IActionResult> GetDashboard()
    {
        var token = Request.Headers["Authorization"]
            .ToString()
            .Replace("Bearer ", "");

        var result = await _service.GetDashboard(token);

        return Ok(result);
    }
}