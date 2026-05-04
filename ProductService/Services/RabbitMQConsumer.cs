using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using ProductService.Data;
using ProductService.Configuration;
using ProductService.Events;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

public class RabbitMQConsumer
{
    private readonly IServiceProvider _serviceProvider;
    private readonly RabbitMqOptions _options;

    public RabbitMQConsumer(IServiceProvider serviceProvider, IOptions<RabbitMqOptions> options)
    {
        _serviceProvider = serviceProvider;
        _options = options.Value;
    }

    public void Start()
    {
        var factory = new ConnectionFactory()
        {
            HostName = _options.HostName,
            UserName = _options.UserName,
            Password = _options.Password
        };

        var connection = factory.CreateConnection();
        var channel = connection.CreateModel();

        channel.QueueDeclare(_options.InventoryUpdateQueue, false, false, false);

        var consumer = new EventingBasicConsumer(channel);

        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            var data = JsonSerializer.Deserialize<InventoryUpdateEvent>(json);

            if (data == null)
                return;

            //  CREATE SCOPE HERE
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var product = context.Products
                    .Include(p => p.Inventory)
                    .FirstOrDefault(p => p.Id == data.ProductId);

                if (product != null && product.Inventory != null)
                {
                    if (product.Inventory.Quantity >= data.Quantity)
                    {
                        product.Inventory.Quantity -= data.Quantity;
                        context.SaveChanges();

                        Console.WriteLine($" Inventory updated for product {data.ProductId}");
                    }
                    else
                    {
                        Console.WriteLine(" Insufficient stock");
                    }
                }
            }
        };

        channel.BasicConsume(_options.InventoryUpdateQueue, true, consumer);

        Console.WriteLine(" ProductService listening...");
    }
}
