using System.Reactive.Linq;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using NetEntityAutomation.Extensions.LightExtensionMethods;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.FSM.LightFsm;

public record FsmConfig
{
    public long HoldTimeSeconds { get; init; } = 360;
    public long HoldTimeMinutes { get; init; } = 0;
    public long WaitForOffSeconds { get; init; } = 180;
    public long WaitForOffMinutes { get; init; } = 0;
    public MotionSwitchLightFsm.FsmState InitialState { get; init; } = MotionSwitchLightFsm.FsmState.Off;

    /// <summary> When the automation starts </summary>
    public TimeSpan StartAtTime { get; init; } = DateTime.Parse("19:00:00").TimeOfDay;

    /// <summary> When the automation stops </summary>
    public TimeSpan StopAtTime { get; init; } = DateTime.Parse("06:00:00").TimeOfDay;

    /// <summary> Add custom behaviour during the night </summary>
    public bool NightMode { get; init; } = false;

    /// <summary> Sun entity from Home Assistant </summary>
    public object Sun { get; init; } = null!;

    public required IEnumerable<ILightEntityCore> Lights { get; init; }
}

public class MotionSwitchLightFsm
{
    private ILogger _logger;

    public enum FsmState
    {
        OnByMotion,
        OnBySwitch,
        Off,
        WaitingForMotion
    }

    private enum FsmTrigger
    {
        MotionOn,
        MotionOff,
        SwitchOn,
        SwitchOff,
        TimeElapsed
    }
    
    public FsmState State => _stateMachine.State;
    private readonly StateMachine<FsmState, FsmTrigger> _stateMachine;

    private readonly IEnumerable<ILightEntityCore>? _lights;

    private IDisposable? _timer;
    private readonly FsmConfig Config;
    private const string StoragePath = "storage/fsm.json";
    private const long WaitTime = 30;

    private bool NotWorkingHours()
    {   
        var now = DateTime.Now.TimeOfDay;
        var isWorkingHours = !(Config.StopAtTime <= now && now <= Config.StartAtTime);
        _logger.LogDebug("Is working {Now} hours: {IsWorkingHours}", now,  isWorkingHours);
        return isWorkingHours;
    }
        

    public MotionSwitchLightFsm(ILogger logger, FsmConfig config)
    {
        _logger = logger;
        _stateMachine = new StateMachine<FsmState, FsmTrigger>(FsmState.Off);
        _lights = config.Lights;
        Config = config;
        InitFSM();
    }

    [System.Text.Json.Serialization.JsonConstructor]
    public MotionSwitchLightFsm(string state, ILogger logger)
    {
        _logger = logger;
        var fsmState = (FsmState)Enum.Parse(typeof(FsmState), state);
        _stateMachine = new StateMachine<FsmState, FsmTrigger>(fsmState);
        InitFSM();
    }

    private void InitFSM()
    {
        _stateMachine.OnTransitionCompleted(_ => UpdateState());

        _stateMachine.Configure(FsmState.Off)
            .OnEntry(TurnOffLights)
            .Ignore(FsmTrigger.SwitchOff)
            .Ignore(FsmTrigger.MotionOff)
            .Ignore(FsmTrigger.TimeElapsed)
            .PermitIf(FsmTrigger.MotionOn, FsmState.OnByMotion, NotWorkingHours)
            .PermitIf(FsmTrigger.SwitchOn, FsmState.OnBySwitch);

        _stateMachine.Configure(FsmState.OnByMotion)
            .OnEntry(TurnOnLights)
            .PermitReentry(FsmTrigger.MotionOn)
            .Ignore(FsmTrigger.TimeElapsed)
            .Permit(FsmTrigger.SwitchOn, FsmState.OnBySwitch)
            .PermitIf(FsmTrigger.MotionOff, FsmState.Off, NotWorkingHours)
            .Permit(FsmTrigger.SwitchOff, FsmState.Off);

        _stateMachine.Configure(FsmState.OnBySwitch)
            .OnEntry(TurnOnLights)
            .OnEntry(() => StartTimer(Config.HoldTimeSeconds + Config.HoldTimeMinutes * 60))
            .PermitReentry(FsmTrigger.MotionOn)
            .Ignore(FsmTrigger.MotionOff)
            .PermitReentry(FsmTrigger.SwitchOn)
            .Permit(FsmTrigger.SwitchOff, FsmState.Off)
            .Permit(FsmTrigger.TimeElapsed, FsmState.WaitingForMotion);

        _stateMachine.Configure(FsmState.WaitingForMotion)
            .OnEntry(() => StartTimer(Config.WaitForOffSeconds + Config.WaitForOffMinutes * 60))
            .Ignore(FsmTrigger.MotionOff)
            .Permit(FsmTrigger.MotionOn, FsmState.OnBySwitch)
            .Permit(FsmTrigger.SwitchOn, FsmState.OnBySwitch)
            .Permit(FsmTrigger.SwitchOff, FsmState.Off)
            .Permit(FsmTrigger.TimeElapsed, FsmState.Off);
        _logger.LogInformation("FSM initialized in state {State}", State);
    }

    private void StartTimer(long waitTime = WaitTime)
    {
        _logger.LogInformation("Starting timer for {WaitTime} seconds", waitTime);
        _timer?.Dispose();
        _timer = Observable.Timer(TimeSpan.FromSeconds(waitTime))
            .Subscribe(_ => TimeElapsed());
    }

    private void TurnOnLights()
    {
        _logger.LogInformation("Turning on lights");
        _lights?.TurnOn();
        _timer?.Dispose();
    }

    private void TurnOffLights()
    {
        _logger.LogInformation("Turning off lights");
        _lights?.TurnOff();
        _timer?.Dispose();
    }

    private void UpdateState()
    {
        _logger.LogInformation("Updating state in storage {State}", State);
        File.WriteAllText(StoragePath, ToJson());
    }


    public void SwitchOn()
    {
        _logger.LogInformation("Switching on");
        _stateMachine.Fire(FsmTrigger.SwitchOn);
    }

    public void SwitchOff()
    {
        _logger.LogInformation("Switching off");
        _stateMachine.Fire(FsmTrigger.SwitchOff);
    }

    public void MotionOn()
    {
        _logger.LogInformation("Motion on");
        _stateMachine.Fire(FsmTrigger.MotionOn);
    }

    public void MotionOff()
    {
        _logger.LogInformation("Motion off");
        _stateMachine.Fire(FsmTrigger.MotionOff);
    }

    public void TimeElapsed()
    {
        _logger.LogInformation("Time elapsed");
        _stateMachine.Fire(FsmTrigger.TimeElapsed);
    }

    private string ToJson()
    {
        _logger.LogInformation("Serializing to JSON");
        return JsonConvert.SerializeObject(this);
    }

    public static MotionSwitchLightFsm? FromJson(string jsonString)
    {
        return JsonConvert.DeserializeObject<MotionSwitchLightFsm>(jsonString);
    }
}