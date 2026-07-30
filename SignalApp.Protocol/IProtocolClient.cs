using System;
using System.Threading;
using System.Threading.Tasks;

namespace SignalApp.Protocol
{
    public interface IProtocolClient : IDisposable
    {
        Task<ProtocolRecord> ReadNextAsync(CancellationToken token);
    }
}