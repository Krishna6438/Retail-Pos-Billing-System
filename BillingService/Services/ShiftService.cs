using BillingService.Data;
using BillingService.DTOs;
using BillingService.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BillingService.Services;

public class ShiftService
{
    private readonly AppDbContext _context;

    public ShiftService(AppDbContext context)
    {
        _context = context;
    }

    public Shift? GetCurrentShift(int userId)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.UserId == userId && s.Status == "Open");
        if (shift != null)
        {
            // Calculate current expected cash dynamically
            var cashPayments = _context.Bills
                .Include(b => b.Payment)
                .Where(b => b.ShiftId == shift.Id && b.Payment != null && b.Payment.Method == "Cash" && b.Payment.Status == "Completed")
                .Sum(b => b.Payment!.Amount);
                
            shift.ExpectedCash = shift.StartingFloat + cashPayments;
        }
        return shift;
    }

    public int OpenShift(int userId, decimal startingFloat)
    {
        var existingShift = GetCurrentShift(userId);

        if (existingShift != null)
            throw new Exception("You already have an open shift");

        var shift = new Shift
        {
            UserId = userId,
            OpenedAt = DateTime.UtcNow,
            StartingFloat = startingFloat,
            Status = "Open"
        };

        _context.Shifts.Add(shift);
        _context.SaveChanges();

        return shift.Id;
    }

    public object CloseShift(int userId, int shiftId, decimal actualCash)
    {
        var shift = _context.Shifts.FirstOrDefault(s => s.Id == shiftId && s.UserId == userId);

        if (shift == null)
            throw new Exception("Shift not found");

        if (shift.Status == "Closed")
            throw new Exception("Shift is already closed");

        // Calculate expected cash
        // Expected Cash = Starting Float + Cash Payments - Cash Refunds
        var cashPayments = _context.Bills
            .Include(b => b.Payment)
            .Where(b => b.ShiftId == shiftId && b.Payment != null && b.Payment.Method == "Cash" && b.Payment.Status == "Completed")
            .Sum(b => b.Payment!.Amount);

        // For simplicity, assuming no cash refunds for now, or add if Returns exist for this shift
        
        var expectedCash = shift.StartingFloat + cashPayments;
        var variance = actualCash - expectedCash;

        shift.ExpectedCash = expectedCash;
        shift.ActualCash = actualCash;
        shift.Variance = variance;
        shift.ClosedAt = DateTime.UtcNow;
        shift.Status = "Closed";

        _context.SaveChanges();

        return new
        {
            shift.Id,
            shift.OpenedAt,
            shift.ClosedAt,
            shift.StartingFloat,
            shift.ExpectedCash,
            shift.ActualCash,
            shift.Variance
        };
    }

    public List<Shift> GetAllShifts()
    {
        return _context.Shifts.OrderByDescending(s => s.OpenedAt).ToList();
    }
}
