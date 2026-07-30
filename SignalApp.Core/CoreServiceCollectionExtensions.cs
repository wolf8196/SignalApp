using Microsoft.Extensions.DependencyInjection;
using Savage.Providers;
using SignalApp.Protocol;

namespace SignalApp.Core
{
    public static class CoreServiceCollectionExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddProtocolClient();
            services.AddSingleton<IMonitorService, MonitorService>();
            services.AddSingleton<IRecordManager, RecordManagerV2>();
            services.AddSingleton<IGuidProvider, GuidProvider>();

            return services;
        }
    }
}