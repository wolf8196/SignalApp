using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using SignalApp.Simulator.Core;
using SignalApp.Utils.Wpf;

namespace SignalApp.Simulator
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public App()
        {
            Services = ConfigureServices();
        }

        public IServiceProvider Services { get; }

        private static ServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddTransient<MainViewModel>();
            services.AddSingleton<IDispatcher, SynchronizationContextDispatcher>();
            services.AddCore();

            return services.BuildServiceProvider();
        }
    }
}