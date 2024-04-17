namespace PublicApi.Utils.RabbitMQ.Interface
{
    public interface IRabbitMQConsumer
    {
        void CloseConnection();
        Task ProcessMessage(string message);
    }
}
