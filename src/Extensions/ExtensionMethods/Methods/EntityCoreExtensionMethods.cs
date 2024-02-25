using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Extensions.ExtensionMethods;

public static class EntityCoreExtensionMethods
{
    public static IObservable<StateChange> StateChange(this IEntityCore entity)
    {
        return entity.HaContext.StateAllChanges().Where(e => e.Entity.EntityId == entity.EntityId);
    }
    
    public static string? State(this IEntityCore entity)
    {
        return entity.HaContext.GetState(entity.EntityId)?.State;
    }
}