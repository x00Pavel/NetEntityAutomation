using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Core.Triggers;

public interface ITriggerBase
{
    public bool IsOn();
    public bool IsOff();
}

public interface ITriggerBase<out TEventType> : ITriggerBase
{
    public IObservable<TEventType> TriggerEvent { get; }
    public IObservable<TEventType> On { get; }
    public IObservable<TEventType>? Off { get; }
}

public interface IStateChangeTrigger : ITriggerBase<StateChange>;

public static class TriggerBaseExtensions
{
    
    public static bool IsAllOn(this IEnumerable<ITriggerBase> trigger)
    {
        return trigger.All(x => x.IsOn());
    }
    
    public static bool IsAllOff(this IEnumerable<ITriggerBase> trigger)
    {
        return trigger.All(x => x.IsOff());
    }
    
    public static bool IsAnyOn(this IEnumerable<ITriggerBase> trigger)
    {
        return trigger.Any(x => x.IsOn());
    }
    
    public static bool IsAnyOff(this IEnumerable<ITriggerBase> trigger)
    {
        return trigger.All(x => x.IsOff());
    }
}