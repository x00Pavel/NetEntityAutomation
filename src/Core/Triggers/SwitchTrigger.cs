using System.Reactive.Linq;
using NetDaemon.HassModel;
using NetEntityAutomation.Extensions.Events;

namespace NetEntityAutomation.Core.Triggers;

public class SwitchTriggerBase(IEnumerable<string> switchIds) : ITriggerBase<ZhaEventData>
{
    public IObservable<ZhaEventData> TriggerEvent =>
        HaContext.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => switchIds.Contains(e.Data?.DeviceIeee))
            .Select(e => e.Data!);

    public IObservable<ZhaEventData> On => TriggerEvent.Where(e => e.Command == "on");
    public IObservable<ZhaEventData>? Off => TriggerEvent.Where(e => e.Command == "off");

    public bool IsOn()
    {
        throw new NotImplementedException();
    }

    public bool IsOff()
    {
        throw new NotImplementedException();
    }

    public IHaContext? HaContext { get; set; }

    private IEnumerable<string> switchIds = switchIds;

    public SwitchTriggerBase(string switchId) : this(new List<string> { switchId })
    {
    }
}