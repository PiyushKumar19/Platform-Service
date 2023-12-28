using System.Text;
using System.Text.Json;
using PlatformService.Dtos;
using RabbitMQ.Client;

namespace PlatformService.AsyncDataServices
{
    public class MessageBusClient : IMessageBusClient
    {
        private readonly IConfiguration configuration;
        private readonly IConnection _connection;
        private readonly IModel _channel;

        public MessageBusClient(IConfiguration configuration)
        {
            this.configuration = configuration;
            var factory = new ConnectionFactory() {HostName = configuration["RabbitMQHost"], 
                Port = int.Parse(configuration["RabbitMQPort"])};
            
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
                
                _channel.ExchangeDeclare(exchange: "trigger", type: ExchangeType.Fanout);

                _connection.ConnectionShutdown += RabbitMQ_ConnectionShutdown;

                Console.WriteLine("\n--> Connected to Message Bus\n");
            }
            catch(Exception ex)
            {
                Console.WriteLine($"\n--> Could not connect to the Message Bus: {ex.Message}\n");
            }
        }

        private void RabbitMQ_ConnectionShutdown(object? sender, ShutdownEventArgs e)
        {
            Console.WriteLine("\n--> RabbitMQ connection shutdown\n");
        }

        public void PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            var message = JsonSerializer.Serialize(platformPublishedDto);

            if (_connection.IsOpen)
            {
                Console.WriteLine("\n--> RabbitMQ Connection is Open, sending message...\n");
                // To send data
                SendMessage(message);
            }
            else
            {
                Console.WriteLine("\n--> RabbitMQ Connection is closed, not sending \n");
            }
        }

        private void SendMessage(string message)
        {
            var genBody = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(exchange: "trigger",
                routingKey: "",
                basicProperties: null,
                body: genBody);
            
            Console.WriteLine($"\n--> We have sent {message}\n");
        }

        private void Dispose()
        {
            Console.WriteLine("\n--> Connection is disposed");

            if (_connection.IsOpen)
            {
                _channel.Close();
                _connection.Close();
            }
        }
    }
}