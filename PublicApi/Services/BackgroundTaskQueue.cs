using PublicApi.Services.Interface;
using System.Collections.Concurrent;

namespace PublicApi.Services
{
    public class BackgroundTaskQueue : IBackgroundTaskQueue
    {
        private ConcurrentQueue<Func<CancellationToken, Task>> _workItems = new ConcurrentQueue<Func<CancellationToken, Task>>();
        private SemaphoreSlim _signal = new SemaphoreSlim(0);

        public void Enqueue(Func<CancellationToken, Task> task)
        {
            if (task == null)
            {
                throw new ArgumentNullException(nameof(task));
            }

            _workItems.Enqueue(task);
            _signal.Release();
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            await _signal.WaitAsync(cancellationToken);
            _workItems.TryDequeue(out var workItem);

            return workItem;
        }
    }
}
