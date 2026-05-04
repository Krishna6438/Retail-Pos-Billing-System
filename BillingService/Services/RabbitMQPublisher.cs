using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using BillingService.Configuration;
using BillingService.Events;
using Microsoft.Extensions.Options;

namespace BillingService.Services;

public class RabbitMQPublisher
{
    private readonly ConnectionFactory _factory;
    private readonly RabbitMqOptions _options;

    public RabbitMQPublisher(IOptions<RabbitMqOptions> options)
    {
        _options = options.Value;
        _factory = new ConnectionFactory()
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };
    }

    public void PublishBillCreated(BillCreatedEvent evt)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(
            queue: _options.BillCreatedQueue,
            durable: false,
            exclusive: false,
            autoDelete: false);

        var message = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            exchange: "",
            routingKey: _options.BillCreatedQueue,
            body: body);

        Console.WriteLine("Event Published");
    }
    
    public void PublishInventoryUpdate(InventoryUpdateEvent evt)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(_options.InventoryUpdateQueue, false, false, false);

        var message = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish("", _options.InventoryUpdateQueue, null, body);

        Console.WriteLine("Inventory event published");
    }
    
    public void PublishPaymentCompleted(PaymentCompletedEvent evt)
    {
        using var connection = _factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.QueueDeclare(_options.PaymentQueue, false, false, false);

        var message = JsonSerializer.Serialize(evt);
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish("", _options.PaymentQueue, null, body);

        Console.WriteLine(" Payment event published");
    }
}
