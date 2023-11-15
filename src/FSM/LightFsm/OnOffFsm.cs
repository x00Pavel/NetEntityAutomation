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
public class MotionSwitchLightFsm : LightFsm<OnOffFsmState, OnOffFsmTrigger>
{
    public MotionSwitchLightFsm(ILogger logger, IFsmConfig<OnOffFsmState> config, string storagePath) : base(logger, config, storagePath)
    {
    }

    // [System.Text.Json.Serialization.JsonConstructor]
    // public MotionSwitchLightFsm(string state, ILogger logger): base(logger)
    // {
    //     _logger = logger;
    //     var fsmState = (FsmState)Enum.Parse(typeof(FsmState), state);
    //     StateMachine = new StateMachine<FsmState, FsmTrigger>(fsmState);
    //     InitFSM();
    // }
    
    protected override void InitFsm()
    {
        // Switch triggers the action without any conditions
        StateMachine.OnTransitionCompleted(_ => UpdateState());

        StateMachine.Configure(OnOffFsmState.Off)
            .OnEntry(TurnOffLights)
            .PermitReentry(OnOffFsmTrigger.SwitchOff)
            .Ignore(OnOffFsmTrigger.MotionOff)
            .Ignore(OnOffFsmTrigger.TimeElapsed)
            .PermitIf(OnOffFsmTrigger.MotionOn, OnOffFsmState.OnByMotion, SensorConditions)
            .PermitIf(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch, () => Config.SwitchConditionMet);

        StateMachine.Configure(OnOffFsmState.OnByMotion)
            .OnEntry(TurnOnLights)
            .OnEntry(() => StartTimer(TimeSpan.FromMinutes(5))) // FIXME: move this value to the config
            .PermitReentry(OnOffFsmTrigger.MotionOn)
            .Permit(OnOffFsmTrigger.TimeElapsed, OnOffFsmState.OnBySwitch)
            .Permit(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch)
            .PermitIf(OnOffFsmTrigger.MotionOff, OnOffFsmState.Off, WorkingHours)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off);

        StateMachine.Configure(OnOffFsmState.OnBySwitch)
            .OnEntry(TurnOnLights)
            .OnEntry(() => StartTimer(Config.HoldOnTime))
            .PermitReentry(OnOffFsmTrigger.MotionOn)
            .Ignore(OnOffFsmTrigger.MotionOff)
            .PermitReentry(OnOffFsmTrigger.SwitchOn)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off)
            .PermitIf(OnOffFsmTrigger.TimeElapsed, OnOffFsmState.WaitingForMotion, WorkingHours);

        StateMachine.Configure(OnOffFsmState.WaitingForMotion)
            .OnEntry(() => StartTimer(Config.WaitForOffTime))
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

    public void MotionOn()
    {
        try
        {
            Logger.LogInformation("Motion on");
            StateMachine.Fire(OnOffFsmTrigger.MotionOn);
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
            StateMachine.Fire(OnOffFsmTrigger.MotionOff);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }
}