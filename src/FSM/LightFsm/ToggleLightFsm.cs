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

public class ToggleLightFsm : LightFsm<ToggleFsmState, ToggleFsmTrigger>
{
    public ToggleLightFsm(ILogger logger, IFsmConfig<ToggleFsmState> config, string storageFileName) : base(logger, config, storageFileName)
    {
    }

    protected override void InitFsm()
    {
        StateMachine.Configure(ToggleFsmState.Off)
            .OnActivate(TurnOffLights)
            .OnEntry(TurnOffLights)
            .Ignore(ToggleFsmTrigger.TimeElapsed)
            .Ignore(ToggleFsmTrigger.MotionOff)
            .PermitIf(ToggleFsmTrigger.MotionOn, ToggleFsmState.On, SensorConditions)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.On);

        StateMachine.Configure(ToggleFsmState.On)
            .OnActivate(() => TurnOnWithTimer(Config.HoldOnTime))
            .OnEntry(() => TurnOnWithTimer(Config.HoldOnTime))
            .PermitReentry(ToggleFsmTrigger.MotionOn)
            .Permit(ToggleFsmTrigger.TimeElapsed, ToggleFsmState.WaitingForMotion)
            .Permit(ToggleFsmTrigger.MotionOff, ToggleFsmState.WaitingForMotion)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.Off);

        StateMachine.Configure(ToggleFsmState.WaitingForMotion)
            .OnActivate(() => TurnOnWithTimer(Config.WaitForOffTime))
            .OnEntry(() => TurnOnWithTimer(Config.WaitForOffTime))
            .Ignore(ToggleFsmTrigger.MotionOff)
            .Permit(ToggleFsmTrigger.TimeElapsed, ToggleFsmState.Off)
            .Permit(ToggleFsmTrigger.MotionOn, ToggleFsmState.On)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.Off);
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