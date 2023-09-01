using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.LightExtensionMethods;
using Newtonsoft.Json;
using Stateless;

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
public class MotionSwitchLightFsm : BaseFsm<OnOffFsmState, OnOffFsmTrigger>
{
    private readonly IEnumerable<ILightEntityCore>? _lights;

    private const long WaitTime = 30;
    
    public MotionSwitchLightFsm(ILogger logger, FsmConfig<OnOffFsmState> config) : base(logger, config)
    {
        StateMachine = new StateMachine<OnOffFsmState, OnOffFsmTrigger>(OnOffFsmState.Off);
        _lights = config.Lights;
        InitFSM();
    }

    // [System.Text.Json.Serialization.JsonConstructor]
    // public MotionSwitchLightFsm(string state, ILogger logger): base(logger)
    // {
    //     _logger = logger;
    //     var fsmState = (FsmState)Enum.Parse(typeof(FsmState), state);
    //     StateMachine = new StateMachine<FsmState, FsmTrigger>(fsmState);
    //     InitFSM();
    // }

    private void InitFSM()
    {
        StateMachine.OnTransitionCompleted(_ => UpdateState());

        StateMachine.Configure(OnOffFsmState.Off)
            .OnEntry(TurnOffLights)
            .PermitReentry(OnOffFsmTrigger.SwitchOff)
            .Ignore(OnOffFsmTrigger.MotionOff)
            .Ignore(OnOffFsmTrigger.TimeElapsed)
            .PermitIf(OnOffFsmTrigger.MotionOn, OnOffFsmState.OnByMotion, WorkingHours)
            .PermitIf(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch);

        StateMachine.Configure(OnOffFsmState.OnByMotion)
            .OnEntry(TurnOnLights)
            .PermitReentry(OnOffFsmTrigger.MotionOn)
            .Ignore(OnOffFsmTrigger.TimeElapsed)
            .Permit(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch)
            .PermitIf(OnOffFsmTrigger.MotionOff, OnOffFsmState.Off, WorkingHours)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off);

        StateMachine.Configure(OnOffFsmState.OnBySwitch)
            .OnEntry(TurnOnLights)
            .OnEntry(() => StartTimer(Config.HoldTimeSeconds + Config.HoldTimeMinutes * 60))
            .PermitReentry(OnOffFsmTrigger.MotionOn)
            .Ignore(OnOffFsmTrigger.MotionOff)
            .PermitReentry(OnOffFsmTrigger.SwitchOn)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off)
            .Permit(OnOffFsmTrigger.TimeElapsed, OnOffFsmState.WaitingForMotion);

        StateMachine.Configure(OnOffFsmState.WaitingForMotion)
            .OnEntry(() => StartTimer(Config.WaitForOffSeconds + Config.WaitForOffMinutes * 60))
            .Ignore(OnOffFsmTrigger.MotionOff)
            .Permit(OnOffFsmTrigger.MotionOn, OnOffFsmState.OnBySwitch)
            .Permit(OnOffFsmTrigger.SwitchOn, OnOffFsmState.OnBySwitch)
            .Permit(OnOffFsmTrigger.SwitchOff, OnOffFsmState.Off)
            .Permit(OnOffFsmTrigger.TimeElapsed, OnOffFsmState.Off);
        Logger.LogInformation("FSM initialized in state {State}", State);
    }

    private void TurnOnLights()
    {
        Logger.LogInformation("Turning on lights");
        _lights?.TurnOn();
        Timer?.Dispose();
    }

    private void TurnOffLights()
    {
        Logger.LogInformation("Turning off lights");
        _lights?.TurnOff();
        Timer?.Dispose();
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
    private void StartTimer(long waitTime = WaitTime)
    {
        Logger.LogInformation("Starting timer for {WaitTime} seconds", waitTime);
        Timer?.Dispose();
        Timer = Observable.Timer(TimeSpan.FromSeconds(waitTime))
            .Subscribe(_ => TimeElapsed());
    }
    
    public void TimeElapsed()
    {
        try
        {
            Logger.LogInformation("Time elapsed");
            StateMachine.Fire(OnOffFsmTrigger.TimeElapsed);
        }
        catch (InvalidOperationException e)
        {
            Logger.LogInformation(e.Message);
        }
    }
}