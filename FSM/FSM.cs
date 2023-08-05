using LightExtensionMethods;
using Microsoft.Extensions.Logging;
using NetDaemon.HassModel.Entities;
using Newtonsoft.Json;
using Stateless;

namespace NetEntityAutomation.FSM;

public class MotionSwitchLightFSM
{
    private ILogger _logger;

    public enum FSMState
    {
        OnByMotion,
        OnBySwitch,
        Off,
        WaitingForMotion
    }

    private enum FSMTrigger
    {
        MotionOn,
        MotionOff,
        SwitchOn,
        SwitchOff,
        TimeElapsed
    }

    public FSMState State => _stateMachine.State;
    private readonly StateMachine<FSMState, FSMTrigger> _stateMachine;
    
    private readonly IEnumerable<ILightEntity> _lights;
    
    private IDisposable? _timer;
    private const string StoragePath = "storage/fsm.json";

    public MotionSwitchLightFSM(ILogger logger, IEnumerable<ILightEntity> lights)
    {
        _logger = logger;
        _stateMachine = new StateMachine<FSMState, FSMTrigger>(FSMState.Off);
        _lights = lights;
        InitFSM();
    }

    [System.Text.Json.Serialization.JsonConstructor]
    public MotionSwitchLightFSM(string state, ILogger logger)
    {
        _logger = logger;
        var fsmState = (FSMState)Enum.Parse(typeof(FSMState), state);
        _stateMachine = new StateMachine<FSMState, FSMTrigger>(fsmState);
        InitFSM();
    }

    private void InitFSM()
    {
        _stateMachine.OnTransitionCompleted(_ => UpdateState());

        _stateMachine.Configure(FSMState.Off)
            .OnEntry(TurnOffLights)
            .Ignore(FSMTrigger.SwitchOff)
            .Ignore(FSMTrigger.MotionOff)
            .Ignore(FSMTrigger.TimeElapsed)
            .Permit(FSMTrigger.MotionOn, FSMState.OnByMotion)
            .Permit(FSMTrigger.SwitchOn, FSMState.OnBySwitch);

        _stateMachine.Configure(FSMState.OnByMotion)
            .OnEntry(TurnOnLights)
            .Ignore(FSMTrigger.MotionOn)
            .Ignore(FSMTrigger.TimeElapsed)
            .Permit(FSMTrigger.SwitchOn, FSMState.OnBySwitch)
            .Permit(FSMTrigger.MotionOff, FSMState.Off)
            .Permit(FSMTrigger.SwitchOff, FSMState.Off);

        _stateMachine.Configure(FSMState.OnBySwitch)
            .OnEntry(TurnOnLights)
            .Ignore(FSMTrigger.MotionOn)
            .Ignore(FSMTrigger.MotionOff)
            .Permit(FSMTrigger.SwitchOff, FSMState.Off)
            .Permit(FSMTrigger.TimeElapsed, FSMState.WaitingForMotion);

        _stateMachine.Configure(FSMState.WaitingForMotion)
            .OnEntry(_ => StartMotionTimer())
            .OnExit(_ => StopMotionTimer())
            .Ignore(FSMTrigger.MotionOff)
            .Permit(FSMTrigger.MotionOn, FSMState.OnBySwitch)
            .Permit(FSMTrigger.SwitchOn, FSMState.OnBySwitch)
            .Permit(FSMTrigger.TimeElapsed, FSMState.Off);
        _logger.LogInformation("FSM initialized in state {State}", State);
    }
    
    private void TurnOnLights()
    {
        _logger.LogInformation("[FSM] Turning on lights");
        _lights.TurnOn();
    }
    
    private void TurnOffLights()
    {
        _logger.LogInformation("[FSM] Turning off lights");
        _lights.TurnOff();
    }
    
    private void UpdateState()
    {
        _logger.LogInformation("Updating state in storage {State}", State);
        _logger.LogInformation("JSON state {State}", ToJson());
        File.WriteAllText(StoragePath, ToJson());
    }

    private void StopMotionTimer()
    {
        _logger.LogInformation("Stopping timer");
    }

    private void StartMotionTimer()
    {
        _logger.LogInformation("Starting timer");
    }

    public void SwitchOn()
    {   
        _logger.LogInformation("Switching on");
        _stateMachine.Fire(FSMTrigger.SwitchOn);
    }

    public void SwitchOff()
    {   
        _logger.LogInformation("Switching off");
        _stateMachine.Fire(FSMTrigger.SwitchOff);
    }

    public void MotionOn()
    {   
        _logger.LogInformation("Motion on");
        _stateMachine.Fire(FSMTrigger.MotionOn);
    }

    public void MotionOff()
    {   
        _logger.LogInformation("Motion off");
        _stateMachine.Fire(FSMTrigger.MotionOff);
    }

    public void TimeElapsed()
    {   
        _logger.LogInformation("Time elapsed");
        _stateMachine.Fire(FSMTrigger.TimeElapsed);
    }

    private string ToJson()
    {   
        _logger.LogInformation("Serializing to JSON");
        return JsonConvert.SerializeObject(this);
    }

    public static MotionSwitchLightFSM? FromJson(string jsonString)
    {
        return JsonConvert.DeserializeObject<MotionSwitchLightFSM>(jsonString);
    }
}