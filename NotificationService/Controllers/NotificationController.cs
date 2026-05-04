using Microsoft.AspNetCore.Authorization;
using NotificationService.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;


namespace NotificationService.Controllers;

[Authorize]
[ApiController]
[Route("notifications")]
public class NotificationController : ControllerBase
{
    private readonly Services.NotificationService _service;

    public NotificationController(Services.NotificationService service)
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

    private bool IsAdmin()
        => User.IsInRole("Admin");

    [HttpGet("{userId}")]
    public IActionResult GetUserNotifications(int userId)
    {
        if (!IsAdmin() && userId != GetCurrentUserId())
            return Forbid();

        return Ok(_service.GetUserNotifications(userId));
    }

    [HttpPut("read/{id}")]
    public IActionResult MarkAsRead(int id)
    {
        if (!IsAdmin() && !_service.CanAccessNotification(id, GetCurrentUserId()))
            return Forbid();

        return Ok(new { message = _service.MarkAsRead(id) });
    }
}
