using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using NetEntityAutomation.Room.Core;
using Scrutor;

namespace NetEntityAutomation.Room.RoomManager;

public static class RoomManagerInstaller
{
    public static IServiceCollection AddRoomManager(this IServiceCollection serviceCollection)
    {
        Console.WriteLine(Assembly.GetCallingAssembly().FullName);
        serviceCollection.Scan(selector =>
                // Entry assembly is ND app in this case.
                selector.FromEntryAssembly()
                    .AddClasses(
                        classSelector => classSelector.AssignableTo<IRoomConfigV1>()
                    )
                    .UsingRegistrationStrategy(RegistrationStrategy.Append)
                    .AsImplementedInterfaces()
            )
            // Scoped lifetime for RoomManager is used because IHAContext is scoped
            .AddScoped<IRoomManager, NetEntityAutomation.Room.RoomManager.RoomManager>();
        return serviceCollection;
    }
}