using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SignalApp.Utils
{
    public abstract class Worker : BackgroundService
    {
        private readonly SemaphoreSlim _lock;

        public Worker()
        {
            _lock = new SemaphoreSlim(1, 1);
        }

        public override async Task StartAsync(CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                if (ExecuteTask != null && !ExecuteTask.IsCompleted) // prevent duplicate start
                {
                    return;
                }

                await StartInternalAsync(cancellationToken);

                await base.StartAsync(cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            await _lock.WaitAsync(cancellationToken);
            try
            {
                await StopInternalAsync(cancellationToken);
                await base.StopAsync(cancellationToken);
            }
            finally
            {
                _lock.Release();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    await ExecuteInternalAsync(stoppingToken);
                }
            }
            catch
            {
                // ignore for now
            }
        }

        protected virtual Task StartInternalAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected virtual Task StopInternalAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        protected virtual Task ExecuteInternalAsync(CancellationToken token)
        {
            return Task.CompletedTask;
        }

        public override void Dispose()
        {
            _lock.Dispose();
            base.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}