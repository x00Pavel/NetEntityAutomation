using System.Reactive.Linq;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.Events;

namespace NetEntityAutomation.Room.Triggers;

public class ZigbeeButton(IHaContext context, string deviceIeee): IEntityCore, ITriggerBase<ZhaEventData>
{
    public IHaContext HaContext { get; set; } = context;
    public string EntityId { get; } = deviceIeee;
    public string onCmd {get; init;} = "on";
    public string offCmd {get; init;} = "off";
    
    public IObservable<ZhaEventData> TriggerEvent =>
        HaContext.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => e.Data?.DeviceIeee == EntityId)
            .Select(e => e.Data!);

    public IObservable<ZhaEventData> On => TriggerEvent.Where(e => e.Command == onCmd);
    public IObservable<ZhaEventData> Off => TriggerEvent.Where(e => e.Command == offCmd);
    public bool IsOn()
    {
        throw new NotImplementedException();
    }

    public bool IsOff()
    {
        throw new NotImplementedException();
    }
}