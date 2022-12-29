using Microsoft.Extensions.DependencyInjection;
using MultiplayerLibrary.Handlers;
using MultiplayerLibrary.Interfaces.Handlers;
using MultiplayerLibrary.Interfaces.Services;
using MultiplayerLibrary.Services;

namespace MultiplayerLibrary;

public static class Main
{
    public static IServiceCollection AddMultiplayerLibrary(this IServiceCollection services)
    {
        services = services.AddSingleton<ICompressionService, CompressionService>();
        services = services.AddTransient<IConnectionHandler, ConnectionHandler>();

        return services;
    }
}
