using System;
using System.Threading;
using System.Threading.Tasks;
using Savage.Providers;
using SignalApp.Protocol;
using SignalApp.Utils;

namespace SignalApp.Core
{
    internal sealed class MonitorService : Worker, IMonitorService
    {
        private readonly IRecordManager recordManager;
        private readonly IGuidProvider guidProvider;
        private readonly IProtocolClientFactory clientFactory;

        private IProtocolClient? client;

        public MonitorService(IRecordManager recordManager, IGuidProvider guidProvider, IProtocolClientFactory clientFactory)
        {
            this.recordManager = recordManager;
            this.guidProvider = guidProvider;
            this.clientFactory = clientFactory;
        }

        public event Action<string>? StateChanged;

        public async Task InitAsync(IStreamProvider streamProvider, CancellationToken token)
        {
            var stream = await streamProvider.GetStreamAsync(token);
            client = clientFactory.Create(stream);
            StateChanged?.Invoke("Initialized");
        }

        protected override async Task StartInternalAsync(CancellationToken token)
        {
            await base.StartInternalAsync(token);
            StateChanged?.Invoke("Started");
        }

        protected override async Task ExecuteInternalAsync(CancellationToken token)
        {
            try
            {
                while (!token.IsCancellationRequested && client != null)
                {
                    var protocolRecord = await client.ReadNextAsync(token);
                    var monitorRecord = new MonitorRecord
                    {
                        Id = guidProvider.NewGuid(),
                        Timestamp = protocolRecord.Timestamp,
                        Bandwidth = protocolRecord.Bandwidth,
                        Frequency = protocolRecord.Frequency,
                        SignalNoiseRatio = protocolRecord.SignalNoiseRatio,
                    };
                    recordManager.PostRecord(monitorRecord);
                }
            }
            catch (Exception ex)
            {
                if (client != null && !token.IsCancellationRequested && ex is not OperationCanceledException) // use client null/not null as stopped flag
                {
                    StateChanged?.Invoke($"Error: {ex.Message}");
                }
            }
        }

        public override async Task StopAsync(CancellationToken token)
        {
            var client = this.client;
            this.client = null;

            await base.StopAsync(token);
            client?.Dispose();

            StateChanged?.Invoke("Stopped");
        }

        public override void Dispose()
        {
            client?.Dispose();
            base.Dispose();
        }
    }
}