using Microsoft.Extensions.DependencyInjection;
using SignalApp.Protocol;

namespace SignalApp.Simulator.Core
{
    public static class CoreServiceCollectionExtensions
    {
        public static IServiceCollection AddCore(this IServiceCollection services)
        {
            services.AddProtocolClient();
            services.AddSingleton<ISimulatorService, SimulatorService>();

            return services;
        }
    }
}