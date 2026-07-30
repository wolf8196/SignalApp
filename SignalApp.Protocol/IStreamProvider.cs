using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SignalApp.Protocol
{
    public interface IStreamProvider
    {
        Task<Stream> GetStreamAsync(CancellationToken token);
    }
}