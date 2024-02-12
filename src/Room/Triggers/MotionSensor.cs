using System.Reactive.Linq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Room.Triggers;

public class MotionSensor(IEnumerable<IBinarySensorEntityCore> sensors, IHaContext context)
    : IStateChangeTrigger
{
    public IHaContext HaContext { get; set; } = context;
    private IEnumerable<IBinarySensorEntityCore> Sensors { get; } = sensors;

    public IObservable<StateChange> TriggerEvent =>
        HaContext.StateChanges().Where(e => Sensors.Any(s => s.EntityId == e.New?.EntityId));

    public IObservable<StateChange> On => TriggerEvent.Where(e => e.New.IsOn());
    public IObservable<StateChange> Off => TriggerEvent.Where(e => e.New.IsOff());

    public MotionSensor(IBinarySensorEntityCore sensor, IHaContext context) : this(
        new List<IBinarySensorEntityCore> { sensor }, context)
    {
    }

    public bool IsOn() => Sensors.Any(s => HaContext.GetState(s.EntityId)?.State == "on");
    public bool IsOff() => Sensors.Any(s => HaContext.GetState(s.EntityId)?.State == "off");
}