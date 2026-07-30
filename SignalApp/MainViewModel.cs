using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MathNet.Numerics;
using SignalApp.Core;
using SignalApp.Protocol;
using SignalApp.Utils.Wpf;

namespace SignalApp
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly IRecordManager recordManager;
        private readonly IMonitorService monitorService;
        private readonly IDispatcher dispatcher;

        [ObservableProperty]
        private ObservableCollection<MonitorRecordModel> records = new ObservableCollection<MonitorRecordModel>();

        [ObservableProperty]
        private string host = "127.0.0.1";

        [ObservableProperty]
        private int port = 1234;

        [ObservableProperty]
        private string state = string.Empty;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        public MainViewModel(IRecordManager recordManager, IMonitorService monitorService, IDispatcher dispatcher)
        {
            this.recordManager = recordManager;
            this.monitorService = monitorService;
            this.dispatcher = dispatcher;
            Subscribe();
        }

        [RelayCommand]
        private async Task Connect()
        {
            await HandleAndShowExceptions(async () =>
            {
                await monitorService.InitAsync(new TcpStreamProvider(Host, Port), default);
                await monitorService.StartAsync(default);
            });
        }

        [RelayCommand]
        private async Task Disconnect()
        {
            await HandleAndShowExceptions(async () =>
            {
                await monitorService.StopAsync(default);
            });
        }

        private void Subscribe()
        {
            recordManager.RecordAdded += rec => dispatcher.Invoke(() => Records.Add(Map(rec)));
            recordManager.RecordUpdated += rec => dispatcher.Invoke(() =>
            {
                var existingRecord = Records.FirstOrDefault(r => r.Id == rec.Id);
                if (existingRecord != null)
                {
                    var index = records.IndexOf(existingRecord);
                    Records[index] = Map(rec);
                }
            });
            monitorService.StateChanged += st => dispatcher.Invoke(() => State = st);
        }

        private async Task HandleAndShowExceptions(Func<Task> func)
        {
            ErrorMessage = string.Empty;

            try
            {
                await func();
            }
            catch (Exception ex)
            {
                ErrorMessage = $"* Error: {ex.Message}";
            }
        }

        private static MonitorRecordModel Map(MonitorRecord record)
        {
            return new MonitorRecordModel
            {
                Id = record.Id,
                Timestamp = record.Timestamp,
                BandwidthKiloHz = record.Bandwidth / Constants.Kilo,
                FrequencyMegaHz = record.Frequency / Constants.Mega,
                SignalNoiseRatio = record.SignalNoiseRatio,
                Count = record.Count,
                IsLive = record.IsLive,
            };
        }
    }
}