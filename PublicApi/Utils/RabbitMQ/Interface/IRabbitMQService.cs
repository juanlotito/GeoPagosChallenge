namespace PublicApi.Utils.RabbitMQ.Interface
{
    public interface IRabbitMQService
    {
        void SendMessage(string queueName, byte[] messageBody);
        void CloseConnection();
    }
}
