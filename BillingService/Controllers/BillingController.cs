using Microsoft.AspNetCore.Mvc;
using BillingService.DTOs;
using BillingService.Services;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BillingService.Controllers;

[Authorize]
[ApiController]
[Route("billing")]
public class BillingController : ControllerBase
{
    private readonly BillingService.Services.BillingService _service;

    public BillingController(BillingService.Services.BillingService service)
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

    //  CREATE BILL
    [HttpPost]
    public async Task<IActionResult> CreateBill([FromBody] CreateBillDTO dto)
    {
        var currentUserId = GetCurrentUserId();

        if (!IsAdmin() && dto.UserId != currentUserId)
            return Forbid();

        var token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

        var result = await _service.CreateBill(dto, token);

        return Ok(result);
    }

    //  GET BILL
    [HttpGet("{billId}")]
    public IActionResult GetBill(int billId)
    {
        var currentUserId = GetCurrentUserId();

        if (!IsAdmin() && !_service.CanAccessBill(billId, currentUserId))
            return Forbid();

        var bill = _service.GetBill(billId);

        if (bill == null)
            return NotFound();

        return Ok(bill);
    }

    //  ANALYTICS
    [Authorize(Roles = "Admin")]
    [HttpGet("analytics/today")]
    public IActionResult GetTodaySales()
        => Ok(_service.GetTodaySales());

    [Authorize(Roles = "Admin")]
    [HttpGet("analytics/revenue")]
    public IActionResult GetRevenue()
        => Ok(_service.GetTotalRevenue());

    [Authorize(Roles = "Admin")]
    [HttpGet("analytics/top-products")]
    public async Task<IActionResult> GetTopProducts()
        => Ok(await _service.GetTopProducts());

    //  FILTERS
    [HttpGet("user/{userId}")]
    public IActionResult GetByUser(int userId)
    {
        if (!IsAdmin() && userId != GetCurrentUserId())
            return Forbid();

        return Ok(_service.GetBillsByUser(userId));
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("date")]
    public IActionResult GetByDate([FromQuery] DateTime date)
        => Ok(_service.GetBillsByDate(date));

    //  COUPON
    [Authorize(Roles = "Admin")]
    [HttpPost("coupon")]
    public IActionResult CreateCoupon([FromBody] CreateCouponDTO dto)
        => Ok(new { message = _service.CreateCoupon(dto) });

    [HttpGet("coupon/{code}")]
    public IActionResult GetCoupon(string code)
    {
        var coupon = _service.GetCouponDetails(code);
        if (coupon == null) return NotFound(new { message = "Invalid or expired coupon" });
        return Ok(coupon);
    }

    [HttpPost("coupon/apply")]
    public IActionResult ApplyCoupon([FromBody] ApplyCouponDTO dto)
    {
        var currentUserId = GetCurrentUserId();

        if (!IsAdmin() && !_service.CanAccessBill(dto.BillId, currentUserId))
            return Forbid();

        return Ok(new { message = _service.ApplyCoupon(dto) });
    }

    //  PAYMENT
    [HttpPut("payment/{billId}")]
    public IActionResult UpdatePayment(int billId, [FromQuery] string status)
    {
        var currentUserId = GetCurrentUserId();

        if (!IsAdmin() && !_service.CanAccessBill(billId, currentUserId))
            return Forbid();

        return Ok(new { message = _service.UpdatePaymentStatus(billId, status) });
    }

    //  REFUND
    [HttpPost("refund/{billId}")]
    public IActionResult Refund(int billId)
    {
        var currentUserId = GetCurrentUserId();

        if (!IsAdmin() && !_service.CanAccessBill(billId, currentUserId))
            return Forbid();

        return Ok(new { message = _service.Refund(billId) });
    }
    
    [HttpPost("checkout/{cartId}")]
    public async Task<IActionResult> Checkout(int cartId)
    {
        var currentUserId = GetCurrentUserId();
        var cartOwnerId = _service.GetCartOwnerId(cartId);

        if (!cartOwnerId.HasValue)
            return NotFound();

        if (!IsAdmin() && cartOwnerId.Value != currentUserId)
            return Forbid();

        var token = HttpContext.Request.Headers["Authorization"].ToString();

        var result = await _service.CheckoutFromCart(cartId, token);

        return Ok(result);
    }
    
    [HttpGet("history/{userId}")]
    public async Task<IActionResult> GetOrderHistory(int userId)
    {
        if (!IsAdmin() && userId != GetCurrentUserId())
            return Forbid();

        var result = await _service.GetOrderHistory(userId);
        return Ok(result);
    }
    
    [HttpGet("invoice/{billId}")]
    public async Task<IActionResult> GetInvoice(int billId)
    {
        var currentUserId = GetCurrentUserId();

        if (!IsAdmin() && !_service.CanAccessBill(billId, currentUserId))
            return Forbid();

        var result = await _service.GetInvoice(billId);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
    
    [HttpGet("invoice/pdf/{billId}")]
    public async Task<IActionResult> DownloadInvoice(int billId)
    {
        var currentUserId = GetCurrentUserId();

        if (!IsAdmin() && !_service.CanAccessBill(billId, currentUserId))
            return Forbid();

        var pdf = await _service.GenerateInvoicePdf(billId);

        return File(pdf, "application/pdf", $"Invoice_{billId}.pdf");
    }
}
