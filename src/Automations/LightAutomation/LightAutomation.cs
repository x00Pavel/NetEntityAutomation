using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.Extensions.Events;

namespace NetEntityAutomation.Automations.LightAutomation;

public abstract class LightAutomation<TFsmState>: BaseAutomation<TFsmState> where TFsmState : struct, Enum
{
    protected LightAutomation(ILogger logger, IAutomationConfig<TFsmState> config, IHaContext haContext) : base(logger, config, haContext)
    {
    }

    protected IObservable<StateChange> MotionSensorEvent =>
        HaContext.StateChanges().Where(e => Config.MotionSensors.Any(s => s.EntityId == e.New?.EntityId));

    protected IObservable<ZhaEventData> SwitchEvent =>
        HaContext.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => Config.SwitchIds?.Contains(e.Data?.DeviceIeee) ?? false)
            .Select(e => e.Data!);
}