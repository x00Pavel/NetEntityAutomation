using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.FSM.LightFsm;

public enum ToggleFsmState
{
    On,
    Off,
    WaitingForMotion
}

public enum ToggleFsmTrigger
{
    MotionOn,
    MotionOff,
    Toggle,
    TimeElapsed
}

public class ToggleFsm : LightFsm<ToggleFsmState, ToggleFsmTrigger>
{
    public ToggleFsm(ILogger logger, IFsmConfig<ToggleFsmState> config) : base(logger, config)
    {
    }
    
    protected override void InitFsm()
    {
        StateMachine.OnTransitionCompleted(_ => UpdateState());
        StateMachine.Configure(ToggleFsmState.Off)
            .OnEntry(TurnOffLights)
            .Ignore(ToggleFsmTrigger.TimeElapsed)
            .PermitReentry(ToggleFsmTrigger.MotionOff)
            .PermitIf(ToggleFsmTrigger.MotionOn, ToggleFsmState.On, () => WorkingHours() && UserDefinedGuard())
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.On);

        StateMachine.Configure(ToggleFsmState.On)
            .OnEntry(TurnOnLights)
            .OnEntry(() => StartTimer(Config.HoldOnTime))
            .PermitReentry(ToggleFsmTrigger.MotionOn)
            .Permit(ToggleFsmTrigger.TimeElapsed, ToggleFsmState.WaitingForMotion)
            .Permit(ToggleFsmTrigger.MotionOff, ToggleFsmState.WaitingForMotion)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.Off);

        StateMachine.Configure(ToggleFsmState.WaitingForMotion)
            .OnEntry(() => StartTimer(Config.WaitForOffTime))
            .Ignore(ToggleFsmTrigger.MotionOff)
            .Permit(ToggleFsmTrigger.TimeElapsed, ToggleFsmState.Off)
            .Permit(ToggleFsmTrigger.MotionOn, ToggleFsmState.On)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.Off);
    }

    public void MotionOn()
    {
        try
        {
            Logger.LogInformation("Motion on");
            StateMachine.Fire(ToggleFsmTrigger.MotionOn);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }

    public void MotionOff()
    {
        try
        {
            Logger.LogInformation("Motion off");
            StateMachine.Fire(ToggleFsmTrigger.MotionOff);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }

    public void Toggle()
    {
        try
        {
            Logger.LogInformation("Toggle");
            StateMachine.Fire(ToggleFsmTrigger.Toggle);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }
}