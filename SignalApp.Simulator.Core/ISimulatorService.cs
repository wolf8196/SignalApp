using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace SignalApp.Simulator.Core
{
    public interface ISimulatorService : IHostedService
    {
        event Action<string>? LogAdded;

        Task InitAsync(int port, CancellationToken token);
    }
}