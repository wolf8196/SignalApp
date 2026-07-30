using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SignalApp.Protocol;
using SignalApp.Utils;

namespace SignalApp.Simulator.Core
{
    public sealed class SimulatorService : Worker, ISimulatorService
    {
        private static readonly List<ProtocolRecord> MockRecords = new List<ProtocolRecord>
        {
            new ProtocolRecord { Frequency = 103_600_000, Bandwidth = 12_500, SignalNoiseRatio = 15.24 },
            new ProtocolRecord { Frequency = 103_598_200, Bandwidth = 12_500, SignalNoiseRatio = 14.87 },
            new ProtocolRecord { Frequency = 103_601_800, Bandwidth = 12_500, SignalNoiseRatio = 15.61 },
            new ProtocolRecord { Frequency = 103_599_400, Bandwidth = 12_500, SignalNoiseRatio = 14.12 },
            new ProtocolRecord { Frequency = 103_602_100, Bandwidth = 12_500, SignalNoiseRatio = 15.93 },

            new ProtocolRecord { Frequency = 104_200_000, Bandwidth = 108_230, SignalNoiseRatio = 14.80 },
            new ProtocolRecord { Frequency = 104_185_000, Bandwidth = 108_230, SignalNoiseRatio = 13.95 },
            new ProtocolRecord { Frequency = 104_215_500, Bandwidth = 108_230, SignalNoiseRatio = 15.22 },

            new ProtocolRecord { Frequency =  99_800_000, Bandwidth = 25_000,  SignalNoiseRatio = 15.45 },
            new ProtocolRecord { Frequency =  99_812_000, Bandwidth = 25_000,  SignalNoiseRatio = 14.70 },
            new ProtocolRecord { Frequency =  99_791_500, Bandwidth = 25_000,  SignalNoiseRatio = 16.08 },

            new ProtocolRecord { Frequency = 105_500_000, Bandwidth = 12_500,  SignalNoiseRatio = 12.01 },
            new ProtocolRecord { Frequency = 105_497_800, Bandwidth = 12_500,  SignalNoiseRatio = 11.68 },
            new ProtocolRecord { Frequency = 105_503_200, Bandwidth = 12_500,  SignalNoiseRatio = 12.44 },

            new ProtocolRecord { Frequency = 103_605_000, Bandwidth = 12_500,  SignalNoiseRatio = 14.35 },
            new ProtocolRecord { Frequency = 103_597_500, Bandwidth = 12_500,  SignalNoiseRatio = 15.10 },
        };

        private readonly IProtocolEncoder encoder;

        private TcpListener? listener;

        public SimulatorService(IProtocolEncoder encoder)
        {
            this.encoder = encoder;
        }

        public event Action<string>? LogAdded;

        public Task InitAsync(int port, CancellationToken token)
        {
            listener = new TcpListener(IPAddress.Loopback, port);
            listener.Start();
            LogAdded?.Invoke($"Listening on port {port}...");
            return Task.CompletedTask;
        }

        protected override async Task ExecuteInternalAsync(CancellationToken token)
        {
            if (listener == null)
            {
                throw new InvalidOperationException("SimulatorService is not initialized. Call InitAsync first.");
            }

            try
            {
                var client = await listener.AcceptTcpClientAsync(token);
                using var stream = client.GetStream();

                var i = 0;
                while (!token.IsCancellationRequested)
                {
                    var record = MockRecords[i % MockRecords.Count] with { Timestamp = DateTime.UtcNow };
                    var bytes = encoder.Encode(record);
                    await stream.WriteAsync(bytes, token);
                    LogAdded?.Invoke($"Returned record {record}.");
                    LogAdded?.Invoke($"Waiting 1 second...");
                    await Task.Delay(1000, token);
                    i++;
                }
            }
            catch (Exception ex) when (ex is not OperationCanceledException && !token.IsCancellationRequested)
            {
                LogAdded?.Invoke($"Client error: {ex.Message}");
            }
        }

        protected override Task StopInternalAsync(CancellationToken token)
        {
            listener?.Dispose();
            return base.StopInternalAsync(token);
        }

        public override void Dispose()
        {
            listener?.Dispose();
            base.Dispose();
        }
    }
}