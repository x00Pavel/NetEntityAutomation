using Microsoft.Extensions.Logging;
using Stateless;

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

public class ToggleFsm : BaseFsm<ToggleFsmState, ToggleFsmTrigger>
{
    public ToggleFsm(ILogger logger, FsmConfig<ToggleFsmState> config) : base(logger, config)
    {
        StateMachine = new StateMachine<ToggleFsmState, ToggleFsmTrigger>(ToggleFsmState.Off);
        InitFsm();
    }

    private void InitFsm()
    {
        StateMachine.OnTransitionCompleted(_ => UpdateState());
        StateMachine.Configure(ToggleFsmState.Off)
            // .OnEntry(TurnOffLights)
            .Ignore(ToggleFsmTrigger.TimeElapsed)
            .PermitReentry(ToggleFsmTrigger.MotionOff)
            .PermitIf(ToggleFsmTrigger.MotionOn, ToggleFsmState.On, WorkingHours)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.On);
        
        StateMachine.Configure(ToggleFsmState.On)
            // .OnEntry(TurnOnLights)
            .PermitReentry(ToggleFsmTrigger.MotionOn)
            .Permit(ToggleFsmTrigger.MotionOff, ToggleFsmState.WaitingForMotion)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.Off);
        StateMachine.Configure(ToggleFsmState.WaitingForMotion)
            // .OnEntry(StartTimer())
            .Permit(ToggleFsmTrigger.TimeElapsed, ToggleFsmState.Off)
            .Permit(ToggleFsmTrigger.MotionOn, ToggleFsmState.On)
            .Permit(ToggleFsmTrigger.Toggle, ToggleFsmState.Off);
    }
}