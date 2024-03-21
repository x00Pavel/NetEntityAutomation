using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Core.Configs;
using NetEntityAutomation.Extensions.Events;

namespace NetEntityAutomation.Core.Fsm;

public enum MainLightState
{
    Off,
    On
}

public enum MainLightTrigger
{
    SwitchOnTrigger,
    SwitchOffTrigger,
    TimerElapsed,
    AllOff
}

public struct MainLightActivateAction
{
    public Action<MainLightFsmBase> OffAction { get; init; }
    public Action<MainLightFsmBase> OnAction { get; init; }
}

public class MainLightFsmBase : FsmBase<MainLightState, MainLightTrigger, ILightEntityCore>
{
    public MainLightFsmBase(ILightEntityCore light, ILogger logger) : base(logger)
    {
        DefaultState = MainLightState.Off;
        Entity = light;
        StoragePath = $"{StorageDir}/{Entity.EntityId}_fsm.json";
        InitFsm();
    }

    public MainLightFsmBase Configure(MainLightActivateAction actions)
    {
        _fsm.Configure(MainLightState.Off)
            .OnActivate(() => actions.OffAction(this))
            .Ignore(MainLightTrigger.TimerElapsed)
            .PermitReentry(MainLightTrigger.SwitchOffTrigger)
            .PermitReentry(MainLightTrigger.AllOff)
            .Permit(MainLightTrigger.SwitchOnTrigger, MainLightState.On);

        _fsm.Configure(MainLightState.On)
            .OnActivate(() => actions.OnAction(this))
            .Ignore(MainLightTrigger.TimerElapsed)
            .PermitReentry(MainLightTrigger.SwitchOnTrigger)
            .Permit(MainLightTrigger.AllOff, MainLightState.Off)
            .Permit(MainLightTrigger.SwitchOffTrigger, MainLightState.Off);

        return this;
    }

    public void FireOn() => _fsm.Fire(MainLightTrigger.SwitchOnTrigger);

    public void FireOff() => _fsm.Fire(MainLightTrigger.SwitchOffTrigger);

    public void FireTimerElapsed() => _fsm.Fire(MainLightTrigger.TimerElapsed);

    public override void FireAllOff() => _fsm.Fire(MainLightTrigger.AllOff);
    
}