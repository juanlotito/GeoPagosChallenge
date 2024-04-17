namespace PublicApi.Utils.RabbitMQ.Interface
{
    public interface IRabbitMQConsumer
    {
        void CloseConnection();
        Task CheckAuthorizationStatus();
        Task ProcessMessage(string message);
    }
}
