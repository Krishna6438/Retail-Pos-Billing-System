using BillingService.DTOs;
using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BillingService.Controllers;

[Authorize]
[ApiController]
[Route("bills")]
public class CartController : ControllerBase
{
    private readonly CartService _service;

    public CartController(CartService service)
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

    [HttpPost("cart/add-item")]
    public async Task<IActionResult> AddItem(AddToCartDTO dto)
    {
        var result = await _service.AddItem(dto);
        return Ok(new { message = result });
    }
    
    [HttpGet("cart/{userId}")]
    public async Task<IActionResult> GetCart(int userId)
    {
        if (!IsAdmin() && userId != GetCurrentUserId())
            return Forbid();

        var cart = await _service.GetCart(userId);

        if (cart == null)
            return NotFound();

        return Ok(cart);
    }
    
    [HttpPut("cart/update-item")]
    public async Task<IActionResult> UpdateItem(UpdateCartDTO dto)
    {
        var result = await _service.UpdateItem(dto);
        return Ok(new { message = result });
    }
    [HttpDelete("cart/remove-item/{userId}/{productId}")]
    public async Task<IActionResult> RemoveItem(int userId, int productId)
    {
        var result = await _service.RemoveItem(userId, productId);
        return Ok(new { message = result });
    }
    
    [HttpPost("cart/{cartId}/hold")]
    public async Task<IActionResult> HoldCart(int cartId)
    {
        if (!IsAdmin() && !_service.CanAccessCart(cartId, GetCurrentUserId()))
            return Forbid();

        var result = await _service.HoldCart(cartId);
        return Ok(new { message = result });
    }
    
    [HttpPost("cart/{cartId}/resume")]
    public async Task<IActionResult> ResumeCart(int cartId)
    {
        if (!IsAdmin() && !_service.CanAccessCart(cartId, GetCurrentUserId()))
            return Forbid();

        var result = await _service.ResumeCart(cartId);
        return Ok(new { message = result });
    }
    
    [HttpPost("cart/{cartId}/checkout")]
    public async Task<IActionResult> Checkout(int cartId, CheckoutDTO dto)
    {
        if (!IsAdmin() && !_service.CanAccessCart(cartId, GetCurrentUserId()))
            return Forbid();

        var result = await _service.Checkout(cartId, dto);
        return Ok(result);
    }
    
    [HttpDelete("cart/{cartId}/clear")]
    public async Task<IActionResult> ClearCart(int cartId)
    {
        if (!IsAdmin() && !_service.CanAccessCart(cartId, GetCurrentUserId()))
            return Forbid();

        var result = await _service.ClearCart(cartId);
        return Ok(new { message = result });
    }
}
