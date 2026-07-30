using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using SignalApp.Protocol;

namespace SignalApp.Core
{
    public interface IMonitorService : IHostedService
    {
        event Action<string> StateChanged;

        Task InitAsync(IStreamProvider streamProvider, CancellationToken token);
    }
}