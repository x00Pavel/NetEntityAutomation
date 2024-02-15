using System.Reactive.Linq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace NetEntityAutomation.Core.Triggers;

/// <summary>
/// Class represents a motion sensor.
/// It can be a single sensor or a group of sensors (e.g. all sensors in a room).
/// </summary>
/// <param name="sensors">A set of sensors to be used</param>
/// <param name="context">Home Assistant context</param>
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