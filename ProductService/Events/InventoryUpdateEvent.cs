namespace ProductService.Events;

public class InventoryUpdateEvent
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
}