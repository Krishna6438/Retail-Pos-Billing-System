namespace NotificationService.Configuration;

public class RabbitMqOptions
{
    public string HostName { get; set; } = "localhost";
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string BillCreatedQueue { get; set; } = "bill_created_queue";
    public string PaymentQueue { get; set; } = "payment_queue";
}
