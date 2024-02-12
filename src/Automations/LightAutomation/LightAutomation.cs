using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Automations.AutomationConfig;
using NetEntityAutomation.Extensions.Events;

namespace NetEntityAutomation.Automations.LightAutomation;

public abstract class LightAutomation<TFsmState>(
    ILogger logger,
    ILightAutomationConfiguration<TFsmState> config,
    IHaContext haContext)
    : BaseAutomation<TFsmState>(logger, config, haContext)
    where TFsmState : struct, Enum
{
    protected IObservable<StateChange> MotionSensorEvent =>
        HaContext.StateChanges().Where(e => Config.MotionSensors.Any(s => s.EntityId == e.New?.EntityId));

    protected IObservable<ZhaEventData> ZhaSwitchEvent =>
        HaContext.Events.Filter<ZhaEventData>("zha_event")
            .Where(e => Config.SwitchIds?.Contains(e.Data?.DeviceIeee) ?? false)
            .Select(e => e.Data!);
    
    private IObservable<StateChange> AllLightEvent => HaContext.StateAllChanges()
        .Where(e => _lights.Select(s => s.EntityId).Contains(e.New?.EntityId));
    
    private IObservable<StateChange> LightEvent(string id) => HaContext.StateAllChanges()
        .Where(e => id == e.New?.EntityId);
    
    private IObservable<StateChange> UserEvent(string id) => LightEvent(id)
        .Where(e => !e.IsAutomationInitiated(config.ServiceAccountId));
    
    private IObservable<StateChange> UserOn(string id) => UserEvent(id)
        .Where(e => e.New?.State == "on");
    
    private IObservable<StateChange> UserOff(string id) => UserEvent(id)
        .Where(e => e.New?.State == "off");
    
    private IObservable<StateChange> AutomationEvent(string id) => LightEvent(id)
        .Where(e => e.IsAutomationInitiated(config.ServiceAccountId));

    private IObservable<StateChange> AutomationOn(string id) => AutomationEvent(id)
        .Where(e => e.New?.State == "on");
    
    private IObservable<StateChange> AutomationOff(string id) => AutomationEvent(id)
        .Where(e => e.New?.State == "off");
}

public class HaContext
{
}