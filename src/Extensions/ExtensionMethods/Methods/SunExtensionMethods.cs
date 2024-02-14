using System.Reactive.Linq;
using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Extensions.ExtensionMethods;

public static class SunExtensionMethods
{
    public static IObservable<StateChange> StateChange(this ISunEntityCore sun)
    {
        return sun.HaContext.StateAllChanges().Where(e => e.New?.EntityId == sun.EntityId);
    }
    
    public static IObservable<StateChange> AboveHorizon(this ISunEntityCore sun)
    {
        return sun.StateChange().Where(e => e.New?.State == "above_horizon");
    }
    
    public static IObservable<StateChange> BelowHorizon(this ISunEntityCore sun)
    {
        return sun.StateChange().Where(e => e.New?.State == "below_horizon");
    }
    
    public static EntityState? GetCurrentState(this ISunEntityCore sun)
    {
        return sun.HaContext.GetState(sun.EntityId);
    }
}