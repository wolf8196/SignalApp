using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SignalApp.Simulator.Core;
using SignalApp.Utils.Wpf;

namespace SignalApp.Simulator
{
    public sealed partial class MainViewModel : ObservableObject
    {
        private readonly ISimulatorService simulatorService;

        [ObservableProperty]
        private int port = 1234;

        [ObservableProperty]
        private string errorMessage = string.Empty;

        [ObservableProperty]
        private ObservableCollection<string> logEntries = [];

        public MainViewModel(ISimulatorService simulatorService, IDispatcher dispatcher)
        {
            this.simulatorService = simulatorService;
            this.simulatorService.LogAdded += message => dispatcher.Invoke(() => LogMessage(message));
        }

        [RelayCommand]
        private async Task ConnectAsync()
        {
            await HandleAndShowExceptions(async () =>
            {
                await simulatorService.InitAsync(Port, default);
                await simulatorService.StartAsync(default);
            });
        }

        [RelayCommand]
        private async Task Disconnect()
        {
            await HandleAndShowExceptions(async () =>
            {
                await simulatorService.StopAsync(default);
            });
        }

        private void LogMessage(string message)
        {
            LogEntries.Add(message);
        }

        private async Task HandleAndShowExceptions(Func<Task> func)
        {
            try
            {
                await func();
            }
            catch (Exception ex)
            {
                LogMessage($"Error: {ex.Message}");
            }
        }
    }
}