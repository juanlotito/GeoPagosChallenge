using Newtonsoft.Json;
using PublicApi.Models.Enum;
using PublicApi.Repositories.Interface;
using PublicApi.Services.Interface;
using PublicApi.Utils.RabbitMQ.Interface;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace PublicApi.Utils.RabbitMQ
{
    public class RabbitMQConsumer : IRabbitMQConsumer
    {
        private readonly ConnectionFactory _factory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _queueName;
        private readonly IPaymentRepository _paymentRepository;
        private Timer _timer;

        public RabbitMQConsumer(string queueName, IPaymentRepository paymentRepository)
        {
            try 
            {
                _factory = new ConnectionFactory() { HostName = "localhost" };
                _connection = _factory.CreateConnection();
                _channel = _connection.CreateModel();
                _queueName = queueName;
                _paymentRepository = paymentRepository;

                _channel.QueueDeclare(queue: _queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new EventingBasicConsumer(_channel);
                consumer.Received += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);
                    await ProcessMessage(message);
                };

                _channel.BasicConsume(queue: _queueName, autoAck: true, consumer: consumer);
            }
            catch (Exception e)
            {
                throw;
            }
            
        }

        public async Task ProcessMessage(string message)
        {
            dynamic messageObject = JsonConvert.DeserializeObject(message);
            int paymentRequestId = messageObject.PaymentRequestId;
            DateTime confirmationDeadline = messageObject.ConfirmationDeadline;

            _timer = new Timer(async _ =>
            {
                await VerifyPendingPayments(paymentRequestId, confirmationDeadline);
            }, null, TimeSpan.Zero, TimeSpan.FromSeconds(10));
        }

        public async Task VerifyPendingPayments(int paymentRequestId, DateTime confirmationDeadLine)
        {
            if (DateTime.Now < confirmationDeadLine)
            {
                var confirmed = await _paymentRepository.GetIsConfirmed(paymentRequestId);

                if (confirmed)
                {
                    await _paymentRepository.UpdatePaymentStatus(paymentRequestId, (int)PaymentStatus.Approved);
                    _timer.Dispose();
                    return;
                }
                else
                {
                    return;
                }
            }
            else
            {
                await _paymentRepository.ReversePayment(paymentRequestId);
            }
        }

        public void CloseConnection()
        {
            _timer.Dispose();
            _channel.Close();
            _connection.Close();
        }

    }
}
