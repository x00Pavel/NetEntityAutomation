using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace NetEntityAutomation.Core.RoomManager;

public static class RoomManagerInstaller
{
    public static IServiceCollection AddRoomManager(this IServiceCollection serviceCollection)
    {
        serviceCollection.Scan(selector =>
                // Entry assembly is ND app in this case.
                selector.FromEntryAssembly()
                    .AddClasses(
                        classSelector => classSelector.AssignableTo<IRoomConfig>()
                    )
                    .UsingRegistrationStrategy(RegistrationStrategy.Append)
                    .AsImplementedInterfaces()
            )
            // Scoped lifetime for RoomManager is used because IHAContext is scoped
            .AddScoped<IRoomManager, RoomManager>();
        return serviceCollection;
    }
}