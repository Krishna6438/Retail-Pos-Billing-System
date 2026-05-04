using Microsoft.AspNetCore.Mvc;
using AuthService.DTOs;
using AuthService.Services;

namespace AuthService.Controllers;

[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly AuthService.Services.AuthService _service;

    public AuthController(AuthService.Services.AuthService service)
    {
        _service = service;
    }

    [HttpPost("register")]
    public IActionResult Register(RegisterRequestDTO dto)
    {
        var result = _service.Register(dto);
        return Ok(result);
    }

    [HttpPost("login")]
    public IActionResult Login(LoginRequestDTO dto)
    {
        var result = _service.Login(dto);

        if (result == null)
            return Unauthorized("Invalid credentials");

        return Ok(result);
    }
}