using Microsoft.Extensions.DependencyInjection;
using Scrutor;

namespace NetEntityAutomation.Automations.AutomationConfig;


// public static class DiInstaller
// {
//     public static IServiceCollection AddRoomAssistant(this IServiceCollection serviceCollection)
//     {
//         serviceCollection.Scan(selector =>
//             selector.FromEntryAssembly()
//                 .AddClasses(
//                     classSelector =>
//                         classSelector.AssignableTo(typeof(IRoom)))
//                 .UsingRegistrationStrategy(RegistrationStrategy.Append)
//                 .AsImplementedInterfaces()
//         );
//         return serviceCollection;
//     }
// }