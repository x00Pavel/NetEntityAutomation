using Microsoft.Extensions.Logging;
using NetEntityAutomation.Automations.AutomationConfig;

namespace NetEntityAutomation.FSM.LightFsm;

public enum OnOffFsmState
{
    OnByMotion,
    OnBySwitch,
    Off,
    WaitingForMotion
}

public enum OnOffFsmTrigger
{
    MotionOn,
    MotionOff,
    SwitchOn,
    SwitchOff,
    TimeElapsed
}

/// <summary>
/// This FSM incorporates both motion and switch events to control the lights.
/// Switch is expected to have 'on' and 'off' commands.
/// </summary>
public class OnOffLightFsm : LightFsm<OnOffFsmState, OnOffFsmTrigger>
{
    public OnOffLightFsm(ILogger logger, IFsmConfig<OnOffFsmState> config, string storageFileName) : base(logger, config, storageFileName)
    {
    }
    
    protected override void InitFsm()
    {
        // Switch triggers the action without any conditions
        StateMachine.Configure(OnOffFsmState.Off)
            .OnActivate(TurnOffLights)
            .OnEntry(TurnOffLights)
            .PermitReentry(OnOffFsmTrigger.SwitchOff)
            .Ignore(OnOffFsmTrigger.MotionOff)
            .Ignore(OnOffFsmTrigger.TimeElapsed)
            .PermitIf(OnOffFsmTrigger.MotionOn, OnOffFsmState.OnByMotion, SensorConditions)
            .PermitIf(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch, () => Config.SwitchConditionMet);

        StateMachine.Configure(OnOffFsmState.OnByMotion)
            .OnActivate(() => TurnOnWithTimer(Config.HoldOnTime))
            .OnEntry(() => TurnOnWithTimer(Config.HoldOnTime))
            .OnEntry(() => StartTimer(TimeSpan.FromMinutes(5))) // FIXME: move this value to the config
            .PermitReentry(OnOffFsmTrigger.MotionOn)
            .Permit(OnOffFsmTrigger.TimeElapsed, OnOffFsmState.OnBySwitch)
            .Permit(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch)
            .PermitIf(OnOffFsmTrigger.MotionOff, OnOffFsmState.Off, WorkingHours)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off);

        StateMachine.Configure(OnOffFsmState.OnBySwitch)
            .OnActivate(() => TurnOnWithTimer(Config.HoldOnTime))
            .OnEntry(() => TurnOnWithTimer(Config.HoldOnTime))
            .OnEntry(() => StartTimer(Config.HoldOnTime))
            .PermitReentry(OnOffFsmTrigger.MotionOn)
            .Ignore(OnOffFsmTrigger.MotionOff)
            .PermitReentry(OnOffFsmTrigger.SwitchOn)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off)
            .PermitIf(OnOffFsmTrigger.TimeElapsed, OnOffFsmState.WaitingForMotion, WorkingHours);

        StateMachine.Configure(OnOffFsmState.WaitingForMotion)
            .OnActivate(() => TurnOnWithTimer(Config.HoldOnTime))
            .OnEntry(() => TurnOnWithTimer(Config.HoldOnTime))
            .Ignore(OnOffFsmTrigger.MotionOff)
            .PermitIf(OnOffFsmTrigger.MotionOn, OnOffFsmState.OnBySwitch, SensorConditions)
            .Permit(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off)
            .Permit(OnOffFsmTrigger.TimeElapsed, OnOffFsmState.Off);
        Logger.LogInformation("FSM initialized in state {State}", State);
    }

    public void SwitchOn()
    {
        try
        {
            Logger.LogInformation("Switching on");
            StateMachine.Fire(OnOffFsmTrigger.SwitchOn);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }

    public void SwitchOff()
    {
        try
        {
            Logger.LogInformation("Switching off");
            StateMachine.Fire(OnOffFsmTrigger.SwitchOff);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }
}