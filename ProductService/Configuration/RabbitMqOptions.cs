namespace ProductService.Configuration;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string InventoryUpdateQueue { get; set; } = "inventory_update_queue";
}
