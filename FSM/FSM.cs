using System.Reactive.Linq;
using LightExtensionMethods;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using Newtonsoft.Json;
using Stateless;

namespace FSM;

public record FsmConfig
{
    public long HoldTimeSeconds { get; init; } = 30;
    public long HoldTimeMinutes { get; init; } = 0;
    public long WaitForOffSeconds { get; init; } = 30;
    public long WaitForOffMinutes { get; init; } = 0;
    public MotionSwitchLightFsm.FsmState InitialState { get; init; } = MotionSwitchLightFsm.FsmState.Off;
    public DateTime StartAtTime { get; init; } = DateTime.Parse("20:00:00");
    public DateTime StopAtTime { get; init; } = DateTime.Parse("08:00:00");

    public bool NightMode { get; init; } = false;
    public object Sun { get; init; } = null!;

    public required IEnumerable<IOnOffTarget> Lights { get; init; }
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
    
    private readonly IEnumerable<IOnOffTarget> _lights;
    
    private IDisposable? _timer;
    private readonly FsmConfig _config;
    private const string StoragePath = "storage/fsm.json";
    private const long WaitTime = 30;
    
    public MotionSwitchLightFsm(ILogger logger, FsmConfig config)
    {
        _logger = logger;
        _stateMachine = new StateMachine<FsmState, FsmTrigger>(FsmState.Off);
        _lights = config.Lights;
        _config = config;
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
            .Permit(FsmTrigger.MotionOn, FsmState.OnByMotion)
            .Permit(FsmTrigger.SwitchOn, FsmState.OnBySwitch);

        _stateMachine.Configure(FsmState.OnByMotion)
            .OnEntry(TurnOnLights)
            .PermitReentry(FsmTrigger.MotionOn)
            .Ignore(FsmTrigger.TimeElapsed)
            .Permit(FsmTrigger.SwitchOn, FsmState.OnBySwitch)
            .Permit(FsmTrigger.MotionOff, FsmState.Off)
            .Permit(FsmTrigger.SwitchOff, FsmState.Off);

        _stateMachine.Configure(FsmState.OnBySwitch)
            .OnEntry(TurnOnLights)
            .OnEntry(() => StartTimer(_config.HoldTimeSeconds))
            .Ignore(FsmTrigger.MotionOn)
            .Ignore(FsmTrigger.MotionOff)
            .PermitReentry(FsmTrigger.SwitchOn)
            .Permit(FsmTrigger.SwitchOff, FsmState.Off)
            .Permit(FsmTrigger.TimeElapsed, FsmState.WaitingForMotion);

        _stateMachine.Configure(FsmState.WaitingForMotion)
            .OnEntry(() => StartTimer(_config.WaitForOffSeconds))
            .Ignore(FsmTrigger.MotionOff)
            .Permit(FsmTrigger.MotionOn, FsmState.OnBySwitch)
            .Permit(FsmTrigger.SwitchOn, FsmState.OnBySwitch)
            .Permit(FsmTrigger.MotionOff, FsmState.Off)
            .Permit(FsmTrigger.TimeElapsed, FsmState.Off);
        _logger.LogInformation("[FSM] FSM initialized in state {State}", State);
    }
    
    private void StartTimer(long waitTime = WaitTime)
    {
        _logger.LogInformation("[FSM] Starting timer for {WaitTime} seconds", waitTime);
        _timer = Observable.Timer(TimeSpan.FromSeconds(waitTime))
            .Subscribe( _ => TimeElapsed());
    }
    
    private void TurnOnLights()
    {   
        _logger.LogInformation("[FSM] Turning on lights");
        _lights.TurnOn();
        _timer?.Dispose();
    }
    
    private void TurnOffLights()
    {
        _logger.LogInformation("[FSM] Turning off lights");
        _lights.TurnOff();
        _timer?.Dispose();
    }
    
    private void UpdateState()
    {
        _logger.LogInformation("[FSM] Updating state in storage {State}", State);
        File.WriteAllText(StoragePath, ToJson());
    }
    
    
    public void SwitchOn()
    {   
        if (_config.StartAtTime <=  DateTime.Now && DateTime.Now <= _config.StopAtTime)
        {
            _logger.LogInformation("[FSM] Switching on");
            _stateMachine.Fire(FsmTrigger.SwitchOn);
        }
        else
        {
            _logger.LogInformation("Automation is not started at this time");
        }
    }

    public void SwitchOff()
    {
        if (_config.StartAtTime <= DateTime.Now && DateTime.Now <= _config.StopAtTime)
        {
            _logger.LogInformation("[FSM] Switching off");
            _stateMachine.Fire(FsmTrigger.SwitchOff);
        }
        else
        {
            _logger.LogInformation("Automation is not started at this time");
        }
    }

    public void MotionOn()
    {
        if (_config.StartAtTime <= DateTime.Now && DateTime.Now <= _config.StopAtTime)
        {
            _logger.LogInformation("[FSM] Motion on");
            _stateMachine.Fire(FsmTrigger.MotionOn);
        }
        else
        {
            _logger.LogInformation("Automation is not started at this time");
        }
    }

    public void MotionOff()
    {
        if (_config.StartAtTime <= DateTime.Now && DateTime.Now <= _config.StopAtTime)
        {
            _logger.LogInformation("[FSM] Motion off");
            _stateMachine.Fire(FsmTrigger.MotionOff);
        }
        else
        {
            _logger.LogInformation("Automation is not started at this time");
        }
    }

    public void TimeElapsed()
    {   
        _logger.LogInformation("[FSM] Time elapsed");
        _stateMachine.Fire(FsmTrigger.TimeElapsed);
    }

    private string ToJson()
    {   
        _logger.LogInformation("[FSM] Serializing to JSON");
        return JsonConvert.SerializeObject(this);
    }

    public static MotionSwitchLightFsm? FromJson(string jsonString)
    {
        return JsonConvert.DeserializeObject<MotionSwitchLightFsm>(jsonString);
    }
}