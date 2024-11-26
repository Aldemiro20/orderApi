using RabbitMQ.Client;
using System;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace orderApi.Services
{
    public interface IRabbitMQProducer
    {
        Task SendMessageAsync<T>(T message, string queueName);
    }

    public class RabbitMQProducer : IRabbitMQProducer
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly ILogger<RabbitMQProducer> _logger;

        public RabbitMQProducer(IConnectionFactory connectionFactory, ILogger<RabbitMQProducer> logger)
        {
            _connectionFactory = connectionFactory;
            _logger = logger;
        }

        public async Task SendMessageAsync<T>(T message, string queueName)
        {
            const int maxRetries = 5; 
            int attempt = 0;
            bool messageSent = false;

            while (attempt < maxRetries && !messageSent)
            {
                try
                {
                    using var connection = _connectionFactory.CreateConnection();
                    using var channel = connection.CreateModel();

                    
                    channel.QueueDeclare(queue: queueName,
                                         durable: true,
                                         exclusive: false,
                                         autoDelete: false,
                                         arguments: null);

                   
                    var settings = new JsonSerializerSettings
                    {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore
                    };
                    var messageBody = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, settings));

                  
                    var properties = channel.CreateBasicProperties();
                    properties.Persistent = true;

                 
                    channel.BasicPublish(exchange: "",
                                         routingKey: queueName,
                                         basicProperties: properties,
                                         body: messageBody);

                  
                    messageSent = true;
                }
                catch (Exception ex)
                {
                    attempt++;
                    _logger.LogError(ex, "Erro ao enviar mensagem para a fila: {QueueName}. Tentativa {Attempt}/{MaxRetries}", queueName, attempt, maxRetries);

                    if (attempt < maxRetries)
                    {
                      
                        await Task.Delay(TimeSpan.FromSeconds(2));
                    }
                    else
                    {
                        _logger.LogError("Número máximo de tentativas atingido. Falha ao enviar mensagem para a fila: {QueueName}", queueName);
                        // Pode ser interessante enviar a mensagem para uma fila de dead-letter aqui ou outro mecanismo de fallback
                    }
                }
            }
        }
    }
}
