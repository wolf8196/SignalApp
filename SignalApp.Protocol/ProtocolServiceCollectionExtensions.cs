using Microsoft.Extensions.DependencyInjection;

namespace SignalApp.Protocol
{
    public static class ProtocolServiceCollectionExtensions
    {
        public static IServiceCollection AddProtocolClient(this IServiceCollection services)
        {
            services.AddSingleton<IProtocolEncoder, ProtocolEncoder>();
            services.AddSingleton<IProtocolClientFactory, ProtocolClientFactory>();
            return services;
        }
    }
}