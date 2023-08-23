using Stateless;

namespace FSM;

public class BlindButtonFsm
{
    private enum FsmState
    {
        Opening,
        Stop,
        Closing
    }
    
    private enum FsmTrigger 
    {
        ButtonPress,
    }
    
    private FsmState State => _stateMachine.State;
    private readonly StateMachine<FsmState, FsmTrigger> _stateMachine;

    public BlindButtonFsm()
    {
        _stateMachine = new StateMachine<FsmState, FsmTrigger>(FsmState.Stop);
    }
}