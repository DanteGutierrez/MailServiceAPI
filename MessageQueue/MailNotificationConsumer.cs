using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using Newtonsoft.Json;

using FluentEmail.Core;
//using FluentEmail.Smtp;

namespace MailServiceAPI.MessageQueue
{

    public class MailNotificationConsumer : BackgroundService
    {
        private readonly ConnectionFactory _factory;
        private IConnection _connection;
        private IModel _channel;
        private readonly IConfiguration _iconfig;
        private readonly IServiceProvider _serviceProvider;

        public MailNotificationConsumer(IConfiguration iconfig, IServiceProvider serviceProvider)
        {

            _iconfig = iconfig;
            _serviceProvider = serviceProvider;

            // this version must look in the actual "ConnectionStrings" section of .json
            //var s2 = _iconfig.GetConnectionString("DefaultConnection");

            _factory = new ConnectionFactory()
            {
                HostName = _iconfig["RabbitMQ:host"],
                Port = int.Parse(_iconfig["RabbitMQ:port"]),
                //UserName = "guest",
                //Password = "password",
                VirtualHost = "/",
            };

            _connection = _factory.CreateConnection();
            _channel = _connection.CreateModel();
            //_channel.QueueDeclare(queue: "notifyQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.QueueDeclare(queue: "notifyQueue", durable: false, exclusive: false, arguments: null);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            stoppingToken.ThrowIfCancellationRequested();

            var consumer = new EventingBasicConsumer(_channel);

            consumer.Shutdown += OnConsumerShutdown;
            consumer.Registered += OnConsumerRegistered;
            consumer.Unregistered += OnConsumerUnregistered;
            consumer.ConsumerCancelled += OnConsumerConsumerCancelled;

            consumer.Received += (model, ea) =>
            {
                Console.WriteLine("New Message Recieved in Message Queue");
                var body = ea.Body;
                var message = Encoding.UTF8.GetString(body.ToArray());
                _channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                Console.WriteLine(message);
                MailNotification mail = JsonConvert.DeserializeObject<MailNotification>(message);

                string bodytext = $"Sent Via:\nUserId: {mail.SenderId}\nUserEmail: {mail.SenderEmail}\nUserName: {mail.SenderName}\n---\n{mail.Content}\n\nThis is an automated email, do not reply.";

                Console.WriteLine(bodytext);

                // you can't dependency-inject the IFluentEmail for some reason here, so I had to find this IServiceProvider work-around
                
                using (var scope = _serviceProvider.CreateScope())
                {
                    var response = scope.ServiceProvider.GetRequiredService<IFluentEmail>()
                        .To(mail.RecieverEmail).Subject(mail.Subject).Body(bodytext)
                        .SendAsync(stoppingToken);
                }
                
            };

            _channel.BasicConsume(queue: "notifyQueue", autoAck: false, consumer: consumer);

            return Task.CompletedTask;
        }

        private void OnConsumerConsumerCancelled(object sender, ConsumerEventArgs e) { }
        private void OnConsumerUnregistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerRegistered(object sender, ConsumerEventArgs e) { }
        private void OnConsumerShutdown(object sender, ShutdownEventArgs e) { }
        private void RabbitMQ_ConnectionShutdown(object sender, ShutdownEventArgs e) { }
    }
}