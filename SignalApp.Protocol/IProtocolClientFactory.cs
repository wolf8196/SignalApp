using System.IO;

namespace SignalApp.Protocol
{
    public interface IProtocolClientFactory
    {
        IProtocolClient Create(Stream stream);
    }
}