using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.Extensions.Events;

namespace NetEntityAutomation.Automations.LightAutomation;

public abstract class LightAutomation<TFsmState>: BaseAutomation where TFsmState : Enum
{
    protected new ILightAutomationConfig<TFsmState> Config { get; init; }

    protected LightAutomation(ILogger logger, ILightAutomationConfig<TFsmState> config, IHaContext haContext) : base(logger, config, haContext)
    {
    }

    protected IObservable<StateChange> MotionSensorEvent =>
        HaContext.StateChanges().Where(e => e.New?.EntityId == Config.MotionSensors.EntityId);

    protected IObservable<ZhaEventData> SwitchEvent =>
        HaContext.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => e.Data?.DeviceIeee == Config.SwitchId)
            .Select(e => e.Data!);
}