using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NotificationService.Configuration;
using NotificationService.Events;
using NotificationService.Models;
using NotificationService.Repositories;

namespace NotificationService.Services;

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

        //  BILL QUEUE
        channel.QueueDeclare(_options.BillCreatedQueue, false, false, false);

        var billConsumer = new EventingBasicConsumer(channel);

        billConsumer.Received += (model, ea) =>
        {
            try
            {
                var message = Encoding.UTF8.GetString(ea.Body.ToArray());
                var billEvent = JsonSerializer.Deserialize<BillCreatedEvent>(message);

                if (billEvent == null)
                    return;

                using var scope = _serviceProvider.CreateScope();
                var repo = scope.ServiceProvider.GetRequiredService<NotificationRepository>();

                repo.Add(new Notification()
                {
                    Message = $"Bill #{billEvent.BillId} created successfully for amount {billEvent.TotalAmount}.",
                    Type = "Bill",
                    UserId = billEvent.UserId
                });

                Console.WriteLine("SAVED TO DB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($" ERROR: {ex.Message}");
            }
        };

        channel.BasicConsume(_options.BillCreatedQueue, true, billConsumer);

        //  PAYMENT QUEUE
        channel.QueueDeclare(_options.PaymentQueue, false, false, false);

        var paymentConsumer = new EventingBasicConsumer(channel);

        paymentConsumer.Received += (model, ea) =>
        {
            var message = Encoding.UTF8.GetString(ea.Body.ToArray());

            var data = JsonSerializer.Deserialize<PaymentCompletedEvent>(message);

            if (data == null || string.IsNullOrWhiteSpace(data.Status))
                return;

            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<NotificationService>();

            service.Create(
                data.UserId,
                $"Payment {data.Status} for Bill #{data.BillId}",
                "Payment"
            );
        };

        channel.BasicConsume(_options.PaymentQueue, true, paymentConsumer);

        Console.WriteLine(" NotificationService listening...");
    }
}
