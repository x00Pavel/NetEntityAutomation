namespace NetEntityAutomation.Room.Triggers;

public interface ITriggerBase {}

public interface ITriggerBase<out TEventType>: ITriggerBase
{
    public new IObservable<TEventType> TriggerEvent { get; }
    public IObservable<TEventType> On { get; }
    public IObservable<TEventType>? Off { get; }
}
