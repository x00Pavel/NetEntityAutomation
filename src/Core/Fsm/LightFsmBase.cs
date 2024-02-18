using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.Events;
using NetEntityAutomation.Core.Configs;

namespace NetEntityAutomation.Core.Fsm;

public enum LightState
{
    OnByMotion,
    OnBySwitch,
    Off,
    OffBySwitch
}

public enum LightTrigger
{
    MotionOnTrigger,
    MotionOffTrigger,
    SwitchOnTrigger,
    SwitchOffTrigger,
    TimerElapsed,
    AllOff
}
public struct LightStateActivateAction
{
    public Action OnByMotionAction { get; init; }
    public Action OnBySwitchAction { get; init; }
    public Action OffAction { get; init; }
    public Action OffBySwitchAction { get; init; }
    
}
public class LightFsmBase : FsmBase<LightState, LightTrigger>
{
    public ILightEntityCore Light { get; set; }

    public LightFsmBase(ILightEntityCore light, AutomationConfig config, ILogger logger) : base(config, logger)
    {
        Logger = logger;
        Light = light;
        DefaultState = LightState.Off;
        StoragePath = $"storage/v1/{light.EntityId}_fsm.json";
        Timer = new CustomTimer(logger);
        InitFsm();
    }

    public void Configure(LightStateActivateAction lightStateActions)
    {
        _fsm.Configure(LightState.Off)
            .OnActivate(lightStateActions.OffAction)
            .OnEntry(Timer.Dispose)
            .Ignore(LightTrigger.TimerElapsed)
            .PermitReentry(LightTrigger.MotionOffTrigger)
            .PermitReentry(LightTrigger.SwitchOffTrigger)
            .PermitReentry(LightTrigger.AllOff)
            .Permit(LightTrigger.MotionOnTrigger, LightState.OnByMotion)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch);

        _fsm.Configure(LightState.OnByMotion)
            .OnActivate(lightStateActions.OnByMotionAction)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.MotionOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Permit(LightTrigger.AllOff, LightState.Off)
            .Permit(LightTrigger.TimerElapsed, LightState.Off)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch)
            .Permit(LightTrigger.SwitchOffTrigger, LightState.OffBySwitch);

        _fsm.Configure(LightState.OnBySwitch)
            .OnActivate(lightStateActions.OnBySwitchAction)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.SwitchOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Permit(LightTrigger.AllOff, LightState.Off)
            .PermitReentry(LightTrigger.MotionOnTrigger)
            .Permit(LightTrigger.TimerElapsed, LightState.Off)
            .Permit(LightTrigger.SwitchOffTrigger, LightState.OffBySwitch);

        _fsm.Configure(LightState.OffBySwitch)
            .OnActivate(lightStateActions.OffBySwitchAction)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.SwitchOffTrigger)
            .Ignore(LightTrigger.MotionOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Ignore(LightTrigger.TimerElapsed)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch)
            .Permit(LightTrigger.AllOff, LightState.Off);
        _fsm.Activate();
    }

    public void FireMotionOff()
    {
        _fsm.Fire(LightTrigger.MotionOffTrigger);
    }

    public void FireMotionOn()
    {
        _fsm.Fire(LightTrigger.MotionOnTrigger);
    }

    public void FireOn()
    {
        _fsm.Fire(LightTrigger.SwitchOnTrigger);
    }

    public void FireOff()
    {
        _fsm.Fire(LightTrigger.SwitchOffTrigger);
    }

    public override void FireAllOff()
    {
        _fsm.Fire(LightTrigger.AllOff);
    }

    public void FireTimerElapsed()
    {
        _fsm.Fire(LightTrigger.TimerElapsed);
    }
}