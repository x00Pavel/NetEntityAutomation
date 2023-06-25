using System;
using System.IO;
using Microsoft.Extensions.Logging;
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

    public enum FSMTrigger
    {
        MotionOn,
        MotionOff,
        SwitchOn,
        SwitchOff,
        TimeElapsed
    }

    public FSMState State => _stateMachine.State;
    private readonly StateMachine<FSMState, FSMTrigger> _stateMachine;

    public MotionSwitchLightFSM(ILogger logger)
    {
        _logger = logger;
        _stateMachine = new StateMachine<FSMState, FSMTrigger>(FSMState.Off);
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
            .Ignore(FSMTrigger.SwitchOff)
            .Ignore(FSMTrigger.MotionOff)
            .Ignore(FSMTrigger.TimeElapsed)
            .Permit(FSMTrigger.MotionOn, FSMState.OnByMotion)
            .Permit(FSMTrigger.SwitchOn, FSMState.OnBySwitch);

        _stateMachine.Configure(FSMState.OnByMotion)
            .Ignore(FSMTrigger.MotionOn)
            .Ignore(FSMTrigger.TimeElapsed)
            .Permit(FSMTrigger.SwitchOn, FSMState.OnBySwitch)
            .Permit(FSMTrigger.MotionOff, FSMState.Off)
            .Permit(FSMTrigger.SwitchOff, FSMState.Off);

        _stateMachine.Configure(FSMState.OnBySwitch)
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
    }

    private void UpdateState()
    {
        _logger.LogInformation("Updating state in storage {State}", State);
        _logger.LogInformation("JSON state {State}", ToJson());
        File.WriteAllText("/app/.storage/config_dump.json", ToJson());
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
        _stateMachine.Fire(FSMTrigger.SwitchOn);
    }

    public void SwitchOff()
    {
        _stateMachine.Fire(FSMTrigger.SwitchOff);
    }

    public void MotionOn()
    {
        _stateMachine.Fire(FSMTrigger.MotionOn);
    }

    public void MotionOff()
    {
        _stateMachine.Fire(FSMTrigger.MotionOff);
    }

    public void TimeElapsed()
    {
        _stateMachine.Fire(FSMTrigger.TimeElapsed);
    }

    public string ToJson()
    {
        return JsonConvert.SerializeObject(this);
    }

    public static MotionSwitchLightFSM FromJson(string jsonString)
    {
        return JsonConvert.DeserializeObject<MotionSwitchLightFSM>(jsonString);
    }
}