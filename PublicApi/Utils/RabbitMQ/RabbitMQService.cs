using PublicApi.Utils.RabbitMQ.Interface;
using RabbitMQ.Client;

public class RabbitMQService : IRabbitMQService
{
    private readonly ConnectionFactory _factory;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private bool _queueDeclared = false;

    public RabbitMQService()
    {
        _factory = new ConnectionFactory() { HostName = "localhost" };
        _connection = _factory.CreateConnection();
        _channel = _connection.CreateModel();
    }

    public void SendMessage(string queueName, byte[] messageBody)
    {
        if (!_queueDeclared)
        {
            _channel.QueueDeclare(queue: queueName, durable: false, exclusive: false, autoDelete: false, arguments: null);
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