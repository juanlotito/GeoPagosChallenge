using PublicApi.Utils.RabbitMQ;
using PublicApi.Utils.RabbitMQ.Interface;
using RabbitMQ.Client;
using System.Data;

public class RabbitMQService : IRabbitMQService
{
    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _queueDeclared = false;
    private readonly IDbConnection _db;
    public RabbitMQService(IDbConnection db)
    {
        _factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
        _db = db;
    }

    public void SendMessage(string queueName, byte[] messageBody)
    {
        if (!_queueDeclared)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
            var consumer = new RabbitMQConsumer("payment_confirmation", new PaymentRepository(_db));
            _queueDeclared = true;
        }

        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: messageBody);
    }

    public void CloseConnection()
    {
        _channel.Close();
        _connection.Close();
    }
}
