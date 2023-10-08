using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.Events;
using StatefulAutomation;

namespace NetEntityAutomation.Automations.LightAutomation;

public abstract class LightAutomation<TFsmState>: StatefulAutomation<ILightAutomationConfig<TFsmState>, TFsmState> where TFsmState : Enum
{
    protected LightAutomation(
        ILogger logger, 
        ILightAutomationConfig<TFsmState> config,
        IHaContext haContext
        ) : base(logger, config, haContext)
    {
    }
    protected IObservable<StateChange> MotionSensorEvent =>
        HaContext.StateChanges().Where(e => e.New?.EntityId == Config.MotionSensors.EntityId);

    protected IObservable<ZhaEventData> SwitchEvent =>
        HaContext.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => e.Data?.DeviceIeee == Config.SwitchId)
            .Select(e => e.Data!);
}