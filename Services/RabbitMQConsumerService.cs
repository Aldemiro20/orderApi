namespace orderApi.Services
{
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Hosting;
    using Newtonsoft.Json;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using System;
    using orderApi.Models;
    using orderApi.Data;

    public class RabbitMQConsumerService : BackgroundService
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RabbitMQConsumerService> _logger;

        public RabbitMQConsumerService(IConnectionFactory connectionFactory, IServiceProvider serviceProvider, ILogger<RabbitMQConsumerService> logger)
        {
            _connectionFactory = connectionFactory;
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "order_queue",
                                 durable: true,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += async (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                var orderModel = JsonConvert.DeserializeObject<OrderModel>(message);

     

                try
                {
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDBContext>();

                  
                        await dbContext.Database.BeginTransactionAsync();                    
                        var existingOrder = await dbContext.Orders
                                                           .FirstOrDefaultAsync(o => o.Id == orderModel.Id);
                        if (existingOrder != null)
                        {
                            existingOrder.Status = "Concluído";
                        }
                        else
                        {
                            dbContext.Orders.Add(orderModel);
                        }

                        await dbContext.SaveChangesAsync();

                        await dbContext.Database.CommitTransactionAsync();

                        channel.BasicAck(ea.DeliveryTag, false);

                    
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Erro ao processar mensagem: {ex.Message}");
                   
                    using (var scope = _serviceProvider.CreateScope())
                    {
                        var dbContext = scope.ServiceProvider.GetRequiredService<OrderDBContext>();
                        await dbContext.Database.RollbackTransactionAsync();
                    }
                }
            };

                channel.BasicConsume(queue: "order_queue",
                                 autoAck: false,
                                 consumer: consumer);

            return Task.CompletedTask;
        }
    }
}
