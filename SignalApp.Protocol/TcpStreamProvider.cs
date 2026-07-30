using System;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SignalApp.Protocol
{

    public class TcpStreamProvider : IStreamProvider
    {
        private readonly string host;
        private readonly int port;

        public TcpStreamProvider(string host, int port)
        {
            this.host = host;
            this.port = port;
        }

        public async Task<Stream> GetStreamAsync(CancellationToken token)
        {
            TcpClient? tcpClient = null;
            try
            {
                tcpClient = new TcpClient();

                var connectValueTask = tcpClient.ConnectAsync(host, port, token);

                if (!connectValueTask.IsCompleted)
                {
                    await connectValueTask.AsTask().WaitAsync(TimeSpan.FromSeconds(5), token);
                }

                return tcpClient.GetStream();
            }
            catch
            {
                tcpClient?.Dispose();
                throw;
            }
        }
    }
}