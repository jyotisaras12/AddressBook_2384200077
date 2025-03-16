
using RepositoryLayer.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text;

namespace RepositoryLayer.Service
{
    public class RabbitMQConsumer : IRabbitMQConsumer
    {
        private readonly ConnectionFactory _connectionFactory;

        public RabbitMQConsumer(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void StartListening()
        {
            var connection = _connectionFactory.CreateConnection();
            var channel = connection.CreateModel();

            channel.QueueDeclare(queue: "TestQueue",
                                 durable: false,
                                 exclusive: false,
                                 autoDelete: false,
                                 arguments: null);

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine($" [x] Received: {message}");
            };

            channel.BasicConsume(queue: "TestQueue",
                                 autoAck: true,
                                 consumer: consumer);
        }
    }
}