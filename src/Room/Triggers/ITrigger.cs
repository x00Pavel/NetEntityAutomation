using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Room.Triggers;

public interface ITriggerBase {}

public interface ITriggerBase<out TEventType>: ITriggerBase
{
    public IObservable<TEventType> TriggerEvent { get; }
    public IObservable<TEventType> On { get; }
    public IObservable<TEventType>? Off { get; }
    public bool IsOn();
    bool IsOff();
}

public interface IStateChangeTrigger: ITriggerBase<StateChange>;