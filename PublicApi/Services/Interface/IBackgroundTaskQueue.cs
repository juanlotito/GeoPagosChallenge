namespace PublicApi.Services.Interface
{
    public interface IBackgroundTaskQueue
    {
        void Enqueue(Func<CancellationToken, Task> task);
        Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken);
    }
}
