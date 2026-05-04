using System.ComponentModel.DataAnnotations;

namespace BillingService.DTOs;

public class BillItemDTO
{
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }
}
