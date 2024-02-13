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
        CreateFsm();

        _fsm.Configure(LightState.Off)
            .OnEntry(Timer.Dispose)
            .Ignore(LightTrigger.TimerElapsed)
            .PermitReentry(LightTrigger.MotionOffTrigger)
            .PermitReentry(LightTrigger.SwitchOffTrigger)
            .PermitReentry(LightTrigger.AllOff)
            .Permit(LightTrigger.MotionOnTrigger, LightState.OnByMotion)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch);

        _fsm.Configure(LightState.OnByMotion)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.MotionOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Permit(LightTrigger.AllOff, LightState.Off)
            .Permit(LightTrigger.TimerElapsed, LightState.Off)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch)
            .Permit(LightTrigger.SwitchOffTrigger, LightState.OffBySwitch);

        _fsm.Configure(LightState.OnBySwitch)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.SwitchOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Permit(LightTrigger.AllOff, LightState.Off)
            .PermitReentry(LightTrigger.MotionOnTrigger)
            .Permit(LightTrigger.TimerElapsed, LightState.Off)
            .Permit(LightTrigger.SwitchOffTrigger, LightState.OffBySwitch);

        _fsm.Configure(LightState.OffBySwitch)
            .OnExit(Timer.Dispose)
            .PermitReentry(LightTrigger.SwitchOffTrigger)
            .Ignore(LightTrigger.MotionOnTrigger)
            .Ignore(LightTrigger.MotionOffTrigger)
            .Ignore(LightTrigger.TimerElapsed)
            .Permit(LightTrigger.SwitchOnTrigger, LightState.OnBySwitch)
            .Permit(LightTrigger.AllOff, LightState.Off);
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