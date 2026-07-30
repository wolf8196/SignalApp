using System.IO;

namespace SignalApp.Protocol
{
    internal class ProtocolClientFactory : IProtocolClientFactory
    {
        private readonly IProtocolEncoder encoder;

        public ProtocolClientFactory(IProtocolEncoder encoder)
        {
            this.encoder = encoder;
        }

        public IProtocolClient Create(Stream stream)
        {
            return new ProtocolClient(encoder, stream);
        }
    }
}